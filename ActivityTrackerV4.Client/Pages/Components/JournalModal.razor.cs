using System.Net.Http.Json;
using System.Text.Json;
using ActivityTrackerV4.Business;
using ActivityTrackerV4.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ActivityTrackerV4.Pages.Components;

public partial class JournalModal : ComponentBase
{
    [Parameter] public JournalEntryModalModel Model { get; set; } = new();

    // Fields
    [Parameter] public CalendarContainer Container { get; set; }
    private string? _newEvent;

    [Parameter] public DateTime JournalDate { get; set; }

    // Properties
    [Parameter] public string? Note { get; set; }
    [Parameter] public int? DayRating { get; set; }
    [Parameter] public List<string> DayEvents { get; set; } = new();
    [Parameter] public List<LedgerLine>? DayLedger { get; set; }
    [Parameter] public string SelectedEvent { get; set; } = "default";



    private async Task SaveData()
    {
        var helperFunctions = new HelperFunctions();
//        helperFunctions.CalculateBadDays(new List<Day>(), Container);
        StateHasChanged();
        DateState.UpdateCalendar(Container);

       await SaveToDatabaseAsync();
    }

    private async Task SaveToDatabaseAsync()
    {
        await _httpClient.PostAsJsonAsync("api/userdata", JsonSerializer.Serialize(Container));
        await localStore.SetItemAsync("Calendar", Container);
    }

    private Month GetOrCreateMonth(DateTime date)
    {
        var year = Container.Year.FirstOrDefault(y => y.Name == date.Year.ToString())
                   ?? CreateYear(date.Year);
        return year.Month.FirstOrDefault(m => m.Name == date.ToString("MMMM"))
               ?? CreateMonth(year, date.Month);
    }

    private Year CreateYear(int year)
    {
        var newYear = new Year { Name = year.ToString() };
        Container.Year.Add(newYear);
        return newYear;
    }

    private Month CreateMonth(Year year, int month)
    {
        var newMonth = new Month { Name = new DateTime(int.Parse(year.Name), month, 1).ToString("MMMM") };
        year.Month.Add(newMonth);
        return newMonth;
    }

    private void UpdateDay(Month month, Day day)
    {
        var existingDay = month.Day.FirstOrDefault(d => d.Date == day.Date);
        if (existingDay == null)
        {
            month.Day.Add(day);
        }
        else
        {
            existingDay.Note = day.Note;
            existingDay.Event = day.Event;
            existingDay.Ledger = day.Ledger;
            existingDay.DayRating = day.DayRating;
        }
    }

    private async Task SaveDay(DateTime date, string? note, List<string> events, List<LedgerLine>? ledger, int? dayRating)
    {
        var month = GetOrCreateMonth(date);
        UpdateDay(month, new Day
        {
            Date = date.Date,
            Note = note,
            Event = events,
            Ledger = ledger,
            DayRating = dayRating
        });

       await SaveData();
    }


    private async Task SaveJournalEntry()
    {
        // Save settings
        Container.Settings = DateState.Container.Settings;

        await SaveDay(JournalDate, Note, DayEvents, DayLedger, DayRating);
        await JSRuntime.InvokeAsync<bool>("CloseModal");
    }

    private void AddLedgerLine()
    {
        DayLedger ??= new List<LedgerLine>();
        DayLedger.Add(new LedgerLine { Id = Guid.NewGuid(), Amount = 0, Description = string.Empty });
    }

    private void DeleteLedgerLine(Guid id)
    {
        DayLedger?.RemoveAll(ledger => ledger.Id == id);
    }

    private Task AddEvent()
    {
        if (!string.IsNullOrWhiteSpace(_newEvent))
        {
            var random = new Random();
            var color = $"#{random.Next(0x1000000):X6}";
            DateState.Container.Settings.Event.Add(new Event { Name = _newEvent, Color = color });
            _newEvent = string.Empty;
        }

        return Task.CompletedTask;
    }

    private void OnDayRatingChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int rating))
        {
            DayRating = rating;
        }
    }

    private void DeleteLineEvent(string eventName)
    {
        DayEvents.Remove(eventName);
    }

    private void AddLineEvent()
    {
        if (SelectedEvent != "default")
        {
            DayEvents.Add(SelectedEvent);
            SelectedEvent = "default";
        }
    }
}
