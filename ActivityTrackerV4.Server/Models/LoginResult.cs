using System.Security.AccessControl;

namespace BlazorAuthAPI.Models
{
    public class LoginResult
    {
        public string Token { get; set; }

        public string RefreshToken { get; set; }    
    }
}
