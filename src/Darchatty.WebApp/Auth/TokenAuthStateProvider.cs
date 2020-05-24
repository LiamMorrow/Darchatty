using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Darchatty.WebApp.Model;
using Darchatty.WebApp.Service;
using Microsoft.AspNetCore.Components.Authorization;

namespace Darchatty.WebApp.Auth
{
    public class TokenAuthStateProvider : AuthenticationStateProvider, IDisposable
    {
        private readonly ILocalStorageService localStorage;
        private readonly StateService stateService;

        public TokenAuthStateProvider(ILocalStorageService localStorage, StateService stateService)
        {
            this.stateService = stateService;
            this.localStorage = localStorage;
            localStorage.Changed += OnStateChange;
        }

        public void Dispose()
        {
            localStorage.Changed -= OnStateChange;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var userIdStr = await localStorage.GetItemAsync<string>("userId");
            if (Guid.TryParse(userIdStr, out var userId))
            {
                var identity = new ClaimsIdentity(
                    new[]
                    {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    }, "Fake authentication type");

                var user = new ClaimsPrincipal(identity);
                stateService.UpdateUserId(userId);
                return new AuthenticationState(user);
            }

            return new AuthenticationState(new ClaimsPrincipal());
        }

        private void OnStateChange(object o, ChangedEventArgs eventArgs)
        {
            if (eventArgs.Key == "userId")
            {
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }
        }
    }
}
