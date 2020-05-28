using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Darchatty.Orleans.GrainInterfaces;
using Darchatty.Orleans.GrainInterfaces.Model;
using Darchatty.Web.Clients;
using Darchatty.Web.Hubs;
using Darchatty.Web.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Orleans;

namespace Darchatty.Gateway.Hubs
{
    [AllowAnonymous]
    public class AuthHub : Hub<IAuthClient>, IAuthHub
    {
        private readonly IGrainFactory grainFactory;

        public AuthHub(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public async Task<Guid> LoginAsync(LoginDto request)
        {
            var users = grainFactory.GetGrain<IUsersGrain>(0);
            var userGrain = await users.GetUserWithUsernameAsync(request.Username);
            return userGrain.GetPrimaryKey();
        }

        public async Task<Guid> RegisterAsync(RegisterDto request)
        {
            var users = grainFactory.GetGrain<IUsersGrain>(0);
            var userGrain = await users.CreateUserAsync(request.Username, request.Password);
            return userGrain.GetPrimaryKey();
        }
    }
}
