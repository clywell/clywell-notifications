using Microsoft.AspNetCore.SignalR;

namespace Clywell.Core.Notifications.SignalR;

/// <summary>
/// Sends notifications in real time via SignalR.
/// Targets by UserId (user-based addressing) or ConnectionId (direct connection).
/// </summary>
internal sealed class SignalRNotificationChannel(
    IHubContext<NotificationHub> hubContext,
    SignalROptions options,
    ILogger<SignalRNotificationChannel> logger) : INotificationChannel
{
    /// <inheritdoc/>
    public NotificationChannel Channel => NotificationChannel.InApp;

    /// <inheritdoc/>
    public async Task<NotificationResult> SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        var notificationId = Guid.NewGuid().ToString("N");

        try
        {
            var payload = new
            {
                id = notificationId,
                subject = message.Content.Subject,
                body = message.Content.PlainTextBody ?? message.Content.HtmlBody,
                priority = message.Priority.ToString(),
                metadata = message.Metadata,
                sentAt = DateTimeOffset.UtcNow
            };

            if (message.Recipient.Groups.Count > 0)
            {
                var failedGroups = new List<string>();
                foreach (var group in message.Recipient.Groups)
                {
                    try
                    {
                        await hubContext.Clients
                            .Group(group)
                            .SendAsync(options.MethodName, payload, cancellationToken)
                            .ConfigureAwait(false);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        failedGroups.Add(group);
                        logger.LogWarning(ex, "Failed to send SignalR notification {NotificationId} to group {Group}", notificationId, group);
                    }
                }

                if (failedGroups.Count == message.Recipient.Groups.Count)
                {
                    return NotificationResult.Failure(notificationId, $"Failed to deliver notification to all groups: {string.Join(", ", failedGroups)}.");
                }

                logger.LogDebug("SignalR notification {NotificationId} sent to {GroupCount} group(s)", notificationId, message.Recipient.Groups.Count);
                return NotificationResult.Success(notificationId);
            }
            else if (options.UseUserBasedAddressing)
            {
                if (string.IsNullOrWhiteSpace(message.Recipient.UserId))
                {
                    return NotificationResult.Failure(notificationId, "Recipient UserId is required for user-based SignalR addressing.");
                }

                await hubContext.Clients
                    .User(message.Recipient.UserId)
                    .SendAsync(options.MethodName, payload, cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(message.Recipient.ConnectionId))
                {
                    return NotificationResult.Failure(notificationId, "Recipient ConnectionId is required for connection-based SignalR addressing.");
                }

                await hubContext.Clients
                    .Client(message.Recipient.ConnectionId)
                    .SendAsync(options.MethodName, payload, cancellationToken)
                    .ConfigureAwait(false);
            }

            logger.LogDebug("SignalR notification {NotificationId} sent to {Target}", notificationId,
                options.UseUserBasedAddressing ? message.Recipient.UserId : message.Recipient.ConnectionId);

            return NotificationResult.Success(notificationId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to send SignalR notification {NotificationId}", notificationId);
            return NotificationResult.Failure(notificationId, "Real-time notification delivery failed.");
        }
    }
}
