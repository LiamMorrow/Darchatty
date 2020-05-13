using System.Threading.Tasks;
using Darchatty.Orleans.GrainInterfaces.Model;
using Orleans;

namespace Darchatty.Orleans.GrainInterfaces
{
    public interface IUserGrain : IGrainWithGuidKey
    {
         Task<UserDetails> GetUserAsync();

         Task UpdateUserAsync(UserDetails user);
    }
}
