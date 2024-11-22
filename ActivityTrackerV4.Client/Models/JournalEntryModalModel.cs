namespace ActivityTrackerV4.Models
{
    public class JournalEntryModalModel
    {
        public DateTime JournalDate { get; set; }
        public string? Note { get; set; }
        public int DayRating { get; set; } = 1;
        public List<string> DayEvents { get; set; } = new();
        public List<LedgerLine>? DayLedger { get; set; }
        public string SelectedEvent { get; set; } = "default";
    }
}
