using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using ActivityTrackerV4.Models;
using ActivityTrackerV4.Pages;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace ActivityTrackerV4.Business;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorageService;
    private readonly HttpClient _httpClient;

    public CustomAuthenticationStateProvider(ILocalStorageService localStorageService, HttpClient httpClient)
    {
        _localStorageService = localStorageService;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity();

        // Get the token from local storage
        var token = await _localStorageService.GetItemAsync<string>("authToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            // If no token exists, return unauthenticated state
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        // Decode the token
        var jwtHandler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwtToken;

        try
        {
            jwtToken = jwtHandler.ReadJwtToken(token);
        }
        catch (Exception)
        {
            // If token cannot be read, consider it invalid and logout
            await Logout();
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        // Check if token is expired or about to expire in less than 5 minutes
        if (jwtToken.ValidTo < DateTime.UtcNow.AddMinutes(5))
        {
            // Attempt to refresh the token
            var refreshToken = await _localStorageService.GetItemAsync<string>("refreshToken");

            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                 var response = await _httpClient.PostAsJsonAsync("api/userdata/refresh", refreshToken);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<Login.LoginResult>();

                    // Update local storage with new tokens
                    token = result.Token; // Update the token with the new one
                    await _localStorageService.SetItemAsync("authToken", token);
                    await _localStorageService.SetItemAsync("refreshToken", result.RefreshToken);
                }
                else
                {
                    // If token refresh fails, consider the user logged out
                    await Logout();
                    return new AuthenticationState(new ClaimsPrincipal(identity));
                }
            }
            else
            {
                // If no refresh token available, logout
                await Logout();
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
        }

        // Create authenticated user identity
        identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        // Set the authorization header for future requests
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Return the authenticated state
        return new AuthenticationState(user);
    }

    public async Task Logout()
    {
        // Clear tokens from local storage
        await _localStorageService.RemoveItemAsync("authToken");
        await _localStorageService.RemoveItemAsync("refreshToken");

        // Notify that the user is logged out
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }

    //public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    //{
    //    var token = await _localStorageService.GetItemAsync<string>("authToken");

    //    var identity = new ClaimsIdentity();
    //    _httpClient.DefaultRequestHeaders.Authorization = null;

    //    if (!string.IsNullOrEmpty(token))
    //    {
    //        try
    //        {
    //            var handler = new JwtSecurityTokenHandler();
    //            var jwtToken = handler.ReadJwtToken(token);
    //            var claims = jwtToken.Claims;
    //            identity = new ClaimsIdentity(claims, "jwt");

    //            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    //        }
    //        catch
    //        {
    //            // If token is invalid
    //            await _localStorageService.RemoveItemAsync("authToken");
    //        }
    //    }

    //    var user = new ClaimsPrincipal(identity);
    //    return await Task.FromResult(new AuthenticationState(user));
    //}

    public void NotifyUserAuthentication(string token)
    {
        // Decode the token to get the claims
        var jwtHandler = new JwtSecurityTokenHandler();
        var jwtToken = jwtHandler.ReadJwtToken(token);
        var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");

        var authenticatedUser = new ClaimsPrincipal(identity);

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(authenticatedUser)));
    }

    public void NotifyUserLogout()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }
}