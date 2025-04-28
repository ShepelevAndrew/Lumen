# 📚 Lumen SSE Library — Sending Events to Clients

### Basic Configuration
If you don't need to manage UserId or DeviceId, you can use the default configuration:

```csharp
app.UseSse();
```

* In this setup, random GUIDs are assigned to UserId and DeviceId.
* You can send messages to all clients.
* You won't be able to target specific users or devices.

### Single Device per User
To allow only one device per user (i.e., only the latest connection is active), configure:

```csharp
app.UseSse(options =>
{
    options.UserId = ctx => ctx.Request.Query["id"]!;
    options.DeviceId = ctx => null;
});
```

* UserId is taken from the query string (?id=...), but you can use claims.
* DeviceId is disabled (null), meaning connecting again will disconnect the previous session for the same user.

### Multiple Devices per User
If you want to allow multiple devices or browser tabs per user:

```csharp
app.UseSse(options =>
{
    options.UserId = ctx => ctx.Request.Query["id"]!;
});
```

* Each device (or tab) will have its own unique DeviceId (generated as a Guid by default).

### Custom DeviceId Support
You can also provide your own DeviceId from the query parameters:

```csharp
app.UseSse(options =>
{
    options.UserId = ctx => ctx.Request.Query["id"]!;
    options.DeviceId = ctx => ctx.Request.Query["deviceId"];
});
```

* Useful if you already have a device identifier in your system.

---

# 📤 Sending Events with ISsePublisher
ISsePublisher provides an easy and flexible way to send events.

1. Send to All Connected Clients

```csharp
app.MapPost("/send-message", async (ISsePublisher sse)
    => await sse.New()
        .SetData(new { Name = "Alex", Body = "Hello!" })
        .SendToAllClientsAsync());
```

2. Send to a Specific User (All Devices)

```csharp
app.MapPost("/send-message/{userId:guid}", async (
    Guid userId,
    ISsePublisher sse)
    => await sse.New()
        .SetData(new { Name = "Alex", Body = "Hello!" })
        .SendToClientAllDevicesAsync(userId));
```

3. Send to a Specific User and Device

```csharp
app.MapPost("/send-message/{userId:guid}/{deviceId:guid}", async (
    Guid userId,
    Guid deviceId,
    ISsePublisher sse)
    => await sse.New()
        .SetData(new { Name = "Alex", Body = "Hello!" })
        .SendAsync(id: new(userId, deviceId)));
```

## Building and Sending Events
You can chain options when creating an event:

```csharp
await sse.New()
    .SetId(Guid.NewGuid())           // (Optional) Set a custom event ID
    .SetRetry(10)                    // (Optional) Set retry delay (in milliseconds) after disconnection
    .SetEvent("new_message")          // (Optional) Specify the event name
    .SetData(message)                 // (Required) Provide the event payload (serialized as JSON)
    .SendToAllClientsAsync();         // (Required) Send the event
```

---

## 📜 SSE Protocol (HTTP Specification)
Each message sent to the client can include the following fields:


### Event Fields

| Field  | Description |
|--------|-------------|
| `event` | The event name. Clients can listen for it using `addEventListener(eventName)`. If omitted, the `onmessage` handler is triggered by default. |
| `data` | The message payload. If multiple `data:` lines are received, they are concatenated with newline characters. |
| `id` | The event ID. It helps the client resume events correctly after a reconnection. |
| `retry` | The delay (in milliseconds) before the browser attempts to reconnect after a disconnection. |

> **Note:** Any other fields are ignored by the client.

---

## ⚠️ Browser Connection Limitations

- **Without HTTP/2**, most browsers (e.g., Chrome, Firefox) limit the number of open SSE connections to **6 per domain**.
- This means you can open **only 6 SSE connections across all tabs** for a domain like `example.com`.
- **With HTTP/2**, the server and client negotiate the maximum number of simultaneous connections (typically **100+**).
- This limitation has been marked as **"Won't Fix"** by major browser vendors.

> **Tip:** Consider using HTTP/2 to avoid these SSE connection limitations.

---

# 🧪 Configuration Options

This document explains how to configure your Server-Sent Events (SSE) setup using `AddSse(configure)` and `UseSse(options)`.

---

## 📦 `AddSse(configure)`

This configures global behavior and limits for all SSE connections.

### 🔧 `SseConfig` Properties

| Property                  | Type       | Description                                                                                                              |
|---------------------------|------------|--------------------------------------------------------------------------------------------------------------------------|
| `PingIntervalMilliseconds` | `int`      | Interval in milliseconds for sending keep-alive `ping` events. When set to 0 (by default), the ping message is disabled. |
| `ConnectionLiveMinutes`    | `int?`     | Optional: Defines how long a connection should stay alive before expiring.                                               |
| `MaxEventsForConnection`   | `int?`     | Optional: Maximum number of events allowed per connection.                                                               |

### ✅ Example Usage

```csharp
builder.Services.AddSse(configure: config =>
{
    config.PingIntervalMilliseconds = 10_000;
    config.ConnectionLiveMinutes = 1;
    config.MaxEventsForConnection = 100;
});
```

---

## 📦 `UseSse(options)`
This configures how the SSE middleware extracts identifying information from the incoming HTTP request.

### ⚙️ `SseOptions` Properties

| Property         | Type      | Description                                                                                                              |
|------------------|-----------|--------------------------------------------------------------------------------------------------------------------------|
| `ConnectionPath` | `PathString`     | Endpoint path to handle the SSE connection. Default: /sse/connection. |
| `UserId`         | `Func<HttpContext, Guid>`     | Function to extract a unique user ID from the request context. |
| `DeviceId`       | `Func<HttpContext, Guid?>`    | Function to optionally extract a device ID from the request.|

### ✅ Example Usage

```csharp
app.UseSse(options =>
{
    options.ConnectionPath = "/sse/connection";

    options.UserId = ctx =>
    {
        var userIdQuery = ctx.Request.Query["userId"].FirstOrDefault() ?? string.Empty;
        return Guid.Parse(userIdQuery);
    };

    options.DeviceId = ctx =>
    {
        var deviceIdQuery = ctx.Request.Query["deviceId"].FirstOrDefault();
        return Guid.TryParse(deviceIdQuery, out var id) ? id : null;
    };
});
```




