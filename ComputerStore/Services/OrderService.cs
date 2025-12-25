using Microsoft.EntityFrameworkCore;
using ComputerStore.Data;
using ComputerStore.Models;

namespace ComputerStore.Services;

public interface IOrderService
{
    Task<CustomerOrder> CreateCustomerOrder(int sellerId, string equipmentName, int quantity, string notes);
    Task<List<SupplierOrder>> GenerateWeeklySupplierOrders(DateTime weekStart);
    Task<bool> TransferEquipmentToStorePoint(int equipmentId, int storePointId);
}

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerOrder> CreateCustomerOrder(
        int sellerId,
        string equipmentName,
        int quantity,
        string notes)
    {
        var seller = await _context.Sellers.FindAsync(sellerId);
        if (seller == null)
            throw new InvalidOperationException("Продавец не найден");

        var order = new CustomerOrder
        {
            SellerId = sellerId,
            OrderDate = DateTime.UtcNow,
            EquipmentName = equipmentName,
            Quantity = quantity,
            Notes = notes,
            IsProcessed = false
        };

        _context.CustomerOrders.Add(order);
        await _context.SaveChangesAsync();

        return order;
    }

    public async Task<List<SupplierOrder>> GenerateWeeklySupplierOrders(DateTime weekStart)
    {
        var weekEnd = weekStart.AddDays(7);

        // Получаем все необработанные заказы за неделю
        var customerOrders = await _context.CustomerOrders
            .Where(o => !o.IsProcessed
                && o.OrderDate >= weekStart
                && o.OrderDate < weekEnd)
            .Include(o => o.Seller)
            .ToListAsync();

        if (!customerOrders.Any())
            return new List<SupplierOrder>();

        // Группируем заказы по наименованию товара
        var groupedOrders = customerOrders
            .GroupBy(o => o.EquipmentName)
            .Select(g => new
            {
                EquipmentName = g.Key,
                TotalQuantity = g.Sum(o => o.Quantity),
                Orders = g.ToList()
            })
            .ToList();

        var supplierOrders = new List<SupplierOrder>();

        foreach (var group in groupedOrders)
        {
            // Находим поставщика, который поставлял такое оборудование
            var supplier = await _context.Equipments
                .Where(e => e.Name == group.EquipmentName)
                .Select(e => e.Supplier)
                .FirstOrDefaultAsync();

            if (supplier == null)
            {
                // Если не нашли конкретного поставщика, берем первого доступного
                supplier = await _context.Suppliers.FirstOrDefaultAsync();
            }

            if (supplier != null)
            {
                var orderDetails = $"{group.EquipmentName} - {group.TotalQuantity} шт. " +
                    $"(Заказов: {group.Orders.Count})";

                var supplierOrder = new SupplierOrder
                {
                    SupplierId = supplier.Id,
                    OrderDate = DateTime.UtcNow,
                    WeekStartDate = weekStart,
                    WeekEndDate = weekEnd,
                    OrderDetails = orderDetails,
                    IsCompleted = false
                };

                supplierOrders.Add(supplierOrder);
            }
        }

        if (supplierOrders.Any())
        {
            _context.SupplierOrders.AddRange(supplierOrders);

            // Отмечаем заказы клиентов как обработанные
            foreach (var order in customerOrders)
            {
                order.IsProcessed = true;
            }

            await _context.SaveChangesAsync();
        }

        return supplierOrders;
    }

    public async Task<bool> TransferEquipmentToStorePoint(int equipmentId, int storePointId)
    {
        var equipment = await _context.Equipments.FindAsync(equipmentId);
        if (equipment == null || equipment.IsSold)
            return false;

        var storePoint = await _context.StorePoints.FindAsync(storePointId);
        if (storePoint == null)
            return false;

        equipment.IsOnCentralWarehouse = false;
        equipment.StorePointId = storePointId;

        await _context.SaveChangesAsync();
        return true;
    }
}