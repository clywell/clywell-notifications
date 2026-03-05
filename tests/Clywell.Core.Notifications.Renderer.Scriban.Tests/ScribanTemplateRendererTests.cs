namespace Clywell.Core.Notifications.Renderer.Scriban.Tests;

public class ScribanTemplateRendererTests
{
    [Fact]
    public async Task RenderAsync_WithValidTemplate_RendersAllParts()
    {
        var provider = new Mock<ITemplateProvider>();
        provider
            .Setup(p => p.GetTemplateAsync("welcome", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TemplateDefinition(
                "Welcome {{ name }}",
                "<h1>Hello {{ name }}</h1>",
                "Hello {{ name }}"));

        var sut = new ScribanTemplateRenderer(provider.Object);

        var result = await sut.RenderAsync("welcome", new Dictionary<string, object>
        {
            ["name"] = "Alice"
        });

        Assert.Equal("Welcome Alice", result.Subject);
        Assert.Equal("<h1>Hello Alice</h1>", result.HtmlBody);
        Assert.Equal("Hello Alice", result.PlainTextBody);
    }

    [Fact]
    public async Task RenderAsync_WithParameters_SubstitutesValues()
    {
        var provider = new Mock<ITemplateProvider>();
        provider
            .Setup(p => p.GetTemplateAsync("greeting", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TemplateDefinition(
                "Hello {{ name }}",
                null,
                null));

        var sut = new ScribanTemplateRenderer(provider.Object);

        var result = await sut.RenderAsync("greeting", new Dictionary<string, object>
        {
            ["name"] = "World"
        });

        Assert.Equal("Hello World", result.Subject);
    }

    [Fact]
    public async Task RenderAsync_WithUnknownTemplateKey_ThrowsInvalidOperationException()
    {
        var provider = new Mock<ITemplateProvider>();
        provider
            .Setup(p => p.GetTemplateAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TemplateDefinition?)null);

        var sut = new ScribanTemplateRenderer(provider.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.RenderAsync("missing", new Dictionary<string, object>()));

        Assert.Equal("Template 'missing' not found.", ex.Message);
    }

    [Fact]
    public async Task RenderAsync_WithNullSubjectTemplate_ReturnsNullSubject()
    {
        var provider = new Mock<ITemplateProvider>();
        provider
            .Setup(p => p.GetTemplateAsync("no-subject", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TemplateDefinition(
                null,
                "<p>Hi</p>",
                "Hi"));

        var sut = new ScribanTemplateRenderer(provider.Object);

        var result = await sut.RenderAsync("no-subject", new Dictionary<string, object>());

        Assert.Null(result.Subject);
        Assert.Equal("<p>Hi</p>", result.HtmlBody);
        Assert.Equal("Hi", result.PlainTextBody);
    }

    [Fact]
    public async Task RenderAsync_WithInvalidScribanSyntax_ThrowsInvalidOperationException()
    {
        var provider = new Mock<ITemplateProvider>();
        provider
            .Setup(p => p.GetTemplateAsync("bad-template", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TemplateDefinition(
                "{{ invalid }",
                null,
                null));

        var sut = new ScribanTemplateRenderer(provider.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.RenderAsync("bad-template", new Dictionary<string, object>()));

        Assert.Contains("Failed to parse Subject template for 'bad-template'", ex.Message);
    }

    [Fact]
    public async Task RenderAsync_WithEmptyParameters_RendersStaticContent()
    {
        var provider = new Mock<ITemplateProvider>();
        provider
            .Setup(p => p.GetTemplateAsync("static", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TemplateDefinition(
                "Static subject",
                "<p>Static html</p>",
                "Static text"));

        var sut = new ScribanTemplateRenderer(provider.Object);

        var result = await sut.RenderAsync("static", new Dictionary<string, object>());

        Assert.Equal("Static subject", result.Subject);
        Assert.Equal("<p>Static html</p>", result.HtmlBody);
        Assert.Equal("Static text", result.PlainTextBody);
    }
}
