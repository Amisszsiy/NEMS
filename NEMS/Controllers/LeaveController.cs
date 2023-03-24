using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NEMS.Data;
using NEMS.Helper;
using NEMS.Models;

namespace NEMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LeaveController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IUserService _userService;
        public LeaveController(ApplicationDbContext db, IUserService userService)
        {
            _db = db;
            _userService = userService;
        }
        public IActionResult Index(LeaveModel leaveModel)
        {
            if(leaveModel.From == null)
            {
                leaveModel.User = _db.Users.FirstOrDefault(x => x.Id != _userService.getCurrentUser().Id).Id;
                leaveModel.From = DateTime.Now.AddMonths(-1);
                leaveModel.Until = DateTime.Now;
            }
            leaveModel.Users = _db.Users.Where(x => x.Id != _userService.getCurrentUser().Id);
            leaveModel.Leaves = _db.Events.Where(x => x.user == leaveModel.User)
                .Where(x => x.start >= leaveModel.From)
                .Where(x => x.end <= leaveModel.Until);

            return View(leaveModel);
        }
    }
}
