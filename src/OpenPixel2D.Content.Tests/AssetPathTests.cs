namespace OpenPixel2D.Content.Tests;

public sealed class AssetPathTests
{
    [Fact]
    public void Constructor_NormalizesSeparatorsAndCollapsesDuplicateDelimiters()
    {
        AssetPath path = new("textures\\characters//player.png");

        Assert.Equal("textures/characters/player.png", path.Value);
        Assert.Equal("textures/characters/player.png", path.ToString());
        Assert.Equal(new AssetPath("textures/characters/player.png"), path);
    }

    [Fact]
    public void ImplicitConversion_FromString_RemainsErgonomic()
    {
        AssetPath path = "fonts\\ui/main.ttf";

        Assert.Equal(new AssetPath("fonts/ui/main.ttf"), path);
    }

    [Fact]
    public void DefaultAssetPath_IsAnEmptySentinel()
    {
        AssetPath path = default;

        Assert.True(path.IsEmpty);
        Assert.Equal(string.Empty, path.Value);
        Assert.Equal(string.Empty, path.ToString());
    }

    [Fact]
    public void Constructor_WithNullValue_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new AssetPath(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("/sprites/player.png")]
    [InlineData("\\sprites\\player.png")]
    [InlineData("C:\\sprites\\player.png")]
    [InlineData("../sprites/player.png")]
    [InlineData("sprites/../player.png")]
    [InlineData("./sprites/player.png")]
    [InlineData("sprites/./player.png")]
    public void Constructor_WithInvalidValue_ThrowsArgumentException(string value)
    {
        Assert.Throws<ArgumentException>(() => new AssetPath(value));
    }
}
