# This script resets the EF Core database to a fresh state.

$projectPath = "src\OpenRP.Boilerplate"
$migrationsPath = "$projectPath\Migrations"
$buildPath = "src\OpenRP.Boilerplate\Server\gamemode\Debug\net6.0"
$initialMigrationName = "InitialCreate"

Write-Host "Checking for EF Core CLI tool..."
$efInstalled = dotnet tool list -g | Select-String "dotnet-ef"

if (-not $efInstalled) {
    Write-Host "EF Core CLI not found. Installing..."
    dotnet tool install --global dotnet-ef
} else {
    Write-Host "EF Core CLI found. Updating to ensure latest version..."
    dotnet tool update --global dotnet-ef
}

# Navigate to the project path
Set-Location -Path $projectPath

# Build the project
Write-Host "Building the project..."
dotnet build --configuration Debug

# Ensure the migrations folder exists inside the project
if (Test-Path $migrationsPath) {
    Write-Host "Removing existing migrations..."
    Remove-Item -Recurse -Force $migrationsPath
}
New-Item -ItemType Directory -Path $migrationsPath | Out-Null

# Drop the database
Write-Host "Dropping the database..."
dotnet ef database drop --force --verbose

# Ensure no existing migrations interfere
Write-Host "Removing any existing migrations (if any)..."
dotnet ef migrations remove --force

# Create a fresh migration (inside the default project migrations folder)
Write-Host "Creating a new migration..."
dotnet ef migrations add $initialMigrationName

# Apply the migration
Write-Host "Applying migration to recreate the database..."
dotnet ef database update

Write-Host "Database reset complete!"

# Restore original working directory
Set-Location -Path $PSScriptRoot