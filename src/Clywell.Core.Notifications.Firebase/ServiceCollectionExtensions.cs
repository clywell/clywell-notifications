using System;
using Clywell.Core.Notifications;
using Clywell.Core.Notifications.Firebase;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring Firebase Push notifications.
/// </summary>
public static class FirebaseServiceCollectionExtensions
{
    /// <summary>
    /// Adds Firebase Cloud Messaging as a push notification provider.
    /// </summary>
    public static IServiceCollection AddNotificationsFirebase(
        this IServiceCollection services,
        Action<FirebaseOptions>? configure = null)
    {
        var options = new FirebaseOptions();
        configure?.Invoke(options);

        // Initialize Firebase context if it hasn't been already.
        if (FirebaseApp.DefaultInstance == null)
        {
            var appOptions = new AppOptions();
            
            if (!string.IsNullOrEmpty(options.CredentialFilePath))
            {
                appOptions.Credential = GoogleCredential.FromFile(options.CredentialFilePath);
            }
            else if (!string.IsNullOrEmpty(options.CredentialJson))
            {
                appOptions.Credential = GoogleCredential.FromJson(options.CredentialJson);
            }
            else
            {
                // Fallback to Application Default Credentials
                appOptions.Credential = GoogleCredential.GetApplicationDefault();
            }

            if (!string.IsNullOrEmpty(options.ProjectId))
            {
                appOptions.ProjectId = options.ProjectId;
            }

            FirebaseApp.Create(appOptions);
        }

        services.TryAddSingleton(sp => FirebaseMessaging.DefaultInstance);
        services.AddTransient<INotificationChannel, FirebasePushChannel>();

        return services;
    }
}
