# Spire Delight mod build script
# Output: build/SpireDelight.dll + build/SpireDelight.json
$ErrorActionPreference = "Stop"

# Use the shared NuGet cache explicitly (HOME is empty, so dotnet may default to a
# project-local .nuget folder otherwise).
$env:NUGET_PACKAGES = "C:\Users\Latticeshop\.nuget\packages"

dotnet build SpireDelight.csproj -c Debug -o build --nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build FAILED (exit=$LASTEXITCODE)" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "Build OK: build/SpireDelight.dll + build/SpireDelight.json" -ForegroundColor Green
