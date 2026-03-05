namespace Clywell.Core.Notifications.Smtp;

/// <summary>
/// Extension methods for registering the SMTP notification channel.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SMTP Email channel using MailKit.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">
    /// Delegate to configure <see cref="SmtpOptions"/>.
    /// <code>
    /// services.AddNotificationsSmtp(smtp =&gt; smtp
    ///     .UseHost("smtp.example.com")
    ///     .WithCredentials("user", "pass")
    ///     .UseSender("noreply@example.com", "My App"));
    /// </code>
    /// </param>
    public static IServiceCollection AddNotificationsSmtp(this IServiceCollection services, Action<SmtpOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new SmtpOptions();
        configure(options);
        services.AddSingleton(options);

        services.TryAddSingleton<ISmtpClientFactory, DefaultSmtpClientFactory>();
        services.AddScoped<INotificationChannel, SmtpNotificationChannel>();

        return services;
    }
}
