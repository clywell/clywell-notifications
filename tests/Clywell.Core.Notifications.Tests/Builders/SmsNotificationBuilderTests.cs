namespace Clywell.Core.Notifications.Tests;

public sealed class SmsNotificationBuilderTests
{
    [Fact]
    public void Build_SetsChannelToSms()
    {
        var request = new SmsNotificationBuilder()
            .To("+123456789")
            .Build();

        Assert.Equal(NotificationChannel.Sms, request.Channel);
    }

    [Fact]
    public void Build_WithPhoneNumber_SetsPhoneNumberOnRecipient()
    {
        var request = new SmsNotificationBuilder()
            .To("+123456789")
            .Build();

        Assert.Equal("+123456789", request.Recipient.PhoneNumber);
    }

    [Fact]
    public void Build_WithPhoneNumberAndName_SetsBoth()
    {
        var request = new SmsNotificationBuilder()
            .To("+123456789", "John")
            .Build();

        Assert.Equal("+123456789", request.Recipient.PhoneNumber);
        Assert.Equal("John", request.Recipient.Name);
    }

    [Fact]
    public void Build_WithPhoneNumberOnly_NameIsNull()
    {
        var request = new SmsNotificationBuilder()
            .To("+123456789")
            .Build();

        Assert.Null(request.Recipient.Name);
    }

    [Fact]
    public void Build_WithBody_SetsBody()
    {
        var request = new SmsNotificationBuilder()
            .To("+123456789")
            .WithBody("body text")
            .Build();

        Assert.Equal("body text", request.Body);
    }

    [Fact]
    public void Build_WithTemplate_SetsTemplateKey()
    {
        var request = new SmsNotificationBuilder()
            .To("+123456789")
            .WithTemplate("otp")
            .Build();

        Assert.Equal("otp", request.TemplateKey);
    }

    [Fact]
    public void Build_WithParameter_PopulatesParameters()
    {
        var request = new SmsNotificationBuilder()
            .To("+123456789")
            .WithParameter("code", "1234")
            .Build();

        Assert.Equal("1234", request.Parameters["code"]);
    }

    [Fact]
    public void Build_WithPriorityCritical_SetsPriority()
    {
        var request = new SmsNotificationBuilder()
            .To("+123456789")
            .WithPriority(NotificationPriority.Critical)
            .Build();

        Assert.Equal(NotificationPriority.Critical, request.Priority);
    }

    [Fact]
    public void Build_WithMetadata_SetsMetadata()
    {
        var request = new SmsNotificationBuilder()
            .To("+123456789")
            .WithMetadata("source", "unit-test")
            .Build();

        Assert.Equal("unit-test", request.Metadata["source"]);
    }

    [Fact]
    public void Build_WithoutCallingTo_ThrowsInvalidOperationException()
    {
        var builder = new SmsNotificationBuilder();

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void To_ReturnsSameBuilderInstance()
    {
        var builder = new SmsNotificationBuilder();

        var result = builder.To("+123456789");

        Assert.True(ReferenceEquals(builder, result));
    }
}
