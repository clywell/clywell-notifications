using Microsoft.Extensions.DependencyInjection;

namespace Clywell.Core.Notifications.Renderer.Scriban.Tests;

public class ScribanServiceCollectionExtensionsTests
{
    [Fact]
    public void AddScribanRenderer_RegistersTemplateRenderer()
    {
        var services = new ServiceCollection();
        services.AddScoped<ITemplateProvider>(_ => Mock.Of<ITemplateProvider>());

        services.AddScribanRenderer();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var renderer = scope.ServiceProvider.GetService<ITemplateRenderer>();

        Assert.NotNull(renderer);
        Assert.IsType<ScribanTemplateRenderer>(renderer);
    }

    [Fact]
    public void AddScribanRenderer_WithNullServices_ThrowsArgumentNullException()
    {
        IServiceCollection? services = null;

        var ex = Assert.Throws<ArgumentNullException>(() =>
            ServiceCollectionExtensions.AddScribanRenderer(services!));

        Assert.Equal("services", ex.ParamName);
    }

    [Fact]
    public void AddScribanRenderer_DoesNotReplaceExistingRegistration()
    {
        var services = new ServiceCollection();
        var existingRenderer = new ExistingTemplateRenderer();

        services.AddScoped<ITemplateRenderer>(_ => existingRenderer);
        services.AddScoped<ITemplateProvider>(_ => Mock.Of<ITemplateProvider>());

        services.AddScribanRenderer();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var resolvedRenderer = scope.ServiceProvider.GetRequiredService<ITemplateRenderer>();

        Assert.Same(existingRenderer, resolvedRenderer);
    }

    private sealed class ExistingTemplateRenderer : ITemplateRenderer
    {
        public Task<RenderedContent> RenderAsync(
            string templateKey,
            Dictionary<string, object> parameters,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new RenderedContent("subject", "html", "text"));
        }
    }
}
