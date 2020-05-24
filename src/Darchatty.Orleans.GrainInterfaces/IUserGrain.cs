using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Darchatty.Orleans.GrainInterfaces.Model;
using Orleans;

namespace Darchatty.Orleans.GrainInterfaces
{
    public interface IUserGrain : IGrainWithGuidKey
    {
        Task<UserDetails> GetUserAsync();

        Task UpdateUserAsync(UserDetails user);

        Task<HashSet<IChatGrain>> GetParticipatingChatsAsync();

        Task AddToParticipatingChatsAsync(Guid chatId);

        Task RemoveFromPartitipatingChatAsync(Guid chatId);
    }
}
