namespace CHAMY_API.Models.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string Password { get; set; }
        public string? Email { get; set; }
        public bool IsAdmin { get; set; } = false;
        public List<UserPermissionDTO> UserPermissions { get; set; } = new List<UserPermissionDTO>();
    }
}
