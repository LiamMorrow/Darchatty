using System.Threading.Tasks;

namespace Darchatty.Web.Clients
{
    public interface IChatClient
    {
        Task RecieveNameAsync(string name);
    }
}
