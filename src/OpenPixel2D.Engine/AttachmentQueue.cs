using OpenPixel2D.Abstractions;

namespace OpenPixel2D.Engine;

internal class ResourceQueue<T> where T : IAttachable
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

    public void QueueAdd(T attachable, Action<T> callback)
    {
        var record = new AttachableRecord(attachable, callback);
        if (!toAdd.Contains(record))
        {
            toAdd.Add(record);
        }
    }

    public void QueueRemove(T attachable, Action<T> callback)
    {
        var record = new AttachableRecord(attachable, callback);

        if (toAdd.Contains(record))
        {
            toAdd.Remove(record);
            return;
        }

        if (!toRemove.Contains(record))
        {
            toRemove.Add(record);
        }
    }

    public void Flush()
    {
        if (toAdd.Count > 0)
        {
            foreach (var record in toAdd)
            {
                record.Callback(record.Attachable);
            }

            foreach (var record in toAdd)
            {
                record.Attachable.Initialize();
            }

            foreach (var record in toAdd)
            {
                record.Attachable.OnStart();
            }
        }

        if (toRemove.Count > 0)
        {
            foreach (var record in toRemove)
            {
                record.Callback(record.Attachable);
            }
        }

        toAdd.Clear();
        toRemove.Clear();
    }
}