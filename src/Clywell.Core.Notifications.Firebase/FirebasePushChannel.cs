using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;

namespace Clywell.Core.Notifications.Firebase;

/// <summary>
/// Dispatches notifications via Firebase Cloud Messaging (FCM).
/// </summary>
public sealed class FirebasePushChannel : INotificationChannel
{
    private readonly FirebaseMessaging _messaging;

    public FirebasePushChannel(FirebaseMessaging messaging)
    {
        _messaging = messaging;
    }

    /// <inheritdoc/>
    public NotificationChannel Channel => NotificationChannel.Push;

    /// <inheritdoc/>
    public async Task<NotificationResult> SendAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default)
    {
        var notificationId = Guid.NewGuid().ToString("N");

        try
        {
            var fcmMessage = new Message
            {
                Notification = new Notification
                {
                    Title = message.Content.Subject,
                    Body = string.IsNullOrEmpty(message.Content.PlainTextBody) 
                        ? message.Content.HtmlBody 
                        : message.Content.PlainTextBody
                }
            };

            // Address resolution
            if (!string.IsNullOrEmpty(message.Recipient.DeviceToken))
            {
                fcmMessage.Token = message.Recipient.DeviceToken;
            }
            else if (!string.IsNullOrEmpty(message.Recipient.UserId))
            {
                fcmMessage.Topic = $"user_{message.Recipient.UserId}";
            }
            else
            {
                return new NotificationResult
                {
                    NotificationId = notificationId,
                    Status = NotificationStatus.Failed,
                    ErrorMessage = "Firebase channel requires Recipient.DeviceToken or Recipient.UserId."
                };
            }

            // Map standard metadata to FCM Data payload
            if (message.Metadata != null && message.Metadata.Count > 0)
            {
                var data = new Dictionary<string, string>();
                foreach (var meta in message.Metadata)
                {
                    if (meta.Value != null)
                    {
                        data[meta.Key] = meta.Value;
                    }
                }
                fcmMessage.Data = data;
            }

            string response = await _messaging.SendAsync(fcmMessage, cancellationToken);
            return new NotificationResult
            {
                NotificationId = notificationId,
                Status = NotificationStatus.Sent,
                SentAt = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new NotificationResult
            {
                NotificationId = notificationId,
                Status = NotificationStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }
}
