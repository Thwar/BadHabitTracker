using System.Collections.Generic;
using System.Linq;
using ThomasCalendar.Models;

namespace ThomasCalendar.Business
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
