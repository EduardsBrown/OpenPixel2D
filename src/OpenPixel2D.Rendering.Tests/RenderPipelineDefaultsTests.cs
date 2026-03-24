using System.Drawing;
using System.Numerics;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering.Tests;

public sealed class RenderPipelineDefaultsTests
{
    [Fact]
    public void CreateProcessorRegistry_BuiltInProcessorsRunInDeterministicOrder()
    {
        RenderQueue queue = new();
        queue.Submit(new SpriteRenderItem(
            new AssetId("player"),
            new Vector2(10f, 20f),
            Vector2.One,
            0f,
            16f,
            16f,
            Color.White));
        queue.Submit(new TextRenderItem(
            new AssetId("ui-font"),
            "Score",
            new Vector2(1f, 2f),
            12f,
            Color.Gold));

        RecordingAssetResolver resolver = new();
        RenderProcessorRegistry registry = RenderPipelineDefaults.CreateProcessorRegistry(resolver);
        RenderFrame frame = new(RenderPipelineDefaults.CreatePassRegistry());

        registry.Process(queue, new RenderPipelineContext(frame, queue));

        Assert.Equal(["texture:player", "font:ui-font"], resolver.Calls);
        Assert.Equal(
            [RenderPassNames.WorldSprites, RenderPassNames.UI],
            frame.GetPopulatedPasses().Select(static pass => pass.Descriptor.Name).ToArray());
    }

    private sealed class RecordingAssetResolver : IRenderAssetResolver
    {
        public List<string> Calls { get; } = [];

        public TextureId ResolveTexture(AssetId asset)
        {
            Calls.Add($"texture:{asset.Value}");
            return new TextureId(asset.Value);
        }

        public FontId ResolveFont(AssetId asset)
        {
            Calls.Add($"font:{asset.Value}");
            return new FontId(asset.Value);
        }
    }
}
