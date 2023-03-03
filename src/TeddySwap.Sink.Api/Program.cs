using Microsoft.EntityFrameworkCore;
using TeddySwap.Sink.Api.Models;
using TeddySwap.Sink.Api.Services;
using TeddySwap.Sink.Data;
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<TeddySwapSinkDbContext>(options =>
{
    options.EnableSensitiveDataLogging(true);
    options.UseNpgsql(builder.Configuration.GetConnectionString("TeddySwapSink"));
});

// export DBSYNC_POSTGRESQL_HOSTNAME="abf9b162cd9c04557972d3e081cdad51-1462675688.us-west-2.elb.amazonaws.com"
// export DBSYNC_POSTGRESQL_PORT="5432"
// export DBSYNC_POSTGRESQL_USER="dmtrro"
// export DBSYNC_POSTGRESQL_PASSWORD="jegvUgg3Lv8Nedc2vNSxHGifyDJRUNkBV9866MQRpyx8PPsg3ro1M5f4aZgGne59"
// export DBSYNC_POSTGRESQL_DATABASE="cardanodbsync"

string hostname = builder.Configuration["DBSYNC_POSTGRESQL_HOSTNAME"] ?? "";
string port = builder.Configuration["DBSYNC_POSTGRESQL_PORT"] ?? "";
string user = builder.Configuration["DBSYNC_POSTGRESQL_USER"] ?? "";
string password = builder.Configuration["DBSYNC_POSTGRESQL_PASSWORD"] ?? "";
string database = builder.Configuration["DBSYNC_POSTGRESQL_DATABASE"] ?? "";
string connectionString = $"Host={hostname};Database={database};Username={user};Password={password};Port={port}";

builder.Services.AddDbContext<CardanoDbSyncContext>(options =>
{
    options.EnableSensitiveDataLogging(true);
    options.UseNpgsql(connectionString);
});

builder.Services.Configure<TeddySwapITNRewardSettings>(options => builder.Configuration.GetSection("TeddySwapITNRewardSettings").Bind(options));
builder.Services.AddControllers();
builder.Services.AddScoped<LeaderboardService>();
builder.Services.AddScoped<AssetService>();
builder.Services.AddApiVersioning(options => options.AssumeDefaultVersionWhenUnspecified = true).AddMvc();
builder.Services.AddHttpContextAccessor();

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
