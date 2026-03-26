using OpenPixel2D.Components;
using OpenPixel2D.Content;
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

        foreach (var entity in World.GetEntitiesWith<SpriteComponent>())
        {
            if (!entity.TryGetComponent(out TransformComponent? transform) || transform == null)
            {
                continue;
            }

            if (!entity.TryGetComponent(out SpriteComponent? sprite) || sprite == null || !sprite.Active)
            {
                continue;
            }

            if (sprite.Asset.IsEmpty)
            {
                continue;
            }

            if (sprite.Width <= 0f || sprite.Height <= 0f)
            {
                continue;
            }

            renderContext.Submission.Submit(new SpriteRenderItem(
                sprite.Asset,
                transform.Position,
                transform.Scale,
                transform.Rotation,
                sprite.Width,
                sprite.Height,
                sprite.Colour));
        }
    }
}
