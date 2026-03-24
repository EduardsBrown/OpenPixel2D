using Microsoft.Xna.Framework.Graphics;
using OpenPixel2D.Rendering;
using OpenPixel2D.Rendering.Abstractions;
using RenderClearOptions = OpenPixel2D.Rendering.Abstractions.ClearOptions;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace OpenPixel2D.Rendering.MonoGame;

/// <summary>
/// Executes completed backend-neutral render frames through MonoGame DesktopGL.
/// Frame construction remains in the core rendering pipeline; this type is execution-only.
/// </summary>
public sealed class MonoGameRenderFrameExecutor : IRenderFrameExecutor, IDisposable
{
    private readonly IMonoGameGraphicsDeviceAdapter _graphicsDevice;
    private readonly IMonoGameSpriteBatchAdapter _spriteBatch;
    private readonly MonoGameRenderStateMapper _stateMapper;
    private readonly IMonoGameResourceLookup _resources;
    private readonly bool _ownsSpriteBatch;

    public MonoGameRenderFrameExecutor(GraphicsDevice graphicsDevice, MonoGameResourceCache resources)
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
        IMonoGameResourceLookup resources,
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

        IReadOnlyList<PreparedDrawCommand> preparedCommands = PreparePass(pass);
        ApplyClear(pass.Descriptor.Clear);

        if (preparedCommands.Count == 0)
        {
            return;
        }

        MonoGameSpriteBatchSettings settings = _stateMapper.Map(pass.Descriptor.State);
        _spriteBatch.Begin(settings);

        try
        {
            for (int i = 0; i < preparedCommands.Count; i++)
            {
                ExecutePreparedCommand(preparedCommands[i]);
            }
        }
        finally
        {
            _spriteBatch.End();
        }
    }

    private IReadOnlyList<PreparedDrawCommand> PreparePass(IRenderCompletedPass pass)
    {
        IReadOnlyList<IRenderCommand> commands = pass.Commands;

        if (commands.Count == 0)
        {
            return Array.Empty<PreparedDrawCommand>();
        }

        List<PreparedDrawCommand> preparedCommands = new(commands.Count);

        for (int i = 0; i < commands.Count; i++)
        {
            preparedCommands.Add(PrepareCommand(pass.Descriptor.Name, commands[i]));
        }

        return preparedCommands;
    }

    private PreparedDrawCommand PrepareCommand(string passName, IRenderCommand command)
    {
        return command switch
        {
            ISpriteRenderCommand spriteCommand => PrepareSpriteCommand(passName, spriteCommand),
            ITextRenderCommand textCommand => PrepareTextCommand(passName, textCommand),
            _ => throw new NotSupportedException(
                $"Render command type '{command.GetType().Name}' is not supported by the MonoGame backend in pass '{passName}'.")
        };
    }

    private PreparedDrawCommand PrepareSpriteCommand(string passName, ISpriteRenderCommand command)
    {
        ValidateStateOverride(passName, command);
        IMonoGameTextureResource texture = _resources.GetRequiredTexture(command.TextureId);
        return PreparedDrawCommand.ForSprite(texture, command, _stateMapper.MapColor(command.Colour));
    }

    private PreparedDrawCommand PrepareTextCommand(string passName, ITextRenderCommand command)
    {
        ValidateStateOverride(passName, command);
        IMonoGameFontResource font = _resources.GetRequiredFont(command.FontId);
        return PreparedDrawCommand.ForText(font, command, _stateMapper.MapColor(command.Colour));
    }

    private void ExecutePreparedCommand(PreparedDrawCommand command)
    {
        switch (command.Kind)
        {
            case PreparedDrawCommandKind.Sprite:
                _spriteBatch.DrawSprite(command.Texture!, command.SpriteCommand!, command.Colour);
                break;
            case PreparedDrawCommandKind.Text:
                _spriteBatch.DrawText(command.Font!, command.TextCommand!, command.Colour);
                break;
            default:
                throw new InvalidOperationException($"Unsupported prepared command kind '{command.Kind}'.");
        }
    }

    private static void ValidateStateOverride(string passName, IRenderCommand command)
    {
        if (command.Metadata.StateOverride is null)
        {
            return;
        }

        throw new NotSupportedException(
            $"Render command type '{command.GetType().Name}' in pass '{passName}' uses Metadata.StateOverride, which is not supported by the MonoGame backend in this batch.");
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

    private static IMonoGameSpriteBatchAdapter CreateSpriteBatchAdapter(GraphicsDevice graphicsDevice)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        return new MonoGameSpriteBatchAdapter(new SpriteBatch(graphicsDevice));
    }

    private enum PreparedDrawCommandKind
    {
        Sprite,
        Text
    }

    private readonly record struct PreparedDrawCommand(
        PreparedDrawCommandKind Kind,
        IMonoGameTextureResource? Texture,
        IMonoGameFontResource? Font,
        ISpriteRenderCommand? SpriteCommand,
        ITextRenderCommand? TextCommand,
        XnaColor Colour)
    {
        public static PreparedDrawCommand ForSprite(
            IMonoGameTextureResource texture,
            ISpriteRenderCommand command,
            XnaColor colour)
        {
            return new PreparedDrawCommand(PreparedDrawCommandKind.Sprite, texture, null, command, null, colour);
        }

        public static PreparedDrawCommand ForText(
            IMonoGameFontResource font,
            ITextRenderCommand command,
            XnaColor colour)
        {
            return new PreparedDrawCommand(PreparedDrawCommandKind.Text, null, font, null, command, colour);
        }
    }
}
