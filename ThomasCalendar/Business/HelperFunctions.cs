using System;
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

        public static string GetTotalMonthSpending(CalendarContainer calendar, DateTime currentDate)
        {
            return "";
        }

        public static IEnumerable<DateTime> GetAllDatesInMonth(int year, int month)
        {
            int days = DateTime.DaysInMonth(year, month);
            for (int day = 1; day <= days; day++)
            {
                yield return new DateTime(year, month, day);
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
