using ActivityTrackerV4.Business;

namespace ActivityTrackerV4.Models
{
    public class DateState
    {
        public CalendarContainer Container { get; private set; }
        public List<Day> Habits = new();

        public event Action OnChange;

        public void UpdateCalendar(CalendarContainer data)
        {
            Container = data;
            NotifyStateChanged();
        }

        public DateTime? GetLastRelapse()
        {
            var helperFunctions = new HelperFunctions();
            helperFunctions.CalculateBadDays(Habits, Container);

            if (Habits.Count != 0)
            {
                return Habits.OrderByDescending(x => x.Date).First().Date;
            }
            else
            {
                return null;
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
