# 🔌 SSE for ASP.NET Core

A lightweight, configurable **Server-Sent Events (SSE)** library for ASP.NET Core.  
Easily add real-time, uni-directional server-to-client messaging without WebSockets.

---

## ✨ Features

- ✅ Simple integration with minimal APIs
- 🧩 Middleware-based connection handling
- 🧠 Supports multiple clients & devices per user
- 🔄 Built-in ping/keep-alive
- 🔧 Fully configurable endpoints and connection logic
- 📦 .NET 6/7/8 compatible

---

## 📦 Installation

Install via NuGet:

```bash
dotnet add package Lumen.SSE
```

---

## 🚀 Usage

#### 1. Configure services

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSse(config =>
{
    config.PingIntervalMilliseconds = 30_000;
    config.ConnectionLiveMinutes = null;
    config.MaxEventsForConnection = null;
});
```

#### 2. Configure middleware

```csharp
var app = builder.Build();

app.UseSse(options =>
{
    options.ConnectionPath = "/sse/connection";

    options.UserId = ctx =>
    {
        var userId = ctx.Request.Query["userId"].FirstOrDefault() ?? string.Empty;
        return Guid.Parse(userId);
    };

    options.DeviceId = ctx =>
    {
        var deviceIdQuery = ctx.Request.Query["deviceId"].FirstOrDefault();
        return Guid.TryParse(deviceIdQuery, out var id) ? id : null;
    };
});
```

#### 3. Send events

```csharp
app.MapPost("/send-message/{clientId:guid}", async (
    Guid clientId,
    Guid? deviceId,
    Message message,
    ISseBuilder sse) =>
{
    const string eventType = "new_message";

    var sender = sse.Build()
        .SetEventName(eventType)
        .SetData(message);

    if (deviceId != null)
        await sender.SendAsync(clientId, deviceId.Value);
    else
        await sender.SendToClientAllDevicesAsync(clientId);
});
```

### 📡 Client-Side (JavaScript)

```js
const userId = "e4d65e91-8dc1-49fb-a13f-bb07f847fcb4";
const deviceId = "bce0f4a6-73ff-45dc-aede-678942aec99e";

const sseUrl = `https://localhost:7287/sse/connection?userId=${userId}&deviceId=${deviceId}`;

const eventSource = new EventSource(sseUrl);

eventSource.onopen = () => {
    console.log("✅ Connected to SSE server");
};

eventSource.onmessage = (event) => {
    try {
        const parsedData = JSON.parse(event.data);
        console.log("📨 Json Message:", parsedData);
    } catch (error) {
        console.warn("📨 Default Message:", event.data);
    }
};

eventSource.addEventListener("new_message", (event) => {
    const parsedData = JSON.parse(event.data);
    console.log("📨 New Message:", parsedData);
});

eventSource.addEventListener("ping", (event) => {
    console.log("🔁 Ping:", event.data);
});

eventSource.addEventListener("disconnect", (event) => {
    console.warn("❌ Disconnected:", event.data);
    eventSource.close();
});

eventSource.onerror = (err) => {
    console.error("🚨 SSE connection error:", err);
};
```