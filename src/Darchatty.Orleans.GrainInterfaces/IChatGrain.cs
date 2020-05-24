using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Darchatty.Orleans.GrainInterfaces.Model;
using Orleans;

namespace Darchatty.Orleans.GrainInterfaces
{
    public interface IChatGrain : IGrainWithStringKey
    {
        Task<List<IChatMessageGrain>> GetMessagesAsync(int count, int lastMessageIndex = -1);

        Task SendMessageAsync(SendChatMessage message);

        Task<ChatDetails> GetChatDetailsAsync();

        Task UpdateChatDetailsAsync(string chatName);

        Task RemoveParticipantFromChatAsync(Guid participantId);

        Task AddParticipantToChatAsync(Guid participantId);
    }
}
