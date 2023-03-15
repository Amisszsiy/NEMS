using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NEMS.Data;
using NEMS.Helper;
using NEMS.Models;

namespace NEMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;

        public RolesController(RoleManager<IdentityRole> roleManager, ApplicationDbContext db, UserManager<ApplicationUser> userManager, IUserService userService)
        {
            _roleManager = roleManager;
            _db = db;
            _userManager = userManager;
            _userService = userService;
        }
        public IActionResult Index()
        {
            RoleModel roleModel = new RoleModel();

            roleModel = getUserRole(roleModel);

            return View(roleModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> editRoleAsync(RoleModel editRole)
        {
            if (HttpContext.Request.Form["submit"] == "edit")
            {
                ApplicationUser editUser = _db.Users.SingleOrDefault(x => x.Id == editRole.UserRole.uid);
                await _userManager.RemoveFromRolesAsync(editUser, await _userManager.GetRolesAsync(editUser));
                await _userManager.AddToRoleAsync(editUser, editRole.UserRole.RoleName);
            }

            editRole = getUserRole(editRole);

            return View("Index", editRole);
        }

        public RoleModel getUserRole(RoleModel roleModel)
        {
            List<ApplicationUser> users = _db.Users.Where(x => x.Id != _userService.getCurrentUser().Id).ToList();
            List<IdentityRole> roles = _db.Roles.Where(x => x.Name != "Admin").ToList();
            List<IdentityUserRole<string>> userRoles = _db.UserRoles.ToList();

            IEnumerable<UserRole> UserRoles =
                from user in users
                join userRole in userRoles on user.Id equals userRole.UserId
                join role in roles on userRole.RoleId equals role.Id
                select new UserRole
                {
                    uid = user.Id,
                    UserName = user.FirstName,
                    rid = role.Id,
                    RoleName = role.Name
                };

            roleModel.UserRoles = UserRoles;
            roleModel.Roles = roles;

            return roleModel;
        }
    }
}
