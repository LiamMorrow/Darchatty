using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Darchatty.Orleans.GrainInterfaces;
using Darchatty.Orleans.GrainInterfaces.Model;
using Darchatty.Orleans.Grains.State;
using Orleans;

namespace Darchatty.Orleans.Grains
{
    public class ChatGrain : TimedPersistGrain<ChatGrainState>, IChatGrain
    {
        public Task<List<IChatMessageGrain>> GetMessagesAsync(int count, int latestMessageIndex = -1)
        {
            if (count <= 0)
            {
                throw new ArgumentException("count must be > 0");
            }

            if (State == null || State.MessageCount == 0)
            {
                return Task.FromResult(new List<IChatMessageGrain>());
            }

            if (latestMessageIndex < 0)
            {
                latestMessageIndex = State.MessageCount;
            }

            var messageGrains = GetDescendingRange(latestMessageIndex, Math.Max(1, latestMessageIndex - count))
                .Select(messageIndex => GrainFactory.GetGrain<IChatMessageGrain>(messageIndex, this.GetPrimaryKeyString()));

            return Task.FromResult(messageGrains.ToList());
        }

        public async Task SendMessageAsync(SendChatMessage message)
        {
            State.Participants ??= new HashSet<Guid>();
            State.Participants.Add(message.SenderUserId);
            var messageIndex = State.MessageCount + 1;
            var messageGrain = GrainFactory.GetGrain<IChatMessageGrain>(messageIndex, this.GetPrimaryKeyString());
            await messageGrain.UpdateMessageDetailsAsync(message, messageIndex);
            State.MessageCount++;
            Dirty = true;
        }

        public Task<ChatDetails> GetChatDetailsAsync()
        {
            // AssignAll enable
            return Task.FromResult(
                new ChatDetails
                {
                    ChatId = Guid.Parse(this.GetPrimaryKeyString()),
                    ChatName = State.ChatName ?? "Unnamed Chat",
                    Participants = State.Participants?.ToHashSet() ?? new HashSet<Guid>(),
                });
        }

        public Task RemoveParticipantFromChatAsync(Guid participantId)
        {
            State.Participants ??= new HashSet<Guid>();
            if (State.Participants.Remove(participantId))
            {
                Dirty = true;
            }

            return Task.CompletedTask;
        }

        public Task AddParticipantToChatAsync(Guid participantId)
        {
            State.Participants ??= new HashSet<Guid>();
            if (State.Participants.Add(participantId))
            {
                Dirty = true;
            }

            return Task.CompletedTask;
        }

        public Task UpdateChatDetailsAsync(string chatName)
        {
            State.ChatName = chatName;
            Dirty = true;
            return Task.CompletedTask;
        }

        private IEnumerable<int> GetDescendingRange(int from, int downTo)
        {
            for (var i = from; i >= downTo; i--)
            {
                yield return i;
            }
        }
    }
}
