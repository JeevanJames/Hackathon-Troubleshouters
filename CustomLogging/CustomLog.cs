using System.Collections.ObjectModel;

using Microsoft.Extensions.Logging;

namespace CustomLogging;

public static class CustomLog
{
    public static readonly ConcurrentCustomLogLevelCollection Levels = new();
}

public readonly record struct CustomLogLevel(string SourceContext, LogLevel Level);

public sealed class CustomLogLevelCollection : Collection<CustomLogLevel>
{
    protected override void InsertItem(int index, CustomLogLevel item)
    {
        int insertionIndex = FindInsertionIndex(item);
        if (insertionIndex >= 0)
            base.InsertItem(insertionIndex, item);
    }

    protected override void SetItem(int index, CustomLogLevel item)
    {
        int insertionIndex = FindInsertionIndex(item);
        if (insertionIndex < 0)
            return;

        // Remove the item at the specified index
        RemoveItem(index);

        // Insert the modified item at the correct position
        base.InsertItem(insertionIndex, item);
    }

    private int FindInsertionIndex(CustomLogLevel item)
    {
        //int index = 0;
        //while (index < Count && string.Compare(this[index].SourceContext, item.SourceContext, StringComparison.Ordinal) < 0)
        //    index++;
        //return index;

        for (int i = 0; i < Count; i++)
        {
            int comparison = string.Compare(this[i].SourceContext, item.SourceContext,
                StringComparison.OrdinalIgnoreCase);
            if (comparison == 0)
                return -1;
            if (comparison > 0)
                return i;
        }

        return Count;
    }

    public bool TryFindClosestMatch(string paramName, out LogLevel level)
    {
        level = LogLevel.None;

        if (string.IsNullOrEmpty(paramName))
            return false;

        string? closestMatch = null;
        foreach (CustomLogLevel item in this)
        {
            // Check for an exact match
            if (item.SourceContext.Equals(paramName, StringComparison.OrdinalIgnoreCase))
            {
                level = item.Level;
                return true;
            }

            // Check for the longest prefix match
            if (paramName.StartsWith(item.SourceContext, StringComparison.OrdinalIgnoreCase) &&
                (closestMatch is null || item.SourceContext.Length > closestMatch.Length))
            {
                closestMatch = item.SourceContext;
                level = item.Level;
            }
        }

        return closestMatch is not null;
    }
}

public sealed class ConcurrentCustomLogLevelCollection
{
    private readonly CustomLogLevelCollection _collection = new();
    private readonly ReaderWriterLockSlim _lock = new();

    public LogLevel DefaultLevel { get; set; } = LogLevel.Warning;

    public void Add(string sourceContext, LogLevel level)
    {
        _lock.EnterWriteLock();
        try
        {
            _collection.Add(new CustomLogLevel(sourceContext, level));
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void AddRange(LogLevel level, params string[] sourceContexts)
    {
        _lock.EnterWriteLock();
        try
        {
            foreach (string sourceContext in sourceContexts)
                _collection.Add(new CustomLogLevel(sourceContext, level));
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void Reset()
    {
        _lock.EnterWriteLock();
        try
        {
            _collection.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public LogLevel FindLogLevelFor(string sourceContext)
    {
        _lock.EnterReadLock();
        try
        {
            return _collection.TryFindClosestMatch(sourceContext, out LogLevel logLevel)
                ? logLevel
                : DefaultLevel;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}