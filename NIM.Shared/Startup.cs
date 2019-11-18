using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using NIM.Shared.Models;
using NIM.Shared.ViewModels;

namespace NIM.Shared
{
   public static class Startup
    {
        public static IServiceCollection ConfigureNimServices(this IServiceCollection services)
        {
            services.AddScoped<Models.GameState>();
            services.AddScoped(_ => new Settings
            {
                Rows = new List<int> { 1, 2, 3, 4 },
                LastMoveWins = true,
                ChangesPerRow = 3,
                Skin = "Torch.png",
            });
            services.AddScoped<IndexViewModel>();
            services.AddScoped<SettingsViewModel>();
            services.AddTransient<NimViewModel>();
            return services;
        }
    }
}
