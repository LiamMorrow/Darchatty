using System.Threading.Tasks;

namespace Darchatty.Web.Hubs
{
    public interface IChatHub
    {
        Task RequestNameAsync();
    }
}
