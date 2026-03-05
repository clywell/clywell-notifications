# Clywell.Core.Notifications

[![Build Status](https://github.com/clywell/clywell-notifications/workflows/ci-cd/badge.svg)](https://github.com/clywell/clywell-notifications/actions)
[![NuGet Version](https://img.shields.io/nuget/v/Clywell.Core.Notifications.svg)](https://www.nuget.org/packages/Clywell.Core.Notifications/)
[![License](https://img.shields.io/github/license/clywell/clywell-notifications.svg)](LICENSE)

Multi-channel notification dispatch for .NET — pluggable channel providers, fluent builder API, template rendering, and real-time delivery via SignalR and Server-Sent Events. Zero infrastructure dependency at the Application layer.

---

## Packages

| Package | NuGet | Description |
|---------|-------|----------|
| `Clywell.Core.Notifications` | [![NuGet](https://img.shields.io/nuget/v/Clywell.Core.Notifications.svg)](https://www.nuget.org/packages/Clywell.Core.Notifications/) | Core abstractions, dispatch pipeline, and fluent builder API |
| `Clywell.Core.Notifications.Smtp` | [![NuGet](https://img.shields.io/nuget/v/Clywell.Core.Notifications.Smtp.svg)](https://www.nuget.org/packages/Clywell.Core.Notifications.Smtp/) | SMTP email provider using MailKit |
| `Clywell.Core.Notifications.Renderer.Scriban` | [![NuGet](https://img.shields.io/nuget/v/Clywell.Core.Notifications.Renderer.Scriban.svg)](https://www.nuget.org/packages/Clywell.Core.Notifications.Renderer.Scriban/) | Scriban template rendering |
| `Clywell.Core.Notifications.SignalR` | [![NuGet](https://img.shields.io/nuget/v/Clywell.Core.Notifications.SignalR.svg)](https://www.nuget.org/packages/Clywell.Core.Notifications.SignalR/) | Real-time in-app delivery via SignalR |
| `Clywell.Core.Notifications.Sse` | [![NuGet](https://img.shields.io/nuget/v/Clywell.Core.Notifications.Sse.svg)](https://www.nuget.org/packages/Clywell.Core.Notifications.Sse/) | Real-time in-app delivery via Server-Sent Events |

---

## Installation

Install the core package and whichever providers you need:

```bash
dotnet add package Clywell.Core.Notifications

# Provider packages (add one or more)
dotnet add package Clywell.Core.Notifications.Smtp
dotnet add package Clywell.Core.Notifications.SignalR
dotnet add package Clywell.Core.Notifications.Sse

# Optional: Scriban template rendering
dotnet add package Clywell.Core.Notifications.Renderer.Scriban
```

---

## Quick Start

### 1. Register services

```csharp
// Core — required
services.AddNotifications(options => options
    .UseDefaultChannel(NotificationChannel.Email)
    .WithMaxRetryAttempts(3)
    .WithRetryDelay(TimeSpan.FromSeconds(2)));

// Email channel (pick any combination of providers)
services.AddNotificationsSmtp(smtp => smtp
    .UseHost("smtp.example.com", 587)
    .WithCredentials("user@example.com", "password")
    .UseSender("noreply@example.com", "My App")
    .WithSsl(true));

// Optional: Scriban template rendering
services.AddScribanRenderer();
services.AddScoped<ITemplateProvider, MyDatabaseTemplateProvider>();
```

### 2. Send notifications

```csharp
public class WelcomeService(INotificationService notifications)
{
    public async Task SendWelcomeEmailAsync(string email, string name, CancellationToken ct)
    {
        await notifications.SendEmailAsync(email => email
            .To(email, name)
            .WithTemplate("welcome")
            .WithParameter("userName", name)
            .WithPriority(NotificationPriority.Normal), ct);
    }
}
```

---

## Fluent Builder API

The fluent builder API provides a channel-aware, IntelliSense-guided way to construct and send notifications. Each channel has its own builder that exposes only the addressing fields relevant to that channel.

### Extension methods on `INotificationService`

| Method | Builder | Channel |
|--------|---------|---------|
| `SendEmailAsync(Action<EmailNotificationBuilder>)` | `EmailNotificationBuilder` | `Email` |
| `SendSmsAsync(Action<SmsNotificationBuilder>)` | `SmsNotificationBuilder` | `Sms` |
| `SendPushAsync(Action<PushNotificationBuilder>)` | `PushNotificationBuilder` | `Push` |
| `SendInAppAsync(Action<InAppNotificationBuilder>)` | `InAppNotificationBuilder` | `InApp` |
| `SendAsync(Func<INotificationBuilder>)` | Any (via `NotificationBuilder`) | Any |

### Email

```csharp
await service.SendEmailAsync(email => email
    .To("user@example.com", "Jane Doe")   // required: address, optional name
    .WithSubject("Welcome aboard!")
    .WithBody("Thanks for signing up.")
    .WithTemplate("welcome")               // mutually optional with inline Subject/Body
    .WithParameter("userName", "Jane")
    .WithParameters(new Dictionary<string, object>   // bulk parameters
    {
        ["activationLink"] = "https://...",
        ["expiresIn"] = "24 hours"
    })
    .WithPriority(NotificationPriority.Critical)
    .WithMetadata("correlationId", "abc-123"));
```

### SMS

```csharp
await service.SendSmsAsync(sms => sms
    .To("+14155552671")               // required: E.164 phone number
    .WithBody("Your code is 847291")
    .WithTemplate("otp-sms")
    .WithParameter("code", "847291"));
```

### Push

```csharp
// Target by device token
await service.SendPushAsync(push => push
    .ToDevice("FCM_DEVICE_TOKEN_HERE")
    .WithTitle("New message")
    .WithBody("You have a new notification")
    .WithPriority(NotificationPriority.Critical));

// Target by user ID (all devices for that user)
await service.SendPushAsync(push => push
    .ToUser("user-123")
    .WithTitle("Order shipped")
    .WithTemplate("order-shipped")
    .WithParameter("orderId", "ORD-9876"));
```

### In-App (SignalR / SSE)

```csharp
// Target a specific user (all their active connections)
await service.SendInAppAsync(inapp => inapp
    .ToUser("user-123")
    .WithSubject("Alert")
    .WithBody("Your session is about to expire."));

// Target a specific connection
await service.SendInAppAsync(inapp => inapp
    .ToConnection("HUB_CONNECTION_ID")
    .WithBody("Connected successfully."));

// Target a single group (e.g. a role or tenant)
await service.SendInAppAsync(inapp => inapp
    .ToGroup("role:admins")
    .WithSubject("System maintenance scheduled")
    .WithBody("Downtime window: Saturday 02:00–04:00 UTC"));

// Target multiple groups — dispatched sequentially to each
await service.SendInAppAsync(inapp => inapp
    .ToGroups(["tenant:acme", "role:managers", "role:admins"])
    .WithTemplate("maintenance-alert")
    .WithParameter("window", "Saturday 02:00-04:00 UTC"));
```

### Generic selector overload

```csharp
await service.SendAsync(() => NotificationBuilder.ViaEmail()
    .To("user@example.com")
    .WithTemplate("welcome"));

await service.SendAsync(() => NotificationBuilder.ViaInApp()
    .ToUser("user-123")
    .WithBody("Something happened"));
```

### Batch sending

```csharp
var requests = users.Select(u => new NotificationRequest
{
    Channel = NotificationChannel.Email,
    Recipient = new NotificationRecipient { Email = u.Email, Name = u.Name },
    TemplateKey = "newsletter",
    Parameters = new Dictionary<string, object> { ["issue"] = "March 2026" }
});

var results = await service.SendAsync(requests, ct);
// Individual failures do not block remaining sends
```

---

## Configuration Reference

### Core — `NotificationOptions`

Configured via `AddNotifications(options => ...)`.

| Method | Default | Description |
|--------|---------|-------------|
| `UseDefaultChannel(NotificationChannel)` | `Email` | Channel used when `NotificationRequest.Channel` is `null` |
| `WithMaxRetryAttempts(int)` | `3` | Max retry attempts per notification on failure |
| `WithRetryDelay(TimeSpan)` | `2s` | Fixed delay between retry attempts |

```csharp
services.AddNotifications(options => options
    .UseDefaultChannel(NotificationChannel.InApp)
    .WithMaxRetryAttempts(5)
    .WithRetryDelay(TimeSpan.FromSeconds(3)));
```

### SMTP — `SmtpOptions`

Configured via `AddNotificationsSmtp(smtp => ...)`.

| Method | Default | Description |
|--------|---------|-------------|
| `UseHost(host, port)` | — | SMTP server hostname and port |
| `WithCredentials(userName, password)` | — | SMTP authentication credentials |
| `UseSender(email, name?)` | — | From address and optional display name |
| `WithSsl(bool)` | `true` | Enable or disable SSL/TLS |

```csharp
services.AddNotificationsSmtp(smtp => smtp
    .UseHost("smtp.sendgrid.net", 587)
    .WithCredentials("apikey", "SG.xxxx")
    .UseSender("noreply@example.com", "Example App")
    .WithSsl(true));
```

### SignalR — `SignalROptions`

Configured via `AddNotificationsSignalR(options => ...)`.

| Method | Default | Description |
|--------|---------|-------------|
| `WithMethodName(string)` | `"ReceiveNotification"` | Client-side hub method name invoked on delivery |
| `UseUserAddressing()` | enabled | Targets `Recipient.UserId` via SignalR user addressing |
| `UseConnectionAddressing()` | — | Targets `Recipient.ConnectionId` directly |

> **Note:** Group-based addressing (`ToGroup`/`ToGroups`) takes precedence over user/connection addressing and is always handled regardless of this setting.

```csharp
services.AddSignalR();
services.AddNotificationsSignalR(options => options
    .WithMethodName("OnNotification")
    .UseUserAddressing());

// Map the hub endpoint
app.MapHub<NotificationHub>("/hubs/notifications");
```

### SSE — `SseOptions`

Configured via `AddNotificationsSse(options => ...)`.

| Method | Default | Description |
|--------|---------|-------------|
| `WithEventName(string)` | `"notification"` | SSE `event:` field sent to clients |
| `UseUserAddressing()` | enabled | Targets all connections for `Recipient.UserId` |
| `UseConnectionAddressing()` | — | Targets `Recipient.ConnectionId` directly |

> **Note:** Group-based addressing takes precedence over user/connection addressing.

```csharp
services.AddNotificationsSse(options => options
    .WithEventName("app-notification")
    .UseUserAddressing());
```

---

## Template Rendering (Scriban)

Install `Clywell.Core.Notifications.Renderer.Scriban` to enable template-based rendering using the [Scriban](https://github.com/scriban/scriban) engine.

### 1. Register the renderer

```csharp
services.AddScribanRenderer();
services.AddScoped<ITemplateProvider, MyTemplateProvider>();
```

### 2. Implement `ITemplateProvider`

Implement `ITemplateProvider` to load templates from your storage of choice (database, file system, etc.):

```csharp
public class DatabaseTemplateProvider(AppDbContext db) : ITemplateProvider
{
    public async Task<TemplateDefinition?> GetTemplateAsync(
        string templateKey,
        CancellationToken cancellationToken = default)
    {
        var template = await db.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Key == templateKey, cancellationToken);

        if (template is null) return null;

        return new TemplateDefinition(
            SubjectTemplate:       template.SubjectTemplate,
            HtmlBodyTemplate:      template.HtmlBodyTemplate,
            PlainTextBodyTemplate: template.PlainTextBodyTemplate);
    }
}
```

### 3. Use template keys in requests

```csharp
await service.SendEmailAsync(email => email
    .To("user@example.com", "Jane")
    .WithTemplate("welcome")
    .WithParameters(new Dictionary<string, object>
    {
        ["userName"] = "Jane",
        ["activationUrl"] = "https://app.example.com/activate?token=abc"
    }));
```

### Template syntax

Templates use [Scriban](https://github.com/scriban/scriban/blob/master/doc/language.md) syntax:

```
Subject: Welcome, {{ userName }}!
HTML: <p>Hi {{ userName }}, click <a href="{{ activationUrl }}">here</a> to activate.</p>
Plain: Hi {{ userName }}, activate your account at {{ activationUrl }}
```

### Inline content (no template)

If no `TemplateKey` is set, `Subject` and `Body` are used directly — no `ITemplateRenderer` is required:

```csharp
await service.SendEmailAsync(email => email
    .To("user@example.com")
    .WithSubject("Quick note")
    .WithBody("This is a plain body — no template needed."));
```

---

## Real-Time Delivery

### SignalR

The `Clywell.Core.Notifications.SignalR` package delivers `InApp` channel notifications via SignalR.

#### Setup

```csharp
// Program.cs
builder.Services.AddSignalR();
builder.Services.AddNotifications();
builder.Services.AddNotificationsSignalR(options => options
    .WithMethodName("ReceiveNotification")
    .UseUserAddressing());

app.MapHub<NotificationHub>("/hubs/notifications");
```

#### Client connection (JavaScript)

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/notifications")
    .build();

connection.on("ReceiveNotification", (notification) => {
    console.log(notification.subject, notification.body);
    // notification: { id, subject, body, priority, metadata, sentAt }
});

await connection.start();
```

#### Payload shape

```json
{
  "id": "a1b2c3d4...",
  "subject": "Order shipped",
  "body": "Your order #1234 has been dispatched.",
  "priority": "Normal",
  "metadata": {},
  "sentAt": "2026-03-05T10:00:00Z"
}
```

#### Addressing modes

| Mode | Recipient field | When to use |
|------|-----------------|-------------|
| User-based | `ToUser(userId)` | Send to all active connections of a user |
| Connection-based | `ToConnection(connectionId)` | Target a specific browser tab / device |
| Group-based | `ToGroup(group)` / `ToGroups(groups)` | Send to a role, tenant, or any named group |

#### Group management (client side)

Clients can join named groups via the `NotificationHub`:

```javascript
// Join a group (e.g. after authentication)
await connection.invoke("JoinGroupAsync", userId);

// Leave a group
await connection.invoke("LeaveGroupAsync", userId);
```

> Groups are validated server-side — a client can only join a group matching their own `UserIdentifier`.

---

### Server-Sent Events (SSE)

The `Clywell.Core.Notifications.Sse` package delivers `InApp` channel notifications over HTTP streaming (SSE). It is simpler than WebSockets and works with standard HTTP/2.

#### Setup

```csharp
// Program.cs
builder.Services.AddNotifications();
builder.Services.AddNotificationsSse(options => options
    .WithEventName("notification")
    .UseUserAddressing());
```

#### Map the SSE endpoint

Consumers map their own SSE endpoint and manage connections via `ISseConnectionManager`:

```csharp
app.MapGet("/notifications/stream", async (
    HttpContext ctx,
    ISseConnectionManager manager,
    CancellationToken ct) =>
{
    ctx.Response.ContentType = "text/event-stream";
    ctx.Response.Headers.CacheControl = "no-cache";

    var connectionId = Guid.NewGuid().ToString("N");
    var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    manager.AddConnection(connectionId, userId, async (data, token) =>
    {
        await ctx.Response.WriteAsync(data, token);
        await ctx.Response.Body.FlushAsync(token);
    });

    try
    {
        // Keep the connection open until client disconnects or server cancels
        await Task.Delay(Timeout.Infinite, ct);
    }
    catch (OperationCanceledException) { }
    finally
    {
        manager.RemoveConnection(connectionId);
    }
});
```

#### Group management (server side)

Unlike SignalR, SSE group membership is managed server-side by the application:

```csharp
// After authenticating, add connection to role/tenant groups
manager.AddConnectionToGroup(connectionId, "role:admins");
manager.AddConnectionToGroup(connectionId, $"tenant:{tenantId}");

// On disconnect
manager.RemoveConnection(connectionId);  // writers cleaned up automatically
// Or remove from a specific group only:
manager.RemoveConnectionFromGroup(connectionId, "role:admins");
```

#### Client consumption (JavaScript)

```javascript
const source = new EventSource("/notifications/stream");

source.addEventListener("notification", (event) => {
    const notification = JSON.parse(event.data);
    console.log(notification.subject, notification.body);
    // notification: { id, subject, body, priority, metadata, sentAt }
});
```

#### Addressing modes

| Mode | Recipient field | Behaviour |
|------|-----------------|-----------|
| User-based | `ToUser(userId)` | Writes to all active connections for that user |
| Connection-based | `ToConnection(connectionId)` | Writes to a single specific connection |
| Group-based | `ToGroup(group)` / `ToGroups(groups)` | Writes to all connections in each group, sequentially |

---

## Notification Result

Every send operation returns a `NotificationResult`:

```csharp
var result = await service.SendEmailAsync(email => email
    .To("user@example.com")
    .WithBody("Hello!"));

if (result.Status == NotificationStatus.Failed)
{
    logger.LogWarning("Notification {Id} failed: {Error}", result.NotificationId, result.ErrorMessage);
}
```

| Property | Type | Description |
|----------|------|-------------|
| `NotificationId` | `string` | Unique identifier for the notification |
| `Status` | `NotificationStatus` | `Pending`, `Queued`, `Sent`, `Delivered`, `Failed`, `Cancelled` |
| `SentAt` | `DateTimeOffset?` | Timestamp of successful delivery |
| `ErrorMessage` | `string?` | Error details if `Status == Failed` |

---

## Retry Behaviour

The dispatch pipeline retries automatically on failure:

- **Attempts**: configurable via `WithMaxRetryAttempts(n)` (default: 3, meaning up to 4 total attempts)
- **Delay**: fixed delay between attempts via `WithRetryDelay(TimeSpan)` (default: 2 seconds)
- **Partial failure on batches**: individual failures in `SendAsync(IEnumerable<NotificationRequest>)` do not block remaining notifications
- **Group partial failure**: if sending to multiple groups and at least one succeeds, the result is `Sent`

---

## Custom Channel Implementation

Implement `INotificationChannel` to add any delivery mechanism (Twilio SMS, Firebase Push, etc.):

```csharp
public sealed class TwilioSmsChannel(ITwilioClient twilio) : INotificationChannel
{
    public NotificationChannel Channel => NotificationChannel.Sms;

    public async Task<NotificationResult> SendAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default)
    {
        var notificationId = Guid.NewGuid().ToString("N");

        try
        {
            await twilio.SendSmsAsync(
                to: message.Recipient.PhoneNumber!,
                body: message.Content.PlainTextBody ?? message.Content.HtmlBody,
                cancellationToken: cancellationToken);

            return NotificationResult.Success(notificationId);
        }
        catch (Exception ex)
        {
            return NotificationResult.Failure(notificationId, ex.Message);
        }
    }
}

// Register
services.AddScoped<INotificationChannel, TwilioSmsChannel>();
```

---

## Notification Audit Logging

Implement `INotificationLogger` to persist delivery results to a database, event bus, or audit trail:

```csharp
public sealed class AuditNotificationLogger(AppDbContext db) : INotificationLogger
{
    public async Task LogAsync(NotificationResult result, CancellationToken cancellationToken = default)
    {
        db.NotificationAuditLog.Add(new NotificationAuditEntry
        {
            NotificationId = result.NotificationId,
            Status = result.Status.ToString(),
            SentAt = result.SentAt,
            ErrorMessage = result.ErrorMessage,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}

// Register
services.AddScoped<INotificationLogger, AuditNotificationLogger>();
```

---

## Architecture

```
INotificationService
└── NotificationService (internal)
    ├── Resolves channel from NotificationRequest.Channel or DefaultChannel
    ├── Renders content via ITemplateRenderer (if TemplateKey set)
    ├── Dispatches to INotificationChannel.SendAsync()
    ├── Retries on failure (MaxRetryAttempts, RetryDelay)
    └── Logs result via INotificationLogger (optional)

INotificationChannel implementations:
  ├── SmtpNotificationChannel       → Email  (MailKit SMTP)
  ├── SignalRNotificationChannel     → InApp  (SignalR Hub)
  └── SseNotificationChannel         → InApp  (HTTP streaming)

ITemplateRenderer:
  └── ScribanTemplateRenderer        → renders via ITemplateProvider

Fluent builder API:
  ├── EmailNotificationBuilder       → .To(), .WithSubject()
  ├── SmsNotificationBuilder         → .To()
  ├── PushNotificationBuilder        → .ToDevice(), .ToUser(), .WithTitle()
  ├── InAppNotificationBuilder       → .ToUser(), .ToConnection(), .ToGroup(), .ToGroups()
  └── NotificationBuilder (static)   → .ViaEmail(), .ViaSms(), .ViaPush(), .ViaInApp()
```

---

## Contributing

See [Backend Development Guide](../docs/BACKEND_DEVELOPMENT_GUIDE.md) for development guidelines.

## License

This project is licensed under the MIT License — see [LICENSE](LICENSE) for details.
