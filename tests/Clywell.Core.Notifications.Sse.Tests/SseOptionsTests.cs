namespace Clywell.Core.Notifications.Sse.Tests;

public sealed class SseOptionsTests
{
    [Fact]
    public void Defaults_EventNameIsNotification()
    {
        var options = new SseOptions();

        Assert.Equal("notification", options.EventName);
    }

    [Fact]
    public void Defaults_UseUserBasedAddressingIsTrue()
    {
        var options = new SseOptions();

        Assert.True(options.UseUserBasedAddressing);
    }

    [Fact]
    public void WithEventName_SetsValue()
    {
        var options = new SseOptions()
            .WithEventName("custom-event");

        Assert.Equal("custom-event", options.EventName);
    }

    [Fact]
    public void UseConnectionAddressing_SetsUserBasedToFalse()
    {
        var options = new SseOptions()
            .UseConnectionAddressing();

        Assert.False(options.UseUserBasedAddressing);
    }

    [Fact]
    public void UseUserAddressing_SetsUserBasedToTrue()
    {
        var options = new SseOptions()
            .UseConnectionAddressing()
            .UseUserAddressing();

        Assert.True(options.UseUserBasedAddressing);
    }

    [Fact]
    public void FluentChaining_ReturnsThis()
    {
        var options = new SseOptions();

        var result = options
            .WithEventName("test")
            .UseConnectionAddressing();

        Assert.Same(options, result);
    }

    [Fact]
    public void WithEventName_NullOrWhitespace_ThrowsArgumentException()
    {
        var options = new SseOptions();

        Assert.Throws<ArgumentException>(() => options.WithEventName(string.Empty));
        Assert.Throws<ArgumentException>(() => options.WithEventName("  "));
    }
}
