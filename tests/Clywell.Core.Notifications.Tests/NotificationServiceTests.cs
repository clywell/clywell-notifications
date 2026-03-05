using Microsoft.Extensions.Logging.Abstractions;

namespace Clywell.Core.Notifications.Tests;

public sealed class NotificationServiceTests
{
    [Fact]
    public async Task SendAsync_WithRegisteredChannel_DispatchesToCorrectChannel()
    {
        var emailChannel = new Mock<INotificationChannel>();
        emailChannel.SetupGet(x => x.Channel).Returns(NotificationChannel.Email);
        emailChannel
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationResult { NotificationId = "email-id", Status = NotificationStatus.Sent, SentAt = DateTimeOffset.UtcNow });

        var smsChannel = new Mock<INotificationChannel>();
        smsChannel.SetupGet(x => x.Channel).Returns(NotificationChannel.Sms);
        var expectedResult = new NotificationResult { NotificationId = "sms-id", Status = NotificationStatus.Sent, SentAt = DateTimeOffset.UtcNow };
        smsChannel
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var service = CreateService(new[] { emailChannel.Object, smsChannel.Object });
        var request = CreateRequest(NotificationChannel.Sms);

        var result = await service.SendAsync(request);

        Assert.Same(expectedResult, result);
        smsChannel.Verify(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        emailChannel.Verify(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendAsync_WithNoRegisteredChannel_ReturnsFailedResult()
    {
        var service = CreateService(Array.Empty<INotificationChannel>());
        var request = CreateRequest(NotificationChannel.Push);

        var result = await service.SendAsync(request);

        Assert.Equal(NotificationStatus.Failed, result.Status);
        Assert.NotNull(result.NotificationId);
        Assert.Contains("No channel registered", result.ErrorMessage);
    }

    [Fact]
    public async Task SendAsync_WithTemplateKey_RendersTemplateBeforeDispatching()
    {
        var expectedContent = new RenderedContent("Rendered subject", "<p>Rendered body</p>", "Rendered body");

        var channel = new Mock<INotificationChannel>();
        channel.SetupGet(x => x.Channel).Returns(NotificationChannel.Email);
        channel
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationResult { NotificationId = "template-id", Status = NotificationStatus.Sent, SentAt = DateTimeOffset.UtcNow });

        var renderer = new Mock<ITemplateRenderer>();
        renderer
            .Setup(x => x.RenderAsync("welcome", It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedContent);

        var service = CreateService(new[] { channel.Object }, renderer: renderer.Object);
        var request = CreateRequest(NotificationChannel.Email) with
        {
            TemplateKey = "welcome",
            Parameters = new Dictionary<string, object> { ["name"] = "Sodiq" }
        };

        var result = await service.SendAsync(request);

        Assert.Equal(NotificationStatus.Sent, result.Status);
        renderer.Verify(x => x.RenderAsync("welcome", request.Parameters, It.IsAny<CancellationToken>()), Times.Once);
        channel.Verify(
            x => x.SendAsync(
                It.Is<NotificationMessage>(m =>
                    m.Content == expectedContent &&
                    m.Recipient == request.Recipient),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_WithTemplateKeyButNoRenderer_ThrowsInvalidOperationException()
    {
        var channel = new Mock<INotificationChannel>();
        channel.SetupGet(x => x.Channel).Returns(NotificationChannel.Email);

        var service = CreateService(new[] { channel.Object });
        var request = CreateRequest(NotificationChannel.Email) with { TemplateKey = "missing-renderer" };

        var result = await service.SendAsync(request);

        Assert.Equal(NotificationStatus.Failed, result.Status);
        Assert.Contains("no ITemplateRenderer is registered", result.ErrorMessage);
        channel.Verify(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendAsync_WhenChannelThrows_ReturnsFailedResultWithErrorMessage()
    {
        var channel = new Mock<INotificationChannel>();
        channel.SetupGet(x => x.Channel).Returns(NotificationChannel.Email);
        channel
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("provider unavailable"));

        var service = CreateService(
            new[] { channel.Object },
            options: new NotificationOptions().WithRetryDelay(TimeSpan.Zero));
        var request = CreateRequest(NotificationChannel.Email);

        var result = await service.SendAsync(request);

        Assert.Equal(NotificationStatus.Failed, result.Status);
        Assert.Equal("provider unavailable", result.ErrorMessage);
    }

    [Fact]
    public async Task SendAsync_Batch_ReturnsResultForEachRequest()
    {
        var channel = new Mock<INotificationChannel>();
        channel.SetupGet(x => x.Channel).Returns(NotificationChannel.Email);
        channel
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationResult { NotificationId = "batch-id", Status = NotificationStatus.Sent, SentAt = DateTimeOffset.UtcNow });

        var service = CreateService(new[] { channel.Object });
        var requests = new[]
        {
            CreateRequest(NotificationChannel.Email),
            CreateRequest(NotificationChannel.Email)
        };

        var results = await service.SendAsync(requests);

        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.Equal(NotificationStatus.Sent, r.Status));
        channel.Verify(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SendAsync_Batch_ContinuesOnIndividualFailure()
    {
        var channel = new Mock<INotificationChannel>();
        channel.SetupGet(x => x.Channel).Returns(NotificationChannel.Email);

        var invocation = 0;
        channel
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                invocation++;
                if (invocation == 1)
                {
                    throw new InvalidOperationException("first failed");
                }

                return Task.FromResult(new NotificationResult
                {
                    NotificationId = "second-id",
                    Status = NotificationStatus.Sent,
                    SentAt = DateTimeOffset.UtcNow
                });
            });

        var service = CreateService(
            new[] { channel.Object },
            options: new NotificationOptions().WithMaxRetryAttempts(0));
        var requests = new[]
        {
            CreateRequest(NotificationChannel.Email),
            CreateRequest(NotificationChannel.Email)
        };

        var results = await service.SendAsync(requests);

        Assert.Equal(2, results.Count);
        Assert.Equal(NotificationStatus.Failed, results[0].Status);
        Assert.Equal("first failed", results[0].ErrorMessage);
        Assert.Equal(NotificationStatus.Sent, results[1].Status);
        channel.Verify(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SendAsync_WithNullChannel_UsesDefaultChannelFromOptions()
    {
        var emailChannel = new Mock<INotificationChannel>();
        emailChannel.SetupGet(x => x.Channel).Returns(NotificationChannel.Email);
        emailChannel
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationResult { NotificationId = "default-channel-id", Status = NotificationStatus.Sent, SentAt = DateTimeOffset.UtcNow });

        var service = CreateService(
            new[] { emailChannel.Object },
            options: new NotificationOptions().UseDefaultChannel(NotificationChannel.Email));

        var request = CreateRequest(NotificationChannel.Sms) with { Channel = null };

        var result = await service.SendAsync(request);

        Assert.Equal(NotificationStatus.Sent, result.Status);
        emailChannel.Verify(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_WhenChannelReturnsFailed_RetriesUpToMaxAttempts()
    {
        var channel = new Mock<INotificationChannel>();
        channel.SetupGet(x => x.Channel).Returns(NotificationChannel.Email);
        channel
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationResult
            {
                NotificationId = "failed-id",
                Status = NotificationStatus.Failed,
                ErrorMessage = "temporary failure"
            });

        var options = new NotificationOptions()
            .WithMaxRetryAttempts(2)
            .WithRetryDelay(TimeSpan.Zero);
        var service = CreateService(new[] { channel.Object }, options: options);

        var result = await service.SendAsync(CreateRequest(NotificationChannel.Email));

        Assert.Equal(NotificationStatus.Failed, result.Status);
        channel.Verify(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task SendAsync_WhenRetrySucceeds_StopsRetrying()
    {
        var channel = new Mock<INotificationChannel>();
        channel.SetupGet(x => x.Channel).Returns(NotificationChannel.Email);

        var attempt = 0;
        channel
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                attempt++;
                if (attempt < 3)
                {
                    return Task.FromResult(new NotificationResult
                    {
                        NotificationId = $"retry-{attempt}",
                        Status = NotificationStatus.Failed,
                        ErrorMessage = "transient"
                    });
                }

                return Task.FromResult(new NotificationResult
                {
                    NotificationId = "retry-success",
                    Status = NotificationStatus.Sent,
                    SentAt = DateTimeOffset.UtcNow
                });
            });

        var options = new NotificationOptions()
            .WithMaxRetryAttempts(5)
            .WithRetryDelay(TimeSpan.Zero);
        var service = CreateService(new[] { channel.Object }, options: options);

        var result = await service.SendAsync(CreateRequest(NotificationChannel.Email));

        Assert.Equal(NotificationStatus.Sent, result.Status);
        channel.Verify(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task SendAsync_WhenChannelThrows_RetriesUpToMaxAttempts()
    {
        var channel = new Mock<INotificationChannel>();
        channel.SetupGet(x => x.Channel).Returns(NotificationChannel.Email);
        channel
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("provider unavailable"));

        var options = new NotificationOptions()
            .WithMaxRetryAttempts(2)
            .WithRetryDelay(TimeSpan.Zero);
        var service = CreateService(new[] { channel.Object }, options: options);

        var result = await service.SendAsync(CreateRequest(NotificationChannel.Email));

        Assert.Equal(NotificationStatus.Failed, result.Status);
        Assert.Equal("provider unavailable", result.ErrorMessage);
        channel.Verify(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task SendAsync_WithNotificationLogger_LogsResult()
    {
        var result = new NotificationResult
        {
            NotificationId = "logged-id",
            Status = NotificationStatus.Sent,
            SentAt = DateTimeOffset.UtcNow
        };

        var channel = new Mock<INotificationChannel>();
        channel.SetupGet(x => x.Channel).Returns(NotificationChannel.Email);
        channel
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        var notificationLogger = new Mock<INotificationLogger>();

        var service = CreateService(new[] { channel.Object }, notificationLogger: notificationLogger.Object);

        await service.SendAsync(CreateRequest(NotificationChannel.Email));

        notificationLogger.Verify(x => x.LogAsync(result, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_WithInlineContent_UsesSubjectAndBody()
    {
        var channel = new Mock<INotificationChannel>();
        channel.SetupGet(x => x.Channel).Returns(NotificationChannel.Email);
        channel
            .Setup(x => x.SendAsync(It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationResult { NotificationId = "inline-id", Status = NotificationStatus.Sent, SentAt = DateTimeOffset.UtcNow });

        var service = CreateService(new[] { channel.Object });
        var request = CreateRequest(NotificationChannel.Email) with
        {
            Subject = "Hello",
            Body = "Inline body"
        };

        await service.SendAsync(request);

        channel.Verify(
            x => x.SendAsync(
                It.Is<NotificationMessage>(m =>
                    m.Content.Subject == "Hello" &&
                    m.Content.HtmlBody == "Inline body" &&
                    m.Content.PlainTextBody == "Inline body"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static NotificationRequest CreateRequest(NotificationChannel channel)
    {
        return new NotificationRequest
        {
            Channel = channel,
            Recipient = new NotificationRecipient
            {
                Email = "user@example.com",
                Name = "User"
            },
            Subject = "subject",
            Body = "body",
            Priority = NotificationPriority.Normal,
            Metadata = new Dictionary<string, string> { ["trace-id"] = Guid.NewGuid().ToString("N") }
        };
    }

    private static NotificationService CreateService(
        IEnumerable<INotificationChannel> channels,
        ITemplateRenderer? renderer = null,
        INotificationLogger? notificationLogger = null,
        NotificationOptions? options = null)
    {
        return new NotificationService(
            channels,
            NullLogger<NotificationService>.Instance,
            options ?? new NotificationOptions(),
            renderer,
            notificationLogger);
    }
}
