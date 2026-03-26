using System.Diagnostics.CodeAnalysis;

namespace OpenPixel2D.Content;

public sealed class ContentManager : IContentManager
{
    private readonly AssetCache _cache = new();
    private readonly IContentPathResolver _pathResolver;
    private readonly AssetLoaderRegistry _loaders;

    public ContentManager()
        : this(new ContentRootPathResolver())
    {
    }

    public ContentManager(string contentRoot)
        : this(new ContentRootPathResolver(contentRoot))
    {
    }

    public ContentManager(IContentPathResolver pathResolver)
        : this(pathResolver, ContentDefaults.CreateLoaderRegistry())
    {
    }

    internal ContentManager(IContentPathResolver pathResolver, AssetLoaderRegistry loaders)
    {
        ArgumentNullException.ThrowIfNull(pathResolver);
        ArgumentNullException.ThrowIfNull(loaders);

        _pathResolver = pathResolver;
        _loaders = loaders;
    }

    public T Load<T>(AssetPath path)
    {
        EnsurePath(path);

        if (_cache.TryGet(path, out T cached))
        {
            return cached;
        }

        string absolutePath = _pathResolver.Resolve(path);
        EnsureFileExists(path, absolutePath);
        AssetLoadContext context = new(path, absolutePath);

        if (!_loaders.TryLoad(context, out T asset))
        {
            throw new InvalidOperationException(
                $"No asset loader is registered for asset '{path}' and type '{typeof(T).FullName}'.");
        }

        EnsureLoadedAsset(asset, path);
        _cache.Store(path, asset);
        return asset;
    }

    public bool TryLoad<T>(AssetPath path, [MaybeNullWhen(false)] out T asset)
    {
        EnsurePath(path);

        if (_cache.TryGet(path, out asset))
        {
            return true;
        }

        string absolutePath;

        try
        {
            absolutePath = _pathResolver.Resolve(path);
            EnsureFileExists(path, absolutePath);
        }
        catch (FileNotFoundException)
        {
            asset = default!;
            return false;
        }

        AssetLoadContext context = new(path, absolutePath);

        if (!_loaders.TryLoad(context, out asset))
        {
            asset = default!;
            return false;
        }

        EnsureLoadedAsset(asset, path);
        _cache.Store(path, asset);
        return true;
    }

    private static void EnsurePath(AssetPath path)
    {
        if (path.IsEmpty)
        {
            throw new ArgumentException("Asset path cannot be empty.", nameof(path));
        }
    }

    private static void EnsureLoadedAsset<T>([NotNull] T asset, AssetPath path)
    {
        if (asset is null)
        {
            throw new InvalidOperationException($"Loader returned null for asset '{path}'.");
        }
    }

    private static void EnsureFileExists(AssetPath path, string absolutePath)
    {
        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException(
                $"Asset '{path}' was not found at '{absolutePath}'.",
                absolutePath);
        }
    }
}
