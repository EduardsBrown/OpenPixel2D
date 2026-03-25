namespace OpenPixel2D.Content.Tests;

public sealed class ContentManagerTests
{
    [Fact]
    public void Load_CachesAssetsByNormalizedPathAndType()
    {
        using TemporaryDirectory temporaryDirectory = new();
        temporaryDirectory.CreateFile("data/player.asset");
        AssetLoaderRegistry registry = new();
        RecordingLoader<PrimaryAsset> loader = new(
            canLoad: static path => path == new AssetPath("data/player.asset"),
            load: static context => new PrimaryAsset(context.AbsolutePath));
        registry.Register(loader);
        ContentManager manager = new(new ContentRootPathResolver(temporaryDirectory.RootPath), registry);

        PrimaryAsset first = manager.Load<PrimaryAsset>(new AssetPath("data\\player.asset"));
        PrimaryAsset second = manager.Load<PrimaryAsset>(new AssetPath("data/player.asset"));

        Assert.Same(first, second);
        Assert.Equal(1, loader.CanLoadCalls);
        Assert.Equal(1, loader.LoadCalls);
    }

    [Fact]
    public void Load_CacheKeyIncludesAssetType()
    {
        using TemporaryDirectory temporaryDirectory = new();
        temporaryDirectory.CreateFile("shared/resource.txt");
        AssetLoaderRegistry registry = new();
        RecordingLoader<PrimaryAsset> primaryLoader = new(
            canLoad: static _ => true,
            load: static context => new PrimaryAsset($"primary:{context.AbsolutePath}"));
        RecordingLoader<SecondaryAsset> secondaryLoader = new(
            canLoad: static _ => true,
            load: static context => new SecondaryAsset($"secondary:{context.AbsolutePath}"));
        registry.Register(primaryLoader);
        registry.Register(secondaryLoader);
        ContentManager manager = new(new ContentRootPathResolver(temporaryDirectory.RootPath), registry);

        PrimaryAsset firstPrimary = manager.Load<PrimaryAsset>(new AssetPath("shared/resource.txt"));
        SecondaryAsset firstSecondary = manager.Load<SecondaryAsset>(new AssetPath("shared/resource.txt"));
        PrimaryAsset secondPrimary = manager.Load<PrimaryAsset>(new AssetPath("shared/resource.txt"));
        SecondaryAsset secondSecondary = manager.Load<SecondaryAsset>(new AssetPath("shared/resource.txt"));

        Assert.Same(firstPrimary, secondPrimary);
        Assert.Same(firstSecondary, secondSecondary);
        Assert.Equal(1, primaryLoader.LoadCalls);
        Assert.Equal(1, secondaryLoader.LoadCalls);
    }

    [Fact]
    public void Load_PassesOriginalAssetPathAndAbsolutePathToSelectedLoader()
    {
        using TemporaryDirectory temporaryDirectory = new();
        string absolutePath = temporaryDirectory.CreateFile("config/settings.json");
        AssetLoaderRegistry registry = new();
        RecordingLoader<PrimaryAsset> loader = new(
            canLoad: static path => path == new AssetPath("config/settings.json"),
            load: static context => new PrimaryAsset(context.AssetPath.ToString()));
        registry.Register(loader);
        ContentManager manager = new(new ContentRootPathResolver(temporaryDirectory.RootPath), registry);

        PrimaryAsset asset = manager.Load<PrimaryAsset>(new AssetPath("config\\settings.json"));

        Assert.Equal("config/settings.json", asset.Id);
        Assert.Equal(new AssetPath("config/settings.json"), loader.LastContext!.Value.AssetPath);
        Assert.Equal(Path.GetFullPath(absolutePath), loader.LastContext.Value.AbsolutePath);
    }

    [Fact]
    public void Load_UsesFirstMatchingLoaderInRegistrationOrder()
    {
        using TemporaryDirectory temporaryDirectory = new();
        temporaryDirectory.CreateFile("data/value.txt");
        AssetLoaderRegistry registry = new();
        RecordingLoader<PrimaryAsset> firstLoader = new(
            canLoad: static _ => true,
            load: static _ => new PrimaryAsset("first"));
        RecordingLoader<PrimaryAsset> secondLoader = new(
            canLoad: static _ => true,
            load: static _ => new PrimaryAsset("second"));
        registry.Register(firstLoader);
        registry.Register(secondLoader);
        ContentManager manager = new(new ContentRootPathResolver(temporaryDirectory.RootPath), registry);

        PrimaryAsset asset = manager.Load<PrimaryAsset>(new AssetPath("data/value.txt"));

        Assert.Equal("first", asset.Id);
        Assert.Equal(1, firstLoader.LoadCalls);
        Assert.Equal(0, secondLoader.LoadCalls);
    }

    [Fact]
    public void TryLoad_WithMissingFile_ReturnsFalse()
    {
        using TemporaryDirectory temporaryDirectory = new();
        AssetLoaderRegistry registry = new();
        RecordingLoader<PrimaryAsset> loader = new(
            canLoad: static _ => true,
            load: static _ => new PrimaryAsset("unexpected"));
        registry.Register(loader);
        ContentManager manager = new(new ContentRootPathResolver(temporaryDirectory.RootPath), registry);

        bool loaded = manager.TryLoad(new AssetPath("missing/file.txt"), out PrimaryAsset? asset);

        Assert.False(loaded);
        Assert.Null(asset);
        Assert.Equal(0, loader.LoadCalls);
    }

    [Fact]
    public void TryLoad_WithUnsupportedType_ReturnsFalse()
    {
        using TemporaryDirectory temporaryDirectory = new();
        temporaryDirectory.CreateFile("data/file.bin");
        ContentManager manager = new(
            new ContentRootPathResolver(temporaryDirectory.RootPath),
            new AssetLoaderRegistry());

        bool loaded = manager.TryLoad(new AssetPath("data/file.bin"), out PrimaryAsset? asset);

        Assert.False(loaded);
        Assert.Null(asset);
    }

    [Fact]
    public void Load_WithUnsupportedType_ThrowsInvalidOperationException()
    {
        using TemporaryDirectory temporaryDirectory = new();
        temporaryDirectory.CreateFile("data/file.bin");
        ContentManager manager = new(
            new ContentRootPathResolver(temporaryDirectory.RootPath),
            new AssetLoaderRegistry());

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => manager.Load<PrimaryAsset>(new AssetPath("data/file.bin")));

        Assert.Contains("No asset loader is registered", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Load_AndTryLoad_WithDefaultAssetPath_ThrowArgumentException()
    {
        using TemporaryDirectory temporaryDirectory = new();
        ContentManager manager = new(
            new ContentRootPathResolver(temporaryDirectory.RootPath),
            new AssetLoaderRegistry());

        Assert.Throws<ArgumentException>(() => manager.Load<PrimaryAsset>(default));
        Assert.Throws<ArgumentException>(() => manager.TryLoad(default, out PrimaryAsset? _));
    }

    private sealed class RecordingLoader<T> : IAssetLoader<T>
    {
        private readonly Func<AssetPath, bool> _canLoad;
        private readonly Func<AssetLoadContext, T> _load;

        public RecordingLoader(Func<AssetPath, bool> canLoad, Func<AssetLoadContext, T> load)
        {
            _canLoad = canLoad;
            _load = load;
        }

        public int CanLoadCalls { get; private set; }

        public int LoadCalls { get; private set; }

        public AssetLoadContext? LastContext { get; private set; }

        public bool CanLoad(AssetPath path)
        {
            CanLoadCalls++;
            return _canLoad(path);
        }

        public T Load(AssetLoadContext context)
        {
            LoadCalls++;
            LastContext = context;
            return _load(context);
        }
    }

    private sealed class PrimaryAsset
    {
        public PrimaryAsset(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }

    private sealed class SecondaryAsset
    {
        public SecondaryAsset(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}
