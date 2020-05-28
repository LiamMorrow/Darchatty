using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Darchatty.Data;
using Darchatty.Data.Model;
using Darchatty.Orleans.GrainInterfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Concurrency;

namespace Darchatty.Orleans.Grains
{
    [Reentrant]
    public class UsersGrain : Grain, IUsersGrain
    {
        public async Task<IUserGrain> GetUserWithUsernameAsync(string username)
        {
            using var scope = ServiceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserDao>>();
            UserDao? user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            return GrainFactory.GetGrain<IUserGrain>(Guid.Parse(user.Id));
        }

        public async Task<IUserGrain> CreateUserAsync(string username, string password)
        {
            using var scope = ServiceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserDao>>();
            var result = await userManager.CreateAsync(
                new UserDao
                {
                    DisplayName = username,
                    UserName = username,
                    EmailConfirmed = true,
                },
                password);
            if (!result.Succeeded)
            {
                throw new Exception("Failed to create user!");
            }

            var user = await userManager.FindByNameAsync(username);
            return GrainFactory.GetGrain<IUserGrain>(Guid.Parse(user.Id));
        }

        public async Task<List<IUserGrain>> SearchUsersAsync(string query)
        {
            using var scope = ServiceProvider.CreateScope();
            var userRepo = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            var userGrains = await userRepo.Users.Where(x => EF.Functions.ILike(x.DisplayName, $"%{query}%"))
                .Select(x => x.Id)
                .Select(id => GrainFactory.GetGrain<IUserGrain>(Guid.Parse(id), null))
                .ToListAsync();
            return userGrains;
        }
    }
}
