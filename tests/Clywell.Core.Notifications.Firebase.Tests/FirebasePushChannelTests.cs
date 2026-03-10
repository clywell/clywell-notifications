using Clywell.Core.Notifications;
using Clywell.Core.Notifications.Firebase;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Clywell.Core.Notifications.Firebase.Tests;

public class FirebasePushChannelTests
{
    private readonly FirebasePushChannel _channel;

    public FirebasePushChannelTests()
    {
        // For unit testing the channel logic without making real HTTP calls to Google, 
        // we would typically mock the underlying Firebase provider.
        // However, FirebaseMessaging.DefaultInstance cannot be easily mocked because it's a sealed class 
        // with internal constructors in the FirebaseAdmin SDK.
        // In a real-world scenario, we'd wrap FirebaseMessaging in our own interface (e.g. IFirebaseClient).
        // For the sake of this test verifying the channel metadata and rejection logic:
        
        // We initialize a mock App to satisfy the FirebaseMessaging constructor requirements if needed
        // but since we are testing failure paths, we can test the immediate rejection logic.
        _channel = new FirebasePushChannel(null!); // Passing null since we can't easily mock the sealed class. We'll test the pre-flight checks.
    }

    [Fact]
    public void Channel_ShouldBePush()
    {
        // Assert
        Assert.Equal(NotificationChannel.Push, _channel.Channel);
    }

    [Fact]
    public async Task SendAsync_WithoutDeviceTokenOrUserId_ReturnsFailure()
    {
        // Arrange
        var message = new NotificationMessage
        {
            Recipient = new NotificationRecipient
            {
                Email = "test@example.com" // Missing DeviceToken and UserId
            },
            Content = new RenderedContent("Test Subject", "Test Body"),
            Priority = NotificationPriority.Normal
        };

        // Act
        var result = await _channel.SendAsync(message, CancellationToken.None);

        // Assert
        Assert.Equal(NotificationStatus.Failed, result.Status);
        Assert.Contains("Firebase channel requires Recipient.DeviceToken or Recipient.UserId", result.ErrorMessage);
    }
}
