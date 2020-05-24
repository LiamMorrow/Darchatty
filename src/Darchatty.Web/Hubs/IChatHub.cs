using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Darchatty.Web.Model;

namespace Darchatty.Web.Hubs
{
    public interface IChatHub
    {
        Task<ParticipantDto> GetParticipantInfoAsync(Guid participantId);

        Task<ChatDto> GetChatInfoAsync(Guid chatId);

        Task<List<ChatMessageDto>> GetMessagesAsync(Guid chatId);

        Task SendMessageAsync(Guid chatId, string messageContentsRaw);

        Task<List<ChatDto>> GetParticipatingChatsAsync();

        Task CreateChatAsync(string chatName);
    }
}
