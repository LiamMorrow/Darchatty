using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Darchatty.Orleans.GrainInterfaces
{
    public interface IUsersGrain : IGrainWithIntegerKey
    {
        Task<IUserGrain> GetUserWithUsernameAsync(string username);

        Task<IUserGrain> CreateUserAsync(string username, string password);

        Task<List<IUserGrain>> SearchUsersAsync(string query);
    }
}
