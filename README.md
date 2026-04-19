# 💬 Convolis

**Convolis** — real-time chat application with AI-powered sentiment analysis, built on **.NET 8**.

---

## ✨ Features

- **Real-time messaging** via Azure SignalR
- **AI Sentiment Analysis** — every message is analyzed by Azure Language Service and labeled as Positive, Neutral, or Negative
- **JWT Authentication** — Access + Refresh token flow with silent token refresh
- **Online tracking** — real-time online/offline status per conversation
- **Global Chat** — auto-joined public room for all users
- **Secure passwords** — hashed with `PasswordHasher<T>`

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core Web API (.NET 8) |
| Real-time | Azure SignalR Service |
| Database | MS SQL Server + Entity Framework Core |
| Auth | JWT Bearer |
| AI | Azure Cognitive Services — Text Analytics |
| Frontend | Blazor WebAssembly |
| UI | Bootstrap 5 |

---

## 🏗️ Project Structure

```
Convolis/
├── Convolis.Api      # Backend: controllers, SignalR hub, EF Core, Azure AI
├── Convolis.UI       # Frontend: Blazor WASM
└── Convolis.Shared   # Shared DTOs between client and server
```

---

## 🚀 Getting Started

### 1. Configure `appsettings.json`

Fill in `Convolis.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=ConvolisDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "AppSettings": {
    "Token": "YOUR_SECRET_KEY_MIN_32_CHARS",
    "Issuer": "Convolis",
    "Audience": "ConvolisUsers"
  },
  "AzureLanguage": {
    "Key": "YOUR_AZURE_KEY",
    "Endpoint": "YOUR_AZURE_ENDPOINT"
  }
}
```

### 2. Apply Migrations

```bash
dotnet ef database update --project Convolis.Api
```

### 3. Run

```bash
# In Convolis.Api folder
dotnet run

# In Convolis.UI folder
dotnet run
```

> Tip: In Visual Studio you can configure **Multiple Startup Projects** to run both at once.

---

## ⚙️ Technical Notes

**Silent Token Refresh** — `App.razor` checks token expiry on startup and automatically refreshes the session using the Refresh Token, so users stay logged in across page reloads.

**Authenticated HTTP Client** — `AuthenticatedHttpHandler` (a `DelegatingHandler`) automatically attaches the JWT to every outgoing request.

**SignalR Group Management** — when a user connects, `ChatHub.OnConnectedAsync` adds them to a SignalR group for each of their conversations. On disconnect, all participants in those conversations receive an updated online count.

**Conversation Naming** — conversation names are resolved server-side per requesting user, so each participant sees the other person's username rather than a stored string.

---

## 👤 Author

**Vadym Marushka**
