using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using Darchatty.Web.Model;

namespace Darchatty.WebApp.Model
{
    public class State : IEquatable<State>
    {
        private readonly ImmutableDictionary<Guid, ChatState> chats;
        private readonly ImmutableDictionary<Guid, ParticipantState> participants;

        public State(
            Guid? userId,
            ImmutableDictionary<Guid, ChatState> chats,
            ImmutableDictionary<Guid, ParticipantState> participants)
        {
            UserId = userId;
            this.chats = chats;
            this.participants = participants;
        }

        public Guid? UserId { get; }

        public IReadOnlyDictionary<Guid, ChatState> Chats => chats;

        public IReadOnlyDictionary<Guid, ParticipantState> Participants => participants;

        public override bool Equals(object? obj)
        {
            return obj is State other && Equals(other);
        }

        public bool Equals(State other)
        {
            return EqualityComparer<Guid?>.Default.Equals(UserId, other.UserId) &&
                   chats == other.chats &&
                   participants == other.participants;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UserId, Chats, Participants);
        }
    }
}
