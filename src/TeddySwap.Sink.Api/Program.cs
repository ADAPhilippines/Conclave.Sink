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
builder.Services.Configure<TeddySwapITNRewardSettings>(options => builder.Configuration.GetSection("TeddySwapITNRewardSettings").Bind(options));
builder.Services.AddControllers();
builder.Services.AddScoped<LeaderboardService>();
builder.Services.AddApiVersioning(options => options.AssumeDefaultVersionWhenUnspecified = true).AddMvc();


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
