namespace OpenPixel2D.Content;

internal sealed class AssetLoaderRegistry
{
    private readonly List<IAssetLoaderRegistration> _loaders = [];

    public void Register<T>(IAssetLoader<T> loader)
    {
        ArgumentNullException.ThrowIfNull(loader);
        _loaders.Add(new AssetLoaderRegistration<T>(loader));
    }

    public bool TryLoad<T>(AssetLoadContext context, out T asset)
    {
        for (int i = 0; i < _loaders.Count; i++)
        {
            if (_loaders[i] is not AssetLoaderRegistration<T> typedRegistration)
            {
                continue;
            }

            if (!typedRegistration.CanLoad(context.AssetPath))
            {
                continue;
            }

            asset = typedRegistration.Load(context);
            return true;
        }

        asset = default!;
        return false;
    }

    private interface IAssetLoaderRegistration
    {
        Type AssetType { get; }
    }

    private sealed class AssetLoaderRegistration<T> : IAssetLoaderRegistration
    {
        private readonly IAssetLoader<T> _loader;

        public AssetLoaderRegistration(IAssetLoader<T> loader)
        {
            _loader = loader;
        }

        public Type AssetType => typeof(T);

        public bool CanLoad(AssetPath path)
        {
            return _loader.CanLoad(path);
        }

        public T Load(AssetLoadContext context)
        {
            return _loader.Load(context);
        }
    }
}
