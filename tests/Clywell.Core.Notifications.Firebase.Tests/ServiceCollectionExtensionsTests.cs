using Clywell.Core.Notifications.Firebase;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Xunit;

namespace Clywell.Core.Notifications.Firebase.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNotificationsFirebase_RegistersFirebasePushChannel()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNotificationsFirebase();

        // Assert
        var descriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(INotificationChannel) && sd.ImplementationType == typeof(FirebasePushChannel));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
    }
}
