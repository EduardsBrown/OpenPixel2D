namespace OpenPixel2D.Content;

public interface IAssetLoader<T>
{
    bool CanLoad(AssetPath path);

    T Load(AssetLoadContext context);
}
