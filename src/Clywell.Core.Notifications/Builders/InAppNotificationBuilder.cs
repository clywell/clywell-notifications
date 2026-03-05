namespace Clywell.Core.Notifications;

/// <summary>
/// Fluent builder for in-app notifications.
/// </summary>
public sealed class InAppNotificationBuilder : NotificationBuilderBase<InAppNotificationBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InAppNotificationBuilder"/> class.
    /// </summary>
    public InAppNotificationBuilder()
        : base(NotificationChannel.InApp)
    {
    }

    /// <summary>
    /// Sets a user recipient.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="name">An optional display name.</param>
    /// <returns>The current builder instance.</returns>
    public InAppNotificationBuilder ToUser(string userId, string? name = null)
    {
        _recipient = new NotificationRecipient
        {
            UserId = userId,
            Name = name
        };

        return this;
    }

    /// <summary>
    /// Sets a real-time connection recipient.
    /// </summary>
    /// <param name="connectionId">The connection identifier.</param>
    /// <param name="name">An optional display name.</param>
    /// <returns>The current builder instance.</returns>
    public InAppNotificationBuilder ToConnection(string connectionId, string? name = null)
    {
        _recipient = new NotificationRecipient
        {
            ConnectionId = connectionId,
            Name = name
        };

        return this;
    }

    /// <summary>
    /// Sets the in-app notification subject.
    /// </summary>
    /// <param name="subject">The subject text.</param>
    /// <returns>The current builder instance.</returns>
    public InAppNotificationBuilder WithSubject(string subject)
    {
        _subject = subject;
        return this;
    }

    /// <summary>
    /// Targets a single group (e.g., a role or tenant identifier) for group-based delivery.
    /// </summary>
    /// <param name="group">The group identifier.</param>
    /// <returns>The current builder instance.</returns>
    public InAppNotificationBuilder ToGroup(string group)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(group);
        _recipient = new NotificationRecipient { Groups = [group] };
        return this;
    }

    /// <summary>
    /// Targets multiple groups for group-based delivery. The channel will dispatch to each group sequentially.
    /// </summary>
    /// <param name="groups">The group identifiers. Must contain at least one entry.</param>
    /// <returns>The current builder instance.</returns>
    public InAppNotificationBuilder ToGroups(IEnumerable<string> groups)
    {
        ArgumentNullException.ThrowIfNull(groups);
        var groupList = groups.ToArray();
        if (groupList.Length == 0)
        {
            throw new ArgumentException("At least one group identifier is required.", nameof(groups));
        }

        if (groupList.Any(g => string.IsNullOrWhiteSpace(g)))
        {
            throw new ArgumentException("Group identifiers cannot be null or whitespace.", nameof(groups));
        }

        _recipient = new NotificationRecipient { Groups = groupList };
        return this;
    }
}
