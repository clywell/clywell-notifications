namespace Clywell.Core.Notifications.Tests;

public sealed class NotificationBuilderTests
{
    [Fact]
    public void ViaEmail_ReturnsEmailNotificationBuilder()
    {
        var result = NotificationBuilder.ViaEmail();

        Assert.IsType<EmailNotificationBuilder>(result);
    }

    [Fact]
    public void ViaSms_ReturnsSmsNotificationBuilder()
    {
        var result = NotificationBuilder.ViaSms();

        Assert.IsType<SmsNotificationBuilder>(result);
    }

    [Fact]
    public void ViaPush_ReturnsPushNotificationBuilder()
    {
        var result = NotificationBuilder.ViaPush();

        Assert.IsType<PushNotificationBuilder>(result);
    }

    [Fact]
    public void ViaInApp_ReturnsInAppNotificationBuilder()
    {
        var result = NotificationBuilder.ViaInApp();

        Assert.IsType<InAppNotificationBuilder>(result);
    }

    [Fact]
    public void ViaEmail_ThenBuild_ProducesEmailChannelRequest()
    {
        var request = NotificationBuilder
            .ViaEmail()
            .To("a@b.com")
            .Build();

        Assert.Equal(NotificationChannel.Email, request.Channel);
    }

    [Fact]
    public void ViaInApp_ThenBuild_ProducesInAppChannelRequest()
    {
        var request = NotificationBuilder
            .ViaInApp()
            .ToUser("user-1")
            .Build();

        Assert.Equal(NotificationChannel.InApp, request.Channel);
    }
}
