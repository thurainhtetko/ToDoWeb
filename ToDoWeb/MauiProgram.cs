using System.Text.Json;
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

            var (fileUrl, fileKey) = LoadSupabaseSettings();
            var supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? fileUrl;
            var supabaseKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY") ?? fileKey;

            if (string.IsNullOrWhiteSpace(supabaseUrl) || !supabaseUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Supabase URL is missing or invalid. Configure Resources/Raw/appsettings.json or set the SUPABASE_URL environment variable.");
            }

            if (string.IsNullOrWhiteSpace(supabaseKey))
            {
                throw new InvalidOperationException("Supabase anon key is missing. Configure Resources/Raw/appsettings.json or set the SUPABASE_ANON_KEY environment variable.");
            }

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

        private static (string? Url, string? Key) LoadSupabaseSettings()
        {
            using var stream = Microsoft.Maui.Storage.FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult();
            using var doc = JsonDocument.Parse(stream);
            var supabase = doc.RootElement.GetProperty("Supabase");
            return (supabase.GetProperty("Url").GetString(), supabase.GetProperty("AnonKey").GetString());
        }
    }
}
