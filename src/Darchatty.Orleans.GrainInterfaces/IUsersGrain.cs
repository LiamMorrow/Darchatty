using System.Threading.Tasks;
using Orleans;

namespace Darchatty.Orleans.GrainInterfaces
{
    public interface IUsersGrain : IGrainWithIntegerKey
    {
        Task<IUserGrain> GetUserWithUsernameAsync(string username);

        Task CreateUserAsync(string username, string password);
    }
}
