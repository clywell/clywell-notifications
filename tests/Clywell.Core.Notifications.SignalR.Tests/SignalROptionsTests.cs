namespace Clywell.Core.Notifications.SignalR.Tests;

public sealed class SignalROptionsTests
{
    [Fact]
    public void Defaults_MethodNameIsReceiveNotification()
    {
        var options = new SignalROptions();

        Assert.Equal("ReceiveNotification", options.MethodName);
    }

    [Fact]
    public void Defaults_UseUserBasedAddressingIsTrue()
    {
        var options = new SignalROptions();

        Assert.True(options.UseUserBasedAddressing);
    }

    [Fact]
    public void WithMethodName_SetsValue()
    {
        var options = new SignalROptions()
            .WithMethodName("OnNotification");

        Assert.Equal("OnNotification", options.MethodName);
    }

    [Fact]
    public void UseConnectionAddressing_SetsUserBasedToFalse()
    {
        var options = new SignalROptions()
            .UseConnectionAddressing();

        Assert.False(options.UseUserBasedAddressing);
    }

    [Fact]
    public void UseUserAddressing_SetsUserBasedToTrue()
    {
        var options = new SignalROptions()
            .UseConnectionAddressing()
            .UseUserAddressing();

        Assert.True(options.UseUserBasedAddressing);
    }

    [Fact]
    public void FluentChaining_ReturnsThis()
    {
        var options = new SignalROptions();

        var result = options
            .WithMethodName("Test")
            .UseConnectionAddressing();

        Assert.Same(options, result);
    }

    [Fact]
    public void WithMethodName_NullOrWhitespace_ThrowsArgumentException()
    {
        var options = new SignalROptions();

        Assert.Throws<ArgumentException>(() => options.WithMethodName(string.Empty));
        Assert.Throws<ArgumentException>(() => options.WithMethodName("  "));
    }
}
