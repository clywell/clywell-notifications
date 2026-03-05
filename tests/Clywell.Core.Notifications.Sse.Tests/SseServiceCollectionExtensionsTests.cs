using Microsoft.Extensions.DependencyInjection;

namespace Clywell.Core.Notifications.Sse.Tests;

public sealed class SseServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNotificationsSse_RegistersChannel()
    {
        var services = new ServiceCollection();

        services.AddNotificationsSse();

        var descriptor = services.SingleOrDefault(x =>
            x.ServiceType == typeof(INotificationChannel) &&
            x.ImplementationType == typeof(SseNotificationChannel));

        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddNotificationsSse_RegistersConnectionManager()
    {
        var services = new ServiceCollection();

        services.AddNotificationsSse();

        var descriptor = services.SingleOrDefault(x =>
            x.ServiceType == typeof(ISseConnectionManager));

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddNotificationsSse_WithNullServices_ThrowsArgumentNullException()
    {
        IServiceCollection? services = null;

        Assert.Throws<ArgumentNullException>(() =>
            ServiceCollectionExtensions.AddNotificationsSse(services!));
    }

    [Fact]
    public void AddNotificationsSse_WithConfigure_AppliesOptions()
    {
        var services = new ServiceCollection();

        services.AddNotificationsSse(options => options
            .WithEventName("custom")
            .UseConnectionAddressing());

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<SseOptions>();

        Assert.Equal("custom", options.EventName);
        Assert.False(options.UseUserBasedAddressing);
    }
}
