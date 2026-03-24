using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering.Tests;

public sealed class RenderQueueTests
{
    [Fact]
    public void Submit_SingleItemType_ReturnsTypedItemsInInsertionOrder()
    {
        RenderQueue queue = new();

        queue.Submit(new TestSpriteItem(1));
        queue.Submit(new TestSpriteItem(2));

        Assert.Equal(
            [new TestSpriteItem(1), new TestSpriteItem(2)],
            queue.GetItems<TestSpriteItem>().ToArray());
    }

    [Fact]
    public void Submit_MultipleItemTypes_KeepsItemsIsolatedByType()
    {
        RenderQueue queue = new();

        queue.Submit(new TestSpriteItem(1));
        queue.Submit(new TestTextItem(7));
        queue.Submit(new TestSpriteItem(2));

        Assert.Equal(
            [new TestSpriteItem(1), new TestSpriteItem(2)],
            queue.GetItems<TestSpriteItem>().ToArray());
        Assert.Equal(
            [new TestTextItem(7)],
            queue.GetItems<TestTextItem>().ToArray());
    }

    [Fact]
    public void GetItems_MissingType_ReturnsEmptySpan()
    {
        RenderQueue queue = new();

        Assert.True(queue.GetItems<TestSpriteItem>().IsEmpty);
    }

    [Fact]
    public void Clear_RemovesSubmittedItemsAcrossAllTypes()
    {
        RenderQueue queue = new();
        queue.Submit(new TestSpriteItem(1));
        queue.Submit(new TestTextItem(2));

        queue.Clear();

        Assert.True(queue.GetItems<TestSpriteItem>().IsEmpty);
        Assert.True(queue.GetItems<TestTextItem>().IsEmpty);
    }

    private readonly record struct TestSpriteItem(int Id) : IRenderItem;
    private readonly record struct TestTextItem(int Id) : IRenderItem;
}
