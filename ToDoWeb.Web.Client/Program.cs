using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ToDoWeb.Shared.Services;
using ToDoWeb.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add device-specific services used by the ToDoWeb.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

await builder.Build().RunAsync();
