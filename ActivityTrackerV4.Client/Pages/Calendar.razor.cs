﻿using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection.Metadata;
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

    private bool isRefreshDetected = false;

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


        // Check if page refresh occurred
        var wasRefreshed = await localStore.GetItemAsync<bool>("PageRefreshed");
        if (wasRefreshed)
        {
            isRefreshDetected = true;
            Console.WriteLine("Page refresh detected. Forcing data refresh...");
            await LoadUserDataAsync(forceRefresh: true);

            // Reset the flag
            await localStore.SetItemAsync("PageRefreshed", false);
        }
        else
        {
            // Normal data load
            await LoadUserDataAsync();
        }

        // Subscribe to LocationChanged to detect page refreshes
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    public void Dispose()
    {
        DateState.OnChange -= StateHasChanged;

        // Unsubscribe from the event to prevent memory leaks
        NavigationManager.LocationChanged -= OnLocationChanged;
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

    private async Task LoadUserDataAsync(bool forceRefresh = false)
    {
        try
        {
            // Step 1: Check for cached data with a timestamp
            var cachedData = await localStore.GetItemAsync<CacheWrapper<CalendarContainer>>("CalendarCache");

            if (!forceRefresh && cachedData != null && !IsCacheExpired(cachedData.Timestamp))
            {
                // Use cached data if valid
                _container = cachedData.Data;
                Console.WriteLine("Loaded data from cache.");
            }
            else
            {
                // Step 2: Fetch data from the server
                var response = await _httpClient.GetStringAsync("api/userdata");
                _container = string.IsNullOrEmpty(response)
                    ? new CalendarContainer()
                    : JsonSerializer.Deserialize<CalendarContainer>(response) ?? new CalendarContainer();

                // Update the cache
                await localStore.SetItemAsync("CalendarCache", new CacheWrapper<CalendarContainer>
                {
                    Data = _container,
                    Timestamp = DateTime.UtcNow
                });

                Console.WriteLine("Fetched and cached latest data from the server.");
            }

            // Step 3: Fetch additional data if necessary
            if (string.IsNullOrWhiteSpace(DateState.FirstName))
            {
                DateState.FirstName = await _httpClient.GetStringAsync("api/userdata/firstName");
            }

            // Step 4: Update the state and UI
            StateHasChanged();
            DateState.UpdateCalendar(_container);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Failed to fetch user data: {ex.Message}");
        }
    }

    // Helper method to check cache expiration
    private bool IsCacheExpired(DateTime cacheTimestamp, int expirationMinutes = 60)
    {
        return DateTime.UtcNow - cacheTimestamp > TimeSpan.FromMinutes(expirationMinutes);
    }

    // Cache wrapper class to store data with a timestamp
    private class CacheWrapper<T>
    {
        public T Data { get; set; }
        public DateTime Timestamp { get; set; }
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

    private async void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
    {
        // Set the "PageRefreshed" flag when navigating to the current page
        await localStore.SetItemAsync("PageRefreshed", true);
    }



}
