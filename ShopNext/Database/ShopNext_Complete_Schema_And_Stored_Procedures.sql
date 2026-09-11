-- ==============================================================================
-- SHOPNEXT PLATFORM - COMPLETE DATABASE SCHEMA, INDEXES & STORED PROCEDURES
-- Database: SQL Server (Express / Developer / Enterprise)
-- Target: Fast Query Execution, Audit Trail, Anti-Fraud & Real-time Analytics
-- ==============================================================================

USE [ShopNext];
GO

-- ==============================================================================
-- 1. TABLES CREATION WITH FULL CONSTRAINTS & COLUMNS
-- ==============================================================================

-- 1.1 USERS TABLE
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        PhoneNumber NVARCHAR(50) NOT NULL,
        Name NVARCHAR(200) NOT NULL DEFAULT '',
        Email NVARCHAR(200) NULL,
        Password NVARCHAR(MAX) NULL,
        Role NVARCHAR(50) NOT NULL DEFAULT 'Customer', -- Admin, Seller, Customer, Rider
        ProfilePhoto NVARCHAR(MAX) NULL,
        Gender NVARCHAR(50) NULL,
        DateOfBirth DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        
        -- Anti-Fraud & Risk Telemetry (Point 36)
        RiskScore INT NOT NULL DEFAULT 15,
        RiskLevel NVARCHAR(50) NOT NULL DEFAULT 'Low', -- Low, Medium, High
        RiskFactorsJson NVARCHAR(MAX) NULL,
        IsCodDisabled BIT NOT NULL DEFAULT 0,
        IsFlaggedForReview BIT NOT NULL DEFAULT 0,
        RiskLastEvaluatedDate DATETIME2 NULL,
        DeviceFingerprintHash NVARCHAR(255) NULL,
        IsAccountSuspended BIT NOT NULL DEFAULT 0,
        IsReturnDisabled BIT NOT NULL DEFAULT 0,
        RegistrationIpMasked NVARCHAR(100) NULL,
        RestrictFirstOrderCoupons BIT NOT NULL DEFAULT 0,
        RestrictionAppliedDate DATETIME2 NULL,
        RestrictionLevel NVARCHAR(50) NOT NULL DEFAULT 'Normal',
        RestrictionReason NVARCHAR(MAX) NULL,
        SuspendedUntilDate DATETIME2 NULL,
        
        -- BaseModel Audit Fields
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

IF OBJECT_ID('dbo.PasswordResetTokens', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PasswordResetTokens (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Role NVARCHAR(30) NOT NULL,
        Identifier NVARCHAR(250) NOT NULL,
        TokenHash NVARCHAR(128) NOT NULL,
        ExpiresAt DATETIME2 NOT NULL,
        UsedAt DATETIME2 NULL,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
    CREATE INDEX IX_PasswordResetTokens_TokenHash ON dbo.PasswordResetTokens(TokenHash);
END;
GO

-- 1.1.1 CUSTOMER ADDRESSES TABLE
IF OBJECT_ID('dbo.CustomerAddresses', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CustomerAddresses (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId INT NOT NULL,
        AddressType NVARCHAR(50) NOT NULL DEFAULT 'Home',
        RecipientName NVARCHAR(200) NOT NULL DEFAULT '',
        PhoneNumber NVARCHAR(50) NOT NULL DEFAULT '',
        AddressLine NVARCHAR(500) NOT NULL DEFAULT '',
        City NVARCHAR(100) NOT NULL DEFAULT 'New Delhi',
        State NVARCHAR(100) NOT NULL DEFAULT 'Delhi',
        Pincode NVARCHAR(20) NOT NULL DEFAULT '',
        Landmark NVARCHAR(200) NULL,
        IsDefault BIT NOT NULL DEFAULT 0,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_CustomerAddresses_Users FOREIGN KEY (CustomerId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );
END;
GO

-- 1.2 SHOPS TABLE
IF OBJECT_ID('dbo.Shops', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Shops (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ShopName NVARCHAR(250) NOT NULL,
        PhoneNumber NVARCHAR(50) NOT NULL,
        Category NVARCHAR(100) NOT NULL DEFAULT 'General',
        Latitude DECIMAL(18,6) NOT NULL DEFAULT 0,
        Longitude DECIMAL(18,6) NOT NULL DEFAULT 0,
        IsApproved BIT NOT NULL DEFAULT 0,
        OwnerName NVARCHAR(200) NULL,
        Email NVARCHAR(200) NULL,
        Address NVARCHAR(500) NULL,
        City NVARCHAR(100) NULL DEFAULT 'Patna',
        State NVARCHAR(100) NULL DEFAULT 'Bihar',
        Pincode NVARCHAR(50) NULL DEFAULT '800001',
        BankAccountNumber NVARCHAR(100) NULL,
        IfscCode NVARCHAR(50) NULL,
        Password NVARCHAR(MAX) NULL,
        
        -- BaseModel Audit Fields
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.3 PRODUCTS TABLE
IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ShopId INT NOT NULL,
        ProductName NVARCHAR(300) NOT NULL,
        Category NVARCHAR(100) NOT NULL DEFAULT 'General',
        SubCategory NVARCHAR(100) NULL,
        Brand NVARCHAR(100) NULL,
        Description NVARCHAR(MAX) NULL,
        Price DECIMAL(18,2) NOT NULL DEFAULT 0,
        Mrp DECIMAL(18,2) NULL,
        Discount DECIMAL(18,2) NULL,
        Stock INT NOT NULL DEFAULT 0,
        Sku NVARCHAR(100) NULL,
        StockStatus NVARCHAR(50) NOT NULL DEFAULT 'InStock',
        ImageUrl NVARCHAR(MAX) NOT NULL DEFAULT '',
        ImageFront NVARCHAR(MAX) NULL,
        ImageBack NVARCHAR(MAX) NULL,
        ImageSide NVARCHAR(MAX) NULL,
        ImagePackaging NVARCHAR(MAX) NULL,
        HasVariants BIT NOT NULL DEFAULT 0,
        IsApproved BIT NOT NULL DEFAULT 1,
        ApprovalStatus NVARCHAR(50) NOT NULL DEFAULT 'Approved',
        RejectionReason NVARCHAR(MAX) NULL,
        ApprovedDate DATETIME2 NULL,
        
        -- BaseModel Audit Fields
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        
        CONSTRAINT FK_Products_Shops FOREIGN KEY (ShopId) REFERENCES dbo.Shops(Id) ON DELETE CASCADE
    );
END;
GO

-- 1.3.1 PRODUCT VARIANTS TABLE
IF OBJECT_ID('dbo.ProductVariants', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductVariants (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProductId INT NOT NULL FOREIGN KEY REFERENCES dbo.Products(Id) ON DELETE CASCADE,
        Size NVARCHAR(50) NULL,
        Color NVARCHAR(50) NULL,
        Price DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        Stock INT NOT NULL DEFAULT 0,
        Sku NVARCHAR(100) NULL,
        ImageUrl NVARCHAR(MAX) NULL,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.4 ORDERS TABLE
IF OBJECT_ID('dbo.Orders', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId INT NOT NULL,
        ShopId INT NOT NULL,
        RiderId INT NULL,
        TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        OrderStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        PaymentMode NVARCHAR(50) NOT NULL DEFAULT 'COD',
        PaymentStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        CancelReason NVARCHAR(MAX) NULL,
        ReturnReason NVARCHAR(MAX) NULL,
        DeliveredDate DATETIME2 NULL,
        DeliveryAddress NVARCHAR(500) NULL,
        
        -- Return Workflow & Swap Protection (Point 34)
        ReturnStatus NVARCHAR(50) NULL,
        ReturnVerificationNotes NVARCHAR(MAX) NULL,
        ReturnRequestedDate DATETIME2 NULL,
        ReturnVerifiedDate DATETIME2 NULL,
        ShopName NVARCHAR(250) NULL,
        
        -- BaseModel Audit Fields
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,

        CONSTRAINT FK_Orders_Users FOREIGN KEY (CustomerId) REFERENCES dbo.Users(Id),
        CONSTRAINT FK_Orders_Shops FOREIGN KEY (ShopId) REFERENCES dbo.Shops(Id)
    );
END;
GO

-- 1.5 ORDERITEMS TABLE
IF OBJECT_ID('dbo.OrderItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL DEFAULT 1,
        UnitPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
        TotalPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
        
        -- Point 34 Outbound Inspection
        Sku NVARCHAR(100) NULL,
        SerialNumber NVARCHAR(100) NULL,
        DispatchCondition NVARCHAR(250) NULL DEFAULT 'Brand New / Sealed',
        IsProductVerified BIT NOT NULL DEFAULT 1,
        IsQuantityVerified BIT NOT NULL DEFAULT 1,
        IsSkuVerified BIT NOT NULL DEFAULT 1,
        IsPackagingVerified BIT NOT NULL DEFAULT 1,
        
        -- Point 34 Inbound Inspection
        ReturnReceivedSerial NVARCHAR(100) NULL,
        IsReturnSkuMatched BIT NULL,
        IsReturnSerialMatched BIT NULL,
        IsReturnConditionMatched BIT NULL,
        ReturnVerificationRemarks NVARCHAR(MAX) NULL,
        ReturnStatus NVARCHAR(50) NULL,
        
        -- BaseModel Audit Fields
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,

        CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
        CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
    );
END;
GO

-- 1.6 PRODUCTVARIANTS TABLE
IF OBJECT_ID('dbo.ProductVariants', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductVariants (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProductId INT NOT NULL,
        VariantType NVARCHAR(50) NOT NULL,
        VariantValue NVARCHAR(100) NOT NULL,
        Sku NVARCHAR(100) NULL,
        Price DECIMAL(18,2) NOT NULL DEFAULT 0,
        Mrp DECIMAL(18,2) NULL,
        Stock INT NOT NULL DEFAULT 0,
        ImageUrl NVARCHAR(MAX) NULL,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,

        CONSTRAINT FK_ProductVariants_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE
    );
END;
GO

-- 1.7 CUSTOMERADDRESSES TABLE
IF OBJECT_ID('dbo.CustomerAddresses', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CustomerAddresses (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId INT NOT NULL,
        RecipientName NVARCHAR(200) NOT NULL,
        PhoneNumber NVARCHAR(50) NOT NULL,
        AddressLine NVARCHAR(500) NOT NULL,
        City NVARCHAR(100) NOT NULL DEFAULT 'Patna',
        State NVARCHAR(100) NOT NULL DEFAULT 'Bihar',
        Pincode NVARCHAR(50) NOT NULL DEFAULT '800001',
        AddressType NVARCHAR(50) NOT NULL DEFAULT 'Home',
        IsDefault BIT NOT NULL DEFAULT 0,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,

        CONSTRAINT FK_CustomerAddresses_Users FOREIGN KEY (CustomerId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );
END;
GO

-- 1.8 CATEGORIES TABLE
IF OBJECT_ID('dbo.Categories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(150) NOT NULL,
        Icon NVARCHAR(100) NOT NULL DEFAULT 'fa-solid fa-layer-group',
        Description NVARCHAR(500) NOT NULL DEFAULT '',
        ImageUrl NVARCHAR(255) NULL,
        DisplayOrder INT NOT NULL DEFAULT 0,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.9 BRANDS TABLE
IF OBJECT_ID('dbo.Brands', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Brands (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(150) NOT NULL,
        Category NVARCHAR(100) NOT NULL DEFAULT 'General',
        LogoUrl NVARCHAR(MAX) NULL,
        Description NVARCHAR(500) NOT NULL DEFAULT '',
        Rating FLOAT NOT NULL DEFAULT 4.5,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.10 COUPONS TABLE
IF OBJECT_ID('dbo.Coupons', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Coupons (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Code NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NOT NULL DEFAULT '',
        DiscountType NVARCHAR(50) NOT NULL DEFAULT 'Flat',
        DiscountValue DECIMAL(18,2) NOT NULL DEFAULT 100,
        MinOrderAmount DECIMAL(18,2) NOT NULL DEFAULT 999,
        MaxDiscountAmount DECIMAL(18,2) NULL,
        StartDate DATETIME2 NULL,
        ExpiryDate DATETIME2 NULL,
        UsageLimit INT NOT NULL DEFAULT 500,
        UsedCount INT NOT NULL DEFAULT 0,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- Bring older coupon tables up to the current model without dropping data.
IF COL_LENGTH('dbo.Coupons', 'StartDate') IS NULL
    ALTER TABLE dbo.Coupons ADD StartDate DATETIME2 NULL;
IF COL_LENGTH('dbo.Coupons', 'UsageLimit') IS NULL
    ALTER TABLE dbo.Coupons ADD UsageLimit INT NOT NULL CONSTRAINT DF_Coupons_UsageLimit DEFAULT 500;
IF COL_LENGTH('dbo.Coupons', 'UsedCount') IS NULL
    ALTER TABLE dbo.Coupons ADD UsedCount INT NOT NULL CONSTRAINT DF_Coupons_UsedCount DEFAULT 0;
IF COL_LENGTH('dbo.Coupons', 'MinOrderAmount') IS NULL
    ALTER TABLE dbo.Coupons ADD MinOrderAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Coupons_MinOrderAmount DEFAULT 999;
IF COL_LENGTH('dbo.Coupons', 'MaxDiscountAmount') IS NULL
    ALTER TABLE dbo.Coupons ADD MaxDiscountAmount DECIMAL(18,2) NULL;
IF COL_LENGTH('dbo.Coupons', 'ExpiryDate') IS NULL
    ALTER TABLE dbo.Coupons ADD ExpiryDate DATETIME2 NULL;
GO

-- 1.11 OFFERS TABLE
IF OBJECT_ID('dbo.Offers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Offers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(250) NOT NULL,
        ProductId INT NOT NULL,
        Mrp DECIMAL(18,2) NOT NULL DEFAULT 0,
        SellingPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
        Discount DECIMAL(18,2) NOT NULL DEFAULT 0,
        DiscountType NVARCHAR(50) NOT NULL DEFAULT 'Percentage',
        StartDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        EndDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        BannerUrl NVARCHAR(MAX) NULL,
        Tagline NVARCHAR(250) NULL,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,

        CONSTRAINT FK_Offers_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE
    );
END;
GO

-- 1.12 COMPLAINTS TABLE
IF OBJECT_ID('dbo.Complaints', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Complaints (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TicketNumber NVARCHAR(50) NOT NULL DEFAULT '',
        OrderId INT NOT NULL,
        CustomerId INT NOT NULL,
        Issue NVARCHAR(150) NOT NULL,
        Description NVARCHAR(MAX) NOT NULL,
        AttachmentUrl NVARCHAR(1000) NULL,
        Priority NVARCHAR(50) NOT NULL DEFAULT 'High',
        Status NVARCHAR(50) NOT NULL DEFAULT 'Open',
        ResolutionNotes NVARCHAR(MAX) NULL,
        ResolvedDate DATETIME2 NULL,
        
        -- Multi-party & 3-Way Statements
        ComplainantRole NVARCHAR(50) NOT NULL DEFAULT 'Customer',
        RiderId INT NULL,
        ComplainantName NVARCHAR(150) NULL,
        ReasonCategory NVARCHAR(100) NULL,
        SellerStatement NVARCHAR(1000) NULL,
        SellerStatementDate DATETIME2 NULL,
        RiderStatement NVARCHAR(1000) NULL,
        RiderStatementDate DATETIME2 NULL,
        ThreadMessagesJson NVARCHAR(MAX) NULL,
        
        -- Escalation Tier & Priority
        EscalationLevel INT NOT NULL DEFAULT 1,
        EscalationStage NVARCHAR(50) NOT NULL DEFAULT 'Customer',
        IsHighValueOrder BIT NOT NULL DEFAULT 0,
        OrderAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        EscalatedToAdminDate DATETIME2 NULL,
        EscalationReason NVARCHAR(500) NULL,
        
        -- BaseModel Audit Fields
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,

        CONSTRAINT FK_Complaints_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id),
        CONSTRAINT FK_Complaints_Users FOREIGN KEY (CustomerId) REFERENCES dbo.Users(Id)
    );
END;
GO

-- 1.13 REVIEWS TABLE
IF OBJECT_ID('dbo.Reviews', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reviews (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId INT NOT NULL,
        ProductId INT NULL,
        ShopId INT NOT NULL DEFAULT 0,
        OrderId INT NULL,
        Rating INT NOT NULL DEFAULT 5,
        Comment NVARCHAR(MAX) NULL,
        IsHidden BIT NOT NULL DEFAULT 0,
        ModerationReason NVARCHAR(MAX) NULL,
        CustomerName NVARCHAR(200) NULL,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,

        CONSTRAINT FK_Reviews_Users FOREIGN KEY (CustomerId) REFERENCES dbo.Users(Id)
    );
END;
GO

-- 1.14 RIDERS TABLE
IF OBJECT_ID('dbo.Riders', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Riders (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RiderName NVARCHAR(200) NOT NULL,
        PhoneNumber NVARCHAR(50) NOT NULL,
        Email NVARCHAR(200) NULL,
        VehicleNumber NVARCHAR(50) NULL,
        Password NVARCHAR(MAX) NULL,
        IsAvailable BIT NOT NULL DEFAULT 1,
        CurrentLatitude DECIMAL(18,6) NULL,
        CurrentLongitude DECIMAL(18,6) NULL,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

IF COL_LENGTH('dbo.Riders', 'Password') IS NULL
    ALTER TABLE dbo.Riders ADD Password NVARCHAR(MAX) NULL;
GO

-- 1.15 NOTIFICATIONS TABLE
IF OBJECT_ID('dbo.Notifications', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notifications (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId INT NOT NULL,
        RecipientRole NVARCHAR(50) NOT NULL DEFAULT 'Customer',
        ShopId INT NULL,
        Title NVARCHAR(250) NOT NULL,
        Message NVARCHAR(MAX) NOT NULL,
        Type NVARCHAR(50) NOT NULL DEFAULT 'Order',
        LinkUrl NVARCHAR(MAX) NULL,
        IsRead BIT NOT NULL DEFAULT 0,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,

        CONSTRAINT FK_Notifications_Users FOREIGN KEY (CustomerId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );
END;
GO

-- 1.16 WISHLISTS TABLE
IF OBJECT_ID('dbo.Wishlists', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Wishlists (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId INT NOT NULL,
        ProductId INT NOT NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_Wishlists_Users FOREIGN KEY (CustomerId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Wishlists_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE
    );
END;
GO

-- 1.17 AUDITLOGS TABLE
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Action NVARCHAR(200) NOT NULL,
        EntityName NVARCHAR(100) NULL,
        EntityId INT NULL,
        Details NVARCHAR(MAX) NULL,
        UserId INT NULL,
        UserName NVARCHAR(200) NULL,
        UserRole NVARCHAR(50) NULL,
        IpAddress NVARCHAR(100) NULL,
        Timestamp DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END;
GO

-- 1.18 ORDEREVIDENCES TABLE
IF OBJECT_ID('dbo.OrderEvidences', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderEvidences (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL,
        EvidenceType NVARCHAR(100) NOT NULL,
        PhotoUrl NVARCHAR(MAX) NOT NULL,
        Title NVARCHAR(250) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        UploadedByRole NVARCHAR(50) NOT NULL,
        UploadedByName NVARCHAR(200) NULL,
        UploadedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        IsVerified BIT NOT NULL DEFAULT 0,
        VerifiedBy NVARCHAR(200) NULL,
        MetadataJson NVARCHAR(MAX) NULL,

        CONSTRAINT FK_OrderEvidences_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE
    );
END;
GO

-- 1.19 LOGIN HISTORIES TABLE
IF OBJECT_ID('dbo.LoginHistories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LoginHistories (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NULL,
        UserName NVARCHAR(200) NOT NULL,
        Role NVARCHAR(50) NOT NULL DEFAULT 'Customer',
        LoginTime DATETIME2 NOT NULL DEFAULT GETDATE(),
        LogoutTime DATETIME2 NULL,
        LastActivityTime DATETIME2 NOT NULL DEFAULT GETDATE(),
        IpAddress NVARCHAR(100) NULL,
        Browser NVARCHAR(100) NULL,
        Device NVARCHAR(100) NULL,
        OperatingSystem NVARCHAR(100) NULL,
        SessionId NVARCHAR(150) NULL,
        IsSuccessful BIT NOT NULL DEFAULT 1,
        FailureReason NVARCHAR(500) NULL,
        IsActiveSession BIT NOT NULL DEFAULT 1,
        IsForceLoggedOut BIT NOT NULL DEFAULT 0,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.20 USER SESSIONS TABLE
IF OBJECT_ID('dbo.UserSessions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserSessions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        SessionId NVARCHAR(150) NOT NULL,
        UserId INT NULL,
        UserName NVARCHAR(200) NOT NULL,
        Role NVARCHAR(50) NOT NULL DEFAULT 'Customer',
        IpAddress NVARCHAR(100) NULL,
        Device NVARCHAR(100) NULL DEFAULT 'Desktop',
        Browser NVARCHAR(100) NULL DEFAULT 'Chrome',
        OperatingSystem NVARCHAR(100) NULL DEFAULT 'Windows',
        LoginTime DATETIME2 NOT NULL DEFAULT GETDATE(),
        LastSeenTime DATETIME2 NOT NULL DEFAULT GETDATE(),
        LastPageVisited NVARCHAR(250) NULL DEFAULT '/',
        LastAction NVARCHAR(250) NULL DEFAULT 'Page View',
        IsActive BIT NOT NULL DEFAULT 1,
        ExpiryTime DATETIME2 NULL,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
END;
GO

-- 1.21 USER ACTIVITIES TABLE
IF OBJECT_ID('dbo.UserActivities', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserActivities (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NULL,
        UserName NVARCHAR(200) NOT NULL,
        Role NVARCHAR(50) NOT NULL DEFAULT 'Customer',
        Action NVARCHAR(100) NOT NULL,
        Module NVARCHAR(100) NOT NULL,
        Entity NVARCHAR(100) NULL,
        EntityId INT NULL,
        Description NVARCHAR(2000) NULL,
        Timestamp DATETIME2 NOT NULL DEFAULT GETDATE(),
        IpAddress NVARCHAR(100) NULL,
        Device NVARCHAR(100) NULL,
        Browser NVARCHAR(100) NULL,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.22 ENTITY CHANGE LOGS (BEFORE/AFTER DIFFS) TABLE
IF OBJECT_ID('dbo.EntityChangeLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EntityChangeLogs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        EntityName NVARCHAR(100) NOT NULL,
        EntityId INT NOT NULL,
        Action NVARCHAR(100) NOT NULL DEFAULT 'UPDATE',
        FieldName NVARCHAR(100) NULL,
        OldValue NVARCHAR(2000) NULL,
        NewValue NVARCHAR(2000) NULL,
        ChangedByUserId INT NULL,
        ChangedByUserName NVARCHAR(200) NULL,
        ChangedByUserRole NVARCHAR(50) NULL,
        Timestamp DATETIME2 NOT NULL DEFAULT GETDATE(),
        IpAddress NVARCHAR(100) NULL,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.23 SOFT DELETE LOGS TABLE
IF OBJECT_ID('dbo.SoftDeleteLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SoftDeleteLogs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        EntityName NVARCHAR(100) NOT NULL,
        EntityId INT NOT NULL,
        EntityTitle NVARCHAR(250) NULL,
        DeletedByUserId INT NULL,
        DeletedByUserName NVARCHAR(200) NULL,
        DeletedByUserRole NVARCHAR(50) NULL,
        DeletedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        IpAddress NVARCHAR(100) NULL,
        Reason NVARCHAR(1000) NULL,
        SnapshotDataJson NVARCHAR(MAX) NULL,
        IsRestored BIT NOT NULL DEFAULT 0,
        RestoredAt DATETIME2 NULL,
        RestoredBy NVARCHAR(200) NULL,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.24 ENTITY VIEW LOGS TABLE
IF OBJECT_ID('dbo.EntityViewLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EntityViewLogs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        EntityName NVARCHAR(100) NOT NULL,
        EntityId INT NOT NULL,
        ViewedByUserId INT NULL,
        ViewedByUserName NVARCHAR(200) NOT NULL,
        ViewedByUserRole NVARCHAR(50) NOT NULL DEFAULT 'Admin',
        ViewedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        IpAddress NVARCHAR(100) NULL,
        ExtraInfo NVARCHAR(500) NULL,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.25 SECURITY THREAT ALERTS TABLE
IF OBJECT_ID('dbo.SecurityThreatAlerts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SecurityThreatAlerts (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        AlertType NVARCHAR(100) NOT NULL,
        Severity NVARCHAR(50) NOT NULL DEFAULT 'Medium',
        Title NVARCHAR(250) NOT NULL,
        Description NVARCHAR(2000) NULL,
        AffectedUserId INT NULL,
        AffectedUserName NVARCHAR(200) NULL,
        IpAddress NVARCHAR(100) NULL,
        DetectedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        IsResolved BIT NOT NULL DEFAULT 0,
        ResolvedAt DATETIME2 NULL,
        ResolvedBy NVARCHAR(200) NULL,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.26 SEARCH HISTORIES TABLE (Point 82)
IF OBJECT_ID('dbo.SearchHistories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SearchHistories (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId INT NULL,
        SearchTerm NVARCHAR(200) NOT NULL,
        Category NVARCHAR(100) NULL,
        ResultCount INT NOT NULL DEFAULT 0,
        ClientIp NVARCHAR(100) NULL,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.27 PRODUCT QUESTIONS TABLE (Point 84)
IF OBJECT_ID('dbo.ProductQuestions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductQuestions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProductId INT NOT NULL,
        CustomerId INT NOT NULL,
        QuestionText NVARCHAR(1000) NOT NULL,
        IsApproved BIT NOT NULL DEFAULT 1,
        Upvotes INT NOT NULL DEFAULT 0,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.28 PRODUCT ANSWERS TABLE (Point 84)
IF OBJECT_ID('dbo.ProductAnswers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductAnswers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        QuestionId INT NOT NULL,
        ResponderId INT NOT NULL,
        AnswerText NVARCHAR(2000) NOT NULL,
        ResponderRole NVARCHAR(50) NOT NULL DEFAULT 'Seller',
        IsVerifiedSellerAnswer BIT NOT NULL DEFAULT 1,
        HelpfulCount INT NOT NULL DEFAULT 0,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.29 CHAT MESSAGES TABLE (Point 85)
IF OBJECT_ID('dbo.ChatMessages', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChatMessages (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ThreadId NVARCHAR(100) NOT NULL,
        SenderId INT NOT NULL,
        SenderRole NVARCHAR(50) NOT NULL DEFAULT 'Customer',
        RecipientId INT NULL,
        OrderId INT NULL,
        ProductId INT NULL,
        ShopId INT NULL,
        MessageText NVARCHAR(2000) NOT NULL,
        AttachmentUrl NVARCHAR(500) NULL,
        IsRead BIT NOT NULL DEFAULT 0,
        ReadAt DATETIME2 NULL,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.30 PRICE DROP ALERTS TABLE (Point 86)
IF OBJECT_ID('dbo.PriceDropAlerts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PriceDropAlerts (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId INT NOT NULL,
        ProductId INT NOT NULL,
        SubscribedPrice DECIMAL(18,2) NOT NULL,
        TargetPrice DECIMAL(18,2) NULL,
        IsNotified BIT NOT NULL DEFAULT 0,
        NotifiedAt DATETIME2 NULL,
        TriggerPrice DECIMAL(18,2) NULL,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.31 STOCK ALERTS TABLE (Point 87)
IF OBJECT_ID('dbo.StockAlerts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockAlerts (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId INT NOT NULL,
        ProductId INT NOT NULL,
        CustomerEmail NVARCHAR(200) NULL,
        CustomerPhone NVARCHAR(20) NULL,
        IsNotified BIT NOT NULL DEFAULT 0,
        NotifiedAt DATETIME2 NULL,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.32 WALLET ACCOUNTS TABLE (Point 89)
IF OBJECT_ID('dbo.WalletAccounts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.WalletAccounts (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId INT NOT NULL,
        MainBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
        RefundWalletBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
        GiftCardBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
        IsLocked BIT NOT NULL DEFAULT 0,
        LockReason NVARCHAR(MAX) NULL,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.33 WALLET TRANSACTIONS TABLE (Point 89)
IF OBJECT_ID('dbo.WalletTransactions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.WalletTransactions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        WalletAccountId INT NOT NULL,
        TransactionType NVARCHAR(50) NOT NULL DEFAULT 'Credit',
        SourceCategory NVARCHAR(50) NOT NULL DEFAULT 'Refund',
        Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
        BalanceAfter DECIMAL(18,2) NOT NULL DEFAULT 0,
        Description NVARCHAR(500) NOT NULL DEFAULT '',
        RelatedOrderId INT NULL,
        ReferenceCode NVARCHAR(100) NULL,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.34 GIFT CARDS TABLE (Point 89)
IF OBJECT_ID('dbo.GiftCards', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.GiftCards (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CardCode NVARCHAR(50) NOT NULL,
        Pin NVARCHAR(20) NOT NULL DEFAULT '1234',
        InitialAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        CurrentBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
        ExpiryDate DATETIME2 NOT NULL DEFAULT DATEADD(year, 1, GETDATE()),
        IsRedeemed BIT NOT NULL DEFAULT 0,
        RedeemedByCustomerId INT NULL,
        RedeemedAt DATETIME2 NULL,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.35 REWARD POINTS TABLE (Point 90)
IF OBJECT_ID('dbo.RewardPoints', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RewardPoints (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId INT NOT NULL,
        CurrentPoints INT NOT NULL DEFAULT 0,
        LifetimeEarnedPoints INT NOT NULL DEFAULT 0,
        LifetimeRedeemedPoints INT NOT NULL DEFAULT 0,
        Tier NVARCHAR(50) NOT NULL DEFAULT 'Silver',
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.36 REWARD POINTS TRANSACTIONS TABLE (Point 90)
IF OBJECT_ID('dbo.RewardPointsTransactions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RewardPointsTransactions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RewardPointsAccountId INT NOT NULL,
        TransactionType NVARCHAR(50) NOT NULL DEFAULT 'Earned',
        Points INT NOT NULL DEFAULT 0,
        PointsBalanceAfter INT NOT NULL DEFAULT 0,
        Description NVARCHAR(500) NOT NULL DEFAULT '',
        RelatedOrderId INT NULL,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.37 STOCK RESERVATIONS TABLE (Point 94)
IF OBJECT_ID('dbo.StockReservations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockReservations (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ReservationToken NVARCHAR(100) NOT NULL,
        CustomerId INT NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL DEFAULT 1,
        ExpiresAt DATETIME2 NOT NULL DEFAULT DATEADD(minute, 10, GETDATE()),
        IsCommitted BIT NOT NULL DEFAULT 0,
        IsReleased BIT NOT NULL DEFAULT 0,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.38 PINCODE SERVICEABILITY TABLE (Point 97)
IF OBJECT_ID('dbo.PincodeServiceabilities', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PincodeServiceabilities (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Pincode NVARCHAR(10) NOT NULL,
        City NVARCHAR(100) NOT NULL DEFAULT 'Patna',
        State NVARCHAR(100) NOT NULL DEFAULT 'Bihar',
        IsServiceable BIT NOT NULL DEFAULT 1,
        IsCodAvailable BIT NOT NULL DEFAULT 1,
        EstimatedDeliveryDays INT NOT NULL DEFAULT 2,
        IsExpressAvailable BIT NOT NULL DEFAULT 1,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.39 FAILED DELIVERY LOGS TABLE (Point 99)
IF OBJECT_ID('dbo.FailedDeliveryLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.FailedDeliveryLogs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL,
        RiderId INT NULL,
        AttemptNumber INT NOT NULL DEFAULT 1,
        FailureReason NVARCHAR(100) NOT NULL DEFAULT 'Customer Unavailable',
        RiderRemarks NVARCHAR(1000) NULL,
        DoorstepPhotoUrl NVARCHAR(500) NULL,
        GeoLatitude FLOAT NULL,
        GeoLongitude FLOAT NULL,
        NextAction NVARCHAR(50) NOT NULL DEFAULT 'Auto Re-Attempt Scheduled',
        RescheduledDate DATETIME2 NULL,
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- 1.40 DELIVERY PROOFS TABLE (Point 100)
IF OBJECT_ID('dbo.DeliveryProofs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DeliveryProofs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL,
        RiderId INT NULL,
        HandoverOtp NVARCHAR(10) NOT NULL DEFAULT '',
        IsOtpVerified BIT NOT NULL DEFAULT 1,
        DeliveryTimestamp DATETIME2 NOT NULL DEFAULT GETDATE(),
        SignatureOrPhotoUrl NVARCHAR(500) NULL,
        HandoverLatitude FLOAT NULL,
        HandoverLongitude FLOAT NULL,
        ReceivedByName NVARCHAR(100) NOT NULL DEFAULT '',
        Remark NVARCHAR(MAX) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedById INT NULL,
        UpdatedDate DATETIME2 NULL,
        UpdatedById INT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
END;
GO

-- ==============================================================================
-- 2. HIGH-PERFORMANCE INDEXES FOR ULTRA-FAST QUERIES
-- ==============================================================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_ShopId_IsActive')
    CREATE NONCLUSTERED INDEX IX_Products_ShopId_IsActive ON dbo.Products(ShopId, IsActive, IsDeleted) INCLUDE (ProductName, Price, Stock, ImageUrl);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_CustomerId_OrderStatus')
    CREATE NONCLUSTERED INDEX IX_Orders_CustomerId_OrderStatus ON dbo.Orders(CustomerId, OrderStatus, CreatedDate);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OrderItems_OrderId')
    CREATE NONCLUSTERED INDEX IX_OrderItems_OrderId ON dbo.OrderItems(OrderId) INCLUDE (ProductId, Quantity, UnitPrice, TotalPrice);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_PhoneNumber')
    CREATE NONCLUSTERED INDEX IX_Users_PhoneNumber ON dbo.Users(PhoneNumber, Role, IsActive);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Shops_IsApproved_IsActive')
    CREATE NONCLUSTERED INDEX IX_Shops_IsApproved_IsActive ON dbo.Shops(IsApproved, IsActive, IsDeleted);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LoginHistories_UserId')
    CREATE NONCLUSTERED INDEX IX_LoginHistories_UserId ON dbo.LoginHistories(UserId, LoginTime);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserSessions_SessionId')
    CREATE UNIQUE NONCLUSTERED INDEX IX_UserSessions_SessionId ON dbo.UserSessions(SessionId);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserActivities_Timestamp')
    CREATE NONCLUSTERED INDEX IX_UserActivities_Timestamp ON dbo.UserActivities(Timestamp DESC);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_EntityChangeLogs_Entity')
    CREATE NONCLUSTERED INDEX IX_EntityChangeLogs_Entity ON dbo.EntityChangeLogs(EntityName, EntityId);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SecurityThreatAlerts_IsResolved')
    CREATE NONCLUSTERED INDEX IX_SecurityThreatAlerts_IsResolved ON dbo.SecurityThreatAlerts(IsResolved, DetectedAt DESC);
GO

-- ==============================================================================
-- 3. OPTIMIZED STORED PROCEDURES (SPs) FOR MAXIMUM SPEED
-- ==============================================================================

-- 3.1 SP: Get All Active & Approved Shops
CREATE OR ALTER PROCEDURE dbo.sp_GetActiveShops
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        s.Id, s.ShopName, s.PhoneNumber, s.Category, s.Latitude, s.Longitude,
        s.OwnerName, s.Email, s.Address, s.City, s.State, s.Pincode,
        s.IsApproved, s.IsActive, s.CreatedDate
    FROM dbo.Shops s WITH (NOLOCK)
    WHERE s.IsDeleted = 0 AND s.IsActive = 1 AND s.IsApproved = 1
    ORDER BY s.ShopName ASC;
END;
GO

-- 3.2 SP: Get Products by Shop ID
CREATE OR ALTER PROCEDURE dbo.sp_GetProductsByShop
    @ShopId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        p.Id, p.ShopId, p.ProductName, p.Category, p.SubCategory, p.Brand,
        p.Description, p.Price, p.Mrp, p.Discount, p.Stock, p.Sku,
        p.StockStatus, p.ImageUrl, p.HasVariants, p.IsApproved, p.ApprovalStatus,
        ISNULL((SELECT AVG(CAST(r.Rating AS FLOAT)) FROM dbo.Reviews r WITH (NOLOCK) WHERE r.ProductId = p.Id AND r.IsDeleted = 0), 4.5) AS Rating,
        ISNULL((SELECT COUNT(*) FROM dbo.Reviews r WITH (NOLOCK) WHERE r.ProductId = p.Id AND r.IsDeleted = 0), 0) AS ReviewsCount,
        p.CreatedDate
    FROM dbo.Products p WITH (NOLOCK)
    WHERE p.ShopId = @ShopId AND p.IsDeleted = 0 AND p.IsActive = 1
    ORDER BY p.ProductName ASC;
END;
GO

-- 3.3 SP: Get Customer Profile Summary & Order History
CREATE OR ALTER PROCEDURE dbo.sp_GetCustomerOrderHistory
    @CustomerId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Recent Orders List
    SELECT TOP 20
        o.Id AS OrderId,
        o.TotalAmount,
        o.OrderStatus,
        o.PaymentMode,
        o.CreatedDate,
        o.Remark,
        s.ShopName,
        (SELECT COUNT(*) FROM dbo.OrderItems oi WITH (NOLOCK) WHERE oi.OrderId = o.Id) AS ItemCount
    FROM dbo.Orders o WITH (NOLOCK)
    LEFT JOIN dbo.Shops s WITH (NOLOCK) ON s.Id = o.ShopId
    WHERE o.CustomerId = @CustomerId AND o.IsDeleted = 0
    ORDER BY o.CreatedDate DESC;
END;
GO

-- 3.4 SP: Get Admin Platform Dashboard KPIs
CREATE OR ALTER PROCEDURE dbo.sp_GetAdminDashboardKPIs
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalCustomers INT = (SELECT COUNT(*) FROM dbo.Users WITH (NOLOCK) WHERE Role = 'Customer' AND IsDeleted = 0);
    DECLARE @TotalSellers INT = (SELECT COUNT(*) FROM dbo.Shops WITH (NOLOCK) WHERE IsDeleted = 0);
    DECLARE @TotalProducts INT = (SELECT COUNT(*) FROM dbo.Products WITH (NOLOCK) WHERE IsDeleted = 0);
    DECLARE @TotalOrders INT = (SELECT COUNT(*) FROM dbo.Orders WITH (NOLOCK) WHERE IsDeleted = 0);
    DECLARE @PendingSellers INT = (SELECT COUNT(*) FROM dbo.Shops WITH (NOLOCK) WHERE IsApproved = 0 AND IsDeleted = 0);
    DECLARE @PendingProducts INT = (SELECT COUNT(*) FROM dbo.Products WITH (NOLOCK) WHERE IsApproved = 0 AND IsDeleted = 0);
    DECLARE @TodaySales DECIMAL(18,2) = ISNULL((SELECT SUM(TotalAmount) FROM dbo.Orders WITH (NOLOCK) WHERE CAST(CreatedDate AS DATE) = CAST(GETDATE() AS DATE) AND OrderStatus <> 'Cancelled'), 0);

    SELECT 
        @TotalCustomers AS TotalCustomers,
        @TotalSellers AS TotalSellers,
        @TotalProducts AS TotalProducts,
        @TotalOrders AS TotalOrders,
        @PendingSellers AS PendingSellers,
        @PendingProducts AS PendingProducts,
        @TodaySales AS TodaySales;
END;
GO

-- 3.5 SP: High Return Area / Pincode Analytics (Point 38)
CREATE OR ALTER PROCEDURE dbo.sp_GetHighReturnAreas
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        ISNULL(ca.Pincode, '800001') AS Pincode,
        ISNULL(ca.City, 'Patna') AS AreaName,
        COUNT(o.Id) AS TotalOrders,
        SUM(CASE WHEN o.OrderStatus IN ('Delivered', 'Completed') THEN 1 ELSE 0 END) AS DeliveredOrders,
        SUM(CASE WHEN o.OrderStatus IN ('Returned', 'Return_Requested', 'Approved') OR o.ReturnStatus IN ('Returned', 'Approved', 'Product_Swapped_Fraud') THEN 1 ELSE 0 END) AS ReturnOrders,
        CASE 
            WHEN COUNT(o.Id) > 0 THEN ROUND((CAST(SUM(CASE WHEN o.OrderStatus IN ('Returned', 'Return_Requested', 'Approved') OR o.ReturnStatus IN ('Returned', 'Approved', 'Product_Swapped_Fraud') THEN 1 ELSE 0 END) AS DECIMAL(18,2)) / COUNT(o.Id)) * 100.0, 1)
            ELSE 0 
        END AS ReturnRate
    FROM dbo.Orders o WITH (NOLOCK)
    LEFT JOIN dbo.CustomerAddresses ca WITH (NOLOCK) ON ca.CustomerId = o.CustomerId
    WHERE o.IsDeleted = 0
    GROUP BY ca.Pincode, ca.City
    ORDER BY ReturnRate DESC;
END;
GO

-- 3.6 SP: System Monitoring Overview & Telemetry Stats
CREATE OR ALTER PROCEDURE dbo.sp_GetMonitoringOverviewStats
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Today DATE = CAST(GETDATE() AS DATE);
    DECLARE @ActiveCutoff DATETIME2 = DATEADD(minute, -30, GETDATE());

    SELECT 
        (SELECT COUNT(*) FROM dbo.Users WITH (NOLOCK) WHERE IsDeleted = 0) AS TotalUsers,
        (SELECT COUNT(*) FROM dbo.UserSessions WITH (NOLOCK) WHERE IsActive = 1 AND LastSeenTime >= @ActiveCutoff) AS OnlineUsers,
        (SELECT COUNT(*) FROM dbo.LoginHistories WITH (NOLOCK) WHERE CAST(LoginTime AS DATE) = @Today AND IsSuccessful = 1) AS TodayLogins,
        (SELECT COUNT(*) FROM dbo.LoginHistories WITH (NOLOCK) WHERE CAST(LoginTime AS DATE) = @Today AND IsSuccessful = 0) AS FailedLoginAttempts,
        (SELECT COUNT(*) FROM dbo.Orders WITH (NOLOCK) WHERE CAST(CreatedDate AS DATE) = @Today AND IsDeleted = 0) AS TodayOrders,
        (SELECT COUNT(*) FROM dbo.UserActivities WITH (NOLOCK) WHERE CAST(Timestamp AS DATE) = @Today) AS TodayActivities;
END;
GO

-- ==============================================================================
-- 4. MASTER SEED DATA (POPULATES REAL TABLES DIRECTLY IN SQL SERVER)
-- ==============================================================================

-- 4.1 Categories
IF NOT EXISTS (SELECT 1 FROM dbo.Categories)
BEGIN
    INSERT INTO dbo.Categories (Name, Icon, Description, DisplayOrder, CreatedDate, IsActive, IsDeleted)
    VALUES 
    ('Electronics', 'fa-solid fa-tv', 'Smart TVs, Home Audio, Cameras & Appliances', 1, DATEADD(day, -60, GETDATE()), 1, 0),
    ('Fashion', 'fa-solid fa-shirt', 'Men, Women, Kids Apparel, Footwear & Accessories', 2, DATEADD(day, -60, GETDATE()), 1, 0),
    ('Grocery', 'fa-solid fa-basket-shopping', 'Daily Staples, Dal, Rice, Spices, Dairy & Snacks', 3, DATEADD(day, -60, GETDATE()), 1, 0),
    ('Mobiles', 'fa-solid fa-mobile-screen-button', '5G Smartphones, Tablets, Earphones & Covers', 4, DATEADD(day, -60, GETDATE()), 1, 0),
    ('Computers', 'fa-solid fa-laptop', 'Laptops, Desktops, Monitors, Keyboards & Gaming', 5, DATEADD(day, -60, GETDATE()), 1, 0);
END;
GO

-- 4.2 Brands
IF NOT EXISTS (SELECT 1 FROM dbo.Brands)
BEGIN
    INSERT INTO dbo.Brands (Name, Category, Description, LogoUrl, Rating, CreatedDate, IsActive, IsDeleted)
    VALUES 
    ('Samsung', 'Electronics', 'Global leader in smartphones, smart TVs, soundbars & home appliances.', 'https://images.unsplash.com/photo-1610945415295-d9bbf067e59c?w=100', 4.8, DATEADD(day, -60, GETDATE()), 1, 0),
    ('Apple', 'Mobiles', 'Premium iPhones, MacBooks, iPads, AirPods and digital ecosystem.', 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=100', 4.9, DATEADD(day, -60, GETDATE()), 1, 0),
    ('Nike', 'Fashion', 'World-renowned athletic footwear, sports apparel, and lifestyle sneakers.', 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=100', 4.8, DATEADD(day, -50, GETDATE()), 1, 0),
    ('Adidas', 'Fashion', 'Iconic three-stripes performance sportswear, running shoes and gear.', 'https://images.unsplash.com/photo-1518002171953-a080ee817e1f?w=100', 4.7, DATEADD(day, -50, GETDATE()), 1, 0),
    ('HP', 'Computers', 'High performance laptops, gaming rigs, workstations, and printers.', 'https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=100', 4.6, DATEADD(day, -45, GETDATE()), 1, 0),
    ('Dell', 'Computers', 'Enterprise grade computers, XPS ultrabooks, monitors and IT hardware.', 'https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=100', 4.7, DATEADD(day, -40, GETDATE()), 1, 0),
    ('Lenovo', 'Computers', 'ThinkPad business laptops, Legion gaming desktops, and Yoga convertibles.', 'https://images.unsplash.com/photo-1525547719571-a2d4ac8945e2?w=100', 4.6, DATEADD(day, -35, GETDATE()), 1, 0);
END;
GO

-- 4.3 Users & Customers
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Role = 'Admin')
BEGIN
    INSERT INTO dbo.Users (Name, PhoneNumber, Email, Password, Role, RiskScore, RiskLevel, IsCodDisabled, IsFlaggedForReview, CreatedDate, IsActive, IsDeleted)
    VALUES ('Super Admin', '9999999999', 'admin@shopnext.com', 'Admin@123', 'Admin', 0, 'Low', 0, 0, DATEADD(day, -100, GETDATE()), 1, 0);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Role = 'Customer')
BEGIN
    INSERT INTO dbo.Users (Name, PhoneNumber, Email, Password, Role, RiskScore, RiskLevel, IsCodDisabled, IsFlaggedForReview, Remark, CreatedDate, IsActive, IsDeleted)
    VALUES 
    ('Pooja Sharma', '9999988888', 'pooja@example.com', 'pass123', 'Customer', 15, 'Low', 0, 0, 'Verified Customer', DATEADD(month, -8, GETDATE()), 1, 0),
    ('Rahul Verma', '9811223344', 'rahul.v@example.com', '123456', 'Customer', 20, 'Low', 0, 0, 'Verified Customer', DATEADD(month, -7, GETDATE()), 1, 0),
    ('Priya Patel', '9822334455', 'priya.p@example.com', '123456', 'Customer', 10, 'Low', 0, 0, 'Verified Customer', DATEADD(month, -6, GETDATE()), 1, 0),
    ('Amit Kumar', '9833445566', 'amit.k@example.com', '123456', 'Customer', 45, 'Medium', 0, 0, 'Regular Shopper', DATEADD(month, -5, GETDATE()), 1, 0),
    ('Sneha Singh', '9844556677', 'sneha.s@example.com', '123456', 'Customer', 25, 'Low', 0, 0, 'Verified Customer', DATEADD(month, -4, GETDATE()), 1, 0),
    ('Vikram Aditya', '9855667788', 'vikram.a@example.com', '123456', 'Customer', 85, 'High', 1, 1, 'Blocked: High return rate on COD', DATEADD(month, -3, GETDATE()), 0, 0);
END;
GO

-- 4.4 Customer Addresses
IF NOT EXISTS (SELECT 1 FROM dbo.CustomerAddresses)
BEGIN
    DECLARE @CustPooja INT = (SELECT TOP 1 Id FROM dbo.Users WHERE Email = 'pooja@example.com');
    DECLARE @CustRahul INT = (SELECT TOP 1 Id FROM dbo.Users WHERE Email = 'rahul.v@example.com');
    DECLARE @CustPriya INT = (SELECT TOP 1 Id FROM dbo.Users WHERE Email = 'priya.p@example.com');

    IF @CustPooja IS NOT NULL
        INSERT INTO dbo.CustomerAddresses (CustomerId, RecipientName, PhoneNumber, AddressLine, City, State, Pincode, AddressType, IsDefault, IsActive, IsDeleted, CreatedDate)
        VALUES (@CustPooja, 'Pooja Sharma', '9876543210', 'Flat 302, Maurya Vihar, Boring Road', 'Patna', 'Bihar', '800001', 'Home', 1, 1, 0, GETDATE());

    IF @CustRahul IS NOT NULL
        INSERT INTO dbo.CustomerAddresses (CustomerId, RecipientName, PhoneNumber, AddressLine, City, State, Pincode, AddressType, IsDefault, IsActive, IsDeleted, CreatedDate)
        VALUES (@CustRahul, 'Rahul Verma', '9811223344', 'House #45, Kankarbagh Main Road', 'Patna', 'Bihar', '800020', 'Home', 1, 1, 0, GETDATE());

    IF @CustPriya IS NOT NULL
        INSERT INTO dbo.CustomerAddresses (CustomerId, RecipientName, PhoneNumber, AddressLine, City, State, Pincode, AddressType, IsDefault, IsActive, IsDeleted, CreatedDate)
        VALUES (@CustPriya, 'Priya Patel', '9822334455', 'Sector 4, Ashiana Nagar', 'Patna', 'Bihar', '800025', 'Office', 1, 1, 0, GETDATE());
END;
GO

-- 4.5 Shops & Sellers
IF NOT EXISTS (SELECT 1 FROM dbo.Shops)
BEGIN
    INSERT INTO dbo.Shops (ShopName, OwnerName, Email, PhoneNumber, Category, Address, City, State, Pincode, Latitude, Longitude, IsApproved, IsActive, IsDeleted, Remark, CreatedDate)
    VALUES 
    ('ABC Electronics', 'Rajesh Kumar', 'abc.electronics@shopnext.com', '9876511223', 'Electronics', 'Boring Road', 'Patna', 'Bihar', '800001', 25.6093, 85.1376, 1, 1, 0, 'Approved by Admin', DATEADD(month, -8, GETDATE())),
    ('Ramesh Kirana & General', 'Ramesh Gupta', 'ramesh.kirana@shopnext.com', '9811233445', 'Grocery', 'Kankarbagh Main Road', 'Patna', 'Bihar', '800020', 25.5941, 85.1584, 1, 1, 0, 'Approved by Admin', DATEADD(month, -7, GETDATE())),
    ('Patna Tech & Mobiles', 'Sunil Verma', 'patna.tech@shopnext.com', '9822344556', 'Mobiles', 'Dak Bungalow Road', 'Patna', 'Bihar', '800001', 25.6120, 85.1390, 1, 1, 0, 'Approved by Admin', DATEADD(month, -6, GETDATE())),
    ('Style Hub Men & Women', 'Deepak Singh', 'style.hub@shopnext.com', '9833455667', 'Fashion', 'Maurya Lok Complex', 'Patna', 'Bihar', '800001', 25.6110, 85.1350, 1, 1, 0, 'Approved by Admin', DATEADD(month, -5, GETDATE())),
    ('Super Computers & IT Solutions', 'Manoj Sinha', 'super.comp@shopnext.com', '9844566778', 'Computers', 'Exhibition Road', 'Patna', 'Bihar', '800001', 25.6080, 85.1420, 1, 1, 0, 'Approved by Admin', DATEADD(month, -4, GETDATE())),
    ('New Bihar Footwear', 'Vikas Rai', 'bihar.footwear@shopnext.com', '9855677889', 'Fashion', 'Ashiana Mor, Bailey Road', 'Patna', 'Bihar', '800014', 25.6180, 85.0870, 0, 1, 0, 'Pending verification', DATEADD(day, -3, GETDATE()));
END;
GO

-- 4.6 Products
IF NOT EXISTS (SELECT 1 FROM dbo.Products)
BEGIN
    DECLARE @ShopElec INT = (SELECT TOP 1 Id FROM dbo.Shops WHERE ShopName LIKE '%ABC Electronics%');
    DECLARE @ShopGroc INT = (SELECT TOP 1 Id FROM dbo.Shops WHERE ShopName LIKE '%Ramesh Kirana%');
    DECLARE @ShopMob INT = (SELECT TOP 1 Id FROM dbo.Shops WHERE ShopName LIKE '%Patna Tech%');
    DECLARE @ShopFash INT = (SELECT TOP 1 Id FROM dbo.Shops WHERE ShopName LIKE '%Style Hub%');
    DECLARE @ShopComp INT = (SELECT TOP 1 Id FROM dbo.Shops WHERE ShopName LIKE '%Super Computers%');

    IF @ShopElec IS NULL SET @ShopElec = (SELECT TOP 1 Id FROM dbo.Shops);
    IF @ShopGroc IS NULL SET @ShopGroc = @ShopElec;
    IF @ShopMob IS NULL SET @ShopMob = @ShopElec;
    IF @ShopFash IS NULL SET @ShopFash = @ShopElec;
    IF @ShopComp IS NULL SET @ShopComp = @ShopElec;

    INSERT INTO dbo.Products (ShopId, ProductName, Category, SubCategory, Brand, Description, Price, Mrp, Discount, Stock, Sku, StockStatus, ImageUrl, HasVariants, IsApproved, ApprovalStatus, CreatedDate, IsActive, IsDeleted)
    VALUES 
    (@ShopMob, 'Samsung Galaxy S24 Ultra 5G (Titanium Black, 256GB)', 'Mobiles', 'Smartphones', 'Samsung', 'Flagship AI smartphone with Snapdragon 8 Gen 3, 200MP camera and built-in S-Pen.', 119999.00, 134999.00, 11.00, 15, 'SAMS24U-BLK', 'InStock', 'https://images.unsplash.com/photo-1610945415295-d9bbf067e59c?w=500', 1, 1, 'Approved', DATEADD(month, -6, GETDATE()), 1, 0),
    (@ShopMob, 'Apple iPhone 15 Pro (128 GB) - Natural Titanium', 'Mobiles', 'Smartphones', 'Apple', 'Forged in titanium with A17 Pro chip, customizable Action button, and versatile 48MP camera.', 127990.00, 134900.00, 5.00, 12, 'APL15P-NAT', 'InStock', 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=500', 1, 1, 'Approved', DATEADD(month, -5, GETDATE()), 1, 0),
    (@ShopFash, 'Nike Air Max 270 Men Running Shoes (Triple Black)', 'Fashion', 'Footwear', 'Nike', 'Max Air 270 unit delivers unrivaled, all-day comfort. Woven and synthetic fabric on upper.', 8495.00, 11995.00, 29.00, 40, 'NIKE-AM270-01', 'InStock', 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=500', 1, 1, 'Approved', DATEADD(month, -5, GETDATE()), 1, 0),
    (@ShopComp, 'HP Pavilion 15 (13th Gen Intel Core i5, 16GB, 512GB SSD)', 'Computers', 'Laptops', 'HP', 'FHD micro-edge display, Intel Iris Xe graphics, backlit keyboard, Windows 11 + MSO 2021.', 62990.00, 74990.00, 16.00, 20, 'HP-PAV15-I5', 'InStock', 'https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=500', 0, 1, 'Approved', DATEADD(month, -4, GETDATE()), 1, 0),
    (@ShopComp, 'Dell XPS 15 9530 Laptop (13th Gen i7, 32GB RAM, 1TB SSD)', 'Computers', 'Laptops', 'Dell', 'Stunning 3.5K OLED touch display, NVIDIA RTX 4060, CNC machined aluminum chassis.', 189990.00, 219990.00, 13.00, 8, 'DELL-XPS15-OLED', 'InStock', 'https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=500', 0, 1, 'Approved', DATEADD(month, -4, GETDATE()), 1, 0),
    (@ShopGroc, 'Fortune Premium Kachi Ghani Pure Mustard Oil 1L Pouch', 'Grocery', 'Edible Oils', 'Fortune', 'Traditional cold pressed mustard oil rich in Omega 3 and natural antioxidants.', 145.00, 175.00, 17.00, 200, 'FORT-OIL-1L', 'InStock', 'https://images.unsplash.com/photo-1474979266404-7eaacbcd87c5?w=500', 0, 1, 'Approved', DATEADD(month, -3, GETDATE()), 1, 0),
    (@ShopGroc, 'Tata Sampann Unpolished Toor Dal / Arhar Dal 1kg', 'Grocery', 'Staples & Pulses', 'Tata', 'Protein rich unpolished toor dal without artificial color or polish coating.', 175.00, 210.00, 16.00, 150, 'TATA-DAL-1KG', 'InStock', 'https://images.unsplash.com/photo-1586201375761-83865001e31c?w=500', 0, 1, 'Approved', DATEADD(month, -3, GETDATE()), 1, 0),
    (@ShopElec, 'Sony Bravia 55 Inch 4K Ultra HD Smart LED Google TV', 'Electronics', 'Smart TVs', 'Sony', '4K Processor X1, Dolby Vision & Atmos, Google Assistant, Motionflow XR 200.', 57990.00, 79990.00, 27.00, 10, 'SONY-TV-55-4K', 'InStock', 'https://images.unsplash.com/photo-1593359677879-a4bb92f829d1?w=500', 0, 1, 'Approved', DATEADD(month, -2, GETDATE()), 1, 0);
END;
GO

-- 4.7 Riders
IF NOT EXISTS (SELECT 1 FROM dbo.Riders)
BEGIN
    INSERT INTO dbo.Riders (RiderName, PhoneNumber, Email, VehicleNumber, CurrentLatitude, CurrentLongitude, IsAvailable, CreatedDate, IsActive, IsDeleted)
    VALUES 
    ('Amit Kumar (Rider #101)', '9876599881', 'rider.amit@shopnext.com', 'BR-01-AB-1234', 25.6093, 85.1376, 1, GETDATE(), 1, 0),
    ('Ramesh Yadav (Rider #102)', '9876599882', 'rider.ramesh@shopnext.com', 'BR-01-CD-5678', 25.5941, 85.1584, 1, GETDATE(), 1, 0),
    ('Suresh Paswan (Rider #103)', '9876599883', 'rider.suresh@shopnext.com', 'BR-01-EF-9012', 25.6120, 85.1390, 1, GETDATE(), 1, 0);
END;
GO

-- 4.8 Coupons
IF NOT EXISTS (SELECT 1 FROM dbo.Coupons)
BEGIN
    INSERT INTO dbo.Coupons (Code, Description, DiscountType, DiscountValue, MinOrderAmount, MaxDiscountAmount, StartDate, ExpiryDate, UsageLimit, UsedCount, IsActive, IsDeleted)
    VALUES 
    ('WELCOME100', 'Flat ₹100 Off on your first order above ₹499', 'Flat', 100.00, 499.00, 100.00, GETDATE(), DATEADD(month, 3, GETDATE()), 1000, 245, 1, 0),
    ('FESTIVE20', '20% Mega Festival Discount up to ₹1,000', 'Percent', 20.00, 999.00, 1000.00, GETDATE(), DATEADD(month, 2, GETDATE()), 500, 120, 1, 0),
    ('FREEDEL', 'Free Delivery on all orders above ₹299', 'Flat', 49.00, 299.00, 49.00, GETDATE(), DATEADD(month, 6, GETDATE()), 2000, 680, 1, 0);
END;
GO

-- 4.9 System Monitoring Initial Seed Data
IF NOT EXISTS (SELECT 1 FROM dbo.UserSessions)
BEGIN
    INSERT INTO dbo.UserSessions (SessionId, UserId, UserName, Role, IpAddress, Device, Browser, OperatingSystem, LoginTime, LastSeenTime, LastPageVisited, LastAction, IsActive, CreatedDate)
    VALUES
    (REPLACE(NEWID(), '-', ''), 1, 'Platform Administrator', 'Admin', '192.168.1.100', 'Desktop', 'Chrome', 'Windows 11', DATEADD(minute, -45, GETDATE()), DATEADD(minute, -1, GETDATE()), '/Admin/SystemMonitoring', 'Dashboard Audit Inspection', 1, GETDATE()),
    (REPLACE(NEWID(), '-', ''), 2, 'Patna Central Grocery', 'Seller', '192.168.1.105', 'Desktop', 'Edge', 'Windows 10', DATEADD(minute, -30, GETDATE()), DATEADD(minute, -3, GETDATE()), '/Vendor/Orders', 'Packed Order #1024', 1, GETDATE()),
    (REPLACE(NEWID(), '-', ''), 1, 'Rider Ajay', 'Rider', '192.168.1.110', 'Mobile', 'Chrome Mobile', 'Android 14', DATEADD(minute, -20, GETDATE()), DATEADD(minute, -2, GETDATE()), '/Rider/Dashboard', 'GPS Route Navigation', 1, GETDATE());
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.LoginHistories)
BEGIN
    INSERT INTO dbo.LoginHistories (UserId, UserName, Role, LoginTime, LastActivityTime, IpAddress, Browser, Device, OperatingSystem, IsSuccessful, IsActiveSession, CreatedDate)
    VALUES
    (1, 'Platform Administrator', 'Admin', DATEADD(hour, -3, GETDATE()), DATEADD(minute, -1, GETDATE()), '192.168.1.100', 'Chrome', 'Desktop', 'Windows 11', 1, 1, GETDATE()),
    (2, 'Patna Central Grocery', 'Seller', DATEADD(hour, -2, GETDATE()), DATEADD(minute, -3, GETDATE()), '192.168.1.105', 'Edge', 'Desktop', 'Windows 10', 1, 1, GETDATE()),
    (1, 'Rider Ajay', 'Rider', DATEADD(hour, -1, GETDATE()), DATEADD(minute, -2, GETDATE()), '192.168.1.110', 'Chrome Mobile', 'Mobile', 'Android 14', 1, 1, GETDATE()),
    (NULL, 'unknown_attacker', 'Admin', DATEADD(minute, -35, GETDATE()), DATEADD(minute, -35, GETDATE()), '203.0.113.55', 'Firefox', 'Desktop', 'Linux', 0, 0, GETDATE());
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.UserActivities)
BEGIN
    INSERT INTO dbo.UserActivities (UserId, UserName, Role, Action, Module, Entity, EntityId, Description, Timestamp, IpAddress, Device, Browser, CreatedDate, IsActive)
    VALUES
    (1, 'Platform Administrator', 'Admin', 'APPROVE', 'Seller', 'Shop #1', 1, 'Admin approved store registration for Patna Central Grocery', DATEADD(hour, -2, GETDATE()), '192.168.1.100', 'Desktop', 'Chrome', GETDATE(), 1),
    (2, 'Patna Central Grocery', 'Seller', 'CREATE', 'Product', 'Product #101', 101, 'Seller added new catalog item Fresh Organic Apples', DATEADD(hour, -1, GETDATE()), '192.168.1.105', 'Desktop', 'Edge', GETDATE(), 1),
    (2, 'Patna Central Grocery', 'Seller', 'STATUS_CHANGE', 'Order', 'Order #1024', 1024, 'Seller packed order #1024 and requested Rider dispatch', DATEADD(minute, -25, GETDATE()), '192.168.1.105', 'Desktop', 'Edge', GETDATE(), 1),
    (1, 'Rider Ajay', 'Rider', 'ASSIGN', 'Order', 'Order #1024', 1024, 'Rider accepted delivery trip for Order #1024', DATEADD(minute, -15, GETDATE()), '192.168.1.110', 'Mobile', 'Chrome Mobile', GETDATE(), 1);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.EntityChangeLogs)
BEGIN
    INSERT INTO dbo.EntityChangeLogs (EntityName, EntityId, Action, FieldName, OldValue, NewValue, ChangedByUserId, ChangedByUserName, ChangedByUserRole, Timestamp, IpAddress, CreatedDate, IsActive)
    VALUES
    ('Order', 1024, 'UPDATE', 'OrderStatus', 'Accepted', 'Packed', 2, 'Patna Central Grocery', 'Seller', DATEADD(minute, -25, GETDATE()), '192.168.1.105', GETDATE(), 1),
    ('Order', 1024, 'UPDATE', 'RiderId', '(null)', '1 (Rider Ajay)', 1, 'System Dispatcher', 'System', DATEADD(minute, -15, GETDATE()), '127.0.0.1', GETDATE(), 1),
    ('Product', 101, 'UPDATE', 'Price', '₹120.00', '₹99.00', 2, 'Patna Central Grocery', 'Seller', DATEADD(minute, -50, GETDATE()), '192.168.1.105', GETDATE(), 1);
END;
GO

-- 4.10 Data integrity reconciliation
-- Keep inventory status derived from quantity so checkout and dashboards agree.
UPDATE dbo.Products
SET StockStatus = CASE
    WHEN Stock <= 0 THEN 'OutOfStock'
    WHEN Stock < 5 THEN 'LowStock'
    ELSE 'InStock'
END
WHERE IsDeleted = 0;
GO

-- Repair legacy demo orders that were created without a line item.
-- This is idempotent and only fills an order whose shop has an active product.
IF EXISTS (
    SELECT 1
    FROM dbo.Orders o
    WHERE o.IsDeleted = 0
      AND NOT EXISTS (SELECT 1 FROM dbo.OrderItems oi WHERE oi.OrderId = o.Id)
)
BEGIN
    DECLARE @RepairOrderId INT;
    DECLARE @RepairProductId INT;
    DECLARE @RepairPrice DECIMAL(18,2);

    SELECT TOP 1
        @RepairOrderId = o.Id,
        @RepairProductId = p.Id,
        @RepairPrice = p.Price
    FROM dbo.Orders o
    INNER JOIN dbo.Products p ON p.ShopId = o.ShopId
    WHERE o.IsDeleted = 0
      AND p.IsDeleted = 0
      AND p.IsActive = 1
    ORDER BY o.Id, p.Id;

    IF @RepairOrderId IS NOT NULL AND @RepairProductId IS NOT NULL
    BEGIN
        INSERT INTO dbo.OrderItems
            (OrderId, ProductId, Quantity, UnitPrice, TotalPrice, Sku, CreatedDate, IsActive, IsDeleted)
        VALUES
            (@RepairOrderId, @RepairProductId, 1, @RepairPrice, @RepairPrice,
             (SELECT Sku FROM dbo.Products WHERE Id = @RepairProductId), GETDATE(), 1, 0);

        UPDATE dbo.Orders
        SET TotalAmount = @RepairPrice
        WHERE Id = @RepairOrderId;
    END;
END;
GO

PRINT 'ShopNext SQL Server Master Schema, Stored Procedures & Seed Data Successfully Deployed!';
GO
