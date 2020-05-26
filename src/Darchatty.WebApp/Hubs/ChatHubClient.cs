using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Darchatty.Web.Clients;
using Darchatty.Web.Hubs;
using Darchatty.Web.Model;
using Darchatty.WebApp.Configuration;
using Darchatty.WebApp.Model;
using Darchatty.WebApp.Service;
using Darchatty.WebApp.Shared;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;

namespace Darchatty.WebApp.Hubs
{
    public class ChatHubClient : IChatHub, IChatClient
    {
        private readonly HubConnection hubConnection;
        private readonly StateService stateService;
        private bool connected;

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
            Action userIdChangeHandler = () =>
            {
                _ = Task.Run(async () =>
                  {
                      try
                      {
                          if (connected)
                          {
                              connected = false;
                              await hubConnection.StopAsync();
                          }

                          if (stateService.State.UserId != null)
                          {
                              await hubConnection.StartAsync();
                              connected = true;
                          }
                      }
                      catch (Exception ex)
                      {
                          Console.WriteLine(ex);
                      }
                  });
            };
            userIdChangeHandler = userIdChangeHandler.Debounce(1000);

            hubConnection.On<string>(nameof(NewChatInfoAsync), NewChatInfoAsync);
            hubConnection.Closed += _ =>
            {
                connected = false;
                return Task.CompletedTask;
            };
            hubConnection.Reconnected += (e) =>
            {
                connected = true;
                return Task.CompletedTask;
            };

            stateService.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(stateService.State.UserId))
                {
                    userIdChangeHandler();
                }
            };
        }

        public async Task<List<ChatMessageDto>> GetMessagesAsync(Guid chatId)
        {
            await EnsureConnectedAsync().ConfigureAwait(false);
            return await hubConnection.InvokeAsync<List<ChatMessageDto>>(nameof(GetMessagesAsync), chatId).ConfigureAwait(false);
        }

        public async Task<string> GetNameAsync(Guid userId)
        {
            await EnsureConnectedAsync().ConfigureAwait(false);
            return await hubConnection.InvokeAsync<string>(nameof(GetNameAsync), userId).ConfigureAwait(false);
        }

        public async Task SendMessageAsync(Guid chatId, string messageContentsRaw)
        {
            await EnsureConnectedAsync().ConfigureAwait(false);
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
            await EnsureConnectedAsync().ConfigureAwait(false);
            return await hubConnection.InvokeAsync<ParticipantDto>(nameof(GetParticipantInfoAsync), participantId)
                .ConfigureAwait(false);
        }

        public async Task<ChatDto> GetChatInfoAsync(Guid chatId)
        {
            await EnsureConnectedAsync().ConfigureAwait(false);
            return await hubConnection.InvokeAsync<ChatDto>(nameof(GetChatInfoAsync), chatId)
                .ConfigureAwait(false);
        }

        public async Task<List<ChatDto>> GetParticipatingChatsAsync()
        {
            await EnsureConnectedAsync().ConfigureAwait(false);
            return await hubConnection.InvokeAsync<List<ChatDto>>(nameof(GetParticipatingChatsAsync))
                .ConfigureAwait(false);
        }

        public async Task CreateChatAsync(string chatName)
        {
            await EnsureConnectedAsync().ConfigureAwait(false);
            await hubConnection.InvokeAsync(nameof(CreateChatAsync), chatName).ConfigureAwait(false);
        }

        private async ValueTask EnsureConnectedAsync()
        {
            if (connected)
            {
                return;
            }

            await Task.Delay(50).ConfigureAwait(false);
            if (connected)
            {
                return;
            }

            await Task.Delay(100).ConfigureAwait(false);
            if (connected)
            {
                return;
            }

            await Task.Delay(200).ConfigureAwait(false);
            if (connected)
            {
                return;
            }

            await Task.Delay(400).ConfigureAwait(false);
            if (connected)
            {
                return;
            }

            await Task.Delay(800).ConfigureAwait(false);
            if (connected)
            {
                return;
            }

            throw new InvalidOperationException("Not connected to the server!");
        }
    }
}
