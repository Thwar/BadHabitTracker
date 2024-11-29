using System.ComponentModel;
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
    private DateTime JournalDate;


    private JournalEntryModalModel ModalModel { get; set; } = new();
    // Properties
    public DateTime CurrentDate { get; private set; } = DateTime.Now;
    public DateTime TodaysDate { get; private set; } = DateTime.Now;
    protected string? Note { get; set; }
    protected int? DayRating { get; set; } 
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
        // RefreshBadDays();
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


            DateState.FirstName = await _httpClient.GetStringAsync("api/userdata/firstName");

            StateHasChanged();
            DateState.UpdateCalendar(_container);
            await localStore.SetItemAsync("Calendar", _container);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Failed to fetch user data: {ex.Message}");
        }
    }


    // UI Interaction Methods

    private void OpenDayJournal(DateTime date, Day? day)
    {
        JournalDate = date;
        Note = day?.Note;
        DayEvents = day?.Event ?? new List<string>();
        DayLedger = day?.Ledger;
        DayRating = day?.DayRating;
        SelectedEvent = "default";
        JSRuntime.InvokeAsync<bool>("OpenModal");
        JSRuntime.InvokeVoidAsync("InitHtmlEditor");

        JSRuntime.InvokeVoidAsync("$('#journalModal').modal", "show");
    }

    private void NavigateMonth(int increment)
    {
        CurrentDate = CurrentDate.AddMonths(increment);
    }

    private void NextMonth() => NavigateMonth(1);
    private void PrevMonth() => NavigateMonth(-1);


}
