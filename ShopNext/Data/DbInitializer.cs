using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopNext.Helpers;
using ShopNext.Models;

namespace ShopNext.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ShopNextDbContext context)
        {
            try
            {
                if (!await context.Database.CanConnectAsync())
                {
                    System.Diagnostics.Debug.WriteLine("DbInitializer: Database connection could not be established. Skipping seed.");
                    return;
                }

                // 1. Ensure basic schema is established
                await context.Database.EnsureCreatedAsync();

                // 2. Safe Column Self-Healer: Add any newly introduced columns to existing SQL Server tables in a single batch
                await ApplySafeSchemaMigrationsAsync(context);

                // 3. Seed Core Default Master Records
                await SeedCoreDataAsync(context);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DbInitializer error: {ex.Message}");
            }
        }

        private static async Task ApplySafeSchemaMigrationsAsync(ShopNextDbContext context)
        {
            try
            {
                string batchScript = @"
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'DeviceFingerprintHash') ALTER TABLE Users ADD DeviceFingerprintHash NVARCHAR(255) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'IsAccountSuspended') ALTER TABLE Users ADD IsAccountSuspended BIT NOT NULL DEFAULT 0;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'IsReturnDisabled') ALTER TABLE Users ADD IsReturnDisabled BIT NOT NULL DEFAULT 0;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'RegistrationIpMasked') ALTER TABLE Users ADD RegistrationIpMasked NVARCHAR(100) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'RestrictFirstOrderCoupons') ALTER TABLE Users ADD RestrictFirstOrderCoupons BIT NOT NULL DEFAULT 0;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'RestrictionAppliedDate') ALTER TABLE Users ADD RestrictionAppliedDate DATETIME2 NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'RestrictionLevel') ALTER TABLE Users ADD RestrictionLevel NVARCHAR(50) NOT NULL DEFAULT 'Normal';
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'RestrictionReason') ALTER TABLE Users ADD RestrictionReason NVARCHAR(MAX) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'SuspendedUntilDate') ALTER TABLE Users ADD SuspendedUntilDate DATETIME2 NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'RiskScore') ALTER TABLE Users ADD RiskScore INT NOT NULL DEFAULT 15;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'RiskLevel') ALTER TABLE Users ADD RiskLevel NVARCHAR(50) NOT NULL DEFAULT 'Low';
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'RiskFactorsJson') ALTER TABLE Users ADD RiskFactorsJson NVARCHAR(MAX) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'IsCodDisabled') ALTER TABLE Users ADD IsCodDisabled BIT NOT NULL DEFAULT 0;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'IsFlaggedForReview') ALTER TABLE Users ADD IsFlaggedForReview BIT NOT NULL DEFAULT 0;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'RiskLastEvaluatedDate') ALTER TABLE Users ADD RiskLastEvaluatedDate DATETIME2 NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'ProfilePhoto') ALTER TABLE Users ADD ProfilePhoto NVARCHAR(MAX) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Gender') ALTER TABLE Users ADD Gender NVARCHAR(50) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'DateOfBirth') ALTER TABLE Users ADD DateOfBirth DATETIME2 NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'CreatedAt') ALTER TABLE Users ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE();
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'CreatedDate') ALTER TABLE Users ADD CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE();
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'UpdatedDate') ALTER TABLE Users ADD UpdatedDate DATETIME2 NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'IsActive') ALTER TABLE Users ADD IsActive BIT NOT NULL DEFAULT 1;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'IsDeleted') ALTER TABLE Users ADD IsDeleted BIT NOT NULL DEFAULT 0;

                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'DeliveredByRiderName') ALTER TABLE Orders ADD DeliveredByRiderName NVARCHAR(100) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'DeliveryOtp') ALTER TABLE Orders ADD DeliveryOtp NVARCHAR(10) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'IsOtpVerified') ALTER TABLE Orders ADD IsOtpVerified BIT NOT NULL DEFAULT 0;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'OtpVerifiedDate') ALTER TABLE Orders ADD OtpVerifiedDate DATETIME2 NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'ReturnStatus') ALTER TABLE Orders ADD ReturnStatus NVARCHAR(50) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'ReturnVerificationNotes') ALTER TABLE Orders ADD ReturnVerificationNotes NVARCHAR(MAX) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'ReturnRequestedDate') ALTER TABLE Orders ADD ReturnRequestedDate DATETIME2 NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'ReturnVerifiedDate') ALTER TABLE Orders ADD ReturnVerifiedDate DATETIME2 NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'ShopName') ALTER TABLE Orders ADD ShopName NVARCHAR(200) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'PaymentMode') ALTER TABLE Orders ADD PaymentMode NVARCHAR(50) NOT NULL DEFAULT 'COD';
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'PaymentStatus') ALTER TABLE Orders ADD PaymentStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending';
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'CancelReason') ALTER TABLE Orders ADD CancelReason NVARCHAR(500) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'ReturnReason') ALTER TABLE Orders ADD ReturnReason NVARCHAR(500) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'DeliveredDate') ALTER TABLE Orders ADD DeliveredDate DATETIME2 NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'DeliveryAddress') ALTER TABLE Orders ADD DeliveryAddress NVARCHAR(MAX) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'CreatedDate') ALTER TABLE Orders ADD CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE();
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'UpdatedDate') ALTER TABLE Orders ADD UpdatedDate DATETIME2 NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'IsActive') ALTER TABLE Orders ADD IsActive BIT NOT NULL DEFAULT 1;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'IsDeleted') ALTER TABLE Orders ADD IsDeleted BIT NOT NULL DEFAULT 0;

                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'ImageFront') ALTER TABLE Products ADD ImageFront NVARCHAR(MAX) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'ImageBack') ALTER TABLE Products ADD ImageBack NVARCHAR(MAX) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'ImageSide') ALTER TABLE Products ADD ImageSide NVARCHAR(MAX) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'ImagePackaging') ALTER TABLE Products ADD ImagePackaging NVARCHAR(MAX) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'HasVariants') ALTER TABLE Products ADD HasVariants BIT NOT NULL DEFAULT 0;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'IsApproved') ALTER TABLE Products ADD IsApproved BIT NOT NULL DEFAULT 1;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'ApprovalStatus') ALTER TABLE Products ADD ApprovalStatus NVARCHAR(50) NOT NULL DEFAULT 'Approved';
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'RejectionReason') ALTER TABLE Products ADD RejectionReason NVARCHAR(MAX) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'ApprovedDate') ALTER TABLE Products ADD ApprovedDate DATETIME2 NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'Mrp') ALTER TABLE Products ADD Mrp DECIMAL(18,2) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'Discount') ALTER TABLE Products ADD Discount DECIMAL(18,2) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'Sku') ALTER TABLE Products ADD Sku NVARCHAR(100) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'StockStatus') ALTER TABLE Products ADD StockStatus NVARCHAR(50) NOT NULL DEFAULT 'InStock';
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'CreatedDate') ALTER TABLE Products ADD CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE();
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'UpdatedDate') ALTER TABLE Products ADD UpdatedDate DATETIME2 NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'IsActive') ALTER TABLE Products ADD IsActive BIT NOT NULL DEFAULT 1;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'IsDeleted') ALTER TABLE Products ADD IsDeleted BIT NOT NULL DEFAULT 0;

                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Shops') AND name = 'OpeningTime') ALTER TABLE Shops ADD OpeningTime NVARCHAR(50) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Shops') AND name = 'ClosingTime') ALTER TABLE Shops ADD ClosingTime NVARCHAR(50) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Shops') AND name = 'IsApproved') ALTER TABLE Shops ADD IsApproved BIT NOT NULL DEFAULT 1;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Shops') AND name = 'Rating') ALTER TABLE Shops ADD Rating FLOAT NOT NULL DEFAULT 4.8;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Shops') AND name = 'ImageUrl') ALTER TABLE Shops ADD ImageUrl NVARCHAR(MAX) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Shops') AND name = 'BannerUrl') ALTER TABLE Shops ADD BannerUrl NVARCHAR(MAX) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Shops') AND name = 'GstNumber') ALTER TABLE Shops ADD GstNumber NVARCHAR(50) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Shops') AND name = 'PanNumber') ALTER TABLE Shops ADD PanNumber NVARCHAR(50) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Shops') AND name = 'Latitude') ALTER TABLE Shops ADD Latitude DECIMAL(18,8) NOT NULL DEFAULT 25.5941;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Shops') AND name = 'Longitude') ALTER TABLE Shops ADD Longitude DECIMAL(18,8) NOT NULL DEFAULT 85.1376;

                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Riders') AND name = 'VehicleType') ALTER TABLE Riders ADD VehicleType NVARCHAR(50) NOT NULL DEFAULT 'Bike';
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Riders') AND name = 'VehicleNumber') ALTER TABLE Riders ADD VehicleNumber NVARCHAR(50) NOT NULL DEFAULT 'BR-01-AB-1234';
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Riders') AND name = 'IsOnline') ALTER TABLE Riders ADD IsOnline BIT NOT NULL DEFAULT 1;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Riders') AND name = 'CurrentLatitude') ALTER TABLE Riders ADD CurrentLatitude DECIMAL(18,8) NOT NULL DEFAULT 25.5941;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Riders') AND name = 'CurrentLongitude') ALTER TABLE Riders ADD CurrentLongitude DECIMAL(18,8) NOT NULL DEFAULT 85.1376;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Riders') AND name = 'TotalDeliveries') ALTER TABLE Riders ADD TotalDeliveries INT NOT NULL DEFAULT 50;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Riders') AND name = 'Rating') ALTER TABLE Riders ADD Rating FLOAT NOT NULL DEFAULT 4.9;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Riders') AND name = 'Password') ALTER TABLE Riders ADD Password NVARCHAR(255) NULL;

                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Reviews') AND name = 'IsHidden') ALTER TABLE Reviews ADD IsHidden BIT NOT NULL DEFAULT 0;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Reviews') AND name = 'ModerationReason') ALTER TABLE Reviews ADD ModerationReason NVARCHAR(500) NULL;

                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Complaints') AND name = 'TicketNumber') ALTER TABLE Complaints ADD TicketNumber NVARCHAR(50) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Complaints') AND name = 'Subject') ALTER TABLE Complaints ADD Subject NVARCHAR(200) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Complaints') AND name = 'EscalationLevel') ALTER TABLE Complaints ADD EscalationLevel INT NOT NULL DEFAULT 1;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Complaints') AND name = 'EscalationStage') ALTER TABLE Complaints ADD EscalationStage NVARCHAR(50) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Complaints') AND name = 'IsHighValueOrder') ALTER TABLE Complaints ADD IsHighValueOrder BIT NOT NULL DEFAULT 0;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Complaints') AND name = 'OrderAmount') ALTER TABLE Complaints ADD OrderAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Complaints') AND name = 'EscalationReason') ALTER TABLE Complaints ADD EscalationReason NVARCHAR(MAX) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Complaints') AND name = 'ComplainantRole') ALTER TABLE Complaints ADD ComplainantRole NVARCHAR(50) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Complaints') AND name = 'ReasonCategory') ALTER TABLE Complaints ADD ReasonCategory NVARCHAR(100) NULL;

                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CustomerAddresses') AND name = 'Landmark') ALTER TABLE CustomerAddresses ADD Landmark NVARCHAR(200) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CustomerAddresses') AND name = 'AddressType') ALTER TABLE CustomerAddresses ADD AddressType NVARCHAR(50) NOT NULL DEFAULT 'Home';
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CustomerAddresses') AND name = 'IsDefault') ALTER TABLE CustomerAddresses ADD IsDefault BIT NOT NULL DEFAULT 0;

                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ProductVariants') AND name = 'Size') ALTER TABLE ProductVariants ADD Size NVARCHAR(50) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ProductVariants') AND name = 'Color') ALTER TABLE ProductVariants ADD Color NVARCHAR(50) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ProductVariants') AND name = 'Sku') ALTER TABLE ProductVariants ADD Sku NVARCHAR(100) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ProductVariants') AND name = 'ImageUrl') ALTER TABLE ProductVariants ADD ImageUrl NVARCHAR(MAX) NULL;

                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AuditLogs') AND name = 'EntityName') ALTER TABLE AuditLogs ADD EntityName NVARCHAR(100) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AuditLogs') AND name = 'UserRole') ALTER TABLE AuditLogs ADD UserRole NVARCHAR(50) NULL;
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AuditLogs') AND name = 'UserName') ALTER TABLE AuditLogs ADD UserName NVARCHAR(100) NULL;
                ";
                await context.Database.ExecuteSqlRawAsync(batchScript);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplySafeSchemaMigrationsAsync warning: {ex.Message}");
            }
        }

        private static async Task SeedCoreDataAsync(ShopNextDbContext context)
        {
            // 1. Seed Admin User
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Role == "Admin");
            if (adminUser == null)
            {
                context.Users.Add(new User
                {
                    Name = "Platform Administrator",
                    PhoneNumber = "9999999999",
                    Email = "admin@shopnext.com",
                    Password = SecurityHelper.HashPassword("Admin@123"),
                    Role = "Admin",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now
                });
                await context.SaveChangesAsync();
            }

            // 2. Seed Customer User
            var defaultCustomer = await context.Users.FirstOrDefaultAsync(u => u.Role == "Customer");
            if (defaultCustomer == null)
            {
                context.Users.Add(new User
                {
                    Name = "Pooja Sharma",
                    PhoneNumber = "9876543210",
                    Email = "customer@shopnext.com",
                    Password = SecurityHelper.HashPassword("Customer@123"),
                    Role = "Customer",
                    Gender = "Female",
                    DateOfBirth = new DateTime(1995, 6, 15),
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now
                });
                await context.SaveChangesAsync();
            }

            // 3. Seed Verified Shops
            if (!await context.Shops.AnyAsync())
            {
                var shops = new List<Shop>
                {
                    new Shop
                    {
                        ShopName = "TechMart Electronics",
                        OwnerName = "Rajesh Gupta",
                        Category = "Electronics",
                        Address = "Fraser Road, Near Dak Bungalow",
                        City = "Patna",
                        State = "Bihar",
                        Pincode = "800001",
                        PhoneNumber = "9822334455",
                        Email = "techmart@shopnext.com",
                        Password = SecurityHelper.HashPassword("Seller@123"),
                        IsApproved = true,
                        Latitude = 25.6120m,
                        Longitude = 85.1380m,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now
                    },
                    new Shop
                    {
                        ShopName = "GreenGrocer Fresh",
                        OwnerName = "Sunil Verma",
                        Category = "Groceries",
                        Address = "Boring Road Crossing",
                        City = "Patna",
                        State = "Bihar",
                        Pincode = "800001",
                        PhoneNumber = "9833445566",
                        Email = "greengrocer@shopnext.com",
                        Password = SecurityHelper.HashPassword("Seller@123"),
                        IsApproved = true,
                        Latitude = 25.6150m,
                        Longitude = 85.1250m,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now
                    },
                    new Shop
                    {
                        ShopName = "Baker's Delight & Cafe",
                        OwnerName = "Ananya Singh",
                        Category = "Bakery",
                        Address = "Kankarbagh Main Road",
                        City = "Patna",
                        State = "Bihar",
                        Pincode = "800020",
                        PhoneNumber = "9844556677",
                        Email = "bakers@shopnext.com",
                        Password = SecurityHelper.HashPassword("Seller@123"),
                        IsApproved = true,
                        Latitude = 25.5900m,
                        Longitude = 85.1500m,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now
                    }
                };

                context.Shops.AddRange(shops);
                await context.SaveChangesAsync();
            }

            // 4. Seed Products
            if (!await context.Products.AnyAsync())
            {
                var techShop = await context.Shops.FirstOrDefaultAsync(s => s.Category == "Electronics");
                var groceryShop = await context.Shops.FirstOrDefaultAsync(s => s.Category == "Groceries");
                var bakeryShop = await context.Shops.FirstOrDefaultAsync(s => s.Category == "Bakery");

                var products = new List<Product>
                {
                    new Product
                    {
                        ShopId = techShop?.Id ?? 1,
                        ProductName = "Apple iPhone 15 Pro 128GB (Natural Titanium)",
                        Category = "Electronics",
                        SubCategory = "Smartphones",
                        Brand = "Apple",
                        Description = "A17 Pro chip, Titanium design, 48MP main camera with 3x Telephoto and USB-C.",
                        Price = 129900m,
                        Mrp = 134900m,
                        Discount = 4m,
                        Stock = 15,
                        Sku = "PROD-IP15P-128",
                        StockStatus = "InStock",
                        ImageUrl = "https://images.unsplash.com/photo-1695048133142-1a20484d2569?w=500",
                        IsApproved = true,
                        ApprovalStatus = "Approved",
                        ApprovedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now
                    },
                    new Product
                    {
                        ShopId = techShop?.Id ?? 1,
                        ProductName = "Sony WH-1000XM5 Wireless Noise Cancelling Headphones",
                        Category = "Electronics",
                        SubCategory = "Audio",
                        Brand = "Sony",
                        Description = "Industry leading noise cancellation with two processors and 8 microphones.",
                        Price = 28990m,
                        Mrp = 34990m,
                        Discount = 17m,
                        Stock = 25,
                        Sku = "PROD-SONY-XM5",
                        StockStatus = "InStock",
                        ImageUrl = "https://images.unsplash.com/photo-1546435770-a3e426bf472b?w=500",
                        IsApproved = true,
                        ApprovalStatus = "Approved",
                        ApprovedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now
                    },
                    new Product
                    {
                        ShopId = groceryShop?.Id ?? 2,
                        ProductName = "Royal Kashmiri Fresh Apples (1 kg Pack)",
                        Category = "Groceries",
                        SubCategory = "Fruits & Veggies",
                        Brand = "Organic Harvest",
                        Description = "Farm fresh sweet, crunchy, premium hand-picked Kashmiri apples.",
                        Price = 180m,
                        Mrp = 240m,
                        Discount = 25m,
                        Stock = 120,
                        Sku = "PROD-APL-1KG",
                        StockStatus = "InStock",
                        ImageUrl = "https://images.unsplash.com/photo-1560806887-1e4cd0b6cbd6?w=500",
                        IsApproved = true,
                        ApprovalStatus = "Approved",
                        ApprovedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now
                    },
                    new Product
                    {
                        ShopId = bakeryShop?.Id ?? 3,
                        ProductName = "Signature Belgian Dark Chocolate Truffle Cake (500g)",
                        Category = "Bakery",
                        SubCategory = "Cakes & Pastries",
                        Brand = "Baker's Choice",
                        Description = "Rich 70% dark Belgian cocoa ganache sponge with chocolate flakes.",
                        Price = 499m,
                        Mrp = 650m,
                        Discount = 23m,
                        Stock = 30,
                        Sku = "PROD-CAKE-TRF",
                        StockStatus = "InStock",
                        ImageUrl = "https://images.unsplash.com/photo-1578985545062-69928b1d9587?w=500",
                        IsApproved = true,
                        ApprovalStatus = "Approved",
                        ApprovedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now
                    }
                };

                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }

            // 5. Seed Riders
            if (!await context.Riders.AnyAsync())
            {
                var riders = new List<Rider>
                {
                    new Rider
                    {
                        RiderName = "Rajesh Kumar",
                        PhoneNumber = "9811223344",
                        Password = SecurityHelper.HashPassword("Rider@123"),
                        VehicleNumber = "BR-01-AB-1234",
                        IsAvailable = true,
                        CurrentLatitude = 25.6120m,
                        CurrentLongitude = 85.1380m,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now
                    },
                    new Rider
                    {
                        RiderName = "Amit Singh",
                        PhoneNumber = "9822334411",
                        Password = SecurityHelper.HashPassword("Rider@123"),
                        VehicleNumber = "BR-01-EV-5678",
                        IsAvailable = true,
                        CurrentLatitude = 25.6150m,
                        CurrentLongitude = 85.1250m,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now
                    }
                };

                context.Riders.AddRange(riders);
                await context.SaveChangesAsync();
            }

            // 6. Seed Coupons
            if (!await context.Coupons.AnyAsync())
            {
                var coupons = new List<Coupon>
                {
                    new Coupon
                    {
                        Code = "WELCOME100",
                        Description = "Flat ₹100 Discount on your first order above ₹499",
                        DiscountType = "Flat",
                        DiscountValue = 100m,
                        MinOrderAmount = 499m,
                        StartDate = DateTime.Now.AddDays(-10),
                        ExpiryDate = DateTime.Now.AddMonths(6),
                        UsageLimit = 1000,
                        UsedCount = 42,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now
                    },
                    new Coupon
                    {
                        Code = "SUPER50",
                        Description = "50% Discount up to ₹200 on Groceries & Bakery",
                        DiscountType = "Percentage",
                        DiscountValue = 50m,
                        MinOrderAmount = 299m,
                        MaxDiscountAmount = 200m,
                        StartDate = DateTime.Now.AddDays(-5),
                        ExpiryDate = DateTime.Now.AddMonths(3),
                        UsageLimit = 500,
                        UsedCount = 18,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now
                    }
                };

                context.Coupons.AddRange(coupons);
                await context.SaveChangesAsync();
            }

            // 7. Seed Pincode Serviceabilities
            if (!await context.PincodeServiceabilities.AnyAsync())
            {
                var pincodes = new List<PincodeServiceability>
                {
                    new PincodeServiceability { Pincode = "800001", City = "Patna", State = "Bihar", IsServiceable = true, IsCodAvailable = true, EstimatedDeliveryDays = 1, IsExpressAvailable = true, IsActive = true, CreatedDate = DateTime.Now },
                    new PincodeServiceability { Pincode = "800020", City = "Patna", State = "Bihar", IsServiceable = true, IsCodAvailable = true, EstimatedDeliveryDays = 1, IsExpressAvailable = true, IsActive = true, CreatedDate = DateTime.Now },
                    new PincodeServiceability { Pincode = "110001", City = "New Delhi", State = "Delhi", IsServiceable = true, IsCodAvailable = true, EstimatedDeliveryDays = 2, IsExpressAvailable = true, IsActive = true, CreatedDate = DateTime.Now },
                    new PincodeServiceability { Pincode = "400001", City = "Mumbai", State = "Maharashtra", IsServiceable = true, IsCodAvailable = true, EstimatedDeliveryDays = 2, IsExpressAvailable = true, IsActive = true, CreatedDate = DateTime.Now },
                    new PincodeServiceability { Pincode = "560001", City = "Bangalore", State = "Karnataka", IsServiceable = true, IsCodAvailable = true, EstimatedDeliveryDays = 2, IsExpressAvailable = true, IsActive = true, CreatedDate = DateTime.Now }
                };

                context.PincodeServiceabilities.AddRange(pincodes);
                await context.SaveChangesAsync();
            }

            // 8. Seed System Monitoring Initial Records
            if (!await context.UserSessions.AnyAsync())
            {
                var sessions = new List<UserSession>
                {
                    new UserSession
                    {
                        SessionId = Guid.NewGuid().ToString("N"),
                        UserId = 1,
                        UserName = "Platform Administrator",
                        Role = "Admin",
                        IpAddress = "192.168.1.100",
                        Device = "Desktop",
                        Browser = "Chrome",
                        OperatingSystem = "Windows 11",
                        LoginTime = DateTime.Now.AddMinutes(-45),
                        LastSeenTime = DateTime.Now.AddMinutes(-1),
                        LastPageVisited = "/Admin/SystemMonitoring",
                        LastAction = "Dashboard Audit Inspection",
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    },
                    new UserSession
                    {
                        SessionId = Guid.NewGuid().ToString("N"),
                        UserId = 2,
                        UserName = "Patna Central Grocery",
                        Role = "Seller",
                        IpAddress = "192.168.1.105",
                        Device = "Desktop",
                        Browser = "Edge",
                        OperatingSystem = "Windows 10",
                        LoginTime = DateTime.Now.AddMinutes(-30),
                        LastSeenTime = DateTime.Now.AddMinutes(-3),
                        LastPageVisited = "/Vendor/Orders",
                        LastAction = "Packed Order #1024",
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    },
                    new UserSession
                    {
                        SessionId = Guid.NewGuid().ToString("N"),
                        UserId = 1,
                        UserName = "Rider Ajay",
                        Role = "Rider",
                        IpAddress = "192.168.1.110",
                        Device = "Mobile",
                        Browser = "Chrome Mobile",
                        OperatingSystem = "Android 14",
                        LoginTime = DateTime.Now.AddMinutes(-20),
                        LastSeenTime = DateTime.Now.AddMinutes(-2),
                        LastPageVisited = "/Rider/Dashboard",
                        LastAction = "GPS Route Navigation",
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    }
                };
                context.UserSessions.AddRange(sessions);
                await context.SaveChangesAsync();
            }

            if (!await context.LoginHistories.AnyAsync())
            {
                var logins = new List<LoginHistory>
                {
                    new LoginHistory { UserId = 1, UserName = "Platform Administrator", Role = "Admin", LoginTime = DateTime.Now.AddHours(-3), LastActivityTime = DateTime.Now.AddMinutes(-1), IpAddress = "192.168.1.100", Browser = "Chrome", Device = "Desktop", OperatingSystem = "Windows 11", IsSuccessful = true, IsActiveSession = true, CreatedDate = DateTime.Now },
                    new LoginHistory { UserId = 2, UserName = "Patna Central Grocery", Role = "Seller", LoginTime = DateTime.Now.AddHours(-2), LastActivityTime = DateTime.Now.AddMinutes(-3), IpAddress = "192.168.1.105", Browser = "Edge", Device = "Desktop", OperatingSystem = "Windows 10", IsSuccessful = true, IsActiveSession = true, CreatedDate = DateTime.Now },
                    new LoginHistory { UserId = 1, UserName = "Rider Ajay", Role = "Rider", LoginTime = DateTime.Now.AddHours(-1), LastActivityTime = DateTime.Now.AddMinutes(-2), IpAddress = "192.168.1.110", Browser = "Chrome Mobile", Device = "Mobile", OperatingSystem = "Android 14", IsSuccessful = true, IsActiveSession = true, CreatedDate = DateTime.Now },
                    new LoginHistory { UserId = null, UserName = "unknown_attacker", Role = "Admin", LoginTime = DateTime.Now.AddMinutes(-35), LastActivityTime = DateTime.Now.AddMinutes(-35), IpAddress = "203.0.113.55", Browser = "Firefox", Device = "Desktop", OperatingSystem = "Linux", IsSuccessful = false, FailureReason = "Invalid password", IsActiveSession = false, CreatedDate = DateTime.Now }
                };
                context.LoginHistories.AddRange(logins);
                await context.SaveChangesAsync();
            }

            if (!await context.UserActivities.AnyAsync())
            {
                var activities = new List<UserActivity>
                {
                    new UserActivity { UserId = 1, UserName = "Platform Administrator", Role = "Admin", Action = "APPROVE", Module = "Seller", Entity = "Shop #1", EntityId = 1, Description = "Admin approved store registration for 'Patna Central Grocery'", Timestamp = DateTime.Now.AddHours(-2), IpAddress = "192.168.1.100", Device = "Desktop", Browser = "Chrome", CreatedDate = DateTime.Now },
                    new UserActivity { UserId = 2, UserName = "Patna Central Grocery", Role = "Seller", Action = "CREATE", Module = "Product", Entity = "Product #101", EntityId = 101, Description = "Seller added new catalog item 'Fresh Organic Apples'", Timestamp = DateTime.Now.AddHours(-1), IpAddress = "192.168.1.105", Device = "Desktop", Browser = "Edge", CreatedDate = DateTime.Now },
                    new UserActivity { UserId = 2, UserName = "Patna Central Grocery", Role = "Seller", Action = "STATUS_CHANGE", Module = "Order", Entity = "Order #1024", EntityId = 1024, Description = "Seller packed order #1024 and requested Rider dispatch", Timestamp = DateTime.Now.AddMinutes(-25), IpAddress = "192.168.1.105", Device = "Desktop", Browser = "Edge", CreatedDate = DateTime.Now },
                    new UserActivity { UserId = 1, UserName = "Rider Ajay", Role = "Rider", Action = "ASSIGN", Module = "Order", Entity = "Order #1024", EntityId = 1024, Description = "Rider accepted delivery trip for Order #1024", Timestamp = DateTime.Now.AddMinutes(-15), IpAddress = "192.168.1.110", Device = "Mobile", Browser = "Chrome Mobile", CreatedDate = DateTime.Now }
                };
                context.UserActivities.AddRange(activities);
                await context.SaveChangesAsync();
            }

            if (!await context.EntityChangeLogs.AnyAsync())
            {
                var diffs = new List<EntityChangeLog>
                {
                    new EntityChangeLog { EntityName = "Order", EntityId = 1024, Action = "UPDATE", FieldName = "OrderStatus", OldValue = "Accepted", NewValue = "Packed", ChangedByUserId = 2, ChangedByUserName = "Patna Central Grocery", ChangedByUserRole = "Seller", Timestamp = DateTime.Now.AddMinutes(-25), IpAddress = "192.168.1.105", CreatedDate = DateTime.Now },
                    new EntityChangeLog { EntityName = "Order", EntityId = 1024, Action = "UPDATE", FieldName = "RiderId", OldValue = "(null)", NewValue = "1 (Rider Ajay)", ChangedByUserId = 1, ChangedByUserName = "System Dispatcher", ChangedByUserRole = "System", Timestamp = DateTime.Now.AddMinutes(-15), IpAddress = "127.0.0.1", CreatedDate = DateTime.Now },
                    new EntityChangeLog { EntityName = "Product", EntityId = 101, Action = "UPDATE", FieldName = "Price", OldValue = "₹120.00", NewValue = "₹99.00", ChangedByUserId = 2, ChangedByUserName = "Patna Central Grocery", ChangedByUserRole = "Seller", Timestamp = DateTime.Now.AddMinutes(-50), IpAddress = "192.168.1.105", CreatedDate = DateTime.Now }
                };
                context.EntityChangeLogs.AddRange(diffs);
                await context.SaveChangesAsync();
            }

            if (!await context.SoftDeleteLogs.AnyAsync())
            {
                var deletes = new List<SoftDeleteLog>
                {
                    new SoftDeleteLog { EntityName = "Product", EntityId = 99, EntityTitle = "Expired Seasonal Mango Juice", DeletedByUserId = 2, DeletedByUserName = "Patna Central Grocery", DeletedByUserRole = "Seller", Reason = "Seasonal item out of production", DeletedAt = DateTime.Now.AddDays(-1), IpAddress = "192.168.1.105", IsRestored = false, CreatedDate = DateTime.Now }
                };
                context.SoftDeleteLogs.AddRange(deletes);
                await context.SaveChangesAsync();
            }

            if (!await context.EntityViewLogs.AnyAsync())
            {
                var views = new List<EntityViewLog>
                {
                    new EntityViewLog { EntityName = "Order", EntityId = 1024, ViewedByUserId = 1, ViewedByUserName = "Platform Administrator", ViewedByUserRole = "Admin", ViewedAt = DateTime.Now.AddMinutes(-10), IpAddress = "192.168.1.100", ExtraInfo = "Admin verified order fulfillment", CreatedDate = DateTime.Now },
                    new EntityViewLog { EntityName = "Order", EntityId = 1024, ViewedByUserId = 2, ViewedByUserName = "Patna Central Grocery", ViewedByUserRole = "Seller", ViewedAt = DateTime.Now.AddMinutes(-26), IpAddress = "192.168.1.105", ExtraInfo = "Seller opened order dispatch drawer", CreatedDate = DateTime.Now },
                    new EntityViewLog { EntityName = "Order", EntityId = 1024, ViewedByUserId = 1, ViewedByUserName = "Rider Ajay", ViewedByUserRole = "Rider", ViewedAt = DateTime.Now.AddMinutes(-16), IpAddress = "192.168.1.110", ExtraInfo = "Rider accepted delivery route", CreatedDate = DateTime.Now }
                };
                context.EntityViewLogs.AddRange(views);
                await context.SaveChangesAsync();
            }

            if (!await context.SecurityThreatAlerts.AnyAsync())
            {
                var alerts = new List<SecurityThreatAlert>
                {
                    new SecurityThreatAlert
                    {
                        AlertType = "FailedLogins",
                        Severity = "High",
                        Title = "Repeated Failed Login Attempts on Admin Gateway",
                        Description = "Multiple unauthorized login attempts detected from IP 203.0.113.55 targeting identifier 'unknown_attacker'.",
                        AffectedUserId = null,
                        AffectedUserName = "unknown_attacker",
                        IpAddress = "203.0.113.55",
                        DetectedAt = DateTime.Now.AddMinutes(-35),
                        IsResolved = false,
                        CreatedDate = DateTime.Now
                    }
                };
                context.SecurityThreatAlerts.AddRange(alerts);
                await context.SaveChangesAsync();
            }
        }
    }
}
