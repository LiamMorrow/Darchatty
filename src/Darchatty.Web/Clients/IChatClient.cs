using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Darchatty.Web.Model;

namespace Darchatty.Web.Clients
{
    public interface IChatClient
    {
        Task NewChatInfoAsync(Guid chatId);
    }
}
