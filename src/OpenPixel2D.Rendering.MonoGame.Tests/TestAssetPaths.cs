namespace OpenPixel2D.Rendering.MonoGame.Tests;

internal static class TestAssetPaths
{
    public static string GetPath(string relativePath)
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}
