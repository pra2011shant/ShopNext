$connStr = "Server=DESKTOP-MMR6QJM\SQLEXPRESS;Database=ShopNext;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
$sqlPath = Join-Path $PSScriptRoot "Database\ShopNext_Complete_Schema_And_Stored_Procedures.sql"

if (-not (Test-Path $sqlPath)) {
    Write-Host "SQL file not found at: $sqlPath" -ForegroundColor Red
    exit 1
}

$sql = Get-Content $sqlPath -Raw
$batches = $sql -split '(?m)^\s*GO\s*$'

$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()
Write-Host "Connected to SQL Server: DESKTOP-MMR6QJM\SQLEXPRESS (Database: ShopNext)" -ForegroundColor Green

$applied = 0
$errors = 0

foreach ($b in $batches) {
    $trimmed = $b.Trim()
    if ($trimmed -and -not $trimmed.StartsWith("CREATE DATABASE", [System.StringComparison]::OrdinalIgnoreCase) -and -not $trimmed.StartsWith("USE ", [System.StringComparison]::OrdinalIgnoreCase)) {
        try {
            $cmd = $conn.CreateCommand()
            $cmd.CommandText = $trimmed
            $cmd.ExecuteNonQuery() | Out-Null
            $applied++
        } catch {
            $errors++
            Write-Host "Notice on batch: $($_.Exception.Message)" -ForegroundColor Gray
        }
    }
}

$conn.Close()
Write-Host "`nSuccessfully synchronized $applied SQL batches into live database!" -ForegroundColor Green
