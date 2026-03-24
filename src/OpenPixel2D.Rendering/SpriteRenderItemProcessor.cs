using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

internal sealed class SpriteRenderItemProcessor : IRenderItemProcessor<SpriteRenderItem>
{
    public void Process(ReadOnlySpan<SpriteRenderItem> items, IRenderPipelineContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (items.IsEmpty)
        {
            return;
        }

        IRenderPassWriter pass = context.Frame.GetPass(RenderPassNames.WorldSprites);

        for (int i = 0; i < items.Length; i++)
        {
            SpriteRenderItem item = items[i];

            pass.Submit(new SpriteRenderCommand(
                new RenderCommandMetadata(item.Layer, item.SortKey, RenderSpace.World),
                ToTextureId(item.Asset),
                item.Position,
                item.Scale,
                item.Rotation,
                item.Origin,
                item.SourceRectangle,
                item.Colour));
        }
    }

    private static TextureId ToTextureId(AssetId asset)
    {
        return asset.Value is null ? default : new TextureId(asset.Value);
    }
}
