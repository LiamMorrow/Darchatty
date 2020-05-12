using System.Threading.Tasks;
using Darchatty.Web.Clients;
using Darchatty.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Darchatty.Gateway.Hubs
{
    public class ChatHub : Hub<IChatClient>, IChatHub
    {
        public Task RequestNameAsync()
        {
            return Clients.Caller.RecieveNameAsync("Lime");
        }
    }
}
