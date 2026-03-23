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

            foreach (SpriteComponent sprite in entity.GetComponents<SpriteComponent>())
            {
                if (!sprite.Visible)
                {
                    continue;
                }

                IRenderPassWriter passWriter = pass ??= renderContext.Frame.GetPass(RenderPassNames.WorldSprites);

                passWriter.Submit(new SpriteRenderCommand(
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
}
