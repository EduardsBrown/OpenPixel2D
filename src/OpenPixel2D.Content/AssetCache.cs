namespace OpenPixel2D.Content;

internal sealed class AssetCache
{
    private readonly Dictionary<AssetCacheKey, object> _assets = new();

    public bool TryGet<T>(AssetPath path, out T asset)
    {
        if (_assets.TryGetValue(new AssetCacheKey(path, typeof(T)), out object? cached))
        {
            asset = (T)cached;
            return true;
        }

        asset = default!;
        return false;
    }

    public void Store<T>(AssetPath path, T asset)
    {
        _assets[new AssetCacheKey(path, typeof(T))] = asset!;
    }

    private readonly record struct AssetCacheKey(AssetPath Path, Type AssetType);
}
