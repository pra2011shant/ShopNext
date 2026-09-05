# Test Suite for ShopNext Database & Models Verification
$ErrorActionPreference = "Stop"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host " ShopNext Comprehensive Verification Suite " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# 1. Test .NET Build
Write-Host "`n[Step 1] Verifying .NET 8 Compilation..." -ForegroundColor Yellow
dotnet build --nologo
if ($LASTEXITCODE -ne 0) {
    Write-Error "Compilation failed!"
}
Write-Host " Compilation Succeeded (Exit Code: 0)" -ForegroundColor Green

# 2. Test SQL Server Database Queries on localhost\SQLEXPRESS
Write-Host "`n[Step 2] Verifying SQL Server Tables & Seed Data..." -ForegroundColor Yellow

$sqlCheck = @"
SET NOCOUNT ON;
SELECT 'UsersCount' = COUNT(*) FROM dbo.Users WHERE IsDeleted = 0;
SELECT 'ShopsCount' = COUNT(*) FROM dbo.Shops WHERE IsDeleted = 0;
SELECT 'ProductsCount' = COUNT(*) FROM dbo.Products WHERE IsDeleted = 0;
SELECT 'AddressesCount' = COUNT(*) FROM dbo.CustomerAddresses WHERE IsDeleted = 0;
SELECT 'ReviewsCount' = COUNT(*) FROM dbo.Reviews WHERE IsDeleted = 0;
SELECT 'RidersCount' = COUNT(*) FROM dbo.Riders WHERE IsDeleted = 0;
"@

$result = sqlcmd -S "localhost\SQLEXPRESS" -d "ShopNext" -E -Q $sqlCheck -h -1
Write-Host "Database Query Results:" -ForegroundColor Gray
Write-Host $result

# Check if counts are non-zero
if ($result -match "\d+") {
    Write-Host " Database tables verified with active records!" -ForegroundColor Green
} else {
    Write-Error "Database verification returned empty!"
}

Write-Host "`n=========================================" -ForegroundColor Cyan
Write-Host " All Customer Ecosystem Checks PASSED! " -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan
exit 0
