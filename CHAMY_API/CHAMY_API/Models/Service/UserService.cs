using CHAMY_API.Data;
using CHAMY_API.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace CHAMY_API.Models.Service
{
    public class UserService: IUserService
    {
        private readonly ApplicationDbContext _context;
        public UserService(ApplicationDbContext context)
        {
            context = _context;
        }
        public UserDTO Authenticate(string username, string password)
        {
            var user = _context.Users
            .Include(u => u.UserPermissions)
            .FirstOrDefault(u => u.UserName == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password)){
                return null;
            }
            return new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                IsAdmin = user.IsAdmin,
                UserPermissions = user.UserPermissions.Select(p => new UserPermissionDTO
                {
                    UserId = p.UserId,
                    PermissionId = p.PermissionId,
                    PermissionName = p.Permission.Name,
                }).ToList()
            };
        }

    }
    public interface IUserService
    {
        UserDTO Authenticate(string username, string password);
    }
}
