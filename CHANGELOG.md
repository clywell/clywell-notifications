# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2026-03-21

### Added

#### Clywell.Core.Notifications

- `RenderingMode` enum - `Local` (default) and `Delegated`; controls whether `NotificationService` renders templates locally or defers rendering to the channel
- `NotificationOptions.RenderingMode` property - defaults to `RenderingMode.Local`
- `NotificationOptions.WithRenderingMode(RenderingMode)` fluent method - configure rendering behaviour at registration time
- `NotificationMessage.TemplateKey` - exposes the original template key on the message passed to channels; useful when channels delegate rendering externally
- `NotificationMessage.Parameters` - exposes the raw template parameters on the message passed to channels

### Changed

#### Clywell.Core.Notifications

- `NotificationService` now populates `NotificationMessage.TemplateKey` and `NotificationMessage.Parameters` from the originating `NotificationRequest`, giving channels access to raw template data regardless of rendering mode
- `NotificationService.ResolveContentAsync` - when `RenderingMode.Delegated` is configured, local rendering is skipped entirely and `RenderedContent(null, null, null)` is returned; the channel is responsible for rendering using `NotificationMessage.TemplateKey` and `NotificationMessage.Parameters`
- `NotificationService.ResolveContentAsync` - when `RenderingMode.Local` is configured (default), the original behaviour is preserved: an `InvalidOperationException` is thrown if a `TemplateKey` is set but no `ITemplateRenderer` is registered; error message now includes a hint to configure `RenderingMode.Delegated`

## [1.0.1] - 2026-03-20

### Changed

#### Clywell.Core.Notifications.Smtp

- Bumped `MailKit` from `4.15.0` to `4.15.1`
- Added `MimeKit` `4.15.1` as an explicit direct package dependency

#### Clywell.Core.Notifications.Renderer.Scriban

- Bumped `Scriban` from `6.5.5` to `6.6.0`

#### All Packages

- Bumped `Microsoft.Extensions.DependencyInjection.Abstractions` from `10.0.3` to `10.0.5`
- Bumped `Microsoft.Extensions.DependencyInjection` from `10.0.3` to `10.0.5`
- Bumped `Microsoft.Extensions.Logging.Abstractions` from `10.0.3` to `10.0.5`

## [1.0.0] - 2026-03-05

### Added

#### Clywell.Core.Notifications

- `INotificationService` — primary API with `SendAsync(NotificationRequest)` and `SendAsync(IEnumerable<NotificationRequest>)` overloads
- `NotificationService` — internal default implementation with channel resolution, template rendering, retry loop, and optional audit logging
- `INotificationChannel` — pluggable interface for channel implementations; implement to add custom delivery mechanisms
- `ITemplateRenderer` — pluggable interface for template rendering engines
- `INotificationLogger` — optional interface for persisting notification delivery results (audit trail)
- `NotificationRequest` — request model with `Channel`, `Recipient`, `TemplateKey`, `Subject`, `Body`, `Parameters`, `Priority`, and `Metadata`
- `NotificationMessage` — internal model passed to channel implementations after content resolution
- `NotificationRecipient` — recipient model with `Email`, `PhoneNumber`, `UserId`, `ConnectionId`, `DeviceToken`, `Name`, and `Groups` fields
- `NotificationResult` — result model with `NotificationId`, `Status`, `SentAt`, and `ErrorMessage`
- `RenderedContent` — immutable record holding `Subject`, `HtmlBody`, and `PlainTextBody` after template rendering
- `NotificationChannel` enum — `Email`, `Sms`, `Push`, `InApp`
- `NotificationPriority` enum — `Normal`, `Critical`
- `NotificationStatus` enum — `Pending`, `Queued`, `Sent`, `Delivered`, `Failed`, `Cancelled`
- `NotificationOptions` — fluent configuration: `UseDefaultChannel`, `WithMaxRetryAttempts`, `WithRetryDelay`
- `ServiceCollectionExtensions.AddNotifications` — registers core services with optional options configuration
- Retry logic — configurable fixed-delay retry loop in `NotificationService`; partial batch failure isolation
- Inline content support — `Subject` and `Body` used directly when no `TemplateKey` is set (no `ITemplateRenderer` required)

#### Fluent Builder API

- `INotificationBuilder` — base contract; `Build()` returns a `NotificationRequest`
- `NotificationBuilderBase<TSelf>` — generic CRTP base class with shared fluent methods:
  - `WithTemplate(string)` — set template key
  - `WithBody(string)` — set notification body
  - `WithParameter(string, object)` — add or replace a single template parameter
  - `WithParameters(IEnumerable<KeyValuePair<string, object>>)` — bulk-merge template parameters
  - `WithPriority(NotificationPriority)` — set delivery priority
  - `WithMetadata(string, string)` — add or replace a metadata entry
  - `Build()` — validates recipient is set (throws `InvalidOperationException` if not) and returns defensive-copied `NotificationRequest`
- `EmailNotificationBuilder` — email-specific builder:
  - `To(string email, string? name = null)` — set recipient email address
  - `WithSubject(string)` — set subject line
- `SmsNotificationBuilder` — SMS-specific builder:
  - `To(string phoneNumber, string? name = null)` — set recipient phone number
- `PushNotificationBuilder` — push-specific builder:
  - `ToDevice(string deviceToken, string? name = null)` — target by device token
  - `ToUser(string userId, string? name = null)` — target by user ID (all devices)
  - `WithTitle(string)` — set notification title (maps to `Subject`)
- `InAppNotificationBuilder` — in-app specific builder:
  - `ToUser(string userId, string? name = null)` — target by user ID
  - `ToConnection(string connectionId, string? name = null)` — target a specific connection
  - `ToGroup(string group)` — target a single named group (role, tenant, etc.)
  - `ToGroups(IEnumerable<string> groups)` — target multiple groups; dispatched sequentially
  - `WithSubject(string)` — set subject line
- `NotificationBuilder` — static factory class:
  - `ViaEmail()` — creates `EmailNotificationBuilder`
  - `ViaSms()` — creates `SmsNotificationBuilder`
  - `ViaPush()` — creates `PushNotificationBuilder`
  - `ViaInApp()` — creates `InAppNotificationBuilder`
- `NotificationServiceExtensions` — extension methods on `INotificationService`:
  - `SendEmailAsync(Action<EmailNotificationBuilder>, CancellationToken)`
  - `SendSmsAsync(Action<SmsNotificationBuilder>, CancellationToken)`
  - `SendPushAsync(Action<PushNotificationBuilder>, CancellationToken)`
  - `SendInAppAsync(Action<InAppNotificationBuilder>, CancellationToken)`
  - `SendAsync(Func<INotificationBuilder>, CancellationToken)` — generic overload using `NotificationBuilder` static factory

#### Clywell.Core.Notifications.Smtp

- `SmtpNotificationChannel` — `INotificationChannel` implementation for `Email` channel; sends via SMTP using MailKit
- `SmtpOptions` — fluent configuration: `UseHost`, `WithCredentials`, `UseSender`, `WithSsl`
- `ISmtpClientFactory` / `DefaultSmtpClientFactory` — factory abstraction for MailKit `SmtpClient` creation
- `ServiceCollectionExtensions.AddNotificationsSmtp` — registers SMTP channel with required options configuration

#### Clywell.Core.Notifications.Renderer.Scriban

- `ScribanTemplateRenderer` — `ITemplateRenderer` implementation using the Scriban template engine; renders `SubjectTemplate`, `HtmlBodyTemplate`, and `PlainTextBodyTemplate` independently
- `ITemplateProvider` — consumer-implemented interface to load template content from any storage backend (database, file system, cache, etc.)
- `TemplateDefinition` — immutable record holding raw template strings: `SubjectTemplate`, `HtmlBodyTemplate`, `PlainTextBodyTemplate`
- `ServiceCollectionExtensions.AddScribanRenderer` — registers `ScribanTemplateRenderer`; consumers must separately register `ITemplateProvider`

#### Clywell.Core.Notifications.SignalR

- `SignalRNotificationChannel` — `INotificationChannel` implementation for `InApp` channel; sends via `IHubContext<NotificationHub>`
- `NotificationHub` — SignalR hub with `JoinGroupAsync` and `LeaveGroupAsync` methods; validates group membership against caller's `UserIdentifier`
- `SignalROptions` — fluent configuration: `WithMethodName`, `UseUserAddressing`, `UseConnectionAddressing`
- Group-based dispatch — when `Recipient.Groups` is non-empty, sends to each group via `hubContext.Clients.Group()` sequentially; partial group failure tolerated
- User-based addressing — targets all connections for `Recipient.UserId`
- Connection-based addressing — targets `Recipient.ConnectionId` directly
- Notification payload shape: `{ id, subject, body, priority, metadata, sentAt }`
- `ServiceCollectionExtensions.AddNotificationsSignalR` — registers the SignalR channel with optional options configuration

#### Clywell.Core.Notifications.Sse

- `SseNotificationChannel` — `INotificationChannel` implementation for `InApp` channel; sends formatted SSE data (`event: {name}\ndata: {json}\n\n`) to registered writer delegates
- `ISseConnectionManager` — public interface for managing active SSE connections:
  - `AddConnection(connectionId, userId, writer)` — register a connection
  - `RemoveConnection(connectionId)` — deregister a connection
  - `GetConnectionsByUserId(userId)` — resolve writers for user-based dispatch
  - `GetConnectionById(connectionId)` — resolve writer for connection-based dispatch
  - `AddConnectionToGroup(connectionId, group)` — add a connection to a named group
  - `RemoveConnectionFromGroup(connectionId, group)` — remove a connection from a named group
  - `GetConnectionsByGroup(group)` — resolve writers for group-based dispatch
- `SseConnectionManager` — thread-safe in-memory implementation using `ConcurrentDictionary`; stale group entries (removed connections still referenced in groups) are silently skipped on dispatch
- `SseOptions` — fluent configuration: `WithEventName`, `UseUserAddressing`, `UseConnectionAddressing`
- Group-based dispatch — when `Recipient.Groups` is non-empty, sends to each group sequentially; partial group failure tolerated (failure only if all groups fail)
- User-based dispatch — sends to all active connections for `Recipient.UserId`; partial connection failure tolerated
- Connection-based dispatch — targets a single `Recipient.ConnectionId`
- Notification payload shape: `{ id, subject, body, priority, metadata, sentAt }`
- `ServiceCollectionExtensions.AddNotificationsSse` — registers SSE channel and `ISseConnectionManager` as singleton

[Unreleased]: https://github.com/clywell/clywell-notifications/compare/v1.1.0...HEAD
[1.1.0]: https://github.com/clywell/clywell-notifications/compare/v1.0.1...v1.1.0
[1.0.1]: https://github.com/clywell/clywell-notifications/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/clywell/clywell-notifications/releases/tag/v1.0.0