using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

internal sealed class SpriteRenderItemProcessor : IRenderItemProcessor<SpriteRenderItem>
{
    private readonly IRenderAssetResolver _assetResolver;

    public SpriteRenderItemProcessor(IRenderAssetResolver assetResolver)
    {
        ArgumentNullException.ThrowIfNull(assetResolver);
        _assetResolver = assetResolver;
    }

    public void Process(ReadOnlySpan<SpriteRenderItem> items, IRenderPipelineContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (items.IsEmpty)
        {
            return;
        }

        BuiltInRenderPassRoute route = BuiltInRenderPassRouting.Sprite;
        IRenderPassWriter pass = context.Frame.GetPass(route.PassName);
        IndexedSpriteRenderItem[] sortedItems = CreateSortedItems(items);

        for (int i = 0; i < sortedItems.Length; i++)
        {
            SpriteRenderItem item = sortedItems[i].Item;

            pass.Submit(new SpriteRenderCommand(
                new RenderCommandMetadata(Layer: 0, SortKey: i, Space: route.Space),
                _assetResolver.ResolveTexture(item.Asset),
                item.Position,
                item.Scale,
                item.Rotation,
                item.Width,
                item.Height,
                item.Colour));
        }
    }

    private static IndexedSpriteRenderItem[] CreateSortedItems(ReadOnlySpan<SpriteRenderItem> items)
    {
        IndexedSpriteRenderItem[] sortedItems = new IndexedSpriteRenderItem[items.Length];

        for (int i = 0; i < items.Length; i++)
        {
            sortedItems[i] = new IndexedSpriteRenderItem(items[i], i);
        }

        Array.Sort(sortedItems, static (left, right) =>
        {
            int result = left.Item.Position.Y.CompareTo(right.Item.Position.Y);

            if (result != 0)
            {
                return result;
            }

            return left.SubmissionIndex.CompareTo(right.SubmissionIndex);
        });

        return sortedItems;
    }

    private readonly record struct IndexedSpriteRenderItem(SpriteRenderItem Item, int SubmissionIndex);
}
