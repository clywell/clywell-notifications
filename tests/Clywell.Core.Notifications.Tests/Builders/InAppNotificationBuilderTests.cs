namespace Clywell.Core.Notifications.Tests;

public sealed class InAppNotificationBuilderTests
{
    [Fact]
    public void Build_SetsChannelToInApp()
    {
        var request = new InAppNotificationBuilder()
            .ToUser("user-1")
            .Build();

        Assert.Equal(NotificationChannel.InApp, request.Channel);
    }

    [Fact]
    public void ToUser_SetsUserIdOnRecipient()
    {
        var request = new InAppNotificationBuilder()
            .ToUser("user-1")
            .Build();

        Assert.Equal("user-1", request.Recipient.UserId);
    }

    [Fact]
    public void ToUser_WithName_SetsBothUserIdAndName()
    {
        var request = new InAppNotificationBuilder()
            .ToUser("user-1", "InApp User")
            .Build();

        Assert.Equal("user-1", request.Recipient.UserId);
        Assert.Equal("InApp User", request.Recipient.Name);
    }

    [Fact]
    public void ToConnection_SetsConnectionIdOnRecipient()
    {
        var request = new InAppNotificationBuilder()
            .ToConnection("conn-1")
            .Build();

        Assert.Equal("conn-1", request.Recipient.ConnectionId);
    }

    [Fact]
    public void ToConnection_WithName_SetsBothConnectionIdAndName()
    {
        var request = new InAppNotificationBuilder()
            .ToConnection("conn-1", "Connection Name")
            .Build();

        Assert.Equal("conn-1", request.Recipient.ConnectionId);
        Assert.Equal("Connection Name", request.Recipient.Name);
    }

    [Fact]
    public void WithSubject_SetsSubject()
    {
        var request = new InAppNotificationBuilder()
            .ToUser("user-1")
            .WithSubject("subject")
            .Build();

        Assert.Equal("subject", request.Subject);
    }

    [Fact]
    public void Build_WithoutRecipient_ThrowsInvalidOperationException()
    {
        var builder = new InAppNotificationBuilder();

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void ToConnection_ThenToUser_LastCallWins()
    {
        var request = new InAppNotificationBuilder()
            .ToConnection("conn-1")
            .ToUser("user-2")
            .Build();

        Assert.Equal("user-2", request.Recipient.UserId);
        Assert.Null(request.Recipient.ConnectionId);
    }

    [Fact]
    public void Build_WithTemplate_SetsTemplateKey()
    {
        var request = new InAppNotificationBuilder()
            .ToUser("user-1")
            .WithTemplate("inapp-template")
            .Build();

        Assert.Equal("inapp-template", request.TemplateKey);
    }

    [Fact]
    public void ToUser_ReturnsSameBuilderInstance()
    {
        var builder = new InAppNotificationBuilder();

        var result = builder.ToUser("user-1");

        Assert.True(ReferenceEquals(builder, result));
    }

    [Fact]
    public void ToConnection_ReturnsSameBuilderInstance()
    {
        var builder = new InAppNotificationBuilder();

        var result = builder.ToConnection("conn-1");

        Assert.True(ReferenceEquals(builder, result));
    }

    [Fact]
    public void WithSubject_ReturnsSameBuilderInstance()
    {
        var builder = new InAppNotificationBuilder();

        var result = builder.WithSubject("subject");

        Assert.True(ReferenceEquals(builder, result));
    }

    [Fact]
    public void ToGroup_SetsGroupsOnRecipient()
    {
        var request = new InAppNotificationBuilder()
            .ToGroup("admins")
            .Build();

        Assert.Single(request.Recipient.Groups);
        Assert.Equal("admins", request.Recipient.Groups[0]);
    }

    [Fact]
    public void ToGroup_ReturnsSameBuilderInstance()
    {
        var builder = new InAppNotificationBuilder();
        var result = builder.ToGroup("admins");
        Assert.Same(builder, result);
    }

    [Fact]
    public void ToGroup_WithWhitespace_ThrowsArgumentException()
    {
        var builder = new InAppNotificationBuilder();
        Assert.Throws<ArgumentException>(() => builder.ToGroup("   "));
    }

    [Fact]
    public void ToGroups_SetsMultipleGroupsOnRecipient()
    {
        var request = new InAppNotificationBuilder()
            .ToGroups(["admins", "managers", "tenant-abc"])
            .Build();

        Assert.Equal(3, request.Recipient.Groups.Count);
        Assert.Contains("admins", request.Recipient.Groups);
        Assert.Contains("managers", request.Recipient.Groups);
        Assert.Contains("tenant-abc", request.Recipient.Groups);
    }

    [Fact]
    public void ToGroups_ReturnsSameBuilderInstance()
    {
        var builder = new InAppNotificationBuilder();
        var result = builder.ToGroups(["admins"]);
        Assert.Same(builder, result);
    }

    [Fact]
    public void ToGroups_WithEmptyCollection_ThrowsArgumentException()
    {
        var builder = new InAppNotificationBuilder();
        Assert.Throws<ArgumentException>(() => builder.ToGroups([]));
    }

    [Fact]
    public void ToGroups_WithOnlyWhitespaceEntries_ThrowsArgumentException()
    {
        var builder = new InAppNotificationBuilder();
        Assert.Throws<ArgumentException>(() => builder.ToGroups(["   ", ""]));
    }

    [Fact]
    public void ToGroups_WithNull_ThrowsArgumentNullException()
    {
        var builder = new InAppNotificationBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.ToGroups(null!));
    }

    [Fact]
    public void ToGroup_ThenToUser_LastCallWins_RecipientHasUserIdNotGroups()
    {
        var request = new InAppNotificationBuilder()
            .ToGroup("admins")
            .ToUser("user-1")
            .Build();

        Assert.Equal("user-1", request.Recipient.UserId);
        Assert.Empty(request.Recipient.Groups);
    }
}
