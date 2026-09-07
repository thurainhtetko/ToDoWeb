using Microsoft.Extensions.Logging;
using ToDoWeb.Services;
using ToDoWeb.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Supabase;

namespace ToDoWeb
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Add device-specific services used by the ToDoWeb.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();

            var supabaseUrl = builder.Configuration["Supabase:Url"] ?? Environment.GetEnvironmentVariable("SUPABASE_URL") ?? "YOUR_SUPABASE_URL";
            var supabaseKey = builder.Configuration["Supabase:AnonKey"] ?? Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY") ?? "YOUR_SUPABASE_ANON_KEY";

            builder.Services.AddScoped(provider => new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            }));

            builder.Services.AddAuthorizationCore();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<SupabaseAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<SupabaseAuthenticationStateProvider>());
            builder.Services.AddScoped<TodoService>();

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
