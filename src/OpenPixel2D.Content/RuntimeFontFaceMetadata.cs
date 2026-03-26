namespace OpenPixel2D.Content;

/// <summary>
/// Backend-neutral metadata extracted from a parsed font face.
/// Values remain in font design units as reported by the font library itself so later backends
/// can choose their own scaling strategy without relying on inferred placeholder conversions.
/// </summary>
public readonly record struct RuntimeFontFaceMetadata(
    string FamilyName,
    string FontName,
    string SubfamilyName,
    RuntimeFontStyle Style,
    int UnitsPerEm,
    int Ascender,
    int Descender,
    int LineGap,
    int LineHeight);
