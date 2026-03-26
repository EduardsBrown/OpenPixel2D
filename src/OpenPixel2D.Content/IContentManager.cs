using System.Diagnostics.CodeAnalysis;

namespace OpenPixel2D.Content;

public interface IContentManager
{
    T Load<T>(AssetPath path);

    bool TryLoad<T>(AssetPath path, [MaybeNullWhen(false)] out T asset);
}
