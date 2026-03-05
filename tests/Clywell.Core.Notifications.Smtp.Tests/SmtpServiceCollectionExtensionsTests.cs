using Microsoft.Extensions.DependencyInjection;

namespace Clywell.Core.Notifications.Smtp.Tests;

public sealed class SmtpServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNotificationsSmtp_RegistersChannel()
    {
        var services = new ServiceCollection();

        services.AddNotificationsSmtp(options => options
            .UseHost("smtp.example.com")
            .UseSender("noreply@example.com"));

        var descriptor = services.SingleOrDefault(x =>
            x.ServiceType == typeof(INotificationChannel) &&
            x.ImplementationType == typeof(SmtpNotificationChannel));

        Assert.NotNull(descriptor);

        var factoryDescriptor = services.SingleOrDefault(x =>
            x.ServiceType == typeof(ISmtpClientFactory) &&
            x.ImplementationType == typeof(DefaultSmtpClientFactory));

        Assert.NotNull(factoryDescriptor);
    }

    [Fact]
    public void AddNotificationsSmtp_WithNullServices_ThrowsArgumentNullException()
    {
        ServiceCollection? services = null;

        Assert.Throws<ArgumentNullException>(() =>
            ServiceCollectionExtensions.AddNotificationsSmtp(services!, _ => { }));
    }

    [Fact]
    public void AddNotificationsSmtp_WithNullConfigure_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() =>
            services.AddNotificationsSmtp(null!));
    }

    [Fact]
    public void AddNotificationsSmtp_AllowsMultipleChannelRegistrations()
    {
        var services = new ServiceCollection();

        services.AddNotificationsSmtp(options => options
            .UseHost("smtp1.example.com")
            .UseSender("noreply1@example.com"));

        services.AddNotificationsSmtp(options => options
            .UseHost("smtp2.example.com")
            .UseSender("noreply2@example.com"));

        var channelRegistrations = services.Count(x =>
            x.ServiceType == typeof(INotificationChannel) &&
            x.ImplementationType == typeof(SmtpNotificationChannel));

        Assert.Equal(2, channelRegistrations);

        var factoryRegistrations = services.Count(x =>
            x.ServiceType == typeof(ISmtpClientFactory) &&
            x.ImplementationType == typeof(DefaultSmtpClientFactory));

        Assert.Equal(1, factoryRegistrations);
    }
}
