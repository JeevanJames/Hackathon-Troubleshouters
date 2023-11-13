using CustomLogging;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class LogController : ControllerBase
{
    [HttpGet("default/{level}")]
    public ActionResult SetDefaultLevel(LogLevel level)
    {
        CustomLog.Levels.DefaultLevel = level;
        return Ok();
    }

    [HttpGet("level")]
    public ActionResult SetLogLevel(string sourceContext, LogLevel level)
    {
        CustomLog.Levels.Add(sourceContext, level);
        return Ok();
    }

    [HttpGet("reset")]
    public ActionResult ResetLogLevels()
    {
        CustomLog.Levels.Reset();
        return Ok();
    }
}