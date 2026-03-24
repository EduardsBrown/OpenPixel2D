using System.Runtime.InteropServices;
using OpenPixel2D.Rendering.Abstractions;

namespace OpenPixel2D.Rendering;

public sealed class RenderQueue : IRenderSubmissionContext
{
    private readonly Dictionary<Type, IRenderItemStorage> _storagesByType = [];

    public void Submit<T>(in T item)
        where T : struct, IRenderItem
    {
        GetOrCreateStorage<T>().Add(item);
    }

    public ReadOnlySpan<T> GetItems<T>()
        where T : struct, IRenderItem
    {
        if (!_storagesByType.TryGetValue(typeof(T), out IRenderItemStorage? storage))
        {
            return ReadOnlySpan<T>.Empty;
        }

        return ((RenderItemStorage<T>)storage).GetItems();
    }

    public void Clear()
    {
        foreach (IRenderItemStorage storage in _storagesByType.Values)
        {
            storage.Clear();
        }
    }

    private RenderItemStorage<T> GetOrCreateStorage<T>()
        where T : struct, IRenderItem
    {
        if (_storagesByType.TryGetValue(typeof(T), out IRenderItemStorage? storage))
        {
            return (RenderItemStorage<T>)storage;
        }

        RenderItemStorage<T> createdStorage = new();
        _storagesByType.Add(typeof(T), createdStorage);
        return createdStorage;
    }

    private interface IRenderItemStorage
    {
        void Clear();
    }

    private sealed class RenderItemStorage<T> : IRenderItemStorage
        where T : struct, IRenderItem
    {
        private readonly List<T> _items = [];

        public void Add(in T item)
        {
            _items.Add(item);
        }

        public ReadOnlySpan<T> GetItems()
        {
            return CollectionsMarshal.AsSpan(_items);
        }

        public void Clear()
        {
            _items.Clear();
        }
    }
}
