using Convolis.UI;
using Convolis.UI.Http;
using Convolis.UI.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Retrieve API URL from appsettings.json
var baseUrl = builder.Configuration["ApiBaseUrl"]
              ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");

// --- Services Configuration ---

// AuthStateService: Manages JWT and user session state in memory
builder.Services.AddSingleton<AuthStateService>();

// AuthenticatedHttpHandler: Intercepts outgoing requests to attach the Bearer token
builder.Services.AddTransient<AuthenticatedHttpHandler>();

// AuthService: Standard client for Login/Register (no token needed)
builder.Services.AddHttpClient<AuthService>(client =>
    client.BaseAddress = new Uri(baseUrl));

// ChatService: Authenticated client for all chat operations
builder.Services.AddHttpClient<ChatService>(client =>
    client.BaseAddress = new Uri(baseUrl))
    .AddHttpMessageHandler<AuthenticatedHttpHandler>();

// HubService: Manages SignalR real-time communication
builder.Services.AddSingleton<HubService>();

// --- Application Start ---
await builder.Build().RunAsync();