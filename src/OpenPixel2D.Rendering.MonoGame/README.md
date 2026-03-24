# OpenPixel2D MonoGame Backend

`OpenPixel2D.Rendering` builds completed `RenderFrame` instances. `OpenPixel2D.Rendering.MonoGame` consumes those completed frames and executes them through MonoGame DesktopGL using `GraphicsDevice` and `SpriteBatch`.

## Responsibilities

- `MonoGameRenderFrameExecutor` executes completed passes in deterministic order.
- `MonoGameRenderStateMapper` keeps all MonoGame state mapping local to the backend.
- `MonoGameResourceCache` owns temporary manual registration for `Texture2D` and `SpriteFont`.
- `OpenPixel2D.Rendering.MonoGame.Smoke` is the minimal real-backend harness used to validate sprite and text execution.

## Current Support

- Sprite commands execute through `SpriteBatch.Draw`.
- Text commands execute through `SpriteBatch.DrawString`.
- Mixed sprite/text content inside a pass executes in original command order within a single `SpriteBatch.Begin` / `End`.
- View clears and pass clears execute through `GraphicsDevice.Clear`.

## Current Limitations

- Manual texture/font registration is temporary until real asset loading is added.
- Render targets are not supported yet; passes targeting offscreen render targets throw.
- Command-level `Metadata.StateOverride` is not supported yet; the backend throws instead of partially applying overrides.
- Unsupported command types throw instead of being ignored.
- Camera/transform matrices are not implemented yet, so `RenderSpace.World` and `RenderSpace.Screen` currently render identically.

## Smoke Harness

Run the backend-focused smoke app with:

```powershell
dotnet run --project src/OpenPixel2D.Rendering.MonoGame.Smoke/OpenPixel2D.Rendering.MonoGame.Smoke.csproj
```

The harness creates a real `GraphicsDevice`, generates a texture and a simple `SpriteFont`, registers both through `MonoGameResourceCache`, builds a tiny completed frame locally, and executes that frame through `MonoGameRenderFrameExecutor`.
