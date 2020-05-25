using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Darchatty.Web.Model;

namespace Darchatty.WebApp.Model
{
    public class ChatState : IEquatable<ChatState>
    {
        public ChatState(Guid chatId, string chatName, ImmutableList<ChatMessageDto> messages, ImmutableHashSet<Guid> participantIds)
        {
            ChatId = chatId;
            ChatName = chatName;
            Messages = messages;
            ParticipantIds = participantIds;
        }

        public Guid ChatId { get; }

        public string ChatName { get; }

        public IReadOnlyList<ChatMessageDto> Messages { get; }

        public IImmutableSet<Guid> ParticipantIds { get; }

        public override bool Equals(object? obj)
        {
            return obj is ChatState other && Equals(other);
        }

        public bool Equals(ChatState other)
        {
            return ChatId.Equals(other.ChatId) &&
                   ChatName == other.ChatName &&
                   Messages.SequenceEqual(other.Messages) &&
                   ParticipantIds.SetEquals(other.ParticipantIds);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ChatId, ChatName, Messages, ParticipantIds);
        }
    }
}
