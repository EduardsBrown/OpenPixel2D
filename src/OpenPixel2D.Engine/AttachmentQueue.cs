using OpenPixel2D.Abstractions;

namespace OpenPixel2D.Engine;

internal sealed class ResourceQueue<T> where T : class, IAttachable
{
    private readonly struct QueueEntry
    {
        public QueueEntry(T attachable)
        {
            Attachable = attachable;
        }

        public T Attachable { get; }
    }

    private readonly Action<T> _addCallback;
    private readonly Action<T> _removeCallback;
    private List<QueueEntry> _pendingAdds = [];
    private List<QueueEntry> _pendingRemoves = [];
    private List<QueueEntry> _processingAdds = [];
    private List<QueueEntry> _processingRemoves = [];

    public ResourceQueue(Action<T> addCallback, Action<T> removeCallback)
    {
        ArgumentNullException.ThrowIfNull(addCallback);
        ArgumentNullException.ThrowIfNull(removeCallback);

        _addCallback = addCallback;
        _removeCallback = removeCallback;
    }

    public bool HasPendingAdd(T attachable)
    {
        return Contains(_pendingAdds, attachable);
    }

    public bool HasPendingRemove(T attachable)
    {
        return Contains(_pendingRemoves, attachable);
    }

    public void QueueAdd(T attachable)
    {
        if (Remove(_pendingRemoves, attachable))
        {
            return;
        }

        if (!Contains(_pendingAdds, attachable))
        {
            _pendingAdds.Add(new QueueEntry(attachable));
        }
    }

    public void QueueRemove(T attachable)
    {
        if (Remove(_pendingAdds, attachable))
        {
            return;
        }

        if (!Contains(_pendingRemoves, attachable))
        {
            _pendingRemoves.Add(new QueueEntry(attachable));
        }
    }

    public void Flush()
    {
        if (_pendingAdds.Count == 0 && _pendingRemoves.Count == 0)
        {
            return;
        }

        Swap(ref _pendingAdds, ref _processingAdds);
        Swap(ref _pendingRemoves, ref _processingRemoves);

        var addCount = _processingAdds.Count;

        for (var i = 0; i < addCount; i++)
        {
            var attachable = _processingAdds[i].Attachable;
            _addCallback(attachable);
        }

        for (var i = 0; i < addCount; i++)
        {
            _processingAdds[i].Attachable.Initialize();
        }

        for (var i = 0; i < addCount; i++)
        {
            _processingAdds[i].Attachable.OnStart();
        }

        var removeCount = _processingRemoves.Count;

        for (var i = 0; i < removeCount; i++)
        {
            var attachable = _processingRemoves[i].Attachable;
            _removeCallback(attachable);
        }

        _processingAdds.Clear();
        _processingRemoves.Clear();
    }

    public void DrainPendingAdds(Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        for (var i = 0; i < _pendingAdds.Count; i++)
        {
            action(_pendingAdds[i].Attachable);
        }

        _pendingAdds.Clear();
    }

    public void Clear()
    {
        _pendingAdds.Clear();
        _pendingRemoves.Clear();
        _processingAdds.Clear();
        _processingRemoves.Clear();
    }

    private static void Swap(ref List<QueueEntry> left, ref List<QueueEntry> right)
    {
        var temp = left;
        left = right;
        right = temp;
    }

    private static bool Contains(List<QueueEntry> entries, T attachable)
    {
        return FindIndex(entries, attachable) >= 0;
    }

    private static bool Remove(List<QueueEntry> entries, T attachable)
    {
        var index = FindIndex(entries, attachable);

        if (index < 0)
        {
            return false;
        }

        entries.RemoveAt(index);
        return true;
    }

    private static int FindIndex(List<QueueEntry> entries, T attachable)
    {
        for (var i = 0; i < entries.Count; i++)
        {
            if (ReferenceEquals(entries[i].Attachable, attachable))
            {
                return i;
            }
        }

        return -1;
    }
}
