-- ==============================================================================
-- SHOPNEXT COMPLETE DATABASE SETUP & STORED PROCEDURES (SQL SERVER)
-- ==============================================================================

SET NOCOUNT ON;

-- 1. USERS TABLE
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        PhoneNumber NVARCHAR(20) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        Email NVARCHAR(100) NULL,
        Password NVARCHAR(255) NULL,
        Role NVARCHAR(50) NOT NULL DEFAULT 'Customer',
        ProfilePhoto NVARCHAR(MAX) NULL,
        Gender NVARCHAR(50) NULL,
        DateOfBirth DATETIME2 NULL,
        RiskScore INT NOT NULL DEFAULT 15,
        RiskLevel NVARCHAR(50) NOT NULL DEFAULT 'Low',
        RiskFactorsJson NVARCHAR(MAX) NULL,
        IsCodDisabled BIT NOT NULL DEFAULT 0,
        IsFlaggedForReview BIT NOT NULL DEFAULT 0,
        RiskLastEvaluatedDate DATETIME2 NULL,
        RestrictionLevel NVARCHAR(50) NOT NULL DEFAULT 'Normal',
        RestrictionReason NVARCHAR(MAX) NULL,
        RestrictionAppliedDate DATETIME2 NULL,
        SuspendedUntilDate DATETIME2 NULL,
        IsReturnDisabled BIT NOT NULL DEFAULT 0,
        IsAccountSuspended BIT NOT NULL DEFAULT 0,
        DeviceFingerprintHash NVARCHAR(255) NULL,
        RegistrationIpMasked NVARCHAR(100) NULL,
        RestrictFirstOrderCoupons BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
END
GO

-- 2. SHOPS TABLE
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Shops')
BEGIN
    CREATE TABLE Shops (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ShopName NVARCHAR(200) NOT NULL,
        OwnerName NVARCHAR(100) NOT NULL,
        Category NVARCHAR(100) NOT NULL,
        Address NVARCHAR(MAX) NOT NULL,
        City NVARCHAR(100) NOT NULL DEFAULT 'Patna',
        State NVARCHAR(100) NOT NULL DEFAULT 'Bihar',
        Pincode NVARCHAR(20) NOT NULL DEFAULT '800001',
        Latitude DECIMAL(18,8) NOT NULL DEFAULT 25.5941,
        Longitude DECIMAL(18,8) NOT NULL DEFAULT 85.1376,
        PhoneNumber NVARCHAR(20) NOT NULL,
        Email NVARCHAR(100) NULL,
        Password NVARCHAR(255) NULL,
        OpeningTime NVARCHAR(50) NULL,
        ClosingTime NVARCHAR(50) NULL,
        IsApproved BIT NOT NULL DEFAULT 1,
        Rating FLOAT NOT NULL DEFAULT 4.8,
        ImageUrl NVARCHAR(MAX) NULL,
        BannerUrl NVARCHAR(MAX) NULL,
        GstNumber NVARCHAR(50) NULL,
        PanNumber NVARCHAR(50) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
END
GO

-- 3. PRODUCTS TABLE
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE Products (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ShopId INT NOT NULL,
        ProductName NVARCHAR(255) NOT NULL,
        Category NVARCHAR(100) NOT NULL DEFAULT 'General',
        SubCategory NVARCHAR(100) NULL,
        Brand NVARCHAR(100) NULL,
        Description NVARCHAR(MAX) NULL,
        Price DECIMAL(18,2) NOT NULL,
        Mrp DECIMAL(18,2) NULL,
        Discount DECIMAL(18,2) NULL,
        Stock INT NOT NULL DEFAULT 0,
        Sku NVARCHAR(100) NULL,
        StockStatus NVARCHAR(50) NOT NULL DEFAULT 'InStock',
        ImageUrl NVARCHAR(MAX) NOT NULL,
        ImageFront NVARCHAR(MAX) NULL,
        ImageBack NVARCHAR(MAX) NULL,
        ImageSide NVARCHAR(MAX) NULL,
        ImagePackaging NVARCHAR(MAX) NULL,
        HasVariants BIT NOT NULL DEFAULT 0,
        IsApproved BIT NOT NULL DEFAULT 1,
        ApprovalStatus NVARCHAR(50) NOT NULL DEFAULT 'Approved',
        RejectionReason NVARCHAR(MAX) NULL,
        ApprovedDate DATETIME2 NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
END
GO

-- 4. RIDERS TABLE
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Riders')
BEGIN
    CREATE TABLE Riders (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RiderName NVARCHAR(100) NOT NULL,
        PhoneNumber NVARCHAR(20) NOT NULL,
        Password NVARCHAR(255) NULL,
        VehicleType NVARCHAR(50) NOT NULL DEFAULT 'Bike',
        VehicleNumber NVARCHAR(50) NOT NULL DEFAULT 'BR-01-AB-1234',
        IsOnline BIT NOT NULL DEFAULT 1,
        CurrentLatitude DECIMAL(18,8) NOT NULL DEFAULT 25.5941,
        CurrentLongitude DECIMAL(18,8) NOT NULL DEFAULT 85.1376,
        TotalDeliveries INT NOT NULL DEFAULT 50,
        Rating FLOAT NOT NULL DEFAULT 4.9,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
END
GO

-- 5. ORDERS TABLE
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders')
BEGIN
    CREATE TABLE Orders (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId INT NOT NULL,
        ShopId INT NOT NULL,
        RiderId INT NULL,
        TotalAmount DECIMAL(18,2) NOT NULL,
        OrderStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        PaymentMode NVARCHAR(50) NOT NULL DEFAULT 'COD',
        PaymentStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        CancelReason NVARCHAR(500) NULL,
        ReturnReason NVARCHAR(500) NULL,
        DeliveredDate DATETIME2 NULL,
        DeliveryAddress NVARCHAR(MAX) NULL,
        ReturnStatus NVARCHAR(50) NULL,
        ReturnVerificationNotes NVARCHAR(MAX) NULL,
        ReturnRequestedDate DATETIME2 NULL,
        ReturnVerifiedDate DATETIME2 NULL,
        DeliveryOtp NVARCHAR(10) NULL,
        IsOtpVerified BIT NOT NULL DEFAULT 0,
        OtpVerifiedDate DATETIME2 NULL,
        DeliveredByRiderName NVARCHAR(100) NULL,
        ShopName NVARCHAR(200) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
END
GO

-- 6. ORDER ITEMS TABLE
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderItems')
BEGIN
    CREATE TABLE OrderItems (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL DEFAULT 1,
        UnitPrice DECIMAL(18,2) NOT NULL,
        TotalPrice DECIMAL(18,2) NOT NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
END
GO

-- 7. REVIEWS TABLE
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Reviews')
BEGIN
    CREATE TABLE Reviews (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId INT NOT NULL,
        ProductId INT NULL,
        ShopId INT NULL,
        OrderId INT NULL,
        Rating FLOAT NOT NULL DEFAULT 5.0,
        Comment NVARCHAR(MAX) NULL,
        IsHidden BIT NOT NULL DEFAULT 0,
        ModerationReason NVARCHAR(500) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
END
GO

-- 8. COUPONS TABLE
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Coupons')
BEGIN
    CREATE TABLE Coupons (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Code NVARCHAR(50) NOT NULL,
        Description NVARCHAR(255) NOT NULL,
        DiscountType NVARCHAR(50) NOT NULL DEFAULT 'Flat',
        DiscountValue DECIMAL(18,2) NOT NULL,
        MinOrderAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        MaxDiscountAmount DECIMAL(18,2) NULL,
        StartDate DATETIME2 NULL,
        ExpiryDate DATETIME2 NULL,
        UsageLimit INT NOT NULL DEFAULT 500,
        UsedCount INT NOT NULL DEFAULT 0,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
END
GO

-- 9. AUDIT LOGS TABLE
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE AuditLogs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NULL,
        UserName NVARCHAR(100) NULL,
        UserRole NVARCHAR(50) NULL,
        Action NVARCHAR(100) NOT NULL,
        EntityName NVARCHAR(100) NULL,
        EntityId INT NULL,
        Details NVARCHAR(2000) NULL,
        IpAddress NVARCHAR(100) NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
END
GO

-- 10. PINCODE SERVICEABILITIES TABLE
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PincodeServiceabilities')
BEGIN
    CREATE TABLE PincodeServiceabilities (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Pincode NVARCHAR(20) NOT NULL,
        City NVARCHAR(100) NOT NULL,
        State NVARCHAR(100) NOT NULL,
        IsServiceable BIT NOT NULL DEFAULT 1,
        IsCodAvailable BIT NOT NULL DEFAULT 1,
        EstimatedDeliveryDays INT NOT NULL DEFAULT 2,
        IsExpressAvailable BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
END
GO

-- STORED PROCEDURE: Get Nearby Shops
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetNearbyShops]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_GetNearbyShops]
GO

CREATE PROCEDURE [dbo].[sp_GetNearbyShops]
    @UserLat DECIMAL(18,8),
    @UserLng DECIMAL(18,8),
    @MaxDistanceKm FLOAT = 25.0
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        s.Id,
        s.ShopName,
        s.OwnerName,
        s.Category,
        s.Address,
        s.City,
        s.State,
        s.Pincode,
        s.Latitude,
        s.Longitude,
        s.PhoneNumber,
        s.Email,
        s.OpeningTime,
        s.ClosingTime,
        s.Rating,
        s.ImageUrl,
        s.BannerUrl,
        -- Haversine formula calculation for distance in KM
        ( 6371 * ACOS( 
            LEAST(1.0, GREATEST(-1.0,
                COS( RADIANS(@UserLat) ) * COS( RADIANS(s.Latitude) ) * 
                COS( RADIANS(s.Longitude) - RADIANS(@UserLng) ) + 
                SIN( RADIANS(@UserLat) ) * SIN( RADIANS(s.Latitude) )
            ))
        )) AS DistanceKm
    FROM Shops s
    WHERE s.IsActive = 1 
      AND s.IsDeleted = 0 
      AND s.IsApproved = 1
    ORDER BY DistanceKm ASC;
END
GO

-- STORED PROCEDURE: Get Admin Dashboard Metrics
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetAdminDashboardSummary]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[sp_GetAdminDashboardSummary]
GO

CREATE PROCEDURE [dbo].[sp_GetAdminDashboardSummary]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        (SELECT COUNT(*) FROM Users WHERE Role = 'Customer' AND IsDeleted = 0) AS TotalCustomers,
        (SELECT COUNT(*) FROM Shops WHERE IsDeleted = 0) AS TotalSellers,
        (SELECT COUNT(*) FROM Products WHERE IsDeleted = 0) AS TotalProducts,
        (SELECT COUNT(*) FROM Orders WHERE IsDeleted = 0) AS TotalOrders,
        (SELECT ISNULL(SUM(TotalAmount), 0) FROM Orders WHERE CAST(CreatedDate AS DATE) = CAST(GETDATE() AS DATE) AND PaymentStatus = 'Paid') AS TodaySales,
        (SELECT COUNT(*) FROM Shops WHERE IsApproved = 0 AND IsDeleted = 0) AS PendingSellerRequests,
        (SELECT COUNT(*) FROM Orders WHERE ReturnStatus = 'Return_Requested') AS PendingReturns;
END
GO
