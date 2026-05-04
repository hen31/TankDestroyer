$botsDir = (Get-Item -Path ".\" -Verbose).FullName

# Build TankDestroyer.API
$apiProject = Join-Path $botsDir "Source\TankDestroyer.API\TankDestroyer.API.csproj"
Write-Host "Building TankDestroyer.API..." -ForegroundColor Cyan
dotnet build $apiProject
if ($LASTEXITCODE -ne 0) {
    Write-Host "API build failed!" -ForegroundColor Red
    exit 1
}

# Build TankDestroyer.Engine
$engineProject = Join-Path $botsDir "Source\TankDestroyer.Engine\TankDestroyer.Engine.csproj"
Write-Host "Building TankDestroyer.Engine..." -ForegroundColor Cyan
dotnet build $engineProject
if ($LASTEXITCODE -ne 0) {
    Write-Host "Engine build failed!" -ForegroundColor Red
    exit 1
}

# Export Godot game for Windows
$godotExe = Join-Path $botsDir "Godot\Godot_v4.6.2-stable_mono_win64.exe"
$gameDir = Join-Path $botsDir "Game"
$exportPath = Join-Path $botsDir "Build\TankDestroyer.exe"

Write-Host "Exporting Godot game for Windows..." -ForegroundColor Cyan
Push-Location $gameDir
& cmd.exe /c $godotExe --headless --export-debug "Windows Desktop" $exportPath
$exportExitCode = $LASTEXITCODE
Pop-Location

if ($exportExitCode -ne 0) {
    Write-Host "Godot export failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Godot export complete!" -ForegroundColor Green

# Start the TankDestroyer game
$exePath = Join-Path $botsDir "Build\TankDestroyer.exe"
if (Test-Path $exePath) {
    Write-Host "Starting TankDestroyer..." -ForegroundColor Yellow
    Start-Process $exePath
} else {
    Write-Host "TankDestroyer.exe not found at: $exePath" -ForegroundColor Red
}
