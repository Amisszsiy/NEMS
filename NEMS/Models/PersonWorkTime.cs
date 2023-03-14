using NEMS.Data;

namespace NEMS.Models
{
    public class PersonWorkTime
    {
        public string User { get; set; }
        public IEnumerable<ApplicationUser> Users { get; set; }
        public TimeTable TimeTable { get; set; }
    }
}
