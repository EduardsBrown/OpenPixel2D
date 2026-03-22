namespace OpenPixel2D.Engine;

[Flags]
internal enum LifecycleFlags
{
    None = 0,
    Initialized = 1 << 0,
    Started = 1 << 1,
    Destroyed = 1 << 2,
    Disposed = 1 << 3
}
