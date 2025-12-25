using Microsoft.EntityFrameworkCore;
using ComputerStore.Data;
using ComputerStore.Models;
using ComputerStore.Models.DTOs;

namespace ComputerStore.Services;

public interface IReportService
{
    Task<List<StorePointEquipmentDto>> GetStorePointEquipment(int storePointId);
    Task<List<WarehouseStateDto>> GetCentralWarehouseState(DateTime date);
    Task<List<TotalWarehouseDto>> GetTotalWarehouse();
    Task<List<SellerSalesDto>> GetSellerSales(int sellerId);
    Task<List<SellerReportDto>> GetSellersSalesReport(DateTime startDate, DateTime endDate);
    Task<List<PopularProductDto>> GetPopularProducts(DateTime startDate, DateTime endDate);
    Task<RevenueReportDto> GetRevenueReport(DateTime startDate, DateTime endDate);
    Task<List<UnsoldProductDto>> GetUnsoldProducts(DateTime startDate, DateTime endDate);
    Task<List<SellerOrderDto>> GetSellerOrders(int sellerId);
    Task<List<WeeklySupplierOrderDto>> GetWeeklySupplierOrders(DateTime weekStart);
    Task<List<SellerScheduleDto>> GetSellerSchedule(int sellerId, int month, int year);
    Task<List<StorePointTurnoverDto>> GetStorePointTurnover(int month, int year);
    Task<List<CashLimitViolationDto>> GetCashLimitViolations();
    Task<List<MonthlyReportDto>> GetMonthlyReport(int month, int year);
}

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. Перечень товаров в торговом зале
    public async Task<List<StorePointEquipmentDto>> GetStorePointEquipment(int storePointId)
    {
        return await _context.Equipments
            .Where(e => e.StorePointId == storePointId && !e.IsSold)
            .Include(e => e.Supplier)
            .Select(e => new StorePointEquipmentDto
            {
                EquipmentId = e.Id,
                Name = e.Name,
                PurchasePrice = e.PurchasePrice,
                SupplierMarkup = e.SupplierMarkup,
                ReceiptDate = e.ReceiptDate,
                InvoiceNumber = e.InvoiceNumber,
                WarrantyMonths = e.WarrantyMonths,
                SupplierName = e.Supplier.Name
            })
            .ToListAsync();
    }

    // 2. Состояние центрального склада на дату
    public async Task<List<WarehouseStateDto>> GetCentralWarehouseState(DateTime date)
    {
        return await _context.Equipments
            .Where(e => e.IsOnCentralWarehouse && !e.IsSold && e.ReceiptDate <= date)
            .Include(e => e.Supplier)
            .Select(e => new WarehouseStateDto
            {
                EquipmentId = e.Id,
                Name = e.Name,
                PurchasePrice = e.PurchasePrice,
                ReceiptDate = e.ReceiptDate,
                SupplierName = e.Supplier.Name
            })
            .ToListAsync();
    }

    // 3. Общий склад фирмы
    public async Task<List<TotalWarehouseDto>> GetTotalWarehouse()
    {
        return await _context.Equipments
            .Where(e => !e.IsSold)
            .Include(e => e.Supplier)
            .Include(e => e.StorePoint)
            .Select(e => new TotalWarehouseDto
            {
                EquipmentId = e.Id,
                Name = e.Name,
                Location = e.IsOnCentralWarehouse
                    ? "Центральный склад"
                    : (e.StorePoint != null ? e.StorePoint.Name : "Неизвестно"),
                PurchasePrice = e.PurchasePrice,
                SupplierName = e.Supplier.Name
            })
            .ToListAsync();
    }

    // 4. Перечень сделок продавца
    public async Task<List<SellerSalesDto>> GetSellerSales(int sellerId)
    {
        return await _context.Sales
            .Where(s => s.SellerId == sellerId)
            .Include(s => s.StorePoint)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Equipment)
            .Select(s => new SellerSalesDto
            {
                SaleId = s.Id,
                SaleDate = s.SaleDate,
                PaymentType = s.PaymentType == PaymentType.Cash ? "Розница" : "Безнал",
                TotalAmount = s.TotalWithSalesTax,
                StorePointName = s.StorePoint.Name,
                Items = s.SaleItems.Select(si => si.Equipment.Name).ToList()
            })
            .ToListAsync();
    }

    // 5. Отчет о продажах продавцов
    public async Task<List<SellerReportDto>> GetSellersSalesReport(DateTime startDate, DateTime endDate)
    {
        return await _context.Sales
            .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
            .Include(s => s.Seller)
            .GroupBy(s => new { s.SellerId, s.Seller.FirstName, s.Seller.LastName, s.Seller.MiddleName })
            .Select(g => new SellerReportDto
            {
                SellerName = $"{g.Key.LastName} {g.Key.FirstName} {g.Key.MiddleName}",
                TotalSales = g.Sum(s => s.TotalWithSalesTax),
                SalesCount = g.Count()
            })
            .OrderByDescending(r => r.TotalSales)
            .ToListAsync();
    }

    // 6. Перечень проданных товаров по популярности
    public async Task<List<PopularProductDto>> GetPopularProducts(DateTime startDate, DateTime endDate)
    {
        return await _context.SaleItems
            .Where(si => si.Sale.SaleDate >= startDate && si.Sale.SaleDate <= endDate)
            .Include(si => si.Equipment)
            .GroupBy(si => si.Equipment.Name)
            .Select(g => new PopularProductDto
            {
                ProductName = g.Key,
                SalesCount = g.Count()
            })
            .OrderByDescending(p => p.SalesCount)
            .ToListAsync();
    }

    // 7. Суммарный доход фирмы
    public async Task<RevenueReportDto> GetRevenueReport(DateTime startDate, DateTime endDate)
    {
        var sales = await _context.Sales
            .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
            .Include(s => s.SaleItems)
            .ToListAsync();

        var cashRevenue = sales
            .Where(s => s.PaymentType == PaymentType.Cash)
            .Sum(s => s.TotalWithSalesTax);

        var cashlessRevenue = sales
            .Where(s => s.PaymentType == PaymentType.Cashless)
            .Sum(s => s.TotalWithVAT);

        var totalProfit = sales
            .SelectMany(s => s.SaleItems)
            .Sum(si => si.FinalPrice - si.PurchasePrice);

        return new RevenueReportDto
        {
            CashRevenue = cashRevenue,
            CashlessRevenue = cashlessRevenue,
            TotalRevenue = cashRevenue + cashlessRevenue,
            TotalProfit = totalProfit
        };
    }

    // 8. Перечень нереализованных товаров
    public async Task<List<UnsoldProductDto>> GetUnsoldProducts(DateTime startDate, DateTime endDate)
    {
        return await _context.Equipments
            .Where(e => !e.IsSold && e.ReceiptDate >= startDate && e.ReceiptDate <= endDate)
            .Include(e => e.StorePoint)
            .Select(e => new UnsoldProductDto
            {
                ProductName = e.Name,
                Location = e.IsOnCentralWarehouse
                    ? "Центральный склад"
                    : (e.StorePoint != null ? e.StorePoint.Name : "Неизвестно"),
                ReceiptDate = e.ReceiptDate,
                DaysInStock = (int)(DateTime.Now - e.ReceiptDate).TotalDays
            })
            .ToListAsync();
    }

    // 9. Заказы продавца за текущую неделю
    public async Task<List<SellerOrderDto>> GetSellerOrders(int sellerId)
    {
        var today = DateTime.Today;
        var weekStart = today.AddDays(-(int)today.DayOfWeek + 1);
        var weekEnd = weekStart.AddDays(7);

        return await _context.CustomerOrders
            .Where(o => o.SellerId == sellerId
                && o.OrderDate >= weekStart
                && o.OrderDate < weekEnd)
            .Select(o => new SellerOrderDto
            {
                OrderId = o.Id,
                OrderDate = o.OrderDate,
                EquipmentName = o.EquipmentName,
                Quantity = o.Quantity,
                Notes = o.Notes,
                IsProcessed = o.IsProcessed
            })
            .ToListAsync();
    }

    // 10. Общий недельный заказ фирмы
    public async Task<List<WeeklySupplierOrderDto>> GetWeeklySupplierOrders(DateTime weekStart)
    {
        var weekEnd = weekStart.AddDays(7);

        return await _context.SupplierOrders
            .Where(o => o.WeekStartDate >= weekStart && o.WeekEndDate <= weekEnd)
            .Include(o => o.Supplier)
            .Select(o => new WeeklySupplierOrderDto
            {
                SupplierName = o.Supplier.Name,
                WeekStart = o.WeekStartDate,
                WeekEnd = o.WeekEndDate,
                OrderedItems = new List<string> { o.OrderDetails }
            })
            .ToListAsync();
    }

    // 11. График работы продавца за месяц
    public async Task<List<SellerScheduleDto>> GetSellerSchedule(int sellerId, int month, int year)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        return await _context.SellerWorkSchedules
            .Where(s => s.SellerId == sellerId
                && s.WorkDate >= startDate
                && s.WorkDate < endDate)
            .Include(s => s.Seller)
            .Include(s => s.StorePoint)
            .Select(s => new SellerScheduleDto
            {
                Date = s.WorkDate,
                SellerName = $"{s.Seller.LastName} {s.Seller.FirstName} {s.Seller.MiddleName}",
                StorePointName = s.StorePoint.Name,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            })
            .OrderBy(s => s.Date)
            .ToListAsync();
    }

    // 12. Торговый оборот по торговым точкам
    public async Task<List<StorePointTurnoverDto>> GetStorePointTurnover(int month, int year)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        return await _context.Sales
            .Where(s => s.SaleDate >= startDate && s.SaleDate < endDate)
            .Include(s => s.StorePoint)
            .GroupBy(s => new { s.StorePointId, s.StorePoint.Name })
            .Select(g => new StorePointTurnoverDto
            {
                StorePointName = g.Key.Name,
                MonthlyRevenue = g.Sum(s => s.TotalWithSalesTax),
                SalesCount = g.Count()
            })
            .OrderByDescending(t => t.MonthlyRevenue)
            .ToListAsync();
    }

    // 13. Торговые точки с превышением лимита кассы
    public async Task<List<CashLimitViolationDto>> GetCashLimitViolations()
    {
        return await _context.CashLimitViolations
            .Include(v => v.CashRegister)
                .ThenInclude(c => c.StorePoint)
            .Select(v => new CashLimitViolationDto
            {
                StorePointName = v.CashRegister.StorePoint.Name,
                CashRegisterNumber = v.CashRegister.RegistrationNumber,
                ViolationDate = v.ViolationDate,
                LimitAmount = v.LimitAmount,
                ActualAmount = v.ActualAmount,
                ExcessAmount = v.ActualAmount - v.LimitAmount
            })
            .OrderByDescending(v => v.ViolationDate)
            .ToListAsync();
    }

    // Ежемесячный отчет о работе фирмы
    public async Task<List<MonthlyReportDto>> GetMonthlyReport(int month, int year)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var storePoints = await _context.StorePoints.ToListAsync();
        var reports = new List<MonthlyReportDto>();

        foreach (var sp in storePoints)
        {
            var shipped = await _context.Equipments
                .Where(e => e.StorePointId == sp.Id
                    && e.ReceiptDate >= startDate
                    && e.ReceiptDate < endDate
                    && !e.IsOnCentralWarehouse)
                .GroupBy(e => e.Name)
                .Select(g => new ShippedProductDto
                {
                    Name = g.Key,
                    Quantity = g.Count(),
                    ShipmentDate = g.Min(e => e.ReceiptDate)
                })
                .ToListAsync();

            var sold = await _context.SaleItems
                .Where(si => si.Sale.StorePointId == sp.Id
                    && si.Sale.SaleDate >= startDate
                    && si.Sale.SaleDate < endDate)
                .Include(si => si.Equipment)
                .GroupBy(si => si.Equipment.Name)
                .Select(g => new SoldProductDto
                {
                    Name = g.Key,
                    Quantity = g.Count(),
                    Revenue = g.Sum(si => si.FinalPrice),
                    Profit = g.Sum(si => si.FinalPrice - si.PurchasePrice)
                })
                .ToListAsync();

            reports.Add(new MonthlyReportDto
            {
                StorePointName = sp.Name,
                ShippedProducts = shipped,
                SoldProducts = sold,
                TotalRevenue = sold.Sum(s => s.Revenue),
                TotalProfit = sold.Sum(s => s.Profit)
            });
        }

        return reports;
    }
}