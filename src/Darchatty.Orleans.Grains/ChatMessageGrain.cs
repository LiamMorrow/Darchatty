using System;
using System.Threading.Tasks;
using Darchatty.Orleans.GrainInterfaces;
using Darchatty.Orleans.GrainInterfaces.Model;
using Darchatty.Orleans.Grains.State;
using Orleans;

namespace Darchatty.Orleans.Grains
{
    public class ChatMessageGrain : TimedPersistGrain<ChatMessageGrainState?>, IChatMessageGrain
    {
        public Task<ChatMessage> GetMessageInfoAsync()
        {
            // AssignAll enable
            return Task.FromResult(
                new ChatMessage
                {
                    MessageId = this.GetPrimaryKey(),
                    ChatMessageNumber = State?.ChatMessageNumber ?? -1,
                    SenderUserId = State?.SenderUserId ?? default,
                    SendTime = State?.SendTime ?? default,
                    MessageContentsRaw = State?.MessageContentsRaw ?? string.Empty,
                });
        }

        public Task UpdateMessageDetailsAsync(SendChatMessage message, int chatMessageNumber)
        {
            Dirty = true;
            State = new ChatMessageGrainState
            {
                ChatMessageNumber = chatMessageNumber,
                SenderUserId = message.SenderUserId,
                MessageContentsRaw = message.MessageContentsRaw,
                SendTime = DateTimeOffset.UtcNow,
            };
            return Task.CompletedTask;
        }
    }
}
