using OpenPixel2D.Content;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

internal sealed class PassthroughRenderAssetResolver : IRenderAssetResolver
{
    public TextureId ResolveTexture(AssetPath asset)
    {
        return asset.IsEmpty
            ? default
            : new TextureId(asset.ToString());
    }

    public FontId ResolveFont(AssetPath asset)
    {
        return asset.IsEmpty
            ? default
            : new FontId(asset.ToString());
    }
}
