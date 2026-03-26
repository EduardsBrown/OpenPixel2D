namespace OpenPixel2D.Content.Tests;

internal sealed class TemporaryDirectory : IDisposable
{
    public TemporaryDirectory()
    {
        RootPath = Path.Combine(
            Path.GetTempPath(),
            "OpenPixel2D.Content.Tests",
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

    public string CreateFile(string relativePath, string contents = "test")
    {
        string fullPath = GetPath(relativePath);

        string? directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(fullPath, contents);
        return fullPath;
    }

    public string CreateFile(string relativePath, ReadOnlySpan<byte> contents)
    {
        string fullPath = GetPath(relativePath);
        string? directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllBytes(fullPath, contents.ToArray());
        return fullPath;
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
