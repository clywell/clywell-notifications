namespace Clywell.Core.Notifications.Tests;

public sealed class NotificationOptionsTests
{
    [Fact]
    public void Defaults_DefaultChannelIsEmail()
    {
        var options = new NotificationOptions();

        Assert.Equal(NotificationChannel.Email, options.DefaultChannel);
    }

    [Fact]
    public void Defaults_MaxRetryAttemptsIsThree()
    {
        var options = new NotificationOptions();

        Assert.Equal(3, options.MaxRetryAttempts);
    }

    [Fact]
    public void Defaults_RetryDelayIsTwoSeconds()
    {
        var options = new NotificationOptions();

        Assert.Equal(TimeSpan.FromSeconds(2), options.RetryDelay);
    }

    [Fact]
    public void UseDefaultChannel_SetsChannel()
    {
        var options = new NotificationOptions()
            .UseDefaultChannel(NotificationChannel.Sms);

        Assert.Equal(NotificationChannel.Sms, options.DefaultChannel);
    }

    [Fact]
    public void WithMaxRetryAttempts_SetsValue()
    {
        var options = new NotificationOptions()
            .WithMaxRetryAttempts(5);

        Assert.Equal(5, options.MaxRetryAttempts);
    }

    [Fact]
    public void WithRetryDelay_SetsValue()
    {
        var options = new NotificationOptions()
            .WithRetryDelay(TimeSpan.FromSeconds(10));

        Assert.Equal(TimeSpan.FromSeconds(10), options.RetryDelay);
    }

    [Fact]
    public void FluentChaining_AllMethodsReturnSameInstance()
    {
        var options = new NotificationOptions();

        var result = options
            .UseDefaultChannel(NotificationChannel.Sms)
            .WithMaxRetryAttempts(5)
            .WithRetryDelay(TimeSpan.FromSeconds(10));

        Assert.Same(options, result);
    }

    [Fact]
    public void WithMaxRetryAttempts_NegativeValue_ThrowsArgumentOutOfRangeException()
    {
        var options = new NotificationOptions();

        Assert.Throws<ArgumentOutOfRangeException>(() => options.WithMaxRetryAttempts(-1));
    }

    [Fact]
    public void RenderingMode_DefaultsToLocal()
    {
        var options = new NotificationOptions();

        Assert.Equal(RenderingMode.Local, options.RenderingMode);
    }

    [Fact]
    public void WithRenderingMode_SetsDelegated_ReturnsCorrectValue()
    {
        var options = new NotificationOptions()
            .WithRenderingMode(RenderingMode.Delegated);

        Assert.Equal(RenderingMode.Delegated, options.RenderingMode);
    }

    [Fact]
    public void WithRenderingMode_ReturnsSameInstance_ForFluentChaining()
    {
        var options = new NotificationOptions();

        var result = options.WithRenderingMode(RenderingMode.Delegated);

        Assert.Same(options, result);
    }

    [Fact]
    public void WithRenderingMode_CanBeChainedWithOtherMethods()
    {
        var options = new NotificationOptions()
            .WithRenderingMode(RenderingMode.Delegated)
            .WithMaxRetryAttempts(5);

        Assert.Equal(RenderingMode.Delegated, options.RenderingMode);
        Assert.Equal(5, options.MaxRetryAttempts);
    }
}
