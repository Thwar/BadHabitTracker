namespace ActivityTrackerV4.Models
{
    public class CalendarContainer
    {
        public List<Year> Year { get; set; }
        public Settings Settings { get; set; }

        public CalendarContainer()
        {
            this.Year = new List<Year>();
            this.Settings = new Settings();
        }
    }

    public class Settings
    {
        public List<Event> Event { get; set; }

        public Settings()
        {
            this.Event = new List<Event>();
        }
    }

    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
    }

    public class Day
    {
        public DateTime Date { get; set; }

        public string? Note { get; set; }

        public bool DidSomethingBad { get; set; }

        public int? DayRating { get; set; }

        public List<string> Event { get; set; }

        public List<LedgerLine>? Ledger { get; set; }

        public Day()
        {
            Event = new List<string>();
            Ledger = new List<LedgerLine>();
        }
    }

    public class Year
    {
        public string Name { get; set; }
        public List<Month> Month { get; set; }

        public Year()
        {
            this.Month = new List<Month>();
        }
    }

    public class Month
    {
        public string Name { get; set; }
        public List<Day> Day { get; set; }

        public Month()
        {
            this.Day = new List<Day>();
        }
    }
}
