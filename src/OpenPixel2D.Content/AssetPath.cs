namespace OpenPixel2D.Content;

public readonly record struct AssetPath
{
    private readonly string? _value;

    public AssetPath(string value)
    {
        _value = Normalize(value);
    }

    private AssetPath(string value, bool bypassValidation)
    {
        _value = value;
    }

    public string Value => _value ?? string.Empty;

    public bool IsEmpty => string.IsNullOrEmpty(_value);

    public static implicit operator AssetPath(string value)
    {
        return new AssetPath(value);
    }

    public override string ToString()
    {
        return Value;
    }

    internal string ToPlatformPath()
    {
        return Value.Replace('/', Path.DirectorySeparatorChar);
    }

    internal static AssetPath UnsafeCreate(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new AssetPath(value, bypassValidation: true);
    }

    private static string Normalize(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Asset path cannot be empty or whitespace.", nameof(value));
        }

        if (IsAbsolute(value))
        {
            throw new ArgumentException("Asset path must be relative.", nameof(value));
        }

        string normalized = value.Replace('\\', '/');
        string[] segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
        {
            throw new ArgumentException("Asset path cannot be empty or whitespace.", nameof(value));
        }

        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] is "." or "..")
            {
                throw new ArgumentException("Asset path cannot contain '.' or '..' segments.", nameof(value));
            }
        }

        return string.Join('/', segments);
    }

    private static bool IsAbsolute(string value)
    {
        return value.StartsWith("/", StringComparison.Ordinal)
            || value.StartsWith("\\", StringComparison.Ordinal)
            || value.StartsWith("//", StringComparison.Ordinal)
            || value.StartsWith("\\\\", StringComparison.Ordinal)
            || (value.Length >= 2 && char.IsLetter(value[0]) && value[1] == ':')
            || Path.IsPathRooted(value);
    }
}
