using System;
using System.Threading.Tasks;
using Darchatty.Data;
using Darchatty.Data.Model;
using Darchatty.Orleans.GrainInterfaces;
using Microsoft.AspNetCore.Identity;
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

        public async Task CreateUserAsync(string username, string password)
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
        }
    }
}
