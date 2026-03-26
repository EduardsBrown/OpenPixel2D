using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenPixel2D.Content;

/// <summary>
/// Loads common runtime image formats and normalizes them into tightly packed RGBA8 data.
/// </summary>
internal sealed class RuntimeImageAssetLoader : IAssetLoader<RuntimeImageAsset>
{
    public bool CanLoad(AssetPath path)
    {
        string extension = Path.GetExtension(path.Value);
        return extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase);
    }

    public RuntimeImageAsset Load(AssetLoadContext context)
    {
        try
        {
            using Image<Rgba32> image = Image.Load<Rgba32>(context.AbsolutePath);
            Rgba32[] pixels = new Rgba32[checked(image.Width * image.Height)];
            image.CopyPixelDataTo(pixels);
            byte[] rgbaBytes = MemoryMarshal.AsBytes<Rgba32>(pixels.AsSpan()).ToArray();
            return new RuntimeImageAsset(image.Width, image.Height, RuntimeImagePixelFormat.Rgba32, rgbaBytes);
        }
        catch (Exception ex) when (ex is not IOException && ex is not UnauthorizedAccessException)
        {
            throw new InvalidDataException(
                $"Asset '{context.AssetPath}' is not a valid supported image file.",
                ex);
        }
    }
}
