using CustomLogging;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("logs")]
public class LogController : ControllerBase
{
    [HttpGet("levels")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<CustomLogLevelDetailsResponse> GetCustomLogLevels()
    {
        return Ok(GetCustomLogLevelDetails());
    }

    [HttpPost("levels/settings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<CustomLogLevelDetailsResponse> SetCustomLogSettings(
        [FromBody] SetCustomLogSettingsRequest request)
    {
        DynamicLogs.Instance.DefaultLevel = request.DefaultLevel;
        DynamicLogs.Instance.OnlyCustomSourceContexts = request.OnlyUseCustomLevels;
        return Ok(GetCustomLogLevelDetails());
    }

    [HttpPost("levels")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<CustomLogLevelDetailsResponse> SetCustomLogLevels(
        [FromBody] SetCustomLogLevelsRequest request)
    {
        foreach ((LogLevel level, string[] sourceContexts) in request)
            DynamicLogs.Instance.AddCustomLogLevels(level, sourceContexts);
        return Ok(GetCustomLogLevelDetails());
    }

    [HttpPost("levels/reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<CustomLogLevelDetailsResponse> ResetCustomLogLevels(
        [FromBody] string[] sourceContexts)
    {
        DynamicLogs.Instance.ResetCustomLogLevels(sourceContexts);
        return Ok(GetCustomLogLevelDetails());
    }

    [HttpGet("source-contexts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IList<string>> GetSourceContexts()
    {
        return Ok(DynamicLogs.Instance.GetSourceContexts());
    }

    private static CustomLogLevelDetailsResponse GetCustomLogLevelDetails() => new()
    {
        DefaultLevel = DynamicLogs.Instance.DefaultLevel,
        OnlyUseCustomLevels = DynamicLogs.Instance.OnlyCustomSourceContexts,
        SourceContexts = DynamicLogs.Instance.GetCustomLogLevels(),
    };
}

public sealed class CustomLogLevelDetailsResponse
{
    public LogLevel DefaultLevel { get; init; }

    public bool OnlyUseCustomLevels { get; init; }

    public required IDictionary<string, LogLevel> SourceContexts { get; init; }
}

public sealed class SetCustomLogLevelsRequest : Dictionary<LogLevel, string[]>
{
}

public sealed record SetCustomLogSettingsRequest(LogLevel DefaultLevel, bool OnlyUseCustomLevels);
