// ToDoWeb.Web.Client/Program.cs
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Authorization;
using ToDoWeb.Shared.Services;
using Supabase;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<ToDoWeb.Shared.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var supabaseUrl = builder.Configuration["Supabase:Url"] ?? "YOUR_SUPABASE_URL";
var supabaseKey = builder.Configuration["Supabase:AnonKey"] ?? "YOUR_SUPABASE_ANON_KEY";

// Register Client on WASM side
builder.Services.AddScoped(provider => new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions
{
    AutoRefreshToken = true,
    AutoConnectRealtime = false
}));

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<SupabaseAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<SupabaseAuthenticationStateProvider>());
builder.Services.AddScoped<TodoService>();

await builder.Build().RunAsync();