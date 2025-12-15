# How to Push Your Project to GitHub

## Prerequisites
- Git installed on your computer
- GitHub account (you have: https://github.com/Ruth-IGR/GSMS)
- Your project folder: `D:\Dotnet\CommunityFinanceAPI`

## Step-by-Step Instructions

### Step 1: Initialize Git Repository (if not already done)

Open PowerShell or Command Prompt and navigate to your project folder:

```powershell
cd D:\Dotnet\CommunityFinanceAPI
```

Check if Git is already initialized:
```powershell
git status
```

If you see "fatal: not a git repository", initialize it:
```powershell
git init
```

### Step 2: Add Remote Repository

Add your GitHub repository as the remote origin:

```powershell
git remote add origin https://github.com/Ruth-IGR/GSMS.git
```

If you already have a remote, update it:
```powershell
git remote set-url origin https://github.com/Ruth-IGR/GSMS.git
```

Verify the remote:
```powershell
git remote -v
```

### Step 3: Stage All Files

Add all files to staging (the `.gitignore` will automatically exclude ignored files):

```powershell
git add .
```

Check what will be committed:
```powershell
git status
```

### Step 4: Create Initial Commit

Commit all files:

```powershell
git commit -m "Initial commit: Community Finance API with React frontend"
```

### Step 5: Push to GitHub

Push to the main branch:

```powershell
git branch -M main
git push -u origin main
```

If you get an error about authentication, you may need to:
- Use a Personal Access Token instead of password
- Or use SSH: `git remote set-url origin git@github.com:Ruth-IGR/GSMS.git`

### Step 6: Verify Upload

Visit your repository: https://github.com/Ruth-IGR/GSMS

You should see all your files uploaded!

---

## For Future Updates

After making changes, use these commands:

```powershell
# 1. Check what changed
git status

# 2. Add changed files
git add .

# 3. Commit changes
git commit -m "Description of your changes"

# 4. Push to GitHub
git push
```

---

## Important Notes

### Files That Will Be Ignored (from .gitignore):
- ✅ `node_modules/` - Frontend dependencies (too large)
- ✅ `bin/`, `obj/` - Build outputs
- ✅ `.env` files - Environment variables (sensitive)
- ✅ `*.log` - Log files
- ✅ `.cursor/` - Cursor AI files
- ✅ Database files (`.db`, `.sqlite`)
- ✅ Visual Studio cache files

### Files That Will Be Included:
- ✅ All source code (`.cs`, `.tsx`, `.ts`, `.csproj`)
- ✅ Configuration files (`appsettings.json`, `package.json`)
- ✅ Migrations folder
- ✅ Frontend `src/` folder
- ✅ README.md and documentation

---

## Troubleshooting

### If you get "Authentication failed":
1. Generate a Personal Access Token on GitHub:
   - Go to GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)
   - Generate new token with `repo` permissions
   - Use the token as your password when pushing

### If you get "Repository not found":
- Check the repository URL is correct
- Make sure the repository exists on GitHub
- Verify you have access to the repository

### If you want to start fresh:
```powershell
# Remove existing git history (if needed)
rm -r .git
git init
git add .
git commit -m "Initial commit"
git remote add origin https://github.com/Ruth-IGR/GSMS.git
git push -u origin main --force
```

---

## Quick Reference Commands

```powershell
# Initialize repository
git init

# Add remote
git remote add origin https://github.com/Ruth-IGR/GSMS.git

# Stage files
git add .

# Commit
git commit -m "Your commit message"

# Push
git push -u origin main

# Check status
git status

# View remote
git remote -v
```

---

## Recommended: Create a README.md

Make sure you have a good README.md file (you already have one!) that includes:
- Project description
- Setup instructions
- How to run the backend and frontend
- Technology stack
- Features

Your README.md is already comprehensive! ✅


