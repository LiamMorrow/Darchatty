using System;
using System.Threading.Tasks;
using Darchatty.Web.Model;

namespace Darchatty.Web.Hubs
{
    public interface IAuthHub
    {
        Task<Guid> LoginAsync(LoginDto request);

        Task<Guid> RegisterAsync(RegisterDto request);
    }
}
