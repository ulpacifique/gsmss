# Step-by-Step Setup Instructions

## ✅ What I've Fixed:

1. **Backend Port Configuration**: Changed from HTTPS (7279) to HTTP (5154)
2. **Frontend API URL**: Updated reports API to use port 5154
3. **AdminController**: Fixed authentication helper method
4. **Database Migration**: Created clean InitialCreate migration

## 🚀 How to Run Everything:

### Step 1: Stop Any Running Backend Processes

Open PowerShell and run:
```powershell
# Find and kill any running dotnet processes on port 5154
netstat -ano | findstr :5154
# Note the PID number, then:
taskkill /F /PID <PID_NUMBER>
```

Or simply close any terminal windows running `dotnet run`.

### Step 2: Start the Backend

```powershell
cd D:\Dotnet\CommunityFinanceAPI\CommunityFinanceAPI
dotnet run --launch-profile http
```

**Expected output:**
- You should see: `Now listening on: http://localhost:5154`
- Database migrations should complete successfully
- Admin and member users should be created

### Step 3: Start the Frontend (in a NEW terminal)

```powershell
cd D:\Dotnet\CommunityFinanceAPI\frontend
npm run dev
```

**Expected output:**
- Frontend should start on `http://localhost:5173/`

### Step 4: Test the Connection

1. Open your browser to `http://localhost:5173/`
2. Login with:
   - **Admin**: `newadmin@community.com` / `Admin@123`
   - **Member**: `newmember@community.com` / `Member@123`
   - **Or**: `cedro@gmail.com` / `cedro@123`

### Step 5: Verify Features Work

✅ **Creating Contributions**: Go to Contributions page, select a goal, enter amount, submit
✅ **Editing Goals** (Admin): Go to Admin > Goals, edit any goal
✅ **Requesting Loans** (Member): Go to Dashboard, click "Request Loan" button

## 📍 Where to Find Features:

- **Loans**: Available in the **Dashboard** page (not a separate menu item)
  - Click "Request Loan" button to request
  - View your loans in the "Loan Management" section
  
- **Reports**: Available in **Admin > Reports** page
  - Export contributions, members, goals
  - View financial and audit reports

## 🔧 If You Still See Issues:

1. **Backend won't start**: Make sure no other process is using port 5154
2. **Frontend can't connect**: Check browser console (F12) for errors
3. **Database errors**: The app will create the database automatically on first run

## 📝 Notes:

- The backend runs on **HTTP** (not HTTPS) on port **5154**
- The frontend runs on port **5173**
- All API calls from frontend go to `http://localhost:5154`
- Loans and Reports features ARE implemented - they're just in different places than you might expect


