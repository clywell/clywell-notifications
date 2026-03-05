using System.Text.Json;

namespace Clywell.Core.Notifications.Sse;

/// <summary>
/// Sends notifications via Server-Sent Events.
/// </summary>
internal sealed class SseNotificationChannel(
    ISseConnectionManager connectionManager,
    SseOptions options,
    ILogger<SseNotificationChannel> logger) : INotificationChannel
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <inheritdoc/>
    public NotificationChannel Channel => NotificationChannel.InApp;

    /// <inheritdoc/>
    public async Task<NotificationResult> SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        var notificationId = Guid.NewGuid().ToString("N");

        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                id = notificationId,
                subject = message.Content.Subject,
                body = message.Content.PlainTextBody ?? message.Content.HtmlBody,
                priority = message.Priority.ToString(),
                metadata = message.Metadata,
                sentAt = DateTimeOffset.UtcNow
            }, JsonOptions);

            var sseData = $"event: {options.EventName}\ndata: {payload}\n\n";

            if (message.Recipient.Groups.Count > 0)
            {
                var failedGroups = new List<string>();
                foreach (var group in message.Recipient.Groups)
                {
                    var groupWriters = connectionManager.GetConnectionsByGroup(group);
                    if (groupWriters.Count == 0)
                    {
                        logger.LogWarning("No active SSE connections for group {Group}", group);
                        failedGroups.Add(group);
                        continue;
                    }

                    var groupFailureCount = 0;
                    foreach (var writer in groupWriters)
                    {
                        try
                        {
                            await writer(sseData, cancellationToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            groupFailureCount++;
                            logger.LogWarning(ex, "Failed to write SSE data to a connection in group {Group}", group);
                        }
                    }

                    if (groupFailureCount == groupWriters.Count)
                    {
                        failedGroups.Add(group);
                    }
                }

                if (failedGroups.Count == message.Recipient.Groups.Count)
                {
                    return NotificationResult.Failure(notificationId, $"Failed to deliver SSE notification to all groups: {string.Join(", ", failedGroups)}.");
                }

                logger.LogDebug("SSE notification {NotificationId} sent to {GroupCount} group(s)", notificationId, message.Recipient.Groups.Count);
                return NotificationResult.Success(notificationId);
            }
            else if (options.UseUserBasedAddressing)
            {
                if (string.IsNullOrWhiteSpace(message.Recipient.UserId))
                {
                    return NotificationResult.Failure(notificationId, "Recipient UserId is required for user-based SSE addressing.");
                }

                var writers = connectionManager.GetConnectionsByUserId(message.Recipient.UserId);

                if (writers.Count == 0)
                {
                    logger.LogWarning("No active SSE connections for user {UserId}", message.Recipient.UserId);
                    return NotificationResult.Failure(notificationId, $"No active SSE connections for user '{message.Recipient.UserId}'.");
                }

                var failureCount = 0;
                foreach (var writer in writers)
                {
                    try
                    {
                        await writer(sseData, cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        logger.LogWarning(ex, "Failed to write SSE data to one connection for user {UserId}", message.Recipient.UserId);
                    }
                }

                if (failureCount == writers.Count)
                {
                    return NotificationResult.Failure(notificationId, $"All SSE connections failed for user '{message.Recipient.UserId}'.");
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(message.Recipient.ConnectionId))
                {
                    return NotificationResult.Failure(notificationId, "Recipient ConnectionId is required for connection-based SSE addressing.");
                }

                var writer = connectionManager.GetConnectionById(message.Recipient.ConnectionId);

                if (writer is null)
                {
                    return NotificationResult.Failure(notificationId, $"No active SSE connection for ConnectionId '{message.Recipient.ConnectionId}'.");
                }

                await writer(sseData, cancellationToken).ConfigureAwait(false);
            }

            logger.LogDebug("SSE notification {NotificationId} sent to {Target}",
                notificationId,
                options.UseUserBasedAddressing ? message.Recipient.UserId : message.Recipient.ConnectionId);

            return NotificationResult.Success(notificationId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to send SSE notification {NotificationId}", notificationId);
            return NotificationResult.Failure(notificationId, "Real-time notification delivery failed.");
        }
    }
}
