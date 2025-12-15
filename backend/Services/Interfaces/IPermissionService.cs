using CommunityFinanceAPI.Models.DTOs;

namespace CommunityFinanceAPI.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> UserHasPermissionAsync(int userId, string permissionName);
        Task<IEnumerable<string>> GetUserPermissionsAsync(int userId);
        Task<bool> GrantPermissionAsync(int userId, int permissionId, int grantedBy);
        Task<bool> RevokePermissionAsync(int userId, int permissionId);
        Task<IEnumerable<PermissionResponse>> GetAllPermissionsAsync();
        Task<IEnumerable<UserPermissionResponse>> GetUserPermissionsDetailedAsync(int userId);
    }
}


