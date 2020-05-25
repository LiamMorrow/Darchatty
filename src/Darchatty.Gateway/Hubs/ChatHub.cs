using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Darchatty.Orleans.GrainInterfaces;
using Darchatty.Orleans.GrainInterfaces.Model;
using Darchatty.Web.Clients;
using Darchatty.Web.Hubs;
using Darchatty.Web.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Orleans;

namespace Darchatty.Gateway.Hubs
{
    [Authorize]
    public class ChatHub : Hub<IChatClient>, IChatHub
    {
        private readonly IGrainFactory grainFactory;

        public ChatHub(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public async Task<List<ChatMessageDto>> GetMessagesAsync(Guid chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
            return await GetLatestChatMessagesAsync(chatId);
        }

        public async Task SendMessageAsync(Guid chatId, string messageContentsRaw)
        {
            var chatGrain = grainFactory.GetGrain<IChatGrain>(chatId.ToString());
            await chatGrain.SendMessageAsync(new SendChatMessage
            {
                MessageContentsRaw = messageContentsRaw,
                SenderUserId = GetUserId(),
            });
            await Clients.Group(chatId.ToString()).NewChatInfoAsync(chatId.ToString());
        }

        public async Task<List<ChatDto>> GetParticipatingChatsAsync()
        {
            var userGrain = grainFactory.GetGrain<IUserGrain>(GetUserId());
            var chatGrains = await userGrain.GetParticipatingChatsAsync();
            // AssignAll enable
            return (await Task.WhenAll(chatGrains.Select(grain => grain.GetChatDetailsAsync())))
                .Select(chatData => new ChatDto
                {
                    ChatId = chatData.ChatId,
                    ChatName = chatData.ChatName,
                    ParticipantIds = chatData.Participants,
                }).ToList();
        }

        public async Task<ParticipantDto> GetParticipantInfoAsync(Guid participantId)
        {
            var userGrain = grainFactory.GetGrain<IUserGrain>(participantId);
            var user = await userGrain.GetUserAsync();
            return new ParticipantDto
            {
                ParticipantId = participantId,
                ParticipantName = user.Name,
            };
        }

        public async Task<ChatDto> GetChatInfoAsync(Guid chatId)
        {
            var chatGrain = grainFactory.GetGrain<IChatGrain>(chatId.ToString());
            var chat = await chatGrain.GetChatDetailsAsync();
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
            return new ChatDto
            {
                ChatId = chatId,
                ChatName = chat.ChatName,
                ParticipantIds = chat.Participants,
            };
        }

        public async Task CreateChatAsync(string chatName)
        {
            var chatId = Guid.NewGuid();
            var chatGrain = grainFactory.GetGrain<IChatGrain>(chatId.ToString());
            var userGrain = grainFactory.GetGrain<IUserGrain>(GetUserId());
            await chatGrain.UpdateChatDetailsAsync(chatName);
            await chatGrain.AddParticipantToChatAsync(GetUserId());
            await userGrain.AddToParticipatingChatsAsync(chatId);
            await Clients.Caller.NewChatInfoAsync(chatId.ToString());
        }

        private async Task<List<ChatMessageDto>> GetLatestChatMessagesAsync(Guid chatId)
        {
            var chatGrain = grainFactory.GetGrain<IChatGrain>(chatId.ToString());
            var messageGrains = await chatGrain.GetMessagesAsync(10);
            var messageTasks = messageGrains.Select(msgGrain => msgGrain.GetMessageInfoAsync());
            // AssignAll enable
            var messages = (await Task.WhenAll(messageTasks))
                .Select(msg => new ChatMessageDto
                {
                    ChatMessageNumber = msg.ChatMessageNumber,
                    SenderUserId = msg.SenderUserId,
                    SendTime = msg.SendTime,
                    MessageContentsRaw = msg.MessageContentsRaw,
                });

            return messages.ToList();
        }

        private Guid GetUserId()
        {
            return Guid.Parse(Context.UserIdentifier);
        }
    }
}
