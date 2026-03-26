namespace OpenPixel2D.Runtime.Tests;

internal sealed class TemporaryDirectory : IDisposable
{
    public TemporaryDirectory()
    {
        RootPath = Path.Combine(
            Path.GetTempPath(),
            "OpenPixel2D.Runtime.Tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(RootPath);
    }

    public string RootPath { get; }

    public string GetPath(string relativePath)
    {
        return Path.Combine(
            RootPath,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    public string CopyFile(string sourcePath, string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);

        string destinationPath = GetPath(relativePath);
        string? directory = Path.GetDirectoryName(destinationPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.Copy(sourcePath, destinationPath, overwrite: true);
        return destinationPath;
    }

    public void Dispose()
    {
        if (Directory.Exists(RootPath))
        {
            Directory.Delete(RootPath, recursive: true);
        }
    }
}
