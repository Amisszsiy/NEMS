using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NEMS.Data;
using NEMS.Helper;
using NEMS.Models;

namespace NEMS.Controllers
{
    public class WorkTimeController: Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClockService _clock;

        public WorkTimeController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IClockService clock)
        {
            _db = db;
            _userManager = userManager;
            _clock = clock;
        }

        public IActionResult Index(SummaryViewModel summary)
        {

            if (summary.From == null)
            {
                summary.Users = _db.Users;
                summary.TimeTables = _db.TimeTables.Where(x => x.date == DateTime.Today);
                summary.User = HomeController._user.Id;
                return View(summary);
            }

            summary = getQuery(summary);

            return View(summary);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult editClock(SummaryViewModel summary)
        {
            TimeTable editRecord;
            editRecord = _db.TimeTables.FirstOrDefault(x => x.id == summary.Clock.id);
            editRecord = _clock.editWorkDay(editRecord, summary.Clock);

            _db.TimeTables.Update(editRecord);
            _db.SaveChanges();

            summary = getQuery(summary);

            return View("Index", summary);
        }

        public IActionResult allSummary()
        {
            return View();
        }

        private SummaryViewModel getQuery(SummaryViewModel summary)
        {
            summary.Users = _db.Users;
            summary.TimeTables = _db.TimeTables.Where(x => x.date >= summary.From)
                .Where(x => x.date <= summary.Until)
                .Where(x => x.uid == summary.User);

            return summary;
        }
    }
}
