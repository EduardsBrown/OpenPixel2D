# OpenPixel2D MonoGame Backend

`OpenPixel2D.Runtime` owns the runtime flow, including world lifecycle orchestration and public time updates. `OpenPixel2D.Rendering` builds completed `RenderFrame` instances, and `OpenPixel2D.Rendering.MonoGame` executes those completed frames through MonoGame DesktopGL using `GraphicsDevice` and `SpriteBatch`.

## Responsibilities

- `MonoGameEngineHostBridge` adapts MonoGame `GameTime` and loop callbacks into the backend-neutral runtime host.
- `MonoGameRenderFrameExecutor` executes completed passes in deterministic order.
- `MonoGameRenderStateMapper` keeps all MonoGame state mapping local to the backend.
- `MonoGameResourceCache` resolves backend ids to MonoGame resources, including lazy creation from runtime-loaded content assets.
- `OpenPixel2D.Rendering.MonoGame.Smoke` is the minimal real-backend harness used to validate the host-driven runtime path.

## Runtime Flow

- MonoGame `Game.Update` / `Game.Draw` stay in the MonoGame layer.
- `MonoGameEngineHostBridge` converts `GameTime` into `EngineTimeStep`.
- `EngineHost` publishes the public `OpenPixel2D.Engine.Time` facade before gameplay update logic runs.
- `EngineHost` orchestrates `World`, `RenderPipelineCoordinator`, and `IRenderFrameExecutor`.
- The MonoGame backend remains execution-focused; it does not own engine lifecycle or public time state.

## Current Support

- Sprite commands execute through `SpriteBatch.Draw`.
- Text commands execute through `SpriteBatch.DrawString`, including a runtime TTF-backed path via `FontStashSharp.MonoGame`.
- Mixed sprite/text content inside a pass executes in original command order within a single `SpriteBatch.Begin` / `End`.
- View clears and pass clears execute through `GraphicsDevice.Clear`.
- The smoke harness runs through the real runtime host path and exercises runtime-driven public time updates.
- Content-backed render resolution validates `AssetPath` values through `ContentManager` before the backend creates resources.

## Current Limitations

- `RuntimeFontAsset` is still a parsed source asset plus direct design-unit metadata, not a backend-neutral render-ready font pipeline.
- The MonoGame text bridge is intentionally first-version only: runtime `.ttf` loading works, but public font fallback management, shaping, HarfBuzz integration, and non-`.ttf` formats remain future work.
- Render targets are not supported yet; passes targeting offscreen render targets throw.
- Command-level `Metadata.StateOverride` is not supported yet; the backend throws instead of partially applying overrides.
- Unsupported command types throw instead of being ignored.
- Camera/transform matrices are not implemented yet, so `RenderSpace.World` and `RenderSpace.Screen` currently render identically.
- Public builder/configuration APIs and processor registration are still future work.

## Smoke Harness

Run the backend-focused smoke app with:

```powershell
dotnet run --project src/OpenPixel2D.Rendering.MonoGame.Smoke/OpenPixel2D.Rendering.MonoGame.Smoke.csproj
```

The harness creates a real `GraphicsDevice`, points `EngineHost` and `MonoGameResourceCache` at the output-directory content root, loads a runtime image plus `.ttf` font by `AssetPath`, and executes the resulting frames through `MonoGameRenderFrameExecutor`.
