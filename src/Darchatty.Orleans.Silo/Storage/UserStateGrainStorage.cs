using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Darchatty.Data.Model;
using Darchatty.Orleans.Grains.State;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;

namespace Darchatty.Orleans.Silo.Storage
{
    public class UserStateGrainStorage : IGrainStorage
    {
        private readonly IServiceProvider services;

        public UserStateGrainStorage(IServiceProvider services)
        {
            this.services = services;
        }

        public Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            return Task.CompletedTask;
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            using var scope = services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<UserManager<UserDao>>();
            var user = await repository.FindByIdAsync(grainReference.GetPrimaryKey().ToString());
            grainState.State = new UserGrainState
            {
                Name = user?.DisplayName,
                ParticipatingChats = new HashSet<Guid>(), // TODO
            };
        }

        public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            if (!(grainState.State is UserGrainState state) || state.Name == null)
            {
                return;
            }

            using var scope = services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<UserManager<UserDao>>();
            var user = await repository.FindByIdAsync(grainReference.GetPrimaryKey().ToString());
            if (user == null)
            {
                // Todo should do something like throw here, but for now nvm
                return;
            }

            user.DisplayName = state.Name;

            await repository.UpdateAsync(user);
        }
    }
}
