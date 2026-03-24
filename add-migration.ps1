param(
    [Parameter(Mandatory=$true)]
    [string]$MigrationName
)

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Dual Migration Creator (SQL Server & PostgreSql)" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

if ([string]::IsNullOrWhiteSpace($MigrationName))
{
    Write-Host "ERROR: Migration name cannot be empty!" -ForegroundColor Red
    exit 1
}

Write-Host "Migration Name: $MigrationName" -ForegroundColor Yellow
Write-Host ""

Write-Host "[1/2] Creating SQL Server Migration..." -ForegroundColor Cyan
dotnet ef migrations add $MigrationName --context AppDbContext --project LMS.Infrastructure --startup-project LMS.API

if ($LASTEXITCODE -ne 0)
{
    Write-Host ""
    Write-Host "ERROR: Failed to create SQL Server migration!" -ForegroundColor Red
    exit 1
}

Write-Host "SUCCESS: SQL Server migration created" -ForegroundColor Green
Write-Host ""

Write-Host "[2/2] Creating PostgreSql Migration..." -ForegroundColor Cyan
dotnet ef migrations add $MigrationName --context PostgreSqlDbContext --output-dir Migrations/PostgreSql --project LMS.Infrastructure --startup-project LMS.API

if ($LASTEXITCODE -ne 0)
{
    Write-Host ""
    Write-Host "ERROR: Failed to create PostgreSql migration!" -ForegroundColor Red
    Write-Host ""
    Write-Host "SQL Server migration was created, but PostgreSql migration failed." -ForegroundColor Yellow
    exit 1
}

Write-Host "SUCCESS: PostgreSql migration created" -ForegroundColor Green
Write-Host ""

Write-Host "==================================" -ForegroundColor Green
Write-Host "COMPLETED!" -ForegroundColor Green
Write-Host "==================================" -ForegroundColor Green
Write-Host ""
Write-Host "Created migrations:" -ForegroundColor Cyan
Write-Host "  - SQL Server: LMS.Infrastructure/Migrations/$MigrationName..." -ForegroundColor White
Write-Host "  - PostgreSql: LMS.Infrastructure/Migrations/PostgreSql/$MigrationName..." -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Review the generated migrations" -ForegroundColor White
Write-Host "  2. Update Database (SqlServer):" -ForegroundColor White
Write-Host "     dotnet ef database update --context AppDbContext --project LMS.Infrastructure --startup-project LMS.API" -ForegroundColor White
Write-Host "  3. Update Database (PostgreSql):" -ForegroundColor White
Write-Host "     dotnet ef database update --context PostgreSqlDbContext --project LMS.Infrastructure --startup-project LMS.API" -ForegroundColor White
Write-Host ""
