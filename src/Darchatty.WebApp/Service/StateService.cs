using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using Darchatty.WebApp.Model;

namespace Darchatty.WebApp.Service
{
    public class StateService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public State State { get; private set; } = new State(default, ImmutableDictionary<Guid, ChatState>.Empty, ImmutableDictionary<Guid, ParticipantState>.Empty);

        public void UpdateChatState(ChatState newChatState)
        {
            if (State.Chats.TryGetValue(newChatState.ChatId, out var existing) && existing.Equals(newChatState))
            {
                return;
            }

            var chats = State.Chats.AsImmutableDictionary()
                .SetItem(newChatState.ChatId, newChatState);
            State = new State(State.UserId, chats, State.Participants.AsImmutableDictionary());
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State.Chats)));
        }

        public void UpdateUserId(Guid userId)
        {
            if (userId == State.UserId)
            {
                return;
            }

            State = new State(userId, State.Chats.AsImmutableDictionary(), State.Participants.AsImmutableDictionary());
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State.UserId)));
        }

        public void UpdateParticipants(IEnumerable<ParticipantState> newParticipantInfo)
        {
            var participantsWithNewData = newParticipantInfo
                .Where(
                    newParticipant =>
                    !State.Participants.TryGetValue(newParticipant.ParticipantId, out var oldParticipant) ||
                    !newParticipant.Equals(oldParticipant))
                .Select(participant => new KeyValuePair<Guid, ParticipantState>(participant.ParticipantId, participant))
                .ToList();
            if (!participantsWithNewData.Any())
            {
                return;
            }

            var participants = State.Participants.AsImmutableDictionary()
                .SetItems(participantsWithNewData);
            State = new State(State.UserId, State.Chats.AsImmutableDictionary(), participants);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State.Participants)));
        }
    }
}
