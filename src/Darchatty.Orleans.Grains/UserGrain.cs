using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Darchatty.Data.Model;
using Darchatty.Orleans.GrainInterfaces;
using Darchatty.Orleans.GrainInterfaces.Model;
using Darchatty.Orleans.Grains.State;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Orleans;

namespace Darchatty.Orleans.Grains
{
    public class UserGrain : TimedPersistGrain<UserGrainState>, IUserGrain
    {
        public Task AddToParticipatingChatsAsync(Guid chatId)
        {
            State.ParticipatingChats ??= new HashSet<Guid>();
            if (State.ParticipatingChats.Add(chatId))
            {
                Dirty = true;
            }

            return GrainFactory.GetGrain<IChatGrain>(chatId.ToString())
                .AddParticipantToChatAsync(this.GetPrimaryKey());
        }

        public Task<HashSet<IChatGrain>> GetParticipatingChatsAsync()
        {
            if (State.ParticipatingChats == null)
            {
                return Task.FromResult(new HashSet<IChatGrain>());
            }

            var chatGrains = State.ParticipatingChats
                    .Select(chatId => GrainFactory.GetGrain<IChatGrain>(chatId.ToString()))
                    .ToHashSet();
            return Task.FromResult(chatGrains);
        }

        public Task<UserDetails> GetUserAsync()
        {
            return Task.FromResult(
                new UserDetails
                {
                    Name = State?.Name ?? "Unknown user",
                });
        }

        public async Task<bool> PasswordMatchesAsync(string password)
        {
            using var scope = ServiceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserDao>>();
            var user = await userManager.FindByIdAsync(this.GetPrimaryKey().ToString());
            if (user == null)
            {
                return false;
            }

            return await userManager.CheckPasswordAsync(user, password);
        }

        public Task RemoveFromPartitipatingChatAsync(Guid chatId)
        {
            if (State?.ParticipatingChats?.Remove(chatId) ?? false)
            {
                Dirty = true;
            }

            return GrainFactory.GetGrain<IChatGrain>(chatId.ToString())
                .RemoveParticipantFromChatAsync(this.GetPrimaryKey());
        }

        public Task UpdateUserAsync(UserDetails user)
        {
            State.Name = user.Name;
            Dirty = true;
            return Task.CompletedTask;
        }
    }
}
