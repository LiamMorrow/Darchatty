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
    public class ChatGrain : TimedPersistGrain<ChatGrainState?>, IChatGrain
    {
        public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(int lastMessageIndex, int count)
        {
            if (count <= 0)
            {
                throw new ArgumentException("count must be > 0");
            }

            if (State == null || State.MessageCount == 0)
            {
                return Enumerable.Empty<ChatMessage>();
            }

            if (lastMessageIndex < 0)
            {
                lastMessageIndex = State.MessageCount;
            }

            var messageGrains = Enumerable.Range(Math.Max(lastMessageIndex - count, 1), lastMessageIndex - 1).Reverse()
                .Select(messageIndex => GrainFactory.GetGrain<IChatMessageGrain>(messageIndex, this.GetPrimaryKeyString()));

            var messageTasks = messageGrains.Select(grain => grain.GetMessageInfoAsync());
            await Task.WhenAll(messageTasks);

            return messageTasks.Select(x => x.Result).ToList();
        }

        public async Task SendMessageAsync(SendChatMessage message)
        {
            State ??= NewState();
            var messageIndex = State.MessageCount + 1;
            var messageGrain = GrainFactory.GetGrain<IChatMessageGrain>(messageIndex, this.GetPrimaryKeyString());
            await messageGrain.UpdateMessageDetailsAsync(message, messageIndex);
            State.MessageCount++;
        }

        private ChatGrainState NewState() => new ChatGrainState
        {
            MessageCount = 0,
        };
    }
}
