using LogPipe.Ui.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LogPipe.Ui.Tests;

public sealed class SmokeTests : BunitContext
{
    [Fact]
    public void App_renders()
    {
        var cut = Render<Routes>();
        Assert.Contains("logpipe", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }
}
