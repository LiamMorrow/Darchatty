using System.Threading.Tasks;
using Darchatty.Orleans.GrainInterfaces;
using Darchatty.Orleans.GrainInterfaces.Model;
using Darchatty.Orleans.Grains.State;
using Orleans;

namespace Darchatty.Orleans.Grains
{
    public class UserGrain : TimedPersistGrain<UserGrainState?>, IUserGrain
    {
        public Task<UserDetails> GetUserAsync()
        {
            return Task.FromResult(
                new UserDetails
                {
                    Name = State?.Name ?? "Unknown user",
                });
        }

        public Task UpdateUserAsync(UserDetails user)
        {
            State = new UserGrainState
            {
                Name = user.Name,
            };
            Dirty = true;
            return Task.CompletedTask;
        }
    }
}
