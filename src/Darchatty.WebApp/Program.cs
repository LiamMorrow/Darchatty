using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Blazored.LocalStorage;
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

            builder.Services.AddSingleton<IChatHub, ChatHubClient>();
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
