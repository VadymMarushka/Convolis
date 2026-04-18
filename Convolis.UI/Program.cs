using Convolis.UI;
using Convolis.UI.Http;
using Convolis.UI.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var baseUrl = builder.Configuration["ApiBaseUrl"]!;

// Auth state (singleton - holds token in memory + localStorage)
builder.Services.AddSingleton<AuthStateService>();

// HTTP handler that attaches JWT
builder.Services.AddTransient<AuthenticatedHttpHandler>();

// Authenticated HttpClient for API calls
builder.Services.AddHttpClient<ChatService>(client =>
    client.BaseAddress = new Uri(baseUrl))
    .AddHttpMessageHandler<AuthenticatedHttpHandler>();

builder.Services.AddHttpClient<AuthService>(client =>
    client.BaseAddress = new Uri(baseUrl));

// SignalR hub service
builder.Services.AddSingleton<HubService>();

await builder.Build().RunAsync();