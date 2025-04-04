namespace CHAMY_API.Models.DTO
{
    public class UserPermissionDTO
    {
        public int UserId { get; set; }
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty; // Lấy tên quyền
    }
}
