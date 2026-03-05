namespace Clywell.Core.Notifications.Tests;

public sealed class PushNotificationBuilderTests
{
    [Fact]
    public void Build_SetsChannelToPush()
    {
        var request = new PushNotificationBuilder()
            .ToDevice("device-1")
            .Build();

        Assert.Equal(NotificationChannel.Push, request.Channel);
    }

    [Fact]
    public void ToDevice_SetsDeviceTokenOnRecipient()
    {
        var request = new PushNotificationBuilder()
            .ToDevice("device-token")
            .Build();

        Assert.Equal("device-token", request.Recipient.DeviceToken);
    }

    [Fact]
    public void ToDevice_WithName_SetsBothDeviceTokenAndName()
    {
        var request = new PushNotificationBuilder()
            .ToDevice("device-token", "Device Name")
            .Build();

        Assert.Equal("device-token", request.Recipient.DeviceToken);
        Assert.Equal("Device Name", request.Recipient.Name);
    }

    [Fact]
    public void ToUser_SetsUserIdOnRecipient()
    {
        var request = new PushNotificationBuilder()
            .ToUser("user-1")
            .Build();

        Assert.Equal("user-1", request.Recipient.UserId);
    }

    [Fact]
    public void ToUser_WithName_SetsBothUserIdAndName()
    {
        var request = new PushNotificationBuilder()
            .ToUser("user-1", "Push User")
            .Build();

        Assert.Equal("user-1", request.Recipient.UserId);
        Assert.Equal("Push User", request.Recipient.Name);
    }

    [Fact]
    public void WithTitle_MapsToSubjectOnRequest()
    {
        const string title = "Push title";

        var request = new PushNotificationBuilder()
            .ToUser("user-1")
            .WithTitle(title)
            .Build();

        Assert.Equal(title, request.Subject);
    }

    [Fact]
    public void ToDevice_ThenToUser_LastCallWins()
    {
        var request = new PushNotificationBuilder()
            .ToDevice("device-token")
            .ToUser("user-2")
            .Build();

        Assert.Equal("user-2", request.Recipient.UserId);
        Assert.Null(request.Recipient.DeviceToken);
    }

    [Fact]
    public void Build_WithoutRecipient_ThrowsInvalidOperationException()
    {
        var builder = new PushNotificationBuilder();

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_WithTemplate_SetsTemplateKey()
    {
        var request = new PushNotificationBuilder()
            .ToUser("user-1")
            .WithTemplate("push-template")
            .Build();

        Assert.Equal("push-template", request.TemplateKey);
    }

    [Fact]
    public void ToDevice_ReturnsSameBuilderInstance()
    {
        var builder = new PushNotificationBuilder();

        var result = builder.ToDevice("device-token");

        Assert.True(ReferenceEquals(builder, result));
    }

    [Fact]
    public void ToUser_ReturnsSameBuilderInstance()
    {
        var builder = new PushNotificationBuilder();

        var result = builder.ToUser("user-1");

        Assert.True(ReferenceEquals(builder, result));
    }

    [Fact]
    public void WithTitle_ReturnsSameBuilderInstance()
    {
        var builder = new PushNotificationBuilder();

        var result = builder.WithTitle("title");

        Assert.True(ReferenceEquals(builder, result));
    }
}
