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

            if (string.IsNullOrWhiteSpace(sprite.Asset.Value))
            {
                continue;
            }

            float width = ResolveDimension(sprite.Width, sprite.SourceRectangle?.Width);
            float height = ResolveDimension(sprite.Height, sprite.SourceRectangle?.Height);

            if (width <= 0f || height <= 0f)
            {
                continue;
            }

            renderContext.Submission.Submit(new SpriteRenderItem(
                sprite.Asset,
                transform.Position,
                transform.Scale,
                transform.Rotation,
                width,
                height,
                sprite.Colour,
                sprite.Layer,
                sprite.SortKey,
                sprite.Origin,
                sprite.SourceRectangle));
        }
    }

    private static float ResolveDimension(float value, float? fallback)
    {
        if (value != 0f)
        {
            return value;
        }

        return fallback ?? 0f;
    }
}
