using System.Text.Json.Serialization;
using CustomLogging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

DynamicLogs.Instance.AddCustomLogLevels(LogLevel.Information,
    "Serilog", "Microsoft", "System.Diagnostics", "Microsoft.AspNetCore", "Jeevan", "System",
    "Serilog", "Microsoft", "System.Diagnostics", "Microsoft.AspNetCore", "Jeevan", "System");

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
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
