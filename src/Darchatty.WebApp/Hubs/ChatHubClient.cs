using System.Threading;
using System.Threading.Tasks;
using Darchatty.Web.Clients;
using Darchatty.Web.Hubs;
using Darchatty.WebApp.Configuration;
using Darchatty.WebApp.Model;
using Microsoft.AspNetCore.SignalR.Client;

namespace Darchatty.WebApp.Hubs
{
    public class ChatHubClient : IChatHub, IChatClient
    {
        private readonly HubConnection hubConnection;
        private readonly IState state;
        private int started;

        public ChatHubClient(
            GatewayConfiguration gatewayConfiguration,
            IState state)
        {
            this.state = state;
            hubConnection = new HubConnectionBuilder()
                .WithUrl(gatewayConfiguration.Endpoint + "/chat")
                .WithAutomaticReconnect()
                .Build();
        }

        public Task RecieveNameAsync(string name)
        {
            state.Name = name;
            return Task.CompletedTask;
        }

        public async Task RequestNameAsync()
        {
            await StartOrNoopAsync().ConfigureAwait(false);
            await hubConnection.InvokeAsync(nameof(RequestNameAsync)).ConfigureAwait(false);
        }

        private async ValueTask StartOrNoopAsync()
        {
            if (Interlocked.CompareExchange(ref started, 1, 0) == 0)
            {
                hubConnection.On(nameof(RecieveNameAsync), (string name) => RecieveNameAsync(name));
                await hubConnection.StartAsync().ConfigureAwait(false);
            }
        }
    }
}
