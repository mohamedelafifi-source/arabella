# Deploy Arabella app to Azure App Service (Windows)
# Usage: .\deploy-azure.ps1   (run in PowerShell from this folder)
# Prereqs: Azure CLI (az) installed and logged in: az login

$ErrorActionPreference = "Stop"

$APP_NAME = "ArabellaDB"
$RESOURCE_GROUP = "ArabellaApp1"
$PLAN_NAME = "ArabellaApp1PlanWin"   # Windows plan (use a different name if you had a Linux plan before)
$SKU = "B1"
$LOCATION = "canadacentral"   # Must match if resource group already exists

Set-Location $PSScriptRoot

# Clean publish folder first to avoid nested publish and "file in use" warnings
$publishPath = Join-Path $PSScriptRoot "publish"
if (Test-Path $publishPath) { Remove-Item -Recurse -Force $publishPath -ErrorAction SilentlyContinue }

Write-Host "=== Building and publishing (Windows x64, self-contained for .NET 10) ===" -ForegroundColor Cyan
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish

Write-Host "=== Creating deployment package ===" -ForegroundColor Cyan
# Include SQLite database so the web app has your data (run deploy from the folder that contains arabella.db)
$dbFile = Join-Path $PSScriptRoot "arabella.db"
if (Test-Path $dbFile) {
    Copy-Item $dbFile -Destination (Join-Path $publishPath "arabella.db") -Force
    Write-Host "  Included arabella.db in deploy package." -ForegroundColor Green
} else {
    Write-Host "  Warning: arabella.db not found in project folder; web app will start with an empty database." -ForegroundColor Yellow
}
$zipPath = Join-Path $PSScriptRoot "deploy.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path "$publishPath\*" -DestinationPath $zipPath

Write-Host "=== Ensuring Azure resources exist (Windows App Service) ===" -ForegroundColor Cyan
cmd /c "az group show --name $RESOURCE_GROUP 2>nul"
if ($LASTEXITCODE -ne 0) { cmd /c "az group create --name $RESOURCE_GROUP --location $LOCATION -o none 2>nul" }

# Windows plan (--is-linux false) — show output so we see if it fails
Write-Host "Creating or verifying App Service Plan '$PLAN_NAME'..." -ForegroundColor Cyan
# Omit --is-linux for Windows plan (Linux plan would use --is-linux)
$planOutput = cmd /c "az appservice plan create --resource-group $RESOURCE_GROUP --name $PLAN_NAME --sku $SKU 2>&1"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Plan create output:" -ForegroundColor Yellow
    Write-Host $planOutput
    # Plan might already exist; check
    cmd /c "az appservice plan show --resource-group $RESOURCE_GROUP --name $PLAN_NAME -o none 2>nul"
    if ($LASTEXITCODE -ne 0) { throw "App Service Plan $PLAN_NAME could not be created. See output above." }
}

# Create Web App on Windows (.NET 8 runtime slot; app is self-contained so .NET 10 runs)
# Run via cmd with runtime in double quotes so the pipe in DOTNET|8.0 is not interpreted by shell
Write-Host "Creating or verifying Web App '$APP_NAME'..." -ForegroundColor Cyan
# Windows runtime from: az webapp list-runtimes --os-type windows (use dotnet:10 to match app)
$createOutput = cmd /c "az webapp create --resource-group $RESOURCE_GROUP --plan $PLAN_NAME --name $APP_NAME --runtime dotnet:10 2>&1"
if ($LASTEXITCODE -ne 0) {
    cmd /c "az webapp show --resource-group $RESOURCE_GROUP --name $APP_NAME -o none 2>nul"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Web App create failed. Azure CLI output:" -ForegroundColor Red
        Write-Host $createOutput
        throw "Web App $APP_NAME could not be created. See output above for the Azure error."
    }
}
# Self-contained exe: tell App Service to run arabella.exe (via cmd to suppress Azure CLI stderr warning)
cmd /c "az webapp config set --resource-group $RESOURCE_GROUP --name $APP_NAME --startup-file arabella.exe -o none 2>nul"
# Match 64-bit publish (win-x64): turn off 32-bit worker so w3wp runs as 64-bit
cmd /c "az webapp config set --resource-group $RESOURCE_GROUP --name $APP_NAME --use-32bit-worker-process false -o none 2>nul"

Write-Host "=== Deploying zip to App Service ===" -ForegroundColor Cyan
cmd /c "az webapp deploy --resource-group $RESOURCE_GROUP --name $APP_NAME --src-path $zipPath --type zip"
if ($LASTEXITCODE -ne 0) { throw "Deploy failed. Check that $APP_NAME exists in resource group $RESOURCE_GROUP." }

Write-Host "=== Cleaning up ===" -ForegroundColor Cyan
Remove-Item -Recurse -Force ./publish -ErrorAction SilentlyContinue
Remove-Item -Force $zipPath -ErrorAction SilentlyContinue

Write-Host "=== Done ===" -ForegroundColor Green
Write-Host "App URL: https://${APP_NAME}.azurewebsites.net"
Write-Host "Azure portal: https://portal.azure.com -> Resource group: $RESOURCE_GROUP -> $APP_NAME"
Write-Host ""
Write-Host "For pet photo uploads, set App settings on the App Service $APP_NAME (not the resource group): Settings -> Environment variables -> App settings" -ForegroundColor Cyan
Write-Host "  AzureStorage__ConnectionString = (your Azure Storage connection string)" -ForegroundColor Gray
Write-Host "  AzureStorage__ContainerName    = pet-photos" -ForegroundColor Gray
Write-Host "Then save and restart the app." -ForegroundColor Cyan
