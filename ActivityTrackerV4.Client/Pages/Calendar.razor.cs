using System.Net.Http.Json;
using System.Text.Json;
using ActivityTrackerV4.Business;
using ActivityTrackerV4.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ActivityTrackerV4.Pages;

public partial class Calendar : IDisposable
{
    // Fields
    private CalendarContainer _container = new();
    private string? _newEvent;
    private DateTime JournalDate;

    // Properties
    private List<Day> Days = new();
    public DateTime CurrentDate { get; private set; } = DateTime.Now;
    public DateTime TodaysDate { get; private set; } = DateTime.Now;
    protected string? Note { get; set; }
    protected int DayRating { get; set; } = 1;
    protected List<string> DayEvents { get; set; } = new();
    public List<LedgerLine>? DayLedger { get; set; }
    protected string SelectedEvent { get; set; } = "default";

    // Lifecycle Methods
    protected override void OnInitialized()
    {
        DateState.OnChange += StateHasChanged;
    }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (!authState.User.Identity?.IsAuthenticated ?? true)
        {
            NavigationManager.NavigateTo("/login");
            return;
        }

        TodaysDate = CurrentDate = await GetLocalDateAsync();
        await RefreshTokenAsync();
        await LoadUserDataAsync();
        RefreshBadDays();
    }

    public void Dispose()
    {
        DateState.OnChange -= StateHasChanged;
    }

    // Private Methods

    private async Task<DateTime> GetLocalDateAsync()
    {
        var localDate = await JSRuntime.InvokeAsync<string>("GetLocalDate");
        return DateTime.Parse(localDate);
    }

    private async Task RefreshTokenAsync()
    {
        var refreshToken = await localStore.GetItemAsync<string>("refreshToken");
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await _httpClient.PostAsJsonAsync("api/accounts/refresh", refreshToken);
        }
    }

    private async Task LoadUserDataAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync("api/userdata");
            _container = string.IsNullOrEmpty(response)
                ? new CalendarContainer()
                : JsonSerializer.Deserialize<CalendarContainer>(response) ?? new CalendarContainer();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Failed to fetch user data: {ex.Message}");
        }
    }

    private void RefreshBadDays()
    {
        var helperFunctions = new HelperFunctions();
        helperFunctions.CalculateBadDays(new List<Day>(), _container);
    }

    private async Task SaveData()
    {
        var helperFunctions = new HelperFunctions();
        helperFunctions.CalculateBadDays(new List<Day>(), _container);
        StateHasChanged();
        DateState.UpdateCalendar(_container);

       await SaveToDatabaseAsync();
    }

    private async Task SaveToDatabaseAsync()
    {
        await _httpClient.PostAsJsonAsync("api/userdata", JsonSerializer.Serialize(_container));
        await localStore.SetItemAsync("Calendar", _container);
    }

    private Month GetOrCreateMonth(DateTime date)
    {
        var year = _container.Year.FirstOrDefault(y => y.Name == date.Year.ToString())
                   ?? CreateYear(date.Year);
        return year.Month.FirstOrDefault(m => m.Name == date.ToString("MMMM"))
               ?? CreateMonth(year, date.Month);
    }

    private Year CreateYear(int year)
    {
        var newYear = new Year { Name = year.ToString() };
        _container.Year.Add(newYear);
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

    private async Task SaveDay(DateTime date, string? note, List<string> events, List<LedgerLine>? ledger, int dayRating)
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

    // UI Interaction Methods

    private void OpenDayJournal(DateTime date, Day? day)
    {
        JournalDate = date;
        Note = day?.Note;
        DayEvents = day?.Event ?? new List<string>();
        DayLedger = day?.Ledger;
        DayRating = day?.DayRating ?? 0;
        SelectedEvent = "default";
        JSRuntime.InvokeAsync<bool>("OpenModal");
        JSRuntime.InvokeVoidAsync("InitHtmlEditor");
    }

    private async Task SaveJournalEntry()
    {
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

    private void AddEvent()
    {
        if (!string.IsNullOrWhiteSpace(_newEvent))
        {
            var random = new Random();
            var color = $"#{random.Next(0x1000000):X6}";
            DateState.Container.Settings.Event.Add(new Event { Name = _newEvent, Color = color });
            _newEvent = string.Empty;
        }
    }

    private void NavigateMonth(int increment)
    {
        CurrentDate = CurrentDate.AddMonths(increment);
    }

    private void NextMonth() => NavigateMonth(1);
    private void PrevMonth() => NavigateMonth(-1);


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
