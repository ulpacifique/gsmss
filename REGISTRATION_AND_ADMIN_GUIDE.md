# Registration and Admin Account Guide

## ✅ What I Fixed:

1. **Admin Account Creation**: Registration now automatically creates Admin accounts if the email:
   - Contains "admin" (case-insensitive)
   - Ends with "@admin.com"
   - Ends with "@community.com"
   
   Otherwise, users are registered as "Member"

2. **Better Error Messages**: Registration errors now show more detailed messages

3. **Login Instructions**: Added helpful hints on the login page about existing admin accounts

## 🔐 How to Create Accounts:

### Member Account (Default):
- Use any regular email (e.g., `user@example.com`, `john@gmail.com`)
- Will be registered as **Member** role

### Admin Account:
- Use an email containing "admin" (e.g., `admin@example.com`, `myadmin@gmail.com`)
- OR use an email ending with "@admin.com" (e.g., `test@admin.com`)
- OR use an email ending with "@community.com" (e.g., `newadmin@community.com`)
- Will be registered as **Admin** role

## 📝 Existing Admin Accounts:

The following admin accounts are already created in the database:
- **Email**: `newadmin@community.com`
- **Password**: `Admin@123`

You can also use:
- **Email**: `cedro@gmail.com` (if it exists in your database)
- **Password**: `cedro@123`

## 🚀 How to Use:

1. **To Register a New Admin**:
   - Go to the registration page
   - Use an email like `youradmin@community.com` or `admin@example.com`
   - Fill in all fields and submit
   - You'll be automatically registered as Admin

2. **To Login as Admin**:
   - Go to the login page
   - Use: `newadmin@community.com` / `Admin@123`
   - Or use any admin account you created

3. **To Register a Regular Member**:
   - Go to the registration page
   - Use any regular email (not containing "admin" or ending with "@admin.com" or "@community.com")
   - Fill in all fields and submit
   - You'll be registered as Member

## 🔧 Troubleshooting:

### Registration Fails:
1. Check browser console (F12) for detailed error messages
2. Make sure backend is running on `http://localhost:5154`
3. Verify the email isn't already registered
4. Check that password is at least 6 characters

### Can't Login:
1. Verify credentials are correct
2. Check that the user account is active in the database
3. Make sure backend is running and connected to database

## 📍 Next Steps:

1. **Restart the backend** to apply the changes:
   ```powershell
   cd D:\Dotnet\CommunityFinanceAPI\CommunityFinanceAPI
   dotnet run --launch-profile http
   ```

2. **Test Registration**:
   - Try registering with `testadmin@community.com` - should create Admin
   - Try registering with `testuser@gmail.com` - should create Member

3. **Test Login**:
   - Login with `newadmin@community.com` / `Admin@123`
   - You should see Admin features in the dashboard


