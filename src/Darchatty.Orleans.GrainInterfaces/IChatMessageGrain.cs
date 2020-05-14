using System.Threading.Tasks;
using Darchatty.Orleans.GrainInterfaces.Model;
using Orleans;

namespace Darchatty.Orleans.GrainInterfaces
{
    public interface IChatMessageGrain : IGrainWithIntegerCompoundKey
    {
        Task<ChatMessage> GetMessageInfoAsync();

        Task UpdateMessageDetailsAsync(SendChatMessage message, int chatMessageNumber);
    }
}
