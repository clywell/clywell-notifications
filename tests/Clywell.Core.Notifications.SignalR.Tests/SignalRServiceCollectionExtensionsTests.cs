using Microsoft.Extensions.DependencyInjection;

namespace Clywell.Core.Notifications.SignalR.Tests;

public sealed class SignalRServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNotificationsSignalR_RegistersChannel()
    {
        var services = new ServiceCollection();

        services.AddNotificationsSignalR();

        var descriptor = services.SingleOrDefault(x =>
            x.ServiceType == typeof(INotificationChannel) &&
            x.ImplementationType == typeof(SignalRNotificationChannel));

        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddNotificationsSignalR_WithNullServices_ThrowsArgumentNullException()
    {
        IServiceCollection? services = null;

        Assert.Throws<ArgumentNullException>(() =>
            ServiceCollectionExtensions.AddNotificationsSignalR(services!));
    }

    [Fact]
    public void AddNotificationsSignalR_WithConfigure_AppliesOptions()
    {
        var services = new ServiceCollection();

        services.AddNotificationsSignalR(options => options
            .WithMethodName("CustomMethod")
            .UseConnectionAddressing());

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<SignalROptions>();

        Assert.Equal("CustomMethod", options.MethodName);
        Assert.False(options.UseUserBasedAddressing);
    }

    [Fact]
    public void AddNotificationsSignalR_RegistersOptionsAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddNotificationsSignalR();

        var descriptor = services.SingleOrDefault(x =>
            x.ServiceType == typeof(SignalROptions));

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }
}
