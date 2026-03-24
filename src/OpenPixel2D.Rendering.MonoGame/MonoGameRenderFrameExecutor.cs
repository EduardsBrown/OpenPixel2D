using Microsoft.Xna.Framework.Graphics;
using OpenPixel2D.Rendering.Abstractions;
using OpenPixel2D.Rendering;
using RenderClearOptions = OpenPixel2D.Rendering.Abstractions.ClearOptions;

namespace OpenPixel2D.Rendering.MonoGame;

public sealed class MonoGameRenderFrameExecutor : IRenderFrameExecutor, IDisposable
{
    private readonly IMonoGameGraphicsDeviceAdapter _graphicsDevice;
    private readonly IMonoGameSpriteBatchAdapter _spriteBatch;
    private readonly MonoGameRenderStateMapper _stateMapper;
    private readonly MonoGameResourceCache _resources;
    private readonly bool _ownsSpriteBatch;

    public MonoGameRenderFrameExecutor(GraphicsDevice graphicsDevice)
        : this(graphicsDevice, new MonoGameResourceCache())
    {
    }

    internal MonoGameRenderFrameExecutor(GraphicsDevice graphicsDevice, MonoGameResourceCache resources)
        : this(
            new MonoGameGraphicsDeviceAdapter(graphicsDevice),
            CreateSpriteBatchAdapter(graphicsDevice),
            new MonoGameRenderStateMapper(),
            resources,
            ownsSpriteBatch: true)
    {
    }

    internal MonoGameRenderFrameExecutor(
        IMonoGameGraphicsDeviceAdapter graphicsDevice,
        IMonoGameSpriteBatchAdapter spriteBatch,
        MonoGameRenderStateMapper stateMapper,
        MonoGameResourceCache resources,
        bool ownsSpriteBatch = false)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(spriteBatch);
        ArgumentNullException.ThrowIfNull(stateMapper);
        ArgumentNullException.ThrowIfNull(resources);

        _graphicsDevice = graphicsDevice;
        _spriteBatch = spriteBatch;
        _stateMapper = stateMapper;
        _resources = resources;
        _ownsSpriteBatch = ownsSpriteBatch;
    }

    public void RegisterTexture(TextureId textureId, Texture2D texture)
    {
        _resources.RegisterTexture(textureId, texture);
    }

    public void RegisterFont(FontId fontId, SpriteFont font)
    {
        _resources.RegisterFont(fontId, font);
    }

    public void Execute(IRenderCompletedFrame frame, IRenderView? view)
    {
        ArgumentNullException.ThrowIfNull(frame);

        ApplyClear(view?.Clear);

        foreach (IRenderCompletedPass pass in frame.GetPopulatedPasses())
        {
            ExecutePass(pass);
        }
    }

    public void Dispose()
    {
        if (_ownsSpriteBatch)
        {
            _spriteBatch.Dispose();
        }
    }

    private void ExecutePass(IRenderCompletedPass pass)
    {
        if (pass.Descriptor.Target is not null)
        {
            throw new NotSupportedException(
                $"Render target '{pass.Descriptor.Target.Value}' is not supported by the MonoGame backend in this batch.");
        }

        ApplyClear(pass.Descriptor.Clear);

        if (!HasExecutableSprites(pass))
        {
            return;
        }

        MonoGameSpriteBatchSettings settings = _stateMapper.Map(pass.Descriptor.State);
        _spriteBatch.Begin(settings);

        try
        {
            IReadOnlyList<IRenderCommand> commands = pass.Commands;

            for (int i = 0; i < commands.Count; i++)
            {
                if (commands[i] is not ISpriteRenderCommand spriteCommand)
                {
                    continue;
                }

                if (spriteCommand.Metadata.StateOverride is not null)
                {
                    continue;
                }

                IMonoGameTextureResource texture = _resources.GetRequiredTexture(spriteCommand.TextureId);
                _spriteBatch.Draw(texture, spriteCommand, _stateMapper.MapColor(spriteCommand.Colour));
            }
        }
        finally
        {
            _spriteBatch.End();
        }
    }

    private void ApplyClear(RenderClearOptions? clear)
    {
        if (clear is not RenderClearOptions value)
        {
            return;
        }

        Microsoft.Xna.Framework.Graphics.ClearOptions clearOptions = _stateMapper.MapClearOptions(value);

        if (clearOptions == 0)
        {
            return;
        }

        _graphicsDevice.Clear(
            clearOptions,
            _stateMapper.MapColor(value.Colour ?? System.Drawing.Color.Transparent),
            value.Depth,
            value.Stencil);
    }

    private static bool HasExecutableSprites(IRenderCompletedPass pass)
    {
        IReadOnlyList<IRenderCommand> commands = pass.Commands;

        for (int i = 0; i < commands.Count; i++)
        {
            if (commands[i] is ISpriteRenderCommand spriteCommand && spriteCommand.Metadata.StateOverride is null)
            {
                return true;
            }
        }

        return false;
    }

    private static IMonoGameSpriteBatchAdapter CreateSpriteBatchAdapter(GraphicsDevice graphicsDevice)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        return new MonoGameSpriteBatchAdapter(new SpriteBatch(graphicsDevice));
    }
}
