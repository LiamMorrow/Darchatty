using Microsoft.AspNetCore.Components;

namespace Darchatty.WebApp.Shared
{
    public class RedirectToLogin : ComponentBase
    {
        [Inject]
        protected NavigationManager NavigationManager { get; set; } = null!;

        protected override void OnInitialized()
        {
            NavigationManager.NavigateTo("login");
        }
    }
}
