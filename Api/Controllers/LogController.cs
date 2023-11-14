using CustomLogging;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("logs")]
public class LogController : ControllerBase
{
    [HttpGet("levels")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<GetCustomLogLevelsResponse> GetCustomLogLevels()
    {
        return Ok(new GetCustomLogLevelsResponse
        {
            DefaultLevel = DynamicLogs.Instance.DefaultLevel,
            SourceContexts = DynamicLogs.Instance.GetCustomLogLevels(),
        });
    }

    [HttpPost("levels/default/{level}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult SetDefaultLogLevel(LogLevel level)
    {
        DynamicLogs.Instance.DefaultLevel = level;
        return NoContent();
    }

    [HttpPost("levels")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult SetCustomLogLevels([FromBody] SetCustomLogLevelsRequest request)
    {
        foreach ((LogLevel level, string[] sourceContexts) in request)
            DynamicLogs.Instance.AddCustomLogLevels(level, sourceContexts);
        return NoContent();
    }

    [HttpPost("levels/reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult ResetCustomLogLevels()
    {
        DynamicLogs.Instance.ResetCustomLogLevels();
        return NoContent();
    }

    [HttpGet("source-contexts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IList<string>> GetSourceContexts()
    {
        return Ok(DynamicLogs.Instance.GetSourceContexts());
    }
}

public sealed class GetCustomLogLevelsResponse
{
    public LogLevel DefaultLevel { get; init; }

    public required IDictionary<string, LogLevel> SourceContexts { get; init; }
}

public sealed class SetCustomLogLevelsRequest : Dictionary<LogLevel, string[]>
{
}