# OpenPixel2D Runtime

`OpenPixel2D.Runtime` is the minimal orchestration layer between a platform game loop and the engine core.

## Responsibilities

- `EngineHost` owns world lifecycle entry points and runtime orchestration.
- `EngineHost` builds frames through `RenderPipelineCoordinator` and hands them to `IRenderFrameExecutor`.
- `EngineTimeStep` is the authoritative internal timing snapshot supplied by the platform/runtime bridge.
- `OpenPixel2D.Engine.Time` is the public read-only gameplay timing facade updated by the host.

## Current Time Model

- Platform-specific adapters convert native timing into `EngineTimeStep`.
- `EngineHost` publishes zeroed time before `World.Start()`, so `OnStart` sees valid timing values.
- `EngineHost` updates public `Time` on update frames only.
- Rendering uses the latest published gameplay time and does not advance public timing independently.

## Current Stage

- Public builder/configuration APIs are still future work.
- Camera support is still future work.
- Asset/content loading is still future work.
- Public processor registration is still future work.
