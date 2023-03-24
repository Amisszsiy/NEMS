using NEMS.Data;

namespace NEMS.Models
{
    public class LeaveModel
    {
        public IEnumerable<ApplicationUser> Users { get; set; }
        public string? User { get; set; }
        public IEnumerable<Event>? Leaves { get; set; }
        public DateTime? From { get; set; }
        public DateTime? Until { get; set; }
    }
}
