using ActivityTrackerV4;
using ActivityTrackerV4.Business;
using ActivityTrackerV4.Models;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Headers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Determine API base address based on environment
var apiBaseAddress = builder.HostEnvironment.IsDevelopment()
    ? "https://localhost:7086" // Development API
    : "https://blazorauthapi20241129160839.azurewebsites.net"; // Production API

// Register HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register services
builder.Services.AddSingleton<CalendarContainer>();
builder.Services.AddSingleton<DateState>();
builder.Services.AddSingleton<LedgerLine>();
builder.Services.AddSingleton<Quote>();

// Register Blazored.LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Register HttpClient with JWT bearer support
builder.Services.AddScoped(sp =>
{
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var httpClient = new HttpClient { BaseAddress = new Uri(apiBaseAddress) };
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetJwtToken(localStorage));
    return httpClient;
});

// Add an AuthenticationStateProvider to handle user state
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

await builder.Build().RunAsync();

// Helper function to retrieve JWT token from local storage
static string? GetJwtToken(ILocalStorageService localStorage)
{
    var token = Task.Run(() => localStorage.GetItemAsync<string>("authToken")).Result;
    return string.IsNullOrEmpty(token.ToString()) ? string.Empty : token.ToString();
}