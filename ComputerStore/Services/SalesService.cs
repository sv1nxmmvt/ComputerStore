using ComputerStore.Data;
using ComputerStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.ServerSentEvents;

namespace ComputerStore.Services;

public interface ISalesService
{
    Task<Sale> CreateSale(int sellerId, int storePointId, PaymentType paymentType,
        List<SaleItemRequest> items, int? cashRegisterId = null);
    Task CheckAndRecordCashLimitViolation(int cashRegisterId, DateTime saleDate);
}

public class SaleItemRequest
{
    public int EquipmentId { get; set; }
    public decimal SellerMarkup { get; set; }
}

public class SalesService : ISalesService
{
    private readonly ApplicationDbContext _context;
    private const decimal VAT_RATE = 0.18m;
    private const decimal SALES_TAX_RATE = 0.05m;
    private const decimal MAX_TOTAL_MARKUP = 0.30m;

    public SalesService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Sale> CreateSale(
        int sellerId,
        int storePointId,
        PaymentType paymentType,
        List<SaleItemRequest> items,
        int? cashRegisterId = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Проверка продавца и торговой точки
            var seller = await _context.Sellers.FindAsync(sellerId);
            if (seller == null)
                throw new InvalidOperationException("Продавец не найден");

            var storePoint = await _context.StorePoints.FindAsync(storePointId);
            if (storePoint == null)
                throw new InvalidOperationException("Торговая точка не найдена");

            // Проверка возможности безналичного расчета
            if (paymentType == PaymentType.Cashless && !storePoint.CanProcessCashless)
                throw new InvalidOperationException("Данная торговая точка не может обрабатывать безналичные платежи");

            // Проверка кассового аппарата для наличных
            CashRegister? cashRegister = null;
            if (paymentType == PaymentType.Cash)
            {
                if (!cashRegisterId.HasValue)
                    throw new InvalidOperationException("Для наличной оплаты необходимо указать кассовый аппарат");

                cashRegister = await _context.CashRegisters
                    .FirstOrDefaultAsync(c => c.Id == cashRegisterId.Value && c.StorePointId == storePointId);

                if (cashRegister == null)
                    throw new InvalidOperationException("Кассовый аппарат не найден или не принадлежит данной торговой точке");
            }

            // Создание продажи
            var sale = new Sale
            {
                SellerId = sellerId,
                StorePointId = storePointId,
                PaymentType = paymentType,
                CashRegisterId = cashRegisterId,
                SaleDate = DateTime.UtcNow
            };

            var saleItems = new List<SaleItem>();
            decimal totalAmount = 0;

            foreach (var itemRequest in items)
            {
                var equipment = await _context.Equipments
                    .FirstOrDefaultAsync(e => e.Id == itemRequest.EquipmentId && !e.IsSold);

                if (equipment == null)
                    throw new InvalidOperationException($"Оборудование с ID {itemRequest.EquipmentId} не найдено или уже продано");

                // Проверка общей наценки
                var totalMarkup = equipment.SupplierMarkup + itemRequest.SellerMarkup;
                if (totalMarkup > MAX_TOTAL_MARKUP)
                    throw new InvalidOperationException(
                        $"Суммарная наценка ({totalMarkup:P}) превышает максимально допустимую (30%)");

                // Расчет цен
                var priceWithMarkup = equipment.PurchasePrice * (1 + equipment.SupplierMarkup + itemRequest.SellerMarkup);
                var vat = priceWithMarkup * VAT_RATE;
                var priceWithVAT = priceWithMarkup + vat;

                decimal salesTax = 0;
                decimal finalPrice = priceWithVAT;

                if (paymentType == PaymentType.Cash)
                {
                    salesTax = priceWithVAT * SALES_TAX_RATE;
                    finalPrice = priceWithVAT + salesTax;
                }

                var saleItem = new SaleItem
                {
                    Sale = sale,
                    EquipmentId = equipment.Id,
                    PurchasePrice = equipment.PurchasePrice,
                    SupplierMarkup = equipment.SupplierMarkup,
                    SellerMarkup = itemRequest.SellerMarkup,
                    PriceBeforeTaxes = priceWithMarkup,
                    VAT = vat,
                    SalesTax = salesTax,
                    FinalPrice = finalPrice
                };

                saleItems.Add(saleItem);
                totalAmount += finalPrice;

                // Отметка оборудования как проданного
                equipment.IsSold = true;
                equipment.SoldDate = DateTime.UtcNow;
            }

            // Проверка лимита кассы
            if (paymentType == PaymentType.Cash && cashRegister != null)
            {
                var todayStart = DateTime.Today;
                var todayEnd = todayStart.AddDays(1);

                var todaySales = await _context.Sales
                    .Where(s => s.CashRegisterId == cashRegisterId
                        && s.SaleDate >= todayStart
                        && s.SaleDate < todayEnd)
                    .SumAsync(s => s.TotalWithSalesTax);

                var projectedTotal = todaySales + totalAmount;

                if (projectedTotal > cashRegister.CashLimit)
                {
                    // Записываем нарушение
                    var violation = new CashLimitViolation
                    {
                        CashRegisterId = cashRegister.Id,
                        ViolationDate = DateTime.UtcNow,
                        LimitAmount = cashRegister.CashLimit,
                        ActualAmount = projectedTotal
                    };
                    _context.CashLimitViolations.Add(violation);

                    throw new InvalidOperationException(
                        $"Превышен лимит кассы. Лимит: {cashRegister.CashLimit:C}, Текущая сумма: {todaySales:C}, " +
                        $"Сумма продажи: {totalAmount:C}, Итого: {projectedTotal:C}");
                }
            }

            sale.TotalAmount = saleItems.Sum(si => si.PriceBeforeTaxes);
            sale.TotalWithVAT = saleItems.Sum(si => si.PriceBeforeTaxes + si.VAT);
            sale.TotalWithSalesTax = totalAmount;

            // Генерация номера чека или платежного поручения
            if (paymentType == PaymentType.Cash)
            {
                sale.CheckNumber = $"CHK-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }
            else
            {
                sale.PaymentOrderNumber = $"PP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }

            _context.Sales.Add(sale);
            _context.SaleItems.AddRange(saleItems);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return sale;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task CheckAndRecordCashLimitViolation(int cashRegisterId, DateTime saleDate)
    {
        var cashRegister = await _context.CashRegisters.FindAsync(cashRegisterId);
        if (cashRegister == null) return;

        var dayStart = saleDate.Date;
        var dayEnd = dayStart.AddDays(1);

        var dailyTotal = await _context.Sales
            .Where(s => s.CashRegisterId == cashRegisterId
                && s.SaleDate >= dayStart
                && s.SaleDate < dayEnd)
            .SumAsync(s => s.TotalWithSalesTax);

        if (dailyTotal > cashRegister.CashLimit)
        {
            var existingViolation = await _context.CashLimitViolations
                .FirstOrDefaultAsync(v => v.CashRegisterId == cashRegisterId
                    && v.ViolationDate.Date == saleDate.Date);

            if (existingViolation == null)
            {
                var violation = new CashLimitViolation
                {
                    CashRegisterId = cashRegisterId,
                    ViolationDate = saleDate,
                    LimitAmount = cashRegister.CashLimit,
                    ActualAmount = dailyTotal
                };

                _context.CashLimitViolations.Add(violation);
                await _context.SaveChangesAsync();
            }
        }
    }
}