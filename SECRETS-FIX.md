# Fix GitHub push (secret in commit)

GitHub blocked your push because **commit 183ddb7** contained an Azure Storage key in `bin/Debug/net10.0/appsettings.json`.

## What was done in the repo

1. **`.gitignore`** was added so `bin/`, `obj/`, `*.db`, and similar build/local files are never committed again.
2. **`appsettings.json`** was updated: the Azure `ConnectionString` value was removed (left empty) so no secret is stored in the repo.

## Steps to run (in PowerShell, from the project folder)

Run these in `C:\Users\asus\desktop\arabella\arabella`:

```powershell
# 1. Stop tracking the file that contains the secret (removes it from the next commit)
git rm --cached "bin/Debug/net10.0/appsettings.json"

# 2. Stage the new .gitignore and the updated appsettings.json
git add .gitignore appsettings.json

# 3. Amend the last commit so it no longer contains the secret
git commit --amend --no-edit

# 4. Push (you are rewriting the last commit, so use force)
git push --force-with-lease
```

If you prefer to set a new commit message in step 3, use:
`git commit --amend -m "Your new message"` instead of `--no-edit`.

## After pushing: set the key locally

The app needs the Azure connection string at runtime. Set it with **User Secrets** (not in the repo):

```powershell
cd C:\Users\asus\desktop\arabella\arabella
dotnet user-secrets set "AzureStorage:ConnectionString" "DefaultEndpointsProtocol=https;AccountName=YOUR_ACCOUNT;AccountKey=YOUR_KEY;EndpointSuffix=core.windows.net"
```

Replace `YOUR_ACCOUNT` and `YOUR_KEY` with your real storage account name and key (e.g. from Azure Portal → Storage account → Access keys).

Optional:

```powershell
dotnet user-secrets set "AzureStorage:ContainerName" "pet-photos"
```

## If the secret is in an older commit

If commit 183ddb7 is **not** your latest commit, amending is not enough. You must remove the file from the commit that introduced it (e.g. interactive rebase or [BFG / git filter-repo](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/removing-sensitive-data-from-a-repository)).

## Rotate the exposed key

Because the key was in a commit, treat it as compromised. In **Azure Portal** → your Storage account → **Access keys** → **Regenerate** the key and update your user secret with the new connection string.
