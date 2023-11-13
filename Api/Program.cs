using CustomLogging;
using Serilog;

CustomLog.Levels.AddRange(LogLevel.Information,
    "Serilog", "Microsoft", "System.Diagnostics", "Microsoft.AspNetCore", "Jeevan", "System",
    "Serilog", "Microsoft", "System.Diagnostics", "Microsoft.AspNetCore", "Jeevan", "System");

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console());

// Add services to the container.

builder.Services.AddControllers();
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
