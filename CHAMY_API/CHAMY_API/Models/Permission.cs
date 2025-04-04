namespace CHAMY_API.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        // Thuộc tính điều hướng
        public List<UserPermission> UserPermissions { get; set; }
        public List<PermissionRole> PermissionRoles { get; set; } = new List<PermissionRole>();
    }
}

