namespace OpenPixel2D.Content;

/// <summary>
/// Backend-neutral parsed font source asset.
/// The first version keeps the original font bytes and extracted face metadata,
/// but intentionally does not generate glyph atlases or render-ready backend resources.
/// </summary>
public sealed class RuntimeFontAsset
{
    private readonly byte[] _sourceData;

    public RuntimeFontAsset(RuntimeFontFormat format, RuntimeFontFaceMetadata metadata, ReadOnlySpan<byte> sourceData)
    {
        if (sourceData.IsEmpty)
        {
            throw new ArgumentException("Font source data cannot be empty.", nameof(sourceData));
        }

        Format = format;
        Metadata = metadata;
        _sourceData = sourceData.ToArray();
    }

    public RuntimeFontFormat Format { get; }

    public RuntimeFontFaceMetadata Metadata { get; }

    public ReadOnlyMemory<byte> SourceData => _sourceData;
}
