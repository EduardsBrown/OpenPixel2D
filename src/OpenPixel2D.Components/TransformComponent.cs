using System.Numerics;
using OpenPixel2D.Engine;

namespace OpenPixel2D.Components;

public class TransformComponent : Component
{
    public Vector2 Position { get; set; }
    public Vector2 Scale { get; set; }
    public float Rotation { get; set; }
}