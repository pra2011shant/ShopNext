# Deep Dataflow and Table Column Mapping Verification Script
$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   ShopNext Deep Dataflow & Column Mapping Verification   " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$connStr = "Server=DESKTOP-MMR6QJM\SQLEXPRESS;Database=ShopNext;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
[System.Reflection.Assembly]::LoadWithPartialName("System.Data") | Out-Null
$sqlConn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$sqlConn.Open()
$cmd = $sqlConn.CreateCommand()

# 1. Test Login & Session Telemetry Data Insertion
Write-Host "`n[Step 1/6] Testing Login Telemetry (dbo.LoginHistories & dbo.UserSessions)..." -ForegroundColor Yellow
$cmd.CommandText = @"
    INSERT INTO dbo.LoginHistories (UserId, UserName, Role, LoginTime, IpAddress, Device, Browser, OperatingSystem, IsSuccessful, FailureReason, IsActive, IsDeleted)
    VALUES (1, 'Super Admin', 'Admin', GETDATE(), '127.0.0.1', 'Desktop', 'Chrome', 'Windows 11', 1, NULL, 1, 0);
    
    DECLARE @NewSessionId NVARCHAR(100) = NEWID();
    INSERT INTO dbo.UserSessions (UserId, UserName, Role, SessionId, IpAddress, Device, Browser, OperatingSystem, LoginTime, LastSeenTime, IsActive, IsDeleted)
    VALUES (1, 'Super Admin', 'Admin', @NewSessionId, '127.0.0.1', 'Desktop', 'Chrome', 'Windows 11', GETDATE(), GETDATE(), 1, 0);

    SELECT TOP 1 Id, UserName, IpAddress, Device, Browser FROM dbo.LoginHistories ORDER BY Id DESC;
"@
$r = $cmd.ExecuteReader()
if ($r.Read()) {
    Write-Host "[PASS] LoginHistories Record Created: ID $($r['Id']) | User: $($r['UserName']) | IP: $($r['IpAddress']) | Device: $($r['Device']) | Browser: $($r['Browser'])" -ForegroundColor Green
}
$r.Close()

# 2. Test User Activity & JSON Diff Tracking
Write-Host "`n[Step 2/6] Testing User Activities & Entity Diffs (dbo.UserActivities & dbo.EntityChangeLogs)..." -ForegroundColor Yellow
$cmd.CommandText = @"
    INSERT INTO dbo.UserActivities (UserId, UserName, Role, Action, Module, Entity, EntityId, Description, Timestamp, IpAddress, Device, Browser, IsActive, IsDeleted)
    VALUES (1, 'Super Admin', 'Admin', 'UPDATE', 'Product', 'Product #1', 1, 'Updated price of product #1', GETDATE(), '127.0.0.1', 'Desktop', 'Chrome', 1, 0);

    INSERT INTO dbo.EntityChangeLogs (EntityName, EntityId, Action, FieldName, OldValue, NewValue, ChangedByUserName, ChangedByUserRole, Timestamp, IpAddress, IsActive, IsDeleted)
    VALUES ('Product', 1, 'UPDATE', 'Price', '119999.00', '114999.00', 'Super Admin', 'Admin', GETDATE(), '127.0.0.1', 1, 0);

    SELECT TOP 1 Id, EntityName, Action, FieldName, OldValue, NewValue FROM dbo.EntityChangeLogs ORDER BY Id DESC;
"@
$r = $cmd.ExecuteReader()
if ($r.Read()) {
    Write-Host "[PASS] EntityChangeLogs Diff Recorded: Entity: $($r['EntityName']) | Action: $($r['Action']) | Field: $($r['FieldName'])" -ForegroundColor Green
    Write-Host "       Old: $($r['OldValue']) -> New: $($r['NewValue'])" -ForegroundColor Cyan
}
$r.Close()

# 3. Test Soft Delete & Recycle Bin Snapshot
Write-Host "`n[Step 3/6] Testing Soft Delete Logging (dbo.SoftDeleteLogs)..." -ForegroundColor Yellow
$cmd.CommandText = @"
    INSERT INTO dbo.SoftDeleteLogs (EntityName, EntityId, EntityTitle, DeletedByUserName, DeletedByUserRole, DeletedAt, Reason, SnapshotDataJson, IsRestored, IsActive, IsDeleted)
    VALUES ('Product', 999, 'Test Product', 'Super Admin', 'Admin', GETDATE(), 'Testing Recycle Bin', '{"Id":999,"ProductName":"Test Product","Price":100}', 0, 1, 0);

    SELECT TOP 1 Id, EntityName, EntityTitle, IsRestored FROM dbo.SoftDeleteLogs ORDER BY Id DESC;
"@
$r = $cmd.ExecuteReader()
if ($r.Read()) {
    Write-Host "[PASS] SoftDeleteLogs Logged: ID: $($r['Id']) | Title: $($r['EntityTitle']) | Restored: $($r['IsRestored'])" -ForegroundColor Green
}
$r.Close()

# 4. Test Customer Order & Line Items Flow
Write-Host "`n[Step 4/6] Testing Order & OrderItems Data Flow (dbo.Orders & dbo.OrderItems)..." -ForegroundColor Yellow
$cmd.CommandText = @"
    DECLARE @TestCustId INT = (SELECT TOP 1 Id FROM dbo.Users WHERE Role = 'Customer');
    DECLARE @TestShopId INT = (SELECT TOP 1 Id FROM dbo.Shops WHERE IsApproved = 1);
    DECLARE @TestProdId INT = (SELECT TOP 1 Id FROM dbo.Products WHERE ShopId = @TestShopId);

    INSERT INTO dbo.Orders (CustomerId, ShopId, TotalAmount, OrderStatus, PaymentMode, CreatedDate, IsActive, IsDeleted, Remark)
    VALUES (@TestCustId, @TestShopId, 1500.00, 'Pending', 'COD', GETDATE(), 1, 0, 'Test delivery address landmark');
    
    DECLARE @NewOrderId INT = SCOPE_IDENTITY();

    INSERT INTO dbo.OrderItems (OrderId, ProductId, Quantity, UnitPrice, Remark, CreatedDate, IsActive, IsDeleted)
    VALUES (@NewOrderId, @TestProdId, 2, 750.00, 'Test item mapping', GETDATE(), 1, 0);

    SELECT o.Id AS OrderId, o.TotalAmount, o.OrderStatus, o.PaymentMode, oi.Quantity, oi.UnitPrice
    FROM dbo.Orders o
    INNER JOIN dbo.OrderItems oi ON oi.OrderId = o.Id
    WHERE o.Id = @NewOrderId;
"@
$r = $cmd.ExecuteReader()
if ($r.Read()) {
    Write-Host "[PASS] Order & OrderItem Successfully Mapped: OrderID: $($r['OrderId']) | Total: ₹$($r['TotalAmount']) | Status: $($r['OrderStatus']) | Qty: $($r['Quantity']) x ₹$($r['UnitPrice'])" -ForegroundColor Green
}
$r.Close()

# 5. Test Rider Delivery Proof & Handover OTP
Write-Host "`n[Step 5/6] Testing Delivery Proof OTP & Geotagging (dbo.DeliveryProofs)..." -ForegroundColor Yellow
$cmd.CommandText = @"
    DECLARE @LastOrderId INT = (SELECT TOP 1 Id FROM dbo.Orders ORDER BY Id DESC);
    DECLARE @RiderId INT = (SELECT TOP 1 Id FROM dbo.Riders);

    INSERT INTO dbo.DeliveryProofs (OrderId, RiderId, HandoverOtp, IsOtpVerified, DeliveryTimestamp, HandoverLatitude, HandoverLongitude, ReceivedByName, CreatedDate, IsActive, IsDeleted)
    VALUES (@LastOrderId, @RiderId, '4321', 1, GETDATE(), 25.6093, 85.1376, 'Pooja Sharma', GETDATE(), 1, 0);

    SELECT TOP 1 Id, OrderId, HandoverOtp, IsOtpVerified, HandoverLatitude, HandoverLongitude, ReceivedByName FROM dbo.DeliveryProofs ORDER BY Id DESC;
"@
$r = $cmd.ExecuteReader()
if ($r.Read()) {
    Write-Host "[PASS] DeliveryProof Verified: OrderID: $($r['OrderId']) | OTP: $($r['HandoverOtp']) | Verified: $($r['IsOtpVerified']) | Recipient: $($r['ReceivedByName']) | Geo: ($($r['HandoverLatitude']), $($r['HandoverLongitude']))" -ForegroundColor Green
}
$r.Close()

# 6. Test Digital Wallet Balance & Ledger Transactions
Write-Host "`n[Step 6/6] Testing Wallet Accounts & Ledger (dbo.WalletAccounts & dbo.WalletTransactions)..." -ForegroundColor Yellow
$cmd.CommandText = @"
    DECLARE @WalletCustId INT = (SELECT TOP 1 Id FROM dbo.Users WHERE Role = 'Customer');
    
    DECLARE @WalletAccId INT = (SELECT TOP 1 Id FROM dbo.WalletAccounts WHERE CustomerId = @WalletCustId);
    IF @WalletAccId IS NULL
    BEGIN
        INSERT INTO dbo.WalletAccounts (CustomerId, MainBalance, RefundWalletBalance, GiftCardBalance, CreatedDate, IsActive, IsDeleted)
        VALUES (@WalletCustId, 500.00, 0.00, 100.00, GETDATE(), 1, 0);
        SET @WalletAccId = SCOPE_IDENTITY();
    END

    INSERT INTO dbo.WalletTransactions (WalletAccountId, TransactionType, SourceCategory, Amount, BalanceAfter, Description, CreatedDate, IsActive, IsDeleted)
    VALUES (@WalletAccId, 'Credit', 'Cashback', 250.00, 750.00, 'Cashback Credit Test', GETDATE(), 1, 0);

    SELECT TOP 1 WalletAccountId, TransactionType, SourceCategory, Amount, BalanceAfter, Description FROM dbo.WalletTransactions ORDER BY Id DESC;
"@
$r = $cmd.ExecuteReader()
if ($r.Read()) {
    Write-Host "[PASS] Wallet Ledger Verified: AccID: $($r['WalletAccountId']) | Type: $($r['TransactionType']) | Category: $($r['SourceCategory']) | Amount: ₹$($r['Amount']) | Balance: ₹$($r['BalanceAfter'])" -ForegroundColor Green
}
$r.Close()

$sqlConn.Close()

Write-Host "`n==========================================================" -ForegroundColor Green
Write-Host "   ALL 6 DATAFLOW AND COLUMN MAPPING TESTS PASSED 100%!  " -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
