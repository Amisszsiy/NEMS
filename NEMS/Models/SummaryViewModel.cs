using NEMS.Data;

namespace NEMS.Models
{
    public class SummaryViewModel
    {
        public string User { get; set; }
        public DateTime? From { get; set; }
        public DateTime? Until { get; set; }
        public IEnumerable<ApplicationUser> Users { get; set; }
        public TimeTable? Clock { get; set; }
        public IEnumerable<TimeTable>? TimeTables { get; set; }
    }
}
