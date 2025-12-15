namespace CommunityFinanceAPI.Models.DTOs
{
    public class PermissionResponse
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class UserPermissionResponse
    {
        public int UserPermissionId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string PermissionCategory { get; set; } = string.Empty;
        public bool IsGranted { get; set; }
        public DateTime? GrantedAt { get; set; }
        public int? GrantedBy { get; set; }
        public string? GrantedByName { get; set; }
    }

    public class GrantPermissionRequest
    {
        public int PermissionId { get; set; }
    }

    public class RevokePermissionRequest
    {
        public int PermissionId { get; set; }
    }
}


