using Microsoft.AspNetCore.Identity;
using NEMS.Data;

namespace NEMS.Models
{
    public class RoleModel
    {
        public IEnumerable<UserRole>? UserRoles { get; set; }
        public List<IdentityRole>? Roles { get; set; }
        public UserRole? UserRole { get; set; }
    }
}
