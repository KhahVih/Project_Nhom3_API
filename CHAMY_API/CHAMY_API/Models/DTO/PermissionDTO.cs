namespace CHAMY_API.Models.DTO
{
    public class PermissionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<PermissionRoleDTO> PermissionRoles { get; set; }
    }
}
