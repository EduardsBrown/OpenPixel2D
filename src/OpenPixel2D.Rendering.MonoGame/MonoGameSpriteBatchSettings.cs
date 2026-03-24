using Microsoft.Xna.Framework.Graphics;

namespace OpenPixel2D.Rendering.MonoGame;

internal readonly record struct MonoGameSpriteBatchSettings(
    SpriteSortMode SortMode,
    BlendState BlendState,
    SamplerState SamplerState,
    DepthStencilState DepthStencilState,
    RasterizerState RasterizerState);
