#!/bin/bash
# Deploy Arabella app to Azure App Service
# Usage: ./deploy-azure.sh   (run from this folder or pass project path)
# Prereqs: Azure CLI (az) installed and logged in: az login

set -e

APP_NAME="ArabelleDB"
RESOURCE_GROUP="ArabellaApp1"
PLAN_NAME="ArabellaApp1Plan"
SKU="B1"
LOCATION="eastus"

# Project folder (directory containing arabella.csproj)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR"
cd "$PROJECT_DIR"

echo "=== Building and publishing (self-contained for Linux) ==="
dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish

echo "=== Creating deployment package ==="
cd publish
zip -r ../deploy.zip . -q
cd ..

echo "=== Ensuring Azure resources exist ==="
# Create resource group if not exists
az group create --name "$RESOURCE_GROUP" --location "$LOCATION" -o none 2>/dev/null || true

# Create App Service Plan if not exists (Linux, for .NET)
az appservice plan create \
  --resource-group "$RESOURCE_GROUP" \
  --name "$PLAN_NAME" \
  --sku "$SKU" \
  --is-linux true \
  -o none 2>/dev/null || true

# Create Web App if not exists
az webapp create \
  --resource-group "$RESOURCE_GROUP" \
  --plan "$PLAN_NAME" \
  --name "$APP_NAME" \
  --runtime "DOTNET:8" \
  -o none 2>/dev/null || true

# Self-contained app: run the published executable
az webapp config set --resource-group "$RESOURCE_GROUP" --name "$APP_NAME" \
  --startup-file "./arabella" -o none 2>/dev/null || true

echo "=== Deploying zip to App Service ==="
az webapp deployment source config-zip \
  --resource-group "$RESOURCE_GROUP" \
  --name "$APP_NAME" \
  --src deploy.zip

echo "=== Cleaning up ==="
rm -rf publish deploy.zip

echo "=== Done ==="
echo "App URL: https://${APP_NAME}.azurewebsites.net"
echo "Azure portal: https://portal.azure.com → Resource group: $RESOURCE_GROUP → $APP_NAME"
