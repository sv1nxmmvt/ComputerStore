using ComputerStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.ServerSentEvents;

namespace ComputerStore.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<CashRegister> CashRegisters => Set<CashRegister>();
    public DbSet<StorePoint> StorePoints => Set<StorePoint>();
    public DbSet<Seller> Sellers => Set<Seller>();
    public DbSet<SellerWorkSchedule> SellerWorkSchedules => Set<SellerWorkSchedule>();
    public DbSet<Equipment> Equipments => Set<Equipment>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<CustomerOrder> CustomerOrders => Set<CustomerOrder>();
    public DbSet<SupplierOrder> SupplierOrders => Set<SupplierOrder>();
    public DbSet<CashLimitViolation> CashLimitViolations => Set<CashLimitViolation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Supplier
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.ContactPerson).HasMaxLength(200);
        });

        // CashRegister
        modelBuilder.Entity<CashRegister>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RegistrationNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CashLimit).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.StorePoint)
                .WithMany(s => s.CashRegisters)
                .HasForeignKey(e => e.StorePointId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // StorePoint
        modelBuilder.Entity<StorePoint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).HasMaxLength(300);
        });

        // Seller
        modelBuilder.Entity<Seller>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MiddleName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(50);
        });

        // SellerWorkSchedule
        modelBuilder.Entity<SellerWorkSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Seller)
                .WithMany(s => s.WorkSchedules)
                .HasForeignKey(e => e.SellerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.StorePoint)
                .WithMany(s => s.WorkSchedules)
                .HasForeignKey(e => e.StorePointId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Equipment
        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(300);
            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.SupplierMarkup).HasColumnType("decimal(5,2)");
            entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.Equipments)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.StorePoint)
                .WithMany(s => s.Equipments)
                .HasForeignKey(e => e.StorePointId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Sale
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PaymentType).HasConversion<string>();
            entity.Property(e => e.CheckNumber).HasMaxLength(100);
            entity.Property(e => e.PaymentOrderNumber).HasMaxLength(100);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalWithVAT).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalWithSalesTax).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Seller)
                .WithMany(s => s.Sales)
                .HasForeignKey(e => e.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.StorePoint)
                .WithMany(s => s.Sales)
                .HasForeignKey(e => e.StorePointId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CashRegister)
                .WithMany(c => c.Sales)
                .HasForeignKey(e => e.CashRegisterId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // SaleItem
        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.SupplierMarkup).HasColumnType("decimal(5,2)");
            entity.Property(e => e.SellerMarkup).HasColumnType("decimal(5,2)");
            entity.Property(e => e.PriceBeforeTaxes).HasColumnType("decimal(18,2)");
            entity.Property(e => e.VAT).HasColumnType("decimal(18,2)");
            entity.Property(e => e.SalesTax).HasColumnType("decimal(18,2)");
            entity.Property(e => e.FinalPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Sale)
                .WithMany(s => s.SaleItems)
                .HasForeignKey(e => e.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Equipment)
                .WithMany(e => e.SaleItems)
                .HasForeignKey(e => e.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CustomerOrder
        modelBuilder.Entity<CustomerOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EquipmentName).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(e => e.Seller)
                .WithMany(s => s.CustomerOrders)
                .HasForeignKey(e => e.SellerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SupplierOrder
        modelBuilder.Entity<SupplierOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderDetails).HasMaxLength(2000);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.SupplierOrders)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CashLimitViolation
        modelBuilder.Entity<CashLimitViolation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LimitAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ActualAmount).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.CashRegister)
                .WithMany()
                .HasForeignKey(e => e.CashRegisterId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}