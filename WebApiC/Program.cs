using Hasti.Framework.Endpoints.Logging.Extensions;
using Serilog;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, sp, lc) => lc.WithDefaultConfiguration(ctx, sp));

// Add services to the container.
var defaulttrace = Activity.DefaultIdFormat;
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
