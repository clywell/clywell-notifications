namespace Clywell.Core.Notifications;

/// <summary>
/// Interface that all notification builders implement.
/// </summary>
public interface INotificationBuilder
{
    /// <summary>
    /// Builds a <see cref="NotificationRequest"/> instance.
    /// </summary>
    /// <returns>The built notification request.</returns>
    NotificationRequest Build();
}
