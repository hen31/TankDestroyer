$botsDir = (Get-Item -Path ".\" -Verbose).FullName

# Find all .csproj files in the Bots folder and subfolders
$csprojFiles = Get-ChildItem -Path "$botsDir\Bots" -Filter "*.csproj" -Recurse -File

Write-Host "Found $($csprojFiles.Count) bot project(s) to build..."

foreach ($csproj in $csprojFiles) {
    Write-Host "Building: $($csproj.Name)" -ForegroundColor Cyan
    dotnet build $csproj.FullName
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed for $($csproj.Name)" -ForegroundColor Red
        exit 1
    }
}

Write-Host "All bot projects built successfully!" -ForegroundColor Green

# Start the TankDestroyer game
$exePath = Join-Path $botsDir "Build\TankDestroyer.exe"
if (Test-Path $exePath) {
    Write-Host "Starting TankDestroyer..." -ForegroundColor Yellow
    Start-Process $exePath
} else {
    Write-Host "TankDestroyer.exe not found at: $exePath" -ForegroundColor Red
}
