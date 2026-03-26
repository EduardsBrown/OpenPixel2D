namespace OpenPixel2D.Content;

public readonly record struct AssetLoadContext
{
    public AssetLoadContext(AssetPath assetPath, string absolutePath)
    {
        if (assetPath.IsEmpty)
        {
            throw new ArgumentException("Asset path cannot be empty.", nameof(assetPath));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);

        AssetPath = assetPath;
        AbsolutePath = Path.GetFullPath(absolutePath);
    }

    public AssetPath AssetPath { get; }

    public string AbsolutePath { get; }
}
