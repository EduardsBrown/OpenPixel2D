namespace OpenPixel2D.Engine;

internal sealed class AttachmentRegistry<T> where T : class
{
    private readonly List<T> _items = [];
    private readonly List<T> _pendingAdd = [];
    private readonly List<T> _pendingRemove = [];

    public IReadOnlyList<T> Items => _items;
    
    public void Add(T item)
    {
        if (_pendingRemove.Remove(item))
        {
            return;
        }

        if (_items.Contains(item))
        {
            return;
        }

        _pendingAdd.Remove(item);
        _items.Add(item);
    }

    public void Remove(T item)
    {
        if (_pendingAdd.Remove(item))
        {
            return;
        }

        if (!_items.Remove(item))
        {
            return;
        }

        _pendingRemove.Remove(item);
    }

    public void QueueAdd(T item)
    {
        if (_items.Contains(item) || _pendingAdd.Contains(item))
        {
            return;
        }

        if (_pendingRemove.Remove(item))
        {
            return;
        }

        _pendingAdd.Add(item);
    }

    public void QueueRemove(T item)
    {
        if (_pendingAdd.Remove(item))
        {
            return;
        }

        if (!_items.Contains(item) || _pendingRemove.Contains(item))
        {
            return;
        }

        _pendingRemove.Add(item);
    }

    public void FlushRemovals(Action<T> onRemove)
    {
        for (int i = 0; i < _pendingRemove.Count; i++)
        {
            T item = _pendingRemove[i];
            onRemove(item);
            _items.Remove(item);
        }

        _pendingRemove.Clear();
    }

    public void FlushAdditions(Action<T> onAdd)
    {
        for (int i = 0; i < _pendingAdd.Count; i++)
        {
            T item = _pendingAdd[i];
            _items.Add(item);
            onAdd(item);
        }

        _pendingAdd.Clear();
    }
}
