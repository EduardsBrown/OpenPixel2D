using System.Globalization;
using SixLabors.Fonts;
using SixLaborsFontStyle = SixLabors.Fonts.FontStyle;

namespace OpenPixel2D.Content;

/// <summary>
/// Loads TrueType font files into a parsed-source runtime asset.
/// This stage captures font bytes and face metadata only; glyph atlases and backend font resources are later work.
/// </summary>
internal sealed class RuntimeFontAssetLoader : IAssetLoader<RuntimeFontAsset>
{
    public bool CanLoad(AssetPath path)
    {
        return Path.GetExtension(path.Value).Equals(".ttf", StringComparison.OrdinalIgnoreCase);
    }

    public RuntimeFontAsset Load(AssetLoadContext context)
    {
        byte[] sourceData = File.ReadAllBytes(context.AbsolutePath);

        try
        {
            using MemoryStream stream = new(sourceData, writable: false);
            FontCollection collection = new();
            FontFamily family = collection.Add(stream, CultureInfo.InvariantCulture, out FontDescription description);
            FontMetrics metrics = family.CreateFont(12f, description.Style).FontMetrics;
            HorizontalMetrics horizontalMetrics = metrics.HorizontalMetrics;

            return new RuntimeFontAsset(
                RuntimeFontFormat.TrueType,
                new RuntimeFontFaceMetadata(
                    description.FontFamilyInvariantCulture,
                    description.FontNameInvariantCulture,
                    description.FontSubFamilyNameInvariantCulture,
                    MapStyle(description.Style),
                    UnitsPerEm: metrics.UnitsPerEm,
                    Ascender: horizontalMetrics.Ascender,
                    Descender: horizontalMetrics.Descender,
                    LineGap: horizontalMetrics.LineGap,
                    LineHeight: horizontalMetrics.LineHeight),
                sourceData);
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException(
                $"Asset '{context.AssetPath}' is not a valid TrueType font file.",
                ex);
        }
    }

    private static RuntimeFontStyle MapStyle(SixLaborsFontStyle style)
    {
        RuntimeFontStyle mappedStyle = RuntimeFontStyle.Regular;

        if ((style & SixLaborsFontStyle.Bold) != 0)
        {
            mappedStyle |= RuntimeFontStyle.Bold;
        }

        if ((style & SixLaborsFontStyle.Italic) != 0)
        {
            mappedStyle |= RuntimeFontStyle.Italic;
        }

        return mappedStyle;
    }
}
