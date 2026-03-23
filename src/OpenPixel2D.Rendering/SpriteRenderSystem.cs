using OpenPixel2D.Components;
using OpenPixel2D.Engine;
using OpenPixel2D.Engine.Extensions;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

public sealed class SpriteRenderSystem : RenderSystem
{
    public override void Render(IRenderContext renderContext)
    {
        if (World == null)
        {
            return;
        }

        IRenderPassWriter? pass = null;

        foreach (var entity in World.GetEntitiesWith<SpriteComponent>())
        {
            if (!entity.TryGetComponent(out TransformComponent? transform) || transform == null)
            {
                continue;
            }

            if (!entity.TryGetComponent(out SpriteComponent? sprite) || !sprite.Active)
            {
                continue;
            }

            pass = renderContext.Frame.GetPass(RenderPassNames.WorldSprites);
            pass.Submit(new SpriteRenderCommand(
                new RenderCommandMetadata(sprite.Layer, sprite.SortKey, RenderSpace.World),
                sprite.TextureId,
                transform.Position,
                transform.Scale,
                transform.Rotation,
                sprite.Origin,
                sprite.SourceRectangle,
                sprite.Colour));
        }
    }
}