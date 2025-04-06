using System.ComponentModel.DataAnnotations.Schema;

namespace CHAMY_API.Models
{
    public class PermissionRole
    {
        public int PermissionId { get; set; }
        public Permission Permission { get; set; }

        [ForeignKey("Role")]
        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}
