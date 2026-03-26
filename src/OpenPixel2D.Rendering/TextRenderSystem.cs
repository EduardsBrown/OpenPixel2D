using OpenPixel2D.Components;
using OpenPixel2D.Content;
using OpenPixel2D.Engine;
using OpenPixel2D.Engine.Extensions;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

public sealed class TextRenderSystem : RenderSystem
{
    public override void Render(IRenderContext renderContext)
    {
        if (World == null)
        {
            return;
        }

        foreach (Entity entity in World.GetEntitiesWith<TextComponent>())
        {
            if (!entity.TryGetComponent(out TransformComponent? transform) || transform == null)
            {
                continue;
            }

            if (!entity.TryGetComponent(out TextComponent? text) || text == null || !text.Active)
            {
                continue;
            }

            if (text.Asset.IsEmpty || string.IsNullOrEmpty(text.Text) || text.Size <= 0f)
            {
                continue;
            }

            renderContext.Submission.Submit(new TextRenderItem(
                text.Asset,
                text.Text,
                transform.Position,
                text.Size,
                text.Colour));
        }
    }
}
