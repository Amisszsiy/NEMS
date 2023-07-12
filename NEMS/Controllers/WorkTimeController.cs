using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using NEMS.Data;
using NEMS.Helper;
using NEMS.Models;

namespace NEMS.Controllers
{
    [Authorize]
    public class WorkTimeController: Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClockService _clock;
        private readonly IUserService _userService;

        public WorkTimeController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IClockService clock, IUserService userService, IEmailService emailService)
        {
            _db = db;
            _userManager = userManager;
            _clock = clock;
            _userService = userService;
        }

        //Return default worktime table for each user
        [Authorize(Roles = "Admin")]
        public IActionResult Index(SummaryViewModel summary)
        {
            //Provide default model data when first initialize WorkTime summary
            if (summary.From == null)
            {
                summary.Users = _db.Users.Where(x => x.Id != _userService.getCurrentUser().Id);
                summary.User = _db.Users.FirstOrDefault(x => x.Id != _userService.getCurrentUser().Id).Id;
                summary.TimeTables = _db.TimeTables.Where(x => x.date == DateTime.Today).Where(x => x.uid == summary.User);
                return View(summary);
            }

            summary = getQuery(summary);

            return View(summary);
        }

        //Check personal worktime for user not admin
        public IActionResult CheckPersonalWorkTime(SummaryViewModel summary)
        {
            if(summary.From == null)
            {
                summary.TimeTables = _db.TimeTables.Where(x => x.date == DateTime.Today)
                    .Where(x => x.uid == _userService.getCurrentUser().Id);
            }

            summary.TimeTables = _db.TimeTables.Where(x => x.date >= summary.From)
                .Where(x => x.date <= summary.Until)
                .Where(x => x.uid == _userService.getCurrentUser().Id)
                .OrderBy(x => x.date);

            return View(summary);
        }

        //Edit in/out time for specific user
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult editClock(SummaryViewModel summary)
        {
            if (HttpContext.Request.Form["submit"] == "edit")
            {
                TimeTable editRecord;
                editRecord = _db.TimeTables.FirstOrDefault(x => x.id == summary.Clock.id);
                editRecord = _clock.editWorkDay(editRecord, summary.Clock);

                _db.TimeTables.Update(editRecord);
                _db.SaveChanges();
            }
            else if (HttpContext.Request.Form["submit"] == "delete")
            {
                TimeTable deleteRecord;
                deleteRecord = _db.TimeTables.FirstOrDefault(x => x.id == summary.Clock.id);

                _db.TimeTables.Remove(deleteRecord);
                _db.SaveChanges();
            }
            else
            {
                //Something gone wrong if we get this far.
            }

            summary = getQuery(summary);

            return View("Index", summary);
        }

        //Add in/out time from missing day for specific user
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult addClock(SummaryViewModel summary)
        {
            summary.Clock.uid = summary.User;

            if (!_db.TimeTables.Any(x => x.date == summary.Clock.clockin.Date && x.uid == summary.Clock.uid))
            {
                summary.Clock = _clock.addWorkDay(summary.Clock);
                _db.TimeTables.Add(summary.Clock);
                _db.SaveChanges();
            }

            summary = getQuery(summary);

            return View("Index", summary);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult allSummary(AllSummaryModel allSummary, string submit)
        {
            if(allSummary == null)
            {
                allSummary.From = DateTime.Today;
                allSummary.Until= DateTime.Today;
                allSummary = getAllSummary(allSummary);
            }

            allSummary = getAllSummary(allSummary);

            if(submit == "export")
            {
                //Query range of time to export
                IEnumerable<TimeTable> timeTables = _db.TimeTables
                    .Where(x => x.date >= allSummary.From)
                    .Where(x => x.date <= allSummary.Until)
                    .OrderBy(x => x.date);

                //Get all users to reference writing in each sheet
                IEnumerable<ApplicationUser> users = _db.Users.Where(x => x.Id != _userService.getCurrentUser().Id);

                //Prepare file
                string format = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string fileName = DateTime.Now.Date.ToShortDateString() + "_WorkTimeSummary.xlsx";

                //Create workbook and loop through each user/sheet
                using (var workbook = new XLWorkbook())
                {
                    foreach(var user in users)
                    {
                        //Get specific user in/out time from queried range above
                        IEnumerable<TimeTable> userTimeTables = timeTables.Where(x => x.uid == user.Id);
                        var worksheet = workbook.AddWorksheet(user.FirstName);
                        var currentRow = 1;
                        worksheet.Cell(currentRow, 1).Value = "Date";
                        worksheet.Cell(currentRow, 2).Value = "Clock in";
                        worksheet.Cell(currentRow, 3).Value = "Rounded";
                        worksheet.Cell(currentRow, 4).Value = "Clock out";
                        worksheet.Cell(currentRow, 5).Value = "Rounded";
                        worksheet.Cell(currentRow, 6).Value = "OT";
                        worksheet.Cell(currentRow, 7).Value = "ET";
                        worksheet.Cell(currentRow, 8).Value = "PayOT";

                        foreach(var day in userTimeTables)
                        {
                            currentRow++;
                            worksheet.Cell(currentRow, 1).Value = day.date.ToShortDateString();
                            worksheet.Cell(currentRow, 2).Value = day.clockin.ToShortTimeString();
                            worksheet.Cell(currentRow, 3).Value = day.rClockin.ToShortTimeString();
                            worksheet.Cell(currentRow, 4).Value = day.clockout.ToShortTimeString();
                            worksheet.Cell(currentRow, 5).Value = day.rClockout.ToShortTimeString();
                            worksheet.Cell(currentRow, 6).Value = Math.Round(day.ot,2);
                            worksheet.Cell(currentRow, 7).Value = Math.Round(day.et,2);
                            worksheet.Cell(currentRow, 8).Value = Math.Round(day.ot, 2) - Math.Round(day.et, 2);
                        }

                        //Sum ot, et, net ot at last row
                        worksheet.Cell(currentRow + 1, 6).Value = Math.Round(userTimeTables.Select(x => x.ot).Sum(),2);
                        worksheet.Cell(currentRow + 1, 7).Value = Math.Round(userTimeTables.Select(x => x.et).Sum(),2);
                        worksheet.Cell(currentRow + 1, 8).Value = Math.Round(userTimeTables.Select(x => x.ot).Sum() - userTimeTables.Select(x => x.et).Sum(),2);
                    }

                    //Store written workbook in variable content and return as a file
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();

                        return File(content,format,fileName);
                    }
                }
            }

            return View(allSummary);
        }

        private SummaryViewModel getQuery(SummaryViewModel summary)
        {
            summary.Users = _db.Users.Where(x => x.Id != _userService.getCurrentUser().Id);
            summary.TimeTables = _db.TimeTables.Where(x => x.date >= summary.From)
                .Where(x => x.date <= summary.Until)
                .Where(x => x.uid == summary.User)
                .OrderBy(x => x.date);

            return summary;
        }

        private AllSummaryModel getAllSummary(AllSummaryModel allSummary)
        {
            IEnumerable<TimeTable>? timeTables = _db.TimeTables
                .Where(x => x.date >= allSummary.From)
                .Where(x => x.date <= allSummary.Until)
                .OrderBy(x => x.date);

            IEnumerable<ApplicationUser> users = _db.Users.Where(x => x.Id != _userService.getCurrentUser().Id);

            allSummary.Users = new List<UserOT>();

            foreach(ApplicationUser user in users)
            {
                IEnumerable<TimeTable>? userTimeTables = timeTables.Where(x => x.uid == user.Id);
                UserOT userOT= new UserOT();
                userOT.Name = user.FirstName;
                userOT.OT = userTimeTables.Sum(x => x.ot);
                userOT.ET = userTimeTables.Sum(x => x.et);
                userOT.PayOT = userOT.OT - userOT.ET;

                allSummary.Users.Add(userOT);
            }

            return allSummary;
        }
    }
}
