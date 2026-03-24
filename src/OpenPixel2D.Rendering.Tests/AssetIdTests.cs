using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering.Tests;

public sealed class AssetIdTests
{
    [Fact]
    public void AssetId_WithSameValue_AreEqual()
    {
        AssetId first = new("player");
        AssetId second = new("player");

        Assert.Equal(first, second);
        Assert.True(first == second);
    }

    [Fact]
    public void DefaultAssetId_DiffersFromAssignedValue()
    {
        AssetId asset = default;

        Assert.True(asset.Value is null);
        Assert.NotEqual(asset, new AssetId("player"));
    }
}
