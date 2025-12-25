// Models/DTOs/StorePointEquipmentDto.cs
namespace ComputerStore.Models.DTOs;

public class StorePointEquipmentDto
{
    public int EquipmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal SupplierMarkup { get; set; }
    public DateTime ReceiptDate { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int WarrantyMonths { get; set; }
    public string SupplierName { get; set; } = string.Empty;
}

public class WarehouseStateDto
{
    public int EquipmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public DateTime ReceiptDate { get; set; }
    public string SupplierName { get; set; } = string.Empty;
}

public class TotalWarehouseDto
{
    public int EquipmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public string SupplierName { get; set; } = string.Empty;
}

public class SellerSalesDto
{
    public int SaleId { get; set; }
    public DateTime SaleDate { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string StorePointName { get; set; } = string.Empty;
    public List<string> Items { get; set; } = new();
}

public class SellerReportDto
{
    public string SellerName { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
    public int SalesCount { get; set; }
}

public class PopularProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public int SalesCount { get; set; }
}

public class RevenueReportDto
{
    public decimal CashRevenue { get; set; }
    public decimal CashlessRevenue { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalProfit { get; set; }
}

public class UnsoldProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public int DaysInStock { get; set; }
}

public class SellerOrderDto
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Notes { get; set; } = string.Empty;
    public bool IsProcessed { get; set; }
}

public class WeeklySupplierOrderDto
{
    public string SupplierName { get; set; } = string.Empty;
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public List<string> OrderedItems { get; set; } = new();
}

public class SellerScheduleDto
{
    public DateTime Date { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string StorePointName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}

public class StorePointTurnoverDto
{
    public string StorePointName { get; set; } = string.Empty;
    public decimal MonthlyRevenue { get; set; }
    public int SalesCount { get; set; }
}

public class CashLimitViolationDto
{
    public string StorePointName { get; set; } = string.Empty;
    public string CashRegisterNumber { get; set; } = string.Empty;
    public DateTime ViolationDate { get; set; }
    public decimal LimitAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal ExcessAmount { get; set; }
}

public class MonthlyReportDto
{
    public string StorePointName { get; set; } = string.Empty;
    public List<ShippedProductDto> ShippedProducts { get; set; } = new();
    public List<SoldProductDto> SoldProducts { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public decimal TotalProfit { get; set; }
}

public class ShippedProductDto
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime ShipmentDate { get; set; }
}

public class SoldProductDto
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Revenue { get; set; }
    public decimal Profit { get; set; }
}