using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NEMS.Data;
using NEMS.Helper;
using NEMS.Models;
using NuGet.Protocol;

namespace NEMS.Controllers
{
    [Authorize]
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IUserService _userService;

        public CalendarController(ApplicationDbContext db, IUserService userService)
        {
            _db = db;
            _userService = userService;
        }
        public IActionResult Index(CalendarModel calendarModel)
        {
            if (calendarModel.User == null)
            {
                calendarModel.User = _userService.getCurrentUser().Id;
            }

            calendarModel = getEvents(calendarModel);

            return View(calendarModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Schedule(CalendarModel calendarModel)
        {
            if (HttpContext.Request.Form["submit"] == "add")
            {
                ApplicationUser eventOwner = _userService.getCurrentUser();

                Event oEvent = new Event();
                oEvent.title = calendarModel.Event.title;
                oEvent.start = calendarModel.Event.start;
                oEvent.end = calendarModel.Event.end;
                oEvent.description = calendarModel.Event.description ?? "";
                oEvent.allDay = calendarModel.Event.allDay;
                oEvent.user = eventOwner.Id;
                oEvent.isOwner = true;

                _db.Events.Add(oEvent);
                _db.SaveChanges();

                int refId = oEvent.id;

                if (calendarModel.Participants != null)
                {
                    foreach (var participant in calendarModel.Participants)
                    {
                        Event pEvent = new Event();
                        pEvent.title = calendarModel.Event.title;
                        pEvent.start = calendarModel.Event.start;
                        pEvent.end = calendarModel.Event.end;
                        pEvent.description = calendarModel.Event.description ?? "";
                        pEvent.allDay = calendarModel.Event.allDay;
                        pEvent.user = participant;
                        pEvent.isOwner = false;
                        pEvent.refId = refId;

                        _db.Events.Add(pEvent);
                    }

                }
            }
            else if (HttpContext.Request.Form["submit"] == "edit")
            {
                Event editEvent = _db.Events.FirstOrDefault(x => x.id == calendarModel.Event.id);
                editEvent.title = calendarModel.Event.title;
                editEvent.start = calendarModel.Event.start;
                editEvent.end = calendarModel.Event.end;
                editEvent.description = calendarModel.Event.description ?? "";
                editEvent.allDay = calendarModel.Event.allDay;

                _db.Events.Update(editEvent);

                IEnumerable<Event> formalParticipants = _db.Events.Where(x => x.refId == calendarModel.Event.id);
                if (calendarModel.Participants != null)
                {
                    foreach (var participant in calendarModel.Participants)
                    {
                        if (formalParticipants.Any(x => x.user == participant))
                        {
                            Event pEvent = formalParticipants.FirstOrDefault(x => x.user == participant);
                            pEvent.title = calendarModel.Event.title;
                            pEvent.start = calendarModel.Event.start;
                            pEvent.end = calendarModel.Event.end;
                            pEvent.description = calendarModel.Event.description;
                            pEvent.allDay = calendarModel.Event.allDay;
                            _db.Events.Update(pEvent);
                        }
                        else
                        {
                            Event pEvent = new Event();
                            pEvent.title = calendarModel.Event.title;
                            pEvent.start = calendarModel.Event.start;
                            pEvent.end = calendarModel.Event.end;
                            pEvent.description = calendarModel.Event.description;
                            pEvent.allDay = calendarModel.Event.allDay;
                            pEvent.user = participant;
                            pEvent.isOwner = false;
                            pEvent.refId = calendarModel.Event.id;
                            _db.Events.Add(pEvent);
                        }
                    }
                }
                else
                {
                    calendarModel.Participants = new List<string>();
                }

                foreach (var participant in formalParticipants)
                {
                    if (!calendarModel.Participants.Any(x => x == participant.user))
                    {
                        _db.Events.Remove(participant);
                    }
                }
            }
            else if (HttpContext.Request.Form["submit"] == "delete")
            {
                IEnumerable<Event> deleteEvents = _db.Events.Where(x => x.id == calendarModel.Event.id || x.refId == calendarModel.Event.id);

                _db.Events.RemoveRange(deleteEvents);
            }
            else
            {
                //Something wrong happened
                return View("index", calendarModel);
            }

            _db.SaveChanges();

            calendarModel.User = _userService.getCurrentUser().Id;
            calendarModel = getEvents(calendarModel);

            return View("Index", calendarModel);
        }

        public CalendarModel getEvents(CalendarModel calendarModel)
        {
            calendarModel.Events = (from Event in _db.Events.Where(x => x.user == calendarModel.User)
                                    .Where(x => x.start.Date >= DateTime.Now.Date.AddYears(-1))
                                   select new
                                   {
                                       Event.id,
                                       Event.title,
                                       Event.start,
                                       Event.end,
                                       Event.allDay,
                                       Event.description
                                   }).ToList().ToJson();

            calendarModel.Users = _db.Users.Where(x => x.UserName != "th_accounting@nc-net.or.jp");

            return calendarModel;
        }
    }
}
