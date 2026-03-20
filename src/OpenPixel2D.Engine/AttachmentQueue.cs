using OpenPixel2D.Abstractions;

namespace OpenPixel2D.Engine;

internal sealed class ResourceQueue<T> where T : class, IAttachable
{
    private record AttachableRecord(T Attachable, Action<T> Callback);

    private List<AttachableRecord> toAdd { get; } = [];
    private List<AttachableRecord> toRemove { get; } = [];

    public bool HasAnyToAdd()
    {
        return toAdd.Count > 0;
    }

    public bool HasAnyToRemove()
    {
        return toRemove.Count > 0;
    }

    public bool HasPendingAdd(T attachable)
    {
        return Contains(toAdd, attachable);
    }

    public bool HasPendingRemove(T attachable)
    {
        return Contains(toRemove, attachable);
    }

    public void QueueAdd(T attachable, Action<T> callback)
    {
        if (Remove(toRemove, attachable))
        {
            return;
        }

        if (!Contains(toAdd, attachable))
        {
            toAdd.Add(new AttachableRecord(attachable, callback));
        }
    }

    public void QueueRemove(T attachable, Action<T> callback)
    {
        if (Remove(toAdd, attachable))
        {
            return;
        }

        if (!Contains(toRemove, attachable))
        {
            toRemove.Add(new AttachableRecord(attachable, callback));
        }
    }

    public void Flush()
    {
        var toAddSnapshot = toAdd.ToArray();
        var toRemoveSnapshot = toRemove.ToArray();

        toAdd.Clear();
        toRemove.Clear();

        foreach (var record in toAddSnapshot)
        {
            record.Callback(record.Attachable);
        }

        foreach (var record in toAddSnapshot)
        {
            record.Attachable.Initialize();
        }

        foreach (var record in toAddSnapshot)
        {
            record.Attachable.OnStart();
        }

        foreach (var record in toRemoveSnapshot)
        {
            record.Callback(record.Attachable);
        }
    }

    public T[] DrainPendingAdds()
    {
        var pendingAdditions = new T[toAdd.Count];

        for (var i = 0; i < toAdd.Count; i++)
        {
            pendingAdditions[i] = toAdd[i].Attachable;
        }

        return pendingAdditions;
    }

    public void Clear()
    {
        toAdd.Clear();
        toRemove.Clear();
    }

    private static bool Contains(List<AttachableRecord> records, T attachable)
    {
        return FindIndex(records, attachable) >= 0;
    }

    private static bool Remove(List<AttachableRecord> records, T attachable)
    {
        var index = FindIndex(records, attachable);

        if (index < 0)
        {
            return false;
        }

        records.RemoveAt(index);
        return true;
    }

    private static int FindIndex(List<AttachableRecord> records, T attachable)
    {
        for (var i = 0; i < records.Count; i++)
        {
            if (ReferenceEquals(records[i].Attachable, attachable))
            {
                return i;
            }
        }

        return -1;
    }
}