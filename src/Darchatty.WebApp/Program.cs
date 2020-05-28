using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Darchatty.Web.Clients;
using Darchatty.Web.Hubs;
using Darchatty.WebApp.Auth;
using Darchatty.WebApp.Configuration;
using Darchatty.WebApp.Hubs;
using Darchatty.WebApp.Model;
using Darchatty.WebApp.Service;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Darchatty.WebApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddSingleton<ChatHubClient>();
            builder.Services.AddSingleton<IChatHub>(s => s.GetRequiredService<ChatHubClient>());
            builder.Services.AddSingleton<IChatClient>(s => s.GetRequiredService<ChatHubClient>());
            builder.Services.AddSingleton<AuthHubClient>();
            builder.Services.AddSingleton<IAuthHub>(s => s.GetRequiredService<AuthHubClient>());
            builder.Services.AddSingleton<IAuthClient>(s => s.GetRequiredService<AuthHubClient>());
            builder.Services.AddSingleton<StateService>();
            builder.Services.AddSingleton(new GatewayConfiguration
            {
                Endpoint = "http://localhost:5100",
            });
            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddAuthorizationCore();
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddScoped<AuthenticationStateProvider, TokenAuthStateProvider>();

            await builder.Build().RunAsync();
        }
    }
}
