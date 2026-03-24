using System.Drawing;
using Microsoft.Xna.Framework.Graphics;
using OpenPixel2D.Rendering.Abstractions;
using RenderClearOptions = OpenPixel2D.Rendering.Abstractions.ClearOptions;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaCullMode = Microsoft.Xna.Framework.Graphics.CullMode;

namespace OpenPixel2D.Rendering.MonoGame;

internal sealed class MonoGameRenderStateMapper
{
    private readonly Dictionary<RasterizerOptions, RasterizerState> _rasterizerStates = new();

    public MonoGameSpriteBatchSettings Map(RenderState state)
    {
        return new MonoGameSpriteBatchSettings(
            MapSortMode(state.SortMode),
            MapBlendState(state.BlendMode),
            MapSamplerState(state.SamplerMode),
            MapDepthStencilState(state.DepthMode),
            MapRasterizerState(state.Rasterizer));
    }

    public SpriteSortMode MapSortMode(RenderSortMode sortMode)
    {
        return sortMode switch
        {
            RenderSortMode.Deferred => SpriteSortMode.Deferred,
            RenderSortMode.Texture => SpriteSortMode.Texture,
            RenderSortMode.FrontToBack => SpriteSortMode.FrontToBack,
            RenderSortMode.BackToFront => SpriteSortMode.BackToFront,
            RenderSortMode.Immediate => SpriteSortMode.Immediate,
            _ => throw new ArgumentOutOfRangeException(nameof(sortMode), sortMode, "Unsupported render sort mode.")
        };
    }

    public BlendState MapBlendState(BlendMode blendMode)
    {
        return blendMode switch
        {
            BlendMode.Opaque => BlendState.Opaque,
            BlendMode.AlphaBlend => BlendState.AlphaBlend,
            BlendMode.Additive => BlendState.Additive,
            BlendMode.NonPremultiplied => BlendState.NonPremultiplied,
            _ => throw new ArgumentOutOfRangeException(nameof(blendMode), blendMode, "Unsupported blend mode.")
        };
    }

    public SamplerState MapSamplerState(SamplerMode samplerMode)
    {
        return samplerMode switch
        {
            SamplerMode.PointClamp => SamplerState.PointClamp,
            SamplerMode.PointWrap => SamplerState.PointWrap,
            SamplerMode.LinearClamp => SamplerState.LinearClamp,
            SamplerMode.LinearWrap => SamplerState.LinearWrap,
            _ => throw new ArgumentOutOfRangeException(nameof(samplerMode), samplerMode, "Unsupported sampler mode.")
        };
    }

    public DepthStencilState MapDepthStencilState(DepthMode depthMode)
    {
        return depthMode switch
        {
            DepthMode.None => DepthStencilState.None,
            DepthMode.Read => DepthStencilState.DepthRead,
            DepthMode.ReadWrite => DepthStencilState.Default,
            _ => throw new ArgumentOutOfRangeException(nameof(depthMode), depthMode, "Unsupported depth mode.")
        };
    }

    public RasterizerState MapRasterizerState(RasterizerOptions options)
    {
        if (!options.ScissorTestEnabled)
        {
            return options.CullMode switch
            {
                OpenPixel2D.Rendering.Abstractions.CullMode.None => RasterizerState.CullNone,
                OpenPixel2D.Rendering.Abstractions.CullMode.Clockwise => RasterizerState.CullClockwise,
                OpenPixel2D.Rendering.Abstractions.CullMode.CounterClockwise => RasterizerState.CullCounterClockwise,
                _ => throw new ArgumentOutOfRangeException(nameof(options), options.CullMode, "Unsupported cull mode.")
            };
        }

        if (_rasterizerStates.TryGetValue(options, out RasterizerState? state))
        {
            return state;
        }

        state = new RasterizerState
        {
            CullMode = MapCullMode(options.CullMode),
            ScissorTestEnable = true
        };

        _rasterizerStates.Add(options, state);
        return state;
    }

    public Microsoft.Xna.Framework.Graphics.ClearOptions MapClearOptions(RenderClearOptions options)
    {
        Microsoft.Xna.Framework.Graphics.ClearOptions clearOptions = 0;

        if (options.ClearColour)
        {
            clearOptions |= Microsoft.Xna.Framework.Graphics.ClearOptions.Target;
        }

        if (options.ClearDepth)
        {
            clearOptions |= Microsoft.Xna.Framework.Graphics.ClearOptions.DepthBuffer;
        }

        if (options.ClearStencil)
        {
            clearOptions |= Microsoft.Xna.Framework.Graphics.ClearOptions.Stencil;
        }

        return clearOptions;
    }

    public XnaColor MapColor(Color colour)
    {
        return new XnaColor(colour.R, colour.G, colour.B, colour.A);
    }

    private static XnaCullMode MapCullMode(OpenPixel2D.Rendering.Abstractions.CullMode cullMode)
    {
        return cullMode switch
        {
            OpenPixel2D.Rendering.Abstractions.CullMode.None => XnaCullMode.None,
            OpenPixel2D.Rendering.Abstractions.CullMode.Clockwise => XnaCullMode.CullClockwiseFace,
            OpenPixel2D.Rendering.Abstractions.CullMode.CounterClockwise => XnaCullMode.CullCounterClockwiseFace,
            _ => throw new ArgumentOutOfRangeException(nameof(cullMode), cullMode, "Unsupported cull mode.")
        };
    }
}
