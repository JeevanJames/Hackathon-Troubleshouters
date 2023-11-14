using System.Text.Json.Serialization;
using Api;
using CustomLogging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);

// Set Dynamic Logs defaults
IConfigurationSection dynamicLogsSection = builder.Configuration.GetSection("DynamicLogs");
ConfigModel config = dynamicLogsSection.Get<ConfigModel>() ?? new ConfigModel();
DynamicLogs.Instance.DefaultLevel = config.DefaultLevel;
if (config.CustomLevels is not null)
{
    foreach ((LogLevel level, string[] sourceContexts) in config.CustomLevels)
        DynamicLogs.Instance.AddCustomLogLevels(level, sourceContexts);
}

builder.Host.UseSerilog((_, lc) => lc
    .MinimumLevel.Verbose()
    .WriteTo.Console(
        theme: AnsiConsoleTheme.Code,
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}"));

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
