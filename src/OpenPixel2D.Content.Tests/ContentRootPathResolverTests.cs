namespace OpenPixel2D.Content.Tests;

public sealed class ContentRootPathResolverTests
{
    [Fact]
    public void DefaultConstructor_UsesApplicationBaseDirectory()
    {
        ContentRootPathResolver resolver = new();

        Assert.Equal(Path.GetFullPath(AppContext.BaseDirectory), resolver.ContentRoot);
    }

    [Fact]
    public void Resolve_ReturnsAbsolutePathWithinConfiguredContentRoot()
    {
        using TemporaryDirectory temporaryDirectory = new();
        string expectedPath = temporaryDirectory.CreateFile("sprites/player.txt");
        ContentRootPathResolver resolver = new(temporaryDirectory.RootPath);

        string resolvedPath = resolver.Resolve(new AssetPath("sprites\\player.txt"));

        Assert.Equal(Path.GetFullPath(expectedPath), resolvedPath);
    }

    [Fact]
    public void Resolve_WithDefaultAssetPath_ThrowsArgumentException()
    {
        using TemporaryDirectory temporaryDirectory = new();
        ContentRootPathResolver resolver = new(temporaryDirectory.RootPath);

        Assert.Throws<ArgumentException>(() => resolver.Resolve(default));
    }

    [Fact]
    public void Resolve_WithMissingFile_ThrowsFileNotFoundException()
    {
        using TemporaryDirectory temporaryDirectory = new();
        ContentRootPathResolver resolver = new(temporaryDirectory.RootPath);

        FileNotFoundException exception = Assert.Throws<FileNotFoundException>(
            () => resolver.Resolve(new AssetPath("missing/data.json")));

        Assert.Contains("missing/data.json", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Resolve_RejectsPathsThatEscapeTheConfiguredContentRoot()
    {
        using TemporaryDirectory temporaryDirectory = new();
        string contentRoot = Path.Combine(temporaryDirectory.RootPath, "content");
        Directory.CreateDirectory(contentRoot);
        File.WriteAllText(Path.Combine(temporaryDirectory.RootPath, "outside.txt"), "outside");
        ContentRootPathResolver resolver = new(contentRoot);

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => resolver.Resolve(AssetPath.UnsafeCreate("../outside.txt")));

        Assert.Contains("outside the configured content root", exception.Message, StringComparison.Ordinal);
    }
}
