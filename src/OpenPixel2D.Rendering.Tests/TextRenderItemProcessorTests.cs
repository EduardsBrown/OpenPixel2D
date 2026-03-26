using System.Drawing;
using System.Numerics;
using OpenPixel2D.Content;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering.Tests;

public sealed class TextRenderItemProcessorTests
{
    [Fact]
    public void Process_TextItems_UsesAssetResolverAndPopulatesUiPassWithScreenSpaceCommands()
    {
        RenderPassRegistry registry = new();
        registry.RegisterDefaultPasses();
        RenderFrame frame = new(registry);
        RenderQueue queue = new();
        RecordingAssetResolver resolver = new(
            textureResult: default,
            fontResult: new FontId("resolved-font"));
        queue.Submit(new TextRenderItem(
            new AssetPath("ui-font"),
            "Hello",
            new Vector2(12f, 18f),
            24f,
            Color.Gold));

        TextRenderItemProcessor processor = new(resolver);

        processor.Process(queue.GetItems<TextRenderItem>(), new RenderPipelineContext(frame, queue));

        RenderPassBuffer pass = Assert.Single(frame.GetPopulatedPasses());
        TextRenderCommand command = Assert.IsType<TextRenderCommand>(Assert.Single(pass.Commands));

        Assert.Equal(RenderPassNames.UI, pass.Descriptor.Name);
        Assert.Empty(resolver.TextureRequests);
        Assert.Equal([new AssetPath("ui-font")], resolver.FontRequests);
        Assert.Equal(new FontId("resolved-font"), command.FontId);
        Assert.Equal("Hello", command.Text);
        Assert.Equal(new Vector2(12f, 18f), command.Position);
        Assert.Equal(24f, command.Size);
        Assert.Equal(Color.Gold, command.Colour);
        Assert.Equal(0, command.Metadata.Layer);
        Assert.Equal(0L, command.Metadata.SortKey);
        Assert.Equal(RenderSpace.Screen, command.Metadata.Space);
    }

    private sealed class RecordingAssetResolver : IRenderAssetResolver
    {
        private readonly TextureId _textureResult;
        private readonly FontId _fontResult;

        public RecordingAssetResolver(TextureId textureResult, FontId fontResult)
        {
            _textureResult = textureResult;
            _fontResult = fontResult;
        }

        public List<AssetPath> TextureRequests { get; } = [];

        public List<AssetPath> FontRequests { get; } = [];

        public TextureId ResolveTexture(AssetPath asset)
        {
            TextureRequests.Add(asset);
            return _textureResult;
        }

        public FontId ResolveFont(AssetPath asset)
        {
            FontRequests.Add(asset);
            return _fontResult;
        }
    }
}
