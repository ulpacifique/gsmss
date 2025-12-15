using CommunityFinanceAPI.Data;
using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Models.Entities;
using CommunityFinanceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommunityFinanceAPI.Services.Implementations
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;

        public PermissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UserHasPermissionAsync(int userId, string permissionName)
        {
            // Admins have all permissions by default
            var user = await _context.Users.FindAsync(userId);
            if (user?.Role == "Admin")
                return true;

            // Check if user has the specific permission
            var hasPermission = await _context.UserPermissions
                .Include(up => up.Permission)
                .AnyAsync(up => up.UserId == userId 
                    && up.Permission.PermissionName == permissionName 
                    && up.IsGranted 
                    && up.Permission.IsActive);

            return hasPermission;
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user?.Role == "Admin")
            {
                // Return all active permissions for admins
                return await _context.Permissions
                    .Where(p => p.IsActive)
                    .Select(p => p.PermissionName)
                    .ToListAsync();
            }

            return await _context.UserPermissions
                .Include(up => up.Permission)
                .Where(up => up.UserId == userId && up.IsGranted && up.Permission.IsActive)
                .Select(up => up.Permission.PermissionName)
                .ToListAsync();
        }

        public async Task<bool> GrantPermissionAsync(int userId, int permissionId, int grantedBy)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var permission = await _context.Permissions.FindAsync(permissionId);
            if (permission == null)
                throw new KeyNotFoundException("Permission not found");

            // Check if permission already exists
            var existing = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);

            if (existing != null)
            {
                existing.IsGranted = true;
                existing.GrantedAt = DateTime.UtcNow;
                existing.GrantedBy = grantedBy;
                _context.UserPermissions.Update(existing);
            }
            else
            {
                var userPermission = new UserPermission
                {
                    UserId = userId,
                    PermissionId = permissionId,
                    IsGranted = true,
                    GrantedAt = DateTime.UtcNow,
                    GrantedBy = grantedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserPermissions.Add(userPermission);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RevokePermissionAsync(int userId, int permissionId)
        {
            var userPermission = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);

            if (userPermission == null)
                throw new KeyNotFoundException("User permission not found");

            userPermission.IsGranted = false;
            userPermission.UpdatedAt = DateTime.UtcNow;
            _context.UserPermissions.Update(userPermission);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<PermissionResponse>> GetAllPermissionsAsync()
        {
            return await _context.Permissions
                .Where(p => p.IsActive)
                .Select(p => new PermissionResponse
                {
                    PermissionId = p.PermissionId,
                    PermissionName = p.PermissionName,
                    Description = p.Description,
                    Category = p.Category,
                    IsActive = p.IsActive
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<UserPermissionResponse>> GetUserPermissionsDetailedAsync(int userId)
        {
            return await _context.UserPermissions
                .Include(up => up.Permission)
                .Include(up => up.GrantedByUser)
                .Where(up => up.UserId == userId)
                .Select(up => new UserPermissionResponse
                {
                    UserPermissionId = up.UserPermissionId,
                    UserId = up.UserId,
                    UserName = $"{up.User.FirstName} {up.User.LastName}",
                    PermissionId = up.PermissionId,
                    PermissionName = up.Permission.PermissionName,
                    PermissionCategory = up.Permission.Category,
                    IsGranted = up.IsGranted,
                    GrantedAt = up.GrantedAt,
                    GrantedBy = up.GrantedBy,
                    GrantedByName = up.GrantedByUser != null 
                        ? $"{up.GrantedByUser.FirstName} {up.GrantedByUser.LastName}" 
                        : null
                })
                .ToListAsync();
        }
    }
}


