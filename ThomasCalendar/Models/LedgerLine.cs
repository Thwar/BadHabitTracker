using System;

namespace ThomasCalendar.Models
{
    public class LedgerLine
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }
}