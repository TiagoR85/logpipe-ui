using LogPipe.Ui.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LogPipe.Ui.Tests;

public sealed class LevelBadgeTests : BunitContext
{
    [Theory]
    [InlineData("Critical", "text-bg-danger")]
    [InlineData("Error", "text-bg-danger")]
    [InlineData("Warning", "text-bg-warning")]
    [InlineData("Information", "text-bg-info")]
    [InlineData("Debug", "text-bg-secondary")]
    [InlineData("Trace", "text-bg-secondary")]
    public void Renders_bootstrap_class_per_level(string level, string expectedClass)
    {
        var cut = Render<LevelBadge>(parameters => parameters.Add(p => p.Level, level));
        Assert.Contains(expectedClass, cut.Markup);
        Assert.Contains(level, cut.Markup);
    }
}
