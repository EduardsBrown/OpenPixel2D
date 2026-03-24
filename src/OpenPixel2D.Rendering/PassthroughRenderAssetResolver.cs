using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

internal sealed class PassthroughRenderAssetResolver : IRenderAssetResolver
{
    public TextureId ResolveTexture(AssetId asset)
    {
        return string.IsNullOrWhiteSpace(asset.Value)
            ? default
            : new TextureId(asset.Value);
    }

    public FontId ResolveFont(AssetId asset)
    {
        return string.IsNullOrWhiteSpace(asset.Value)
            ? default
            : new FontId(asset.Value);
    }
}
