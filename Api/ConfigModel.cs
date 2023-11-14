namespace Api;

public sealed class ConfigModel
{
    public LogLevel DefaultLevel { get; set; } = LogLevel.Information;

    public Dictionary<LogLevel, string[]>? CustomLevels { get; set; }
}
