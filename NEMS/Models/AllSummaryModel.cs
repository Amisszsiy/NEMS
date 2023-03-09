namespace NEMS.Models
{
    public class AllSummaryModel
    {
        public DateTime? From { get; set; }
        public DateTime? Until { get; set; }
        public List<UserOT>? Users { get; set; }
    }
}
