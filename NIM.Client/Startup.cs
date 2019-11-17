using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace NIM.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<Shared.Models.GameState>();
            services.AddSingleton(_ => new Shared.Models.Settings
            {
                Rows = new List<int> { 1, 2, 3, 4 },
                LastMoveWins = true,
                ChangesPerRow = 3,
                Skin = "Torch.png",
            });
            services.AddSingleton<Shared.ViewModels.SettingsViewModel>();
            services.AddTransient<Shared.ViewModels.NimViewModel>();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
