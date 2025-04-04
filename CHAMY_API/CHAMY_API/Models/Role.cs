namespace CHAMY_API.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<PermissionRole> PermissionRoles { get; set; } = new();
    }
}
