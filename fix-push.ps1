# Run each command separately (one at a time) in PowerShell from this folder:
# cd C:\Users\asus\desktop\arabella\arabella

git rm --cached "bin/Debug/net10.0/appsettings.json"
git add .gitignore appsettings.json
git commit --amend --no-edit
git push --force-with-lease
