using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Darchatty.WebApp.Model;

namespace Darchatty.WebApp.Service
{
    public class StateService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public State State { get; private set; } = new State(default, new Dictionary<Guid, ChatState>(), new Dictionary<Guid, ParticipantState>());

        public void UpdateChatState(ChatState chatState)
        {
            var chats = State.Chats.ToDictionary(x => x.Key, x => x.Value);
            chats[chatState.ChatId] = chatState;
            State = new State(State.UserId, chats, State.Participants);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State.Chats)));
        }

        public void UpdateUserId(Guid userId)
        {
            State = new State(userId, State.Chats, State.Participants);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State.UserId)));
        }
    }
}
