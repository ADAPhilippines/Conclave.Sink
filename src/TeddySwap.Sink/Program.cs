using System.Text.Json;
using TeddySwap.Sink.Data;
using TeddySwap.Sink.Extensions;
using TeddySwap.Common.Models;
using TeddySwap.Sink.Services;
using Microsoft.EntityFrameworkCore;
using TeddySwap.Sink.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = null);
builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.MaxDepth = int.MaxValue);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContextFactory<TeddySwapSinkDbContext>(options =>
{
    options.EnableSensitiveDataLogging(true);
    options.UseNpgsql(builder.Configuration.GetConnectionString("ConclaveSink"));
});
builder.Services.Configure<ConclaveSinkSettings>(options => builder.Configuration.GetSection("ConclaveSinkSettings").Bind(options));
builder.Services.AddSingleton<CardanoService>();
builder.Services.AddOuraReducers();

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
