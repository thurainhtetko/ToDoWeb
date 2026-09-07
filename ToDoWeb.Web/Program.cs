using ToDoWeb.Shared.Services;
using ToDoWeb.Web.Components;
using ToDoWeb.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization; // Added for Auth
using Supabase;

var builder = WebApplication.CreateBuilder(args);
var supabaseUrl = builder.Configuration["Supabase:Url"] ?? "YOUR_SUPABASE_URL";
var supabaseKey = builder.Configuration["Supabase:AnonKey"] ?? "YOUR_SUPABASE_ANON_KEY";

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// 1. Register Supabase Client
// Registered as Scoped so every user circuit/connection gets its own isolated instance.
builder.Services.AddScoped(provider => new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions
{
    AutoRefreshToken = true,
    AutoConnectRealtime = true
}));

// 2. Add Blazor Core Authentication Services
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState(); // Enables cascading auth globally in .NET 8+
// Auth is client-side (WASM localStorage). Register an authentication scheme so the server
// can handle [Authorize] challenges by redirecting anonymous requests to the login page.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
    });
builder.Services.AddScoped<SupabaseAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<SupabaseAuthenticationStateProvider>());
builder.Services.AddScoped<TodoService>();

// Add device-specific services used by the ToDoWeb.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(ToDoWeb.Shared._Imports).Assembly,
        typeof(ToDoWeb.Web.Client._Imports).Assembly);

app.Run();
