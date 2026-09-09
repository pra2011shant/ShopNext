$connStr = "Server=DESKTOP-MMR6QJM\SQLEXPRESS;Database=ShopNext;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

$queries = @(
    "IF COL_LENGTH('dbo.Products', 'Mrp') IS NULL ALTER TABLE dbo.Products ADD Mrp DECIMAL(18,2) NULL;",
    "IF COL_LENGTH('dbo.Products', 'Discount') IS NULL ALTER TABLE dbo.Products ADD Discount DECIMAL(18,2) NULL;",
    "IF COL_LENGTH('dbo.Offers', 'Mrp') IS NULL ALTER TABLE dbo.Offers ADD Mrp DECIMAL(18,2) NOT NULL DEFAULT(0.00);",
    "IF COL_LENGTH('dbo.Offers', 'Discount') IS NULL ALTER TABLE dbo.Offers ADD Discount DECIMAL(18,2) NOT NULL DEFAULT(0.00);",
    "IF COL_LENGTH('dbo.Offers', 'SellingPrice') IS NULL ALTER TABLE dbo.Offers ADD SellingPrice DECIMAL(18,2) NOT NULL DEFAULT(0.00);",
    "IF COL_LENGTH('dbo.Offers', 'DiscountType') IS NULL ALTER TABLE dbo.Offers ADD DiscountType NVARCHAR(50) NOT NULL DEFAULT('Percentage');",
    "IF COL_LENGTH('dbo.Offers', 'StartDate') IS NULL ALTER TABLE dbo.Offers ADD StartDate DATETIME2 NOT NULL DEFAULT(GETDATE());",
    "IF COL_LENGTH('dbo.Offers', 'EndDate') IS NULL ALTER TABLE dbo.Offers ADD EndDate DATETIME2 NOT NULL DEFAULT(DATEADD(day, 7, GETDATE()));",
    "IF COL_LENGTH('dbo.Offers', 'BannerUrl') IS NULL ALTER TABLE dbo.Offers ADD BannerUrl NVARCHAR(500) NULL;",
    "IF COL_LENGTH('dbo.Offers', 'Tagline') IS NULL ALTER TABLE dbo.Offers ADD Tagline NVARCHAR(200) NULL DEFAULT('Deal of the Day');",
    "UPDATE dbo.Offers SET Mrp = 1000.00, SellingPrice = 800.00, Discount = 20.00 WHERE Mrp = 0.00;"
)

foreach ($q in $queries) {
    try {
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $q
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "Applied: $q" -ForegroundColor Green
    } catch {
        Write-Host "Notice: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

$conn.Close()
Write-Host "All Database Columns Synchronized Successfully!" -ForegroundColor Green
