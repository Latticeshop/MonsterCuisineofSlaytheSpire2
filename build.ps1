# Monster Cuisine mod build script
# Output: build/MonsterCuisine.dll + build/MonsterCuisine.json
$ErrorActionPreference = "Stop"

# Use the shared NuGet cache explicitly (HOME is empty, so dotnet may default to a
# project-local .nuget folder otherwise).
$env:NUGET_PACKAGES = "C:\Users\Latticeshop\.nuget\packages"

dotnet build MonsterCuisine.csproj -c Debug -o build --nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build FAILED (exit=$LASTEXITCODE)" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "Build OK: build/MonsterCuisine.dll + build/MonsterCuisine.json" -ForegroundColor Green
