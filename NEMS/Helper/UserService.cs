using Microsoft.AspNetCore.Identity;
using NEMS.Data;

namespace NEMS.Helper
{
    public interface IUserService
    {
        ApplicationUser getCurrentUser();
    }
    public class UserService: IUserService
    {
        private IHttpContextAccessor _contextAccessor;
        private ApplicationDbContext _db;
        private UserManager<ApplicationUser> _userManager;

        private ApplicationUser _currentUser;
        public UserService(IHttpContextAccessor contextAccessor, ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _contextAccessor = contextAccessor;
            _db = db;
            _userManager = userManager;

            _currentUser = _db.Users.Find(_userManager.GetUserId(contextAccessor.HttpContext.User));
        }
        public ApplicationUser getCurrentUser()
        {
            return _currentUser;
        }
    }
}
