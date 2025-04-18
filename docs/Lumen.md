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

---

# 📤 Sending SSE Events

This library provides a flexible and powerful API for delivering messages to clients connected via Server-Sent Events (SSE). You can send events to individual devices, all devices for a user, multiple clients, or broadcast to all connected clients.

---

## ✨ Available Methods

### 1. `SendAsync(SseClientId toClientId, CancellationToken ct = default)`
Send an event to a specific client device.

**Parameters:**
- `toClientId`: Represents the combination of `UserId` and `DeviceId`.
- `ct`: Optional cancellation token.

**Use case:**  
Send an event to one connected device of a user.

---

### 2. `SendAsync(Guid clientId, Guid deviceId, CancellationToken ct = default)`
Send an event to a specific user and device combination.

**Parameters:**
- `clientId`: User ID.
- `deviceId`: Device ID.
- `ct`: Optional cancellation token.

**Use case:**  
If you know both the user and device IDs directly.

---

### 3. `SendAsync(IEnumerable<SseClientId> toClientIds, CancellationToken ct = default)`
Send an event to multiple client devices.

**Parameters:**
- `toClientIds`: A collection of client identifiers.
- `ct`: Optional cancellation token.

**Use case:**  
Notify a group of connected clients (e.g., participants in a chat or room).

---

### 4. `SendToClientAllDevicesAsync(Guid toClientId, CancellationToken ct = default)`
Send an event to **all devices** of a single user.

**Parameters:**
- `toClientId`: User ID.
- `ct`: Optional cancellation token.

**Use case:**  
Keep all user devices in sync (mobile, desktop, etc.).

---

### 5. `SendToAllClientsAsync(CancellationToken ct = default)`
Broadcast an event to **every connected client**.

**Parameters:**
- `ct`: Optional cancellation token.

**Use case:**  
Global notifications, announcements, or system-wide updates.

---

## 🧪 Example Usage

```csharp
var sender = sse.New()
    .SetEventName("new_message")
    .SetData(message);

// Send to all user devices
await sender.SendToClientAllDevicesAsync(userId);

// Send to a specific device
await sender.SendAsync(userId, deviceId);



