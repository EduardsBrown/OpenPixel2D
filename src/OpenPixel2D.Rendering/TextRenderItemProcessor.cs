using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

internal sealed class TextRenderItemProcessor : IRenderItemProcessor<TextRenderItem>
{
    private readonly IRenderAssetResolver _assetResolver;

    public TextRenderItemProcessor(IRenderAssetResolver assetResolver)
    {
        ArgumentNullException.ThrowIfNull(assetResolver);
        _assetResolver = assetResolver;
    }

    public void Process(ReadOnlySpan<TextRenderItem> items, IRenderPipelineContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (items.IsEmpty)
        {
            return;
        }

        BuiltInRenderPassRoute route = BuiltInRenderPassRouting.Text;
        IRenderPassWriter pass = context.Frame.GetPass(route.PassName);

        for (int i = 0; i < items.Length; i++)
        {
            TextRenderItem item = items[i];

            pass.Submit(new TextRenderCommand(
                new RenderCommandMetadata(Layer: 0, SortKey: i, Space: route.Space),
                _assetResolver.ResolveFont(item.Asset),
                item.Text,
                item.Position,
                item.Size,
                item.Colour));
        }
    }
}
