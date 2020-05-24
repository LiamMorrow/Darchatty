using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using Darchatty.Web.Model;

namespace Darchatty.WebApp.Model
{
    public class State
    {
        public State(
            Guid? userId,
            IReadOnlyDictionary<Guid, ChatState> chats,
            IReadOnlyDictionary<Guid, ParticipantState> participants)
        {
            UserId = userId;
            Chats = chats;
            Participants = participants;
        }

        public Guid? UserId { get; }

        public IReadOnlyDictionary<Guid, ChatState> Chats { get; }

        public IReadOnlyDictionary<Guid, ParticipantState> Participants { get; }
    }
}
