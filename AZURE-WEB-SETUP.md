# Azure Web App setup (ArabellaDB)

**Naming:** **ArabellaApp1** is the *resource group* and the *Storage account* name. **ArabellaDB** is the *App Service* (web app). Set environment variables and manage the site under **ArabellaDB**, not under ArabellaApp1.

## 1. Database on the web app

The deploy script **now includes your local `arabella.db`** in the zip when you run `.\deploy-azure.ps1` from the project folder (where `arabella.db` lives). So:

- **Next deploy:** Run `.\deploy-azure.ps1` again from the folder that contains your current `arabella.db`. The script will copy it into the package and your web app will get that data.
- **If you already deployed without the DB:** Run `.\deploy-azure.ps1` once more; your existing local `arabella.db` will be included and will overwrite the empty one on the server.

Make sure the project directory (same folder as `arabella.csproj`) contains the `arabella.db` you want on the web before running the script.

---

## 2. Pet photo upload (Azure Storage)

Photo upload fails with “Check Azure Storage settings” until the **web app** has the Azure Storage connection string. Your Azure subscription is fine; the app just needs these settings on the **App Service**.

### In Azure Portal

1. Open [Azure Portal](https://portal.azure.com) → search for **App Services** → select **ArabellaDB** (the App Service, not the resource group or storage account).
2. In the left menu go to **Settings** → **Environment variables**.
3. Under **App settings** (or the application settings section), click **+ Add** / **New application setting** and add:

   | Name | Value |
   |------|--------|
   | `AzureStorage__ConnectionString` | Your full Storage account connection string (from Storage account → Access keys → Connection string) |
   | `AzureStorage__ContainerName` | `pet-photos` (or the container name you use) |

   Use **double underscore** `__` in the name (not a single `_`). That maps to `AzureStorage:ConnectionString` in the app.

5. Click **Save** at the top, then **Continue** when prompted.
6. Restart the app: **Overview** → **Restart**.

After this, pet photo upload on the web app should work (same Storage account you use on your laptop).

### If you still get "Check Azure Storage settings"

1. **Exact names (two underscores):**
   - `AzureStorage__ConnectionString` (two `_` between AzureStorage and ConnectionString)
   - `AzureStorage__ContainerName` (two `_` between AzureStorage and ContainerName)  
   Not `AzureStorage_ConnectionString` (one underscore) and not in the "Connection strings" section — use **App settings** (the key/value list), not "Connection strings".

2. **Restart:** After saving, use **Overview → Restart** for ArabellaDB.

3. **Check what the app sees:** In Azure Portal → ArabellaDB → **Monitoring → Log stream** (or **Diagnose and solve problems**). Start the app, then look for the line:  
   `Azure Storage for pet photos: configured`  
   If it says `NOT configured`, the app is not reading the variable (wrong name or wrong section).

4. **Value:** For `AzureStorage__ConnectionString`, paste the full connection string from Azure Portal → your **Storage account** (e.g. ArabellaApp1 if that’s the storage account) → **Access keys** → **Connection string**. No extra spaces at the start or end.

---

## Summary

| Issue | What to do |
|-------|------------|
| Database not on web app | Run `.\deploy-azure.ps1` from the project folder that has your `arabella.db`. |
| Photo upload fails on web | Add `AzureStorage__ConnectionString` and `AzureStorage__ContainerName` in App Service **Configuration** → Application settings, then Save and Restart. |
