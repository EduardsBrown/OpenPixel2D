using OpenPixel2D.Content;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

/// <summary>
/// Validates authored asset paths through the runtime content system and returns deterministic
/// backend ids based on the normalized asset path for later backend-side resource creation.
/// </summary>
internal sealed class ContentBackedRenderAssetResolver : IRenderAssetResolver
{
    private readonly IContentManager _content;

    public ContentBackedRenderAssetResolver(IContentManager content)
    {
        ArgumentNullException.ThrowIfNull(content);
        _content = content;
    }

    public TextureId ResolveTexture(AssetPath asset)
    {
        _content.Load<RuntimeImageAsset>(asset);
        return new TextureId(asset.ToString());
    }

    public FontId ResolveFont(AssetPath asset)
    {
        _content.Load<RuntimeFontAsset>(asset);
        return new FontId(asset.ToString());
    }
}
