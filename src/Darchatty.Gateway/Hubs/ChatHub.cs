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
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
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

        public async Task<Guid> CreateChatAsync(string chatName, List<Guid> otherParticipants)
        {
            var chatId = Guid.NewGuid();
            var chatGrain = grainFactory.GetGrain<IChatGrain>(chatId.ToString());
            await chatGrain.UpdateChatDetailsAsync(chatName);
            var participantIds = otherParticipants.Append(GetUserId());
            foreach (var participantId in participantIds)
            {
                var userGrain = grainFactory.GetGrain<IUserGrain>(participantId);
                await chatGrain.AddParticipantToChatAsync(participantId);
                await userGrain.AddToParticipatingChatsAsync(chatId);
            }

            await Clients.Users(participantIds.Select(x => x.ToString()).ToList()).NewChatInfoAsync(chatId.ToString());
            return chatId;
        }

        public async Task<List<Guid>> SearchUsersAsync(string query)
        {
            var usersGrain = grainFactory.GetGrain<IUsersGrain>(0);
            var users = await usersGrain.SearchUsersAsync(query);
            return users.Select(x => x.GetPrimaryKey()).ToList();
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
