using Microsoft.Xna.Framework;
using OpenPixel2D.Rendering.Abstractions;
using OpenPixel2D.Runtime;

namespace OpenPixel2D.Rendering.MonoGame;

public sealed class MonoGameEngineHostBridge
{
    private readonly IEngineHost _host;

    public MonoGameEngineHostBridge(IEngineHost host)
    {
        ArgumentNullException.ThrowIfNull(host);
        _host = host;
    }

    public void Update(GameTime gameTime)
    {
        _host.Update(MonoGameTimeStepAdapter.ToEngineTimeStep(gameTime));
    }

    public void Draw(GameTime gameTime, IRenderView? view = null)
    {
        _host.Render(MonoGameTimeStepAdapter.ToEngineTimeStep(gameTime), view);
    }
}

internal static class MonoGameTimeStepAdapter
{
    public static EngineTimeStep ToEngineTimeStep(GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(gameTime);
        return new EngineTimeStep(gameTime.ElapsedGameTime, gameTime.TotalGameTime);
    }
}
