using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Darchatty.Web.Clients;
using Darchatty.Web.Hubs;
using Darchatty.Web.Model;
using Darchatty.WebApp.Configuration;
using Darchatty.WebApp.Model;
using Darchatty.WebApp.Service;
using Darchatty.WebApp.Shared;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;

namespace Darchatty.WebApp.Hubs
{
    public class AuthHubClient : IAuthHub, IAuthClient
    {
        private readonly HubConnection hubConnection;
        private readonly StateService stateService;
        private bool connected;

        public AuthHubClient(
            GatewayConfiguration gatewayConfiguration,
            StateService stateService)
        {
            this.stateService = stateService;
            hubConnection = new HubConnectionBuilder()
                .WithAutomaticReconnect()
                .WithUrl(gatewayConfiguration.Endpoint + "/hub/auth", opts =>
                {
                    opts.AccessTokenProvider = () =>
                    {
                        return Task.FromResult(stateService.State.UserId.ToString());
                    };
                    opts.SkipNegotiation = true;
                    opts.Transports = HttpTransportType.WebSockets;
                })
                .Build();

            hubConnection.Closed += _ =>
            {
                connected = false;
                return Task.CompletedTask;
            };
            hubConnection.Reconnected += (e) =>
            {
                connected = true;
                return Task.CompletedTask;
            };
            _ = Task.Run(async () =>
                 {
                     await hubConnection.StartAsync();
                     connected = true;
                 });
        }

        public async Task<Guid> LoginAsync(LoginDto request)
        {
            await EnsureConnectedAsync().ConfigureAwait(false);
            return await hubConnection.InvokeAsync<Guid>(nameof(LoginAsync), request)
                .ConfigureAwait(false);
        }

        public async Task<Guid> RegisterAsync(RegisterDto request)
        {
            await EnsureConnectedAsync().ConfigureAwait(false);
            return await hubConnection.InvokeAsync<Guid>(nameof(RegisterAsync), request)
                .ConfigureAwait(false);
        }

        private async ValueTask EnsureConnectedAsync()
        {
            if (connected)
            {
                return;
            }

            await Task.Delay(50).ConfigureAwait(false);
            if (connected)
            {
                return;
            }

            await Task.Delay(100).ConfigureAwait(false);
            if (connected)
            {
                return;
            }

            await Task.Delay(200).ConfigureAwait(false);
            if (connected)
            {
                return;
            }

            await Task.Delay(400).ConfigureAwait(false);
            if (connected)
            {
                return;
            }

            await Task.Delay(800).ConfigureAwait(false);
            if (connected)
            {
                return;
            }

            throw new InvalidOperationException("Not connected to the server!");
        }
    }
}
