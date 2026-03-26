namespace OpenPixel2D.Content;

internal static class ContentDefaults
{
    public static AssetLoaderRegistry CreateLoaderRegistry()
    {
        AssetLoaderRegistry registry = new();
        RegisterBuiltInLoaders(registry);
        return registry;
    }

    internal static void RegisterBuiltInLoaders(AssetLoaderRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        // Registration order is explicit and deterministic so future composition can layer on top.
        registry.Register(new RuntimeImageAssetLoader());
        registry.Register(new RuntimeFontAssetLoader());
    }
}
