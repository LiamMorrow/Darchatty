using System.Collections.Generic;
using System.Threading.Tasks;
using Darchatty.Orleans.GrainInterfaces.Model;
using Orleans;

namespace Darchatty.Orleans.GrainInterfaces
{
    public interface IChatGrain : IGrainWithStringKey
    {
         Task<IEnumerable<ChatMessage>> GetMessagesAsync(int lastMessageIndex, int count);

         Task SendMessageAsync(SendChatMessage message);
    }
}
