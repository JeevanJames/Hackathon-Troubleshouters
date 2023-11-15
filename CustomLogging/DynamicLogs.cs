using System.Collections.ObjectModel;

using Microsoft.Extensions.Logging;

namespace CustomLogging;

public sealed class DynamicLogs
{
    public static readonly DynamicLogs Instance = new();

    private readonly CustomLogLevelCollection _customLogLevels = new();
    private readonly ReaderWriterLockSlim _customLogLevelsLock = new();

    private readonly SourceContextCollection _sourceContexts = new();
    private readonly ReaderWriterLockSlim _sourceContextsLock = new();

    public LogLevel DefaultLevel { get; set; } = LogLevel.Information;

    public bool OnlyCustomSourceContexts { get; set; }

    public void AddCustomLogLevel(string sourceContext, LogLevel level)
    {
        _customLogLevelsLock.EnterWriteLock();
        try
        {
            _customLogLevels.Add(new CustomLogLevel(sourceContext, level));
        }
        finally
        {
            _customLogLevelsLock.ExitWriteLock();
        }
    }

    public void AddCustomLogLevels(LogLevel level, params string[] sourceContexts)
    {
        _customLogLevelsLock.EnterWriteLock();
        try
        {
            foreach (string sourceContext in sourceContexts)
                _customLogLevels.Add(new CustomLogLevel(sourceContext, level));
        }
        finally
        {
            _customLogLevelsLock.ExitWriteLock();
        }
    }

    public IDictionary<string, LogLevel> GetCustomLogLevels()
    {
        _customLogLevelsLock.EnterReadLock();
        try
        {
            return _customLogLevels.OrderBy(c => c.SourceContext)
                .ToDictionary(c => c.SourceContext, c => c.Level, StringComparer.OrdinalIgnoreCase);
        }
        finally
        {
            _customLogLevelsLock.ExitReadLock();
        }
    }

    public void ResetCustomLogLevels(params string[]? sourceContexts)
    {
        _customLogLevelsLock.EnterWriteLock();
        try
        {
            if (sourceContexts is null || sourceContexts.Length == 0)
                _customLogLevels.Clear();
            else
            {
                foreach (string sourceContext in sourceContexts.Where(sc => !string.IsNullOrWhiteSpace(sc)))
                {
                    int index = _customLogLevels.FindIndexOfSourceContext(sourceContext);
                    if (index >= 0)
                        _customLogLevels.RemoveAt(index);
                }
            }
        }
        finally
        {
            _customLogLevelsLock.ExitWriteLock();
        }
    }

    public LogLevel FindLogLevelFor(string sourceContext)
    {
        _customLogLevelsLock.EnterReadLock();
        try
        {
            if (_customLogLevels.Count == 0)
                return DefaultLevel;

            if (_customLogLevels.TryFindClosestMatch(sourceContext, out LogLevel logLevel))
                return logLevel;
            return OnlyCustomSourceContexts ? LogLevel.None : DefaultLevel;
        }
        finally
        {
            _customLogLevelsLock.ExitReadLock();
        }
    }

    public void AddSourceContext(string sourceContext)
    {
        _sourceContextsLock.EnterWriteLock();
        try
        {
            _sourceContexts.Add(sourceContext);
        }
        finally
        {
            _sourceContextsLock.ExitWriteLock();
        }
    }

    public IList<string> GetSourceContexts()
    {
        _sourceContextsLock.EnterReadLock();
        try
        {
            return _sourceContexts.ToList();
        }
        finally
        {
            _sourceContextsLock.ExitReadLock();
        }
    }
}

public readonly record struct CustomLogLevel(string SourceContext, LogLevel Level);

public sealed class CustomLogLevelCollection : Collection<CustomLogLevel>
{
    public int FindIndexOfSourceContext(string sourceContext)
    {
        int low = 0;
        int high = Count - 1;

        while (low <= high)
        {
            int mid = (low + high) / 2;
            int compare = string.Compare(this[mid].SourceContext, sourceContext, StringComparison.Ordinal);

            if (compare == 0)
                return mid;

            if (compare < 0)
                low = mid + 1;
            else
                high = mid - 1;
        }

        return -1;
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

    protected override void InsertItem(int index, CustomLogLevel item)
    {
        int insertionIndex = FindInsertionIndex(item, out bool itemExists);
        if (itemExists)
            RemoveItem(insertionIndex);
        base.InsertItem(insertionIndex, item);
    }

    protected override void SetItem(int index, CustomLogLevel item)
    {
        //int insertionIndex = FindInsertionIndex(item);
        //if (insertionIndex < 0)
        //    return;

        //// Remove the item at the specified index
        //RemoveItem(index);

        //// Insert the modified item at the correct position
        //base.InsertItem(insertionIndex, item);

        throw new NotSupportedException();
    }

    private int FindInsertionIndex(CustomLogLevel item, out bool isExistingItem)
    {
        for (int i = 0; i < Count; i++)
        {
            int comparison = string.Compare(this[i].SourceContext, item.SourceContext,
                StringComparison.OrdinalIgnoreCase);
            if (comparison >= 0)
            {
                isExistingItem = comparison == 0;
                return i;
            }
        }

        isExistingItem = false;
        return Count;
    }
}

public sealed class SourceContextCollection : Collection<string>
{
    public SourceContextCollection() : base(new List<string>(1024))
    {
    }

    protected override void InsertItem(int index, string item)
    {
        int insertionIndex = FindInsertionIndex(item);
        if (insertionIndex >= 0)
            base.InsertItem(insertionIndex, item);
    }

    // Returns -1 if the item exists in the collection
    private int FindInsertionIndex(string str)
    {
        int low = 0;
        int high = Count - 1;

        while (low <= high)
        {
            int mid = (low + high) / 2;
            int compare = string.Compare(this[mid], str, StringComparison.Ordinal);

            if (compare == 0)
                return -1;

            if (compare < 0)
                low = mid + 1;
            else
                high = mid - 1;
        }

        return low;
    }

    protected override void SetItem(int index, string item)
    {
        throw new NotSupportedException("Cannot override existing source contexts.");
    }
}