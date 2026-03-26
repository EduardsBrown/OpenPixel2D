namespace OpenPixel2D.Content.Tests;

public sealed class RuntimeFontAssetTests
{
    [Fact]
    public void Constructor_StoresImmutableFontSource()
    {
        byte[] source = [1, 2, 3, 4];
        RuntimeFontAsset asset = new(
            RuntimeFontFormat.TrueType,
            new RuntimeFontFaceMetadata("Family", "Font", "Regular", RuntimeFontStyle.Regular, 1000, 800, -200, 200, 1200),
            source);
        source[0] = 9;

        Assert.Equal(RuntimeFontFormat.TrueType, asset.Format);
        Assert.Equal("Family", asset.Metadata.FamilyName);
        Assert.Equal([1, 2, 3, 4], asset.SourceData.ToArray());
    }

    [Fact]
    public void Constructor_WithEmptySourceData_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new RuntimeFontAsset(
            RuntimeFontFormat.TrueType,
            new RuntimeFontFaceMetadata("Family", "Font", "Regular", RuntimeFontStyle.Regular, 1000, 800, -200, 200, 1200),
            ReadOnlySpan<byte>.Empty));
    }
}
