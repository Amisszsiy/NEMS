﻿using ClosedXML.Excel;
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
                IEnumerable<TimeTable> timeTables = _db.TimeTables.Where(x => x.date >= allSummary.From)
                    .Where(x => x.date <= allSummary.Until);

                IEnumerable<ApplicationUser> users = _db.Users;

                string format = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                string fileName = DateTime.Now.Date.ToShortDateString() + "_WorkTimeSummary.xlsx";

                using (var workbook = new XLWorkbook())
                {
                    foreach(var user in users)
                    {
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
                    }

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
            summary.Users = _db.Users;
            summary.TimeTables = _db.TimeTables.Where(x => x.date >= summary.From)
                .Where(x => x.date <= summary.Until)
                .Where(x => x.uid == summary.User);

            return summary;
        }

        private AllSummaryModel getAllSummary(AllSummaryModel allSummary)
        {
            IEnumerable<TimeTable>? timeTables = _db.TimeTables.Where(x => x.date >= allSummary.From)
                .Where(x => x.date <= allSummary.Until);

            IEnumerable<ApplicationUser> users = _db.Users;

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