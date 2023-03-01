using TeddySwap.Sink.Data;
using TeddySwap.Sink.Extensions;
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
    options.UseNpgsql(builder.Configuration.GetConnectionString("TeddySwapSink"));
});
builder.Services.Configure<TeddySwapSinkSettings>(options => builder.Configuration.GetSection("TeddySwapSinkSettings").Bind(options));
builder.Services.AddSingleton<CardanoService>();
builder.Services.AddSingleton<ByteArrayService>();
builder.Services.AddSingleton<CborService>();
builder.Services.AddSingleton<DatumService>();
builder.Services.AddScoped<OrderService>();
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
