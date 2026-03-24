using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

/// <summary>
/// Internal seam between authored asset identifiers and low-level render command identifiers.
/// Real loading is intentionally out of scope for this stage.
/// </summary>
internal interface IRenderAssetResolver
{
    TextureId ResolveTexture(AssetId asset);

    FontId ResolveFont(AssetId asset);
}
