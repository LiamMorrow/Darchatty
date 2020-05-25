using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Darchatty.Web.Clients;
using Darchatty.Web.Hubs;
using Darchatty.Web.Model;
using Darchatty.WebApp.Configuration;
using Darchatty.WebApp.Model;
using Darchatty.WebApp.Service;
using Darchatty.WebApp.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;

namespace Darchatty.WebApp.Hubs
{
    public class ChatHubClient : IChatHub, IChatClient
    {
        private readonly HubConnection hubConnection;
        private readonly StateService stateService;
        private readonly AsyncReaderWriterLock readerWriterLock = new AsyncReaderWriterLock();
        private volatile int started;

        public ChatHubClient(
            GatewayConfiguration gatewayConfiguration,
            StateService stateService)
        {
            this.stateService = stateService;
            hubConnection = new HubConnectionBuilder()
                .WithAutomaticReconnect()
                .WithUrl(gatewayConfiguration.Endpoint + "/chat", opts =>
                {
                    opts.AccessTokenProvider = () =>
                    {
                        return Task.FromResult(stateService.State.UserId.ToString());
                    };
                    opts.SkipNegotiation = true;
                    opts.Transports = HttpTransportType.WebSockets;
                })
                .Build();
            Action stateChangeHandler = () =>
            {
                _ = Task.Run(async () =>
                  {
                      using var l = await readerWriterLock.WriterLockAsync();
                      await hubConnection.StopAsync();
                      if (stateService.State.UserId == null)
                      {
                          started = 0;
                          return;
                      }

                      await StartOrNoopAsync();
                  });
            };
            stateChangeHandler = stateChangeHandler.Debounce(1000);

            hubConnection.On<string>(nameof(NewChatInfoAsync), NewChatInfoAsync);
            stateService.PropertyChanged += (o, e) =>
            {
                if (started == 1 && e.PropertyName == nameof(stateService.State.UserId))
                {
                    stateChangeHandler();
                }
            };
        }

        public async Task<List<ChatMessageDto>> GetMessagesAsync(Guid chatId)
        {
            await StartOrNoopAsync().ConfigureAwait(false);
            using var l = await readerWriterLock.ReaderLockAsync().ConfigureAwait(false);
            return await hubConnection.InvokeAsync<List<ChatMessageDto>>(nameof(GetMessagesAsync), chatId).ConfigureAwait(false);
        }

        public async Task<string> GetNameAsync(Guid userId)
        {
            await StartOrNoopAsync().ConfigureAwait(false);
            using var l = await readerWriterLock.ReaderLockAsync().ConfigureAwait(false);
            return await hubConnection.InvokeAsync<string>(nameof(GetNameAsync), userId).ConfigureAwait(false);
        }

        public async Task SendMessageAsync(Guid chatId, string messageContentsRaw)
        {
            await StartOrNoopAsync().ConfigureAwait(false);
            using var l = await readerWriterLock.ReaderLockAsync().ConfigureAwait(false);
            await hubConnection.InvokeAsync(nameof(SendMessageAsync), chatId, messageContentsRaw).ConfigureAwait(false);
        }

        public async Task NewChatInfoAsync(string chatIdStr)
        {
            var chatId = Guid.Parse(chatIdStr);
            var messagesTask = GetMessagesAsync(chatId);
            var chatDetailsTask = GetChatInfoAsync(chatId);
            await Task.WhenAll(messagesTask, chatDetailsTask).ConfigureAwait(false);
            var chatDetails = await chatDetailsTask;
            var messages = await messagesTask;
            stateService.UpdateChatState(
                new ChatState(
                    chatId: chatId,
                    chatName: chatDetails.ChatName,
                    messages: messages.ToImmutableList(),
                    participantIds: chatDetails.ParticipantIds.ToImmutableHashSet()));

            // We own the participantIds object, so this mutation is safe
            chatDetails.ParticipantIds.ExceptWith(stateService.State.Participants.Keys);
            var unknownParticipants = chatDetails.ParticipantIds;
            var newParticipantInfo = (await Task.WhenAll(unknownParticipants.Select(participantId => GetParticipantInfoAsync(participantId))))
                .Select(x => new ParticipantState(x.ParticipantId, x.ParticipantName));
            stateService.UpdateParticipants(newParticipantInfo);
        }

        public async Task<ParticipantDto> GetParticipantInfoAsync(Guid participantId)
        {
            await StartOrNoopAsync().ConfigureAwait(false);
            using var l = await readerWriterLock.ReaderLockAsync().ConfigureAwait(false);
            return await hubConnection.InvokeAsync<ParticipantDto>(nameof(GetParticipantInfoAsync), participantId)
                .ConfigureAwait(false);
        }

        public async Task<ChatDto> GetChatInfoAsync(Guid chatId)
        {
            await StartOrNoopAsync().ConfigureAwait(false);
            using var l = await readerWriterLock.ReaderLockAsync().ConfigureAwait(false);
            return await hubConnection.InvokeAsync<ChatDto>(nameof(GetChatInfoAsync), chatId)
                .ConfigureAwait(false);
        }

        public async Task<List<ChatDto>> GetParticipatingChatsAsync()
        {
            await StartOrNoopAsync().ConfigureAwait(false);
            using var l = await readerWriterLock.ReaderLockAsync().ConfigureAwait(false);
            return await hubConnection.InvokeAsync<List<ChatDto>>(nameof(GetParticipatingChatsAsync))
                .ConfigureAwait(false);
        }

        public async Task CreateChatAsync(string chatName)
        {
            await StartOrNoopAsync().ConfigureAwait(false);
            using var l = await readerWriterLock.ReaderLockAsync().ConfigureAwait(false);
            await hubConnection.InvokeAsync(nameof(CreateChatAsync), chatName).ConfigureAwait(false);
        }

        private async ValueTask StartOrNoopAsync()
        {
            if (Interlocked.CompareExchange(ref started, 1, 0) == 0)
            {
                await hubConnection.StartAsync().ConfigureAwait(false);
            }
        }
    }
}
