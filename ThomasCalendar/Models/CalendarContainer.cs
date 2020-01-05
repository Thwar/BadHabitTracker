using System;
using System.Collections.Generic;

namespace ThomasCalendar.Models
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
        public string Name { get; set; }
        public string Color { get; set; }
    }

    public class Day
    {
        public DateTime Date { get; set; }

        public string Note { get; set; }

        public bool DidSomethingBad { get; set; }

        public List<string> Event { get; set; }

        public Day()
        {
            this.Event = new List<string>();
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
