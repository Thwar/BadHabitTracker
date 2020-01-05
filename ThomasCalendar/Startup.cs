using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using ThomasCalendar.Models;

namespace ThomasCalendar
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //  services.AddProtectedBrowserStorage();
            services.AddBlazoredLocalStorage();
            services.AddSingleton<DateState>();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");

        }
    }
}
