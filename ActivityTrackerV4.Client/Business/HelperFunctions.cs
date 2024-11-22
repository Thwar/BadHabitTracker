
using ActivityTrackerV4.Models;

namespace ActivityTrackerV4.Business
{
    public class HelperFunctions
    {
        public void CalculateBadDays(List<Day> Habits, CalendarContainer container)
        {
            Habits.Clear();

            foreach (var years in container.Year)
            {
                foreach (var months in years.Month)
                {
                    foreach (var badHabit in months.Day)
                    {
                        if (badHabit.DidSomethingBad)
                        {
                            Habits.Add(badHabit);
                        }
                    }
                }
            }
        }

        public static string GetTotalMonthSpending(CalendarContainer calendar, DateTime currentDate)
        {
            decimal totalAmount = 0;
            var currrentYear = calendar.Year.Where(x => x.Name == currentDate.Year.ToString()).FirstOrDefault();

            if (currrentYear != null)
            {
                var currentMonth = currrentYear?.Month.Where(x => x.Name == currentDate.ToString("MMMM")).FirstOrDefault();

                if (currentMonth != null)
                {
                    //  totalAmount = (decimal)(currentMonth?.Day.Sum(x => x.Ledger.Sum(x => x.Amount)));
                    if (currentMonth.Day != null)
                    {
                        totalAmount = (decimal)(currentMonth?.Day.Where(x=>x.Ledger != null).SelectMany(x => x.Ledger).Sum(x => x.Amount));
                    }

                }
            }

            return totalAmount.ToString();
        }

        public static IEnumerable<DateTime> GetAllDatesInMonth(int year, int month)
        {
            int days = DateTime.DaysInMonth(year, month);
            for (int day = 1; day <= days; day++)
            {
                yield return new DateTime(year, month, day);
            }
        }

        public static string GetRatingColor(int rating)
        {
            // Ensure the rating is within the valid range
            rating = Math.Clamp(rating, 1, 10);

            // Define red-to-green gradient
            int red = (int)(255 - (rating - 1) * 25.5); // From 255 to 0
            int green = (int)((rating - 1) * 25.5);     // From 0 to 255

            // Return the color in hex format
            return $"rgb({red}, {green}, 0)";
        }


        public void GetDaysWithEventByName(List<Day> days, CalendarContainer container, string eventName)
        {
            days.Clear();

            foreach (var years in container.Year)
            {
                foreach (var months in years.Month)
                {
                    foreach (var day in months.Day)
                    {
                        if (day.Event != null && day.Event.Contains(eventName))
                        {
                            days.Add(day);
                        }
                    }
                }
            }
        }

        public void UpdateEventName(CalendarContainer container, string oldEventName, string newEventName)
        {
            foreach (var years in container.Year)
            {
                foreach (var months in years.Month)
                {
                    foreach (var day in months.Day)
                    {
                        if (day.Event != null)
                        {
                            var newList = day.Event.Select(s => s.Replace(oldEventName, newEventName)).ToList();
                            day.Event = newList;
                        }
                    }
                }
            }
        }
    }
}
