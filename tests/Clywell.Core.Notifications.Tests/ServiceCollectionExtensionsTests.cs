using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Clywell.Core.Notifications.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNotifications_RegistersNotificationService()
    {
        var services = new ServiceCollection();
        services.AddScoped<ILogger<NotificationService>>(_ => NullLogger<NotificationService>.Instance);
        services.AddScoped<INotificationChannel, TestNotificationChannel>();

        services.AddNotifications();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetService<INotificationService>();

        Assert.NotNull(service);
        Assert.IsType<NotificationService>(service);
    }

    [Fact]
    public void AddNotifications_WithNullServices_ThrowsArgumentNullException()
    {
        IServiceCollection? services = null;

        var action = () => ServiceCollectionExtensions.AddNotifications(services!);

        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void AddNotifications_WithConfigure_AppliesOptions()
    {
        var services = new ServiceCollection();

        services.AddNotifications(options => options
            .UseDefaultChannel(NotificationChannel.Sms)
            .WithMaxRetryAttempts(7)
            .WithRetryDelay(TimeSpan.FromSeconds(10)));

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<NotificationOptions>();

        Assert.Equal(NotificationChannel.Sms, options.DefaultChannel);
        Assert.Equal(7, options.MaxRetryAttempts);
        Assert.Equal(TimeSpan.FromSeconds(10), options.RetryDelay);
    }

    [Fact]
    public void AddNotifications_DoesNotReplaceExistingRegistration()
    {
        var services = new ServiceCollection();
        services.AddScoped<INotificationService, ExistingNotificationService>();

        services.AddNotifications();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<INotificationService>();

        Assert.IsType<ExistingNotificationService>(service);
    }

    private sealed class TestNotificationChannel : INotificationChannel
    {
        public NotificationChannel Channel => NotificationChannel.Email;

        public Task<NotificationResult> SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new NotificationResult
            {
                NotificationId = "test-id",
                Status = NotificationStatus.Sent,
                SentAt = DateTimeOffset.UtcNow
            });
        }
    }

    private sealed class ExistingNotificationService : INotificationService
    {
        public Task<NotificationResult> SendAsync(NotificationRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new NotificationResult
            {
                NotificationId = "existing",
                Status = NotificationStatus.Sent,
                SentAt = DateTimeOffset.UtcNow
            });
        }

        public Task<IReadOnlyList<NotificationResult>> SendAsync(IEnumerable<NotificationRequest> requests, CancellationToken cancellationToken = default)
        {
            var results = requests.Select(_ => new NotificationResult
            {
                NotificationId = "existing",
                Status = NotificationStatus.Sent,
                SentAt = DateTimeOffset.UtcNow
            }).ToList();

            return Task.FromResult<IReadOnlyList<NotificationResult>>(results);
        }
    }
}
