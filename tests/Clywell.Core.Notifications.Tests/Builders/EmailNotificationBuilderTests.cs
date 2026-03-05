namespace Clywell.Core.Notifications.Tests;

public sealed class EmailNotificationBuilderTests
{
    [Fact]
    public void Build_SetsChannelToEmail()
    {
        var request = new EmailNotificationBuilder()
            .To("a@b.com")
            .Build();

        Assert.Equal(NotificationChannel.Email, request.Channel);
    }

    [Fact]
    public void Build_WithEmailRecipient_SetsEmail()
    {
        var request = new EmailNotificationBuilder()
            .To("user@example.com")
            .Build();

        Assert.Equal("user@example.com", request.Recipient.Email);
    }

    [Fact]
    public void Build_WithEmailAndName_SetsBoth()
    {
        var request = new EmailNotificationBuilder()
            .To("user@example.com", "John")
            .Build();

        Assert.Equal("user@example.com", request.Recipient.Email);
        Assert.Equal("John", request.Recipient.Name);
    }

    [Fact]
    public void Build_WithEmailOnly_NameIsNull()
    {
        var request = new EmailNotificationBuilder()
            .To("user@example.com")
            .Build();

        Assert.Null(request.Recipient.Name);
    }

    [Fact]
    public void Build_WithSubject_SetsSubject()
    {
        var request = new EmailNotificationBuilder()
            .To("x@x.com")
            .WithSubject("Hello")
            .Build();

        Assert.Equal("Hello", request.Subject);
    }

    [Fact]
    public void Build_WithBody_SetsBody()
    {
        var request = new EmailNotificationBuilder()
            .To("x@x.com")
            .WithBody("body text")
            .Build();

        Assert.Equal("body text", request.Body);
    }

    [Fact]
    public void Build_WithTemplate_SetsTemplateKey()
    {
        var request = new EmailNotificationBuilder()
            .To("x@x.com")
            .WithTemplate("welcome")
            .Build();

        Assert.Equal("welcome", request.TemplateKey);
    }

    [Fact]
    public void Build_WithParameter_PopulatesParameters()
    {
        var request = new EmailNotificationBuilder()
            .To("x@x.com")
            .WithParameter("key", "value")
            .Build();

        Assert.Equal("value", request.Parameters["key"]);
    }

    [Fact]
    public void Build_WithMultipleParameters_AllPresent()
    {
        var request = new EmailNotificationBuilder()
            .To("x@x.com")
            .WithParameter("key1", "value1")
            .WithParameter("key2", 2)
            .Build();

        Assert.Equal("value1", request.Parameters["key1"]);
        Assert.Equal(2, request.Parameters["key2"]);
    }

    [Fact]
    public void Build_WithPriorityCritical_SetsPriority()
    {
        var request = new EmailNotificationBuilder()
            .To("x@x.com")
            .WithPriority(NotificationPriority.Critical)
            .Build();

        Assert.Equal(NotificationPriority.Critical, request.Priority);
    }

    [Fact]
    public void Build_WithMetadata_SetsMetadata()
    {
        var request = new EmailNotificationBuilder()
            .To("x@x.com")
            .WithMetadata("source", "unit-test")
            .Build();

        Assert.Equal("unit-test", request.Metadata["source"]);
    }

    [Fact]
    public void Build_WithoutCallingTo_ThrowsInvalidOperationException()
    {
        var builder = new EmailNotificationBuilder();

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_ParametersDictionary_IsDefensiveCopy()
    {
        var builder = new EmailNotificationBuilder()
            .To("x@x.com")
            .WithParameter("key", "initial");

        var request = builder.Build();
        builder.WithParameter("key", "updated");

        Assert.Equal("initial", request.Parameters["key"]);
    }

    [Fact]
    public void WithSubject_ReturnsSameBuilderInstance()
    {
        var builder = new EmailNotificationBuilder();

        var result = builder.WithSubject("s");

        Assert.True(ReferenceEquals(builder, result));
    }

    [Fact]
    public void To_ReturnsSameBuilderInstance()
    {
        var builder = new EmailNotificationBuilder();

        var result = builder.To("a@b.com");

        Assert.True(ReferenceEquals(builder, result));
    }

    [Fact]
    public void WithParameters_MergesAllParameters()
    {
        var request = new EmailNotificationBuilder()
            .To("a@b.com")
            .WithParameters(new Dictionary<string, object>
            {
                ["key1"] = "value1",
                ["key2"] = 42
            })
            .Build();

        Assert.Equal("value1", request.Parameters["key1"]);
        Assert.Equal(42, request.Parameters["key2"]);
    }

    [Fact]
    public void WithParameters_ChainedWithWithParameter_AllPresent()
    {
        var request = new EmailNotificationBuilder()
            .To("a@b.com")
            .WithParameter("existing", "old")
            .WithParameters(new Dictionary<string, object> { ["existing"] = "new", ["added"] = "yes" })
            .Build();

        Assert.Equal("new", request.Parameters["existing"]);
        Assert.Equal("yes", request.Parameters["added"]);
    }

    [Fact]
    public void WithParameters_WithNull_ThrowsArgumentNullException()
    {
        var builder = new EmailNotificationBuilder().To("a@b.com");
        Assert.Throws<ArgumentNullException>(() => builder.WithParameters(null!));
    }

    [Fact]
    public void WithParameters_ReturnsSameBuilderInstance()
    {
        var builder = new EmailNotificationBuilder().To("a@b.com");
        var result = builder.WithParameters(new Dictionary<string, object> { ["x"] = 1 });
        Assert.Same(builder, result);
    }
}
