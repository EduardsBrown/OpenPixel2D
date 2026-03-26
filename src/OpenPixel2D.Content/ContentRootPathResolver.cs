namespace OpenPixel2D.Content;

public sealed class ContentRootPathResolver : IContentPathResolver
{
    private readonly string _contentRootPrefix;

    public ContentRootPathResolver()
        : this(AppContext.BaseDirectory)
    {
    }

    public ContentRootPathResolver(string contentRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentRoot);

        ContentRoot = Path.GetFullPath(contentRoot);
        _contentRootPrefix = EnsureTrailingDirectorySeparator(ContentRoot);
    }

    public string ContentRoot { get; }

    public string Resolve(AssetPath path)
    {
        if (path.IsEmpty)
        {
            throw new ArgumentException("Asset path cannot be empty.", nameof(path));
        }

        string resolvedPath = Path.GetFullPath(Path.Combine(ContentRoot, path.ToPlatformPath()));

        if (!IsWithinContentRoot(resolvedPath))
        {
            throw new ArgumentException(
                $"Asset path '{path}' resolves outside the configured content root '{ContentRoot}'.",
                nameof(path));
        }

        return resolvedPath;
    }

    private bool IsWithinContentRoot(string resolvedPath)
    {
        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        return resolvedPath.StartsWith(_contentRootPrefix, comparison);
    }

    private static string EnsureTrailingDirectorySeparator(string path)
    {
        return Path.EndsInDirectorySeparator(path)
            ? path
            : path + Path.DirectorySeparatorChar;
    }
}
