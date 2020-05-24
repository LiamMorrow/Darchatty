using System;
using System.Collections.Generic;
using Darchatty.Web.Model;

namespace Darchatty.WebApp.Model
{
    public class ChatState
    {
        public ChatState(Guid chatId, string chatName, IEnumerable<ChatMessageDto> messages, ISet<Guid> participantIds)
        {
            ChatId = chatId;
            ChatName = chatName;
            Messages = messages;
            ParticipantIds = participantIds;
        }

        public Guid ChatId { get; }

        public string ChatName { get; }

        public IEnumerable<ChatMessageDto> Messages { get; }

        public ISet<Guid> ParticipantIds { get; }
    }
}
