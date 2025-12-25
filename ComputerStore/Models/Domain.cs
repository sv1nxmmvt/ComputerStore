
namespace ComputerStore.Models;

public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;

    public ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
    public ICollection<SupplierOrder> SupplierOrders { get; set; } = new List<SupplierOrder>();
}

public class CashRegister
{
    public int Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public decimal CashLimit { get; set; }
    public int StorePointId { get; set; }

    public StorePoint StorePoint { get; set; } = null!;
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}

public class StorePoint
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool CanProcessCashless { get; set; }

    public ICollection<CashRegister> CashRegisters { get; set; } = new List<CashRegister>();
    public ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<SellerWorkSchedule> WorkSchedules { get; set; } = new List<SellerWorkSchedule>();
}

public class Seller
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public string FullName => $"{LastName} {FirstName} {MiddleName}";

    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<SellerWorkSchedule> WorkSchedules { get; set; } = new List<SellerWorkSchedule>();
    public ICollection<CustomerOrder> CustomerOrders { get; set; } = new List<CustomerOrder>();
}

public class SellerWorkSchedule
{
    public int Id { get; set; }
    public int SellerId { get; set; }
    public int StorePointId { get; set; }
    public DateTime WorkDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public Seller Seller { get; set; } = null!;
    public StorePoint StorePoint { get; set; } = null!;
}

public class Equipment
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal SupplierMarkup { get; set; }
    public DateTime ReceiptDate { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int WarrantyMonths { get; set; }
    public int SupplierId { get; set; }
    public int? StorePointId { get; set; }
    public bool IsOnCentralWarehouse { get; set; }
    public bool IsSold { get; set; }
    public DateTime? SoldDate { get; set; }

    public Supplier Supplier { get; set; } = null!;
    public StorePoint? StorePoint { get; set; }
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}

public enum PaymentType
{
    Cash,
    Cashless
}

public class Sale
{
    public int Id { get; set; }
    public DateTime SaleDate { get; set; }
    public int SellerId { get; set; }
    public int StorePointId { get; set; }
    public PaymentType PaymentType { get; set; }
    public string? CheckNumber { get; set; }
    public string? PaymentOrderNumber { get; set; }
    public int? CashRegisterId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalWithVAT { get; set; }
    public decimal TotalWithSalesTax { get; set; }

    public Seller Seller { get; set; } = null!;
    public StorePoint StorePoint { get; set; } = null!;
    public CashRegister? CashRegister { get; set; }
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}

public class SaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public int EquipmentId { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SupplierMarkup { get; set; }
    public decimal SellerMarkup { get; set; }
    public decimal PriceBeforeTaxes { get; set; }
    public decimal VAT { get; set; }
    public decimal SalesTax { get; set; }
    public decimal FinalPrice { get; set; }

    public Sale Sale { get; set; } = null!;
    public Equipment Equipment { get; set; } = null!;
}

public class CustomerOrder
{
    public int Id { get; set; }
    public int SellerId { get; set; }
    public DateTime OrderDate { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Notes { get; set; } = string.Empty;
    public bool IsProcessed { get; set; }

    public Seller Seller { get; set; } = null!;
}

public class SupplierOrder
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public string OrderDetails { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }

    public Supplier Supplier { get; set; } = null!;
}

public class CashLimitViolation
{
    public int Id { get; set; }
    public int CashRegisterId { get; set; }
    public DateTime ViolationDate { get; set; }
    public decimal LimitAmount { get; set; }
    public decimal ActualAmount { get; set; }

    public CashRegister CashRegister { get; set; } = null!;
}