using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering.Tests;

public sealed class BuiltInRenderPassRoutingTests
{
    [Fact]
    public void BuiltInRoutes_MapSpritesAndTextToExplicitPassesAndSpaces()
    {
        Assert.Equal(new BuiltInRenderPassRoute(RenderPassNames.WorldSprites, RenderSpace.World), BuiltInRenderPassRouting.Sprite);
        Assert.Equal(new BuiltInRenderPassRoute(RenderPassNames.UI, RenderSpace.Screen), BuiltInRenderPassRouting.Text);
    }
}
