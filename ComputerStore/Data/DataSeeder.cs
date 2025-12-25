using ComputerStore.Models;

namespace ComputerStore.Data;

public static class DataSeeder
{
    public static async Task SeedData(ApplicationDbContext context)
    {
        if (context.Suppliers.Any())
            return; // База уже заполнена

        // Поставщики
        var suppliers = new List<Supplier>
        {
            new Supplier
            {
                Name = "ООО 'Комп-Сервис'",
                Address = "г. Москва, ул. Ленина, д. 10",
                Phone = "+7 (495) 123-45-67",
                ContactPerson = "Иванов И.И."
            },
            new Supplier
            {
                Name = "ЗАО 'ТехноМир'",
                Address = "г. Санкт-Петербург, пр. Невский, д. 25",
                Phone = "+7 (812) 987-65-43",
                ContactPerson = "Петров П.П."
            },
            new Supplier
            {
                Name = "ИП Сидоров",
                Address = "г. Казань, ул. Баумана, д. 5",
                Phone = "+7 (843) 555-12-34",
                ContactPerson = "Сидоров С.С."
            }
        };
        context.Suppliers.AddRange(suppliers);
        await context.SaveChangesAsync();

        // Торговые точки
        var storePoints = new List<StorePoint>
        {
            new StorePoint
            {
                Name = "Магазин 'Центральный'",
                Address = "г. Москва, ул. Тверская, д. 15",
                CanProcessCashless = true
            },
            new StorePoint
            {
                Name = "Магазин 'Северный'",
                Address = "г. Москва, ул. Полярная, д. 8",
                CanProcessCashless = true
            },
            new StorePoint
            {
                Name = "Магазин 'Южный'",
                Address = "г. Москва, ул. Южная, д. 22",
                CanProcessCashless = false
            }
        };
        context.StorePoints.AddRange(storePoints);
        await context.SaveChangesAsync();

        // Кассовые аппараты
        var cashRegisters = new List<CashRegister>
        {
            new CashRegister
            {
                RegistrationNumber = "КСА-001-2024",
                CashLimit = 500000m,
                StorePointId = storePoints[0].Id
            },
            new CashRegister
            {
                RegistrationNumber = "КСА-002-2024",
                CashLimit = 300000m,
                StorePointId = storePoints[1].Id
            },
            new CashRegister
            {
                RegistrationNumber = "КСА-003-2024",
                CashLimit = 200000m,
                StorePointId = storePoints[2].Id
            }
        };
        context.CashRegisters.AddRange(cashRegisters);
        await context.SaveChangesAsync();

        // Продавцы
        var sellers = new List<Seller>
        {
            new Seller
            {
                FirstName = "Алексей",
                LastName = "Иванов",
                MiddleName = "Петрович",
                Phone = "+7 (916) 123-45-67"
            },
            new Seller
            {
                FirstName = "Мария",
                LastName = "Смирнова",
                MiddleName = "Ивановна",
                Phone = "+7 (916) 234-56-78"
            },
            new Seller
            {
                FirstName = "Дмитрий",
                LastName = "Кузнецов",
                MiddleName = "Александрович",
                Phone = "+7 (916) 345-67-89"
            },
            new Seller
            {
                FirstName = "Елена",
                LastName = "Попова",
                MiddleName = "Сергеевна",
                Phone = "+7 (916) 456-78-90"
            }
        };
        context.Sellers.AddRange(sellers);
        await context.SaveChangesAsync();

        // График работы продавцов
        var schedules = new List<SellerWorkSchedule>();
        var startDate = new DateTime(2024, 12, 1);
        for (int day = 0; day < 30; day++)
        {
            var date = startDate.AddDays(day);
            schedules.Add(new SellerWorkSchedule
            {
                SellerId = sellers[0].Id,
                StorePointId = storePoints[day % 2].Id,
                WorkDate = date,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(18, 0, 0)
            });
            schedules.Add(new SellerWorkSchedule
            {
                SellerId = sellers[1].Id,
                StorePointId = storePoints[1].Id,
                WorkDate = date,
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(19, 0, 0)
            });
        }
        context.SellerWorkSchedules.AddRange(schedules);
        await context.SaveChangesAsync();

        // Оборудование
        var equipments = new List<Equipment>
        {
            // На центральном складе
            new Equipment
            {
                Name = "Ноутбук Lenovo ThinkPad X1",
                PurchasePrice = 80000m,
                SupplierMarkup = 0.15m,
                ReceiptDate = DateTime.Now.AddMonths(-2),
                InvoiceNumber = "ТТН-001",
                WarrantyMonths = 12,
                SupplierId = suppliers[0].Id,
                IsOnCentralWarehouse = true
            },
            new Equipment
            {
                Name = "Монитор Dell UltraSharp 27\"",
                PurchasePrice = 25000m,
                SupplierMarkup = 0.12m,
                ReceiptDate = DateTime.Now.AddMonths(-1),
                InvoiceNumber = "ТТН-002",
                WarrantyMonths = 24,
                SupplierId = suppliers[1].Id,
                IsOnCentralWarehouse = true
            },
            // В торговых залах
            new Equipment
            {
                Name = "Клавиатура Logitech MX Keys",
                PurchasePrice = 8000m,
                SupplierMarkup = 0.10m,
                ReceiptDate = DateTime.Now.AddDays(-15),
                InvoiceNumber = "ТТН-003",
                WarrantyMonths = 12,
                SupplierId = suppliers[2].Id,
                StorePointId = storePoints[0].Id,
                IsOnCentralWarehouse = false
            },
            new Equipment
            {
                Name = "Мышь Logitech MX Master 3",
                PurchasePrice = 6000m,
                SupplierMarkup = 0.10m,
                ReceiptDate = DateTime.Now.AddDays(-10),
                InvoiceNumber = "ТТН-004",
                WarrantyMonths = 12,
                SupplierId = suppliers[2].Id,
                StorePointId = storePoints[0].Id,
                IsOnCentralWarehouse = false
            },
            new Equipment
            {
                Name = "SSD Samsung 970 EVO 1TB",
                PurchasePrice = 12000m,
                SupplierMarkup = 0.15m,
                ReceiptDate = DateTime.Now.AddDays(-20),
                InvoiceNumber = "ТТН-005",
                WarrantyMonths = 60,
                SupplierId = suppliers[0].Id,
                StorePointId = storePoints[1].Id,
                IsOnCentralWarehouse = false
            },
            new Equipment
            {
                Name = "Процессор Intel Core i7-13700K",
                PurchasePrice = 35000m,
                SupplierMarkup = 0.12m,
                ReceiptDate = DateTime.Now.AddDays(-25),
                InvoiceNumber = "ТТН-006",
                WarrantyMonths = 36,
                SupplierId = suppliers[1].Id,
                StorePointId = storePoints[2].Id,
                IsOnCentralWarehouse = false
            }
        };
        context.Equipments.AddRange(equipments);
        await context.SaveChangesAsync();

        // Заказы клиентов
        var customerOrders = new List<CustomerOrder>
        {
            new CustomerOrder
            {
                SellerId = sellers[0].Id,
                OrderDate = DateTime.Now.AddDays(-3),
                EquipmentName = "Видеокарта NVIDIA RTX 4080",
                Quantity = 2,
                Notes = "Срочно нужно для сборки ПК",
                IsProcessed = false
            },
            new CustomerOrder
            {
                SellerId = sellers[1].Id,
                OrderDate = DateTime.Now.AddDays(-2),
                EquipmentName = "Оперативная память DDR5 32GB",
                Quantity = 4,
                Notes = "Для апгрейда серверов",
                IsProcessed = false
            }
        };
        context.CustomerOrders.AddRange(customerOrders);
        await context.SaveChangesAsync();
    }
}