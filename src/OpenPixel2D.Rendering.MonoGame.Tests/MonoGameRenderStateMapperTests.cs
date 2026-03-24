using Microsoft.Xna.Framework.Graphics;
using OpenPixel2D.Rendering.Abstractions;
using RenderCullMode = OpenPixel2D.Rendering.Abstractions.CullMode;

namespace OpenPixel2D.Rendering.MonoGame.Tests;

public sealed class MonoGameRenderStateMapperTests
{
    private readonly MonoGameRenderStateMapper _mapper = new();

    [Theory]
    [InlineData(RenderSortMode.Deferred, SpriteSortMode.Deferred)]
    [InlineData(RenderSortMode.Texture, SpriteSortMode.Texture)]
    [InlineData(RenderSortMode.FrontToBack, SpriteSortMode.FrontToBack)]
    [InlineData(RenderSortMode.BackToFront, SpriteSortMode.BackToFront)]
    [InlineData(RenderSortMode.Immediate, SpriteSortMode.Immediate)]
    public void MapSortMode_ReturnsExpectedValue(RenderSortMode input, SpriteSortMode expected)
    {
        Assert.Equal(expected, _mapper.MapSortMode(input));
    }

    [Theory]
    [InlineData(BlendMode.Opaque)]
    [InlineData(BlendMode.AlphaBlend)]
    [InlineData(BlendMode.Additive)]
    [InlineData(BlendMode.NonPremultiplied)]
    public void MapBlendState_ReturnsBuiltInBlendState(BlendMode blendMode)
    {
        BlendState state = _mapper.MapBlendState(blendMode);

        Assert.Same(
            blendMode switch
            {
                BlendMode.Opaque => BlendState.Opaque,
                BlendMode.AlphaBlend => BlendState.AlphaBlend,
                BlendMode.Additive => BlendState.Additive,
                BlendMode.NonPremultiplied => BlendState.NonPremultiplied,
                _ => throw new ArgumentOutOfRangeException(nameof(blendMode), blendMode, null)
            },
            state);
    }

    [Theory]
    [InlineData(SamplerMode.PointClamp)]
    [InlineData(SamplerMode.PointWrap)]
    [InlineData(SamplerMode.LinearClamp)]
    [InlineData(SamplerMode.LinearWrap)]
    public void MapSamplerState_ReturnsBuiltInSamplerState(SamplerMode samplerMode)
    {
        SamplerState state = _mapper.MapSamplerState(samplerMode);

        Assert.Same(
            samplerMode switch
            {
                SamplerMode.PointClamp => SamplerState.PointClamp,
                SamplerMode.PointWrap => SamplerState.PointWrap,
                SamplerMode.LinearClamp => SamplerState.LinearClamp,
                SamplerMode.LinearWrap => SamplerState.LinearWrap,
                _ => throw new ArgumentOutOfRangeException(nameof(samplerMode), samplerMode, null)
            },
            state);
    }

    [Theory]
    [InlineData(DepthMode.None)]
    [InlineData(DepthMode.Read)]
    [InlineData(DepthMode.ReadWrite)]
    public void MapDepthStencilState_ReturnsExpectedBuiltInState(DepthMode depthMode)
    {
        DepthStencilState state = _mapper.MapDepthStencilState(depthMode);

        Assert.Same(
            depthMode switch
            {
                DepthMode.None => DepthStencilState.None,
                DepthMode.Read => DepthStencilState.DepthRead,
                DepthMode.ReadWrite => DepthStencilState.Default,
                _ => throw new ArgumentOutOfRangeException(nameof(depthMode), depthMode, null)
            },
            state);
    }

    [Fact]
    public void MapRasterizerState_WithoutScissor_UsesBuiltInState()
    {
        RasterizerState state = _mapper.MapRasterizerState(new RasterizerOptions(RenderCullMode.Clockwise, false));

        Assert.Same(RasterizerState.CullClockwise, state);
    }

    [Fact]
    public void MapRasterizerState_WithScissor_CachesCustomState()
    {
        RasterizerOptions options = new(RenderCullMode.CounterClockwise, true);

        RasterizerState first = _mapper.MapRasterizerState(options);
        RasterizerState second = _mapper.MapRasterizerState(options);

        Assert.Same(first, second);
        Assert.Equal(Microsoft.Xna.Framework.Graphics.CullMode.CullCounterClockwiseFace, first.CullMode);
        Assert.True(first.ScissorTestEnable);
    }

    [Fact]
    public void Map_ComposesAllSpriteBatchStateValues()
    {
        RenderState renderState = new(
            SortMode: RenderSortMode.Texture,
            BlendMode: BlendMode.Additive,
            SamplerMode: SamplerMode.PointWrap,
            DepthMode: DepthMode.Read,
            Rasterizer: new RasterizerOptions(RenderCullMode.Clockwise, true));

        MonoGameSpriteBatchSettings settings = _mapper.Map(renderState);

        Assert.Equal(SpriteSortMode.Texture, settings.SortMode);
        Assert.Same(BlendState.Additive, settings.BlendState);
        Assert.Same(SamplerState.PointWrap, settings.SamplerState);
        Assert.Same(DepthStencilState.DepthRead, settings.DepthStencilState);
        Assert.Equal(Microsoft.Xna.Framework.Graphics.CullMode.CullClockwiseFace, settings.RasterizerState.CullMode);
        Assert.True(settings.RasterizerState.ScissorTestEnable);
    }
}
