namespace OpenPixel2D.Engine;

internal sealed class AttachmentRegistry<T> where T : class
{
    private readonly List<T> _items = [];
    private readonly List<T> _pendingAdd = [];
    private readonly List<T> _pendingRemove = [];

    public IReadOnlyList<T> Items => _items;

    public bool Contains(T item) => _items.Contains(item);
    public bool IsPendingAdd(T item) => _pendingAdd.Contains(item);
    public bool IsPendingRemove(T item) => _pendingRemove.Contains(item);

    public void QueueAdd(T item)
    {
        if (_pendingRemove.Remove(item))
        {
            return;
        }

        if (_items.Contains(item) || _pendingAdd.Contains(item))
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

    public void FlushAdditions(Func<T, bool> onAdd)
    {
        for (int i = 0; i < _pendingAdd.Count;)
        {
            T item = _pendingAdd[i];

            if (!onAdd(item))
            {
                i++;
                continue;
            }

            _items.Add(item);
            _pendingAdd.RemoveAt(i);
        }
    }
}
