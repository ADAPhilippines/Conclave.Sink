using Microsoft.EntityFrameworkCore;
using TeddySwap.Common.Models;
using TeddySwap.Sink.Data;
using TeddySwap.Sink.Extensions;
using TeddySwap.Sink.Models;
using TeddySwap.Sink.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = null);
builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.MaxDepth = int.MaxValue);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContextFactory<TeddySwapSinkDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("TeddySwapSink")));
builder.Services.AddDbContextFactory<TeddySwapOrderSinkDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("TeddySwapSink")));
builder.Services.AddDbContextFactory<TeddySwapNftSinkDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("TeddySwapSink")));
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
else
{
    // Set up oura cursor
    var ouraSettings = builder.Configuration.GetSection("OuraSettings");
    var slot = ouraSettings.GetValue<string>("DefaultSlot");
    var hash = ouraSettings.GetValue<string>("DefaultBlockHash");
    var path = ouraSettings.GetValue<string>("CursorPath");
    var offset = ouraSettings.GetValue<int>("Offset");

    using var scopedProvider = app.Services.CreateScope();
    var service = scopedProvider.ServiceProvider;
    using var dbContext = service.GetService<TeddySwapSinkDbContext>();

    if (dbContext is not null)
    {
        Block? block = await dbContext.Blocks.OrderByDescending(b => b.BlockNumber).Take(offset).LastOrDefaultAsync();

        if (block is not null)
        {
            slot = block.Slot.ToString();
            hash = block.BlockHash;
        }
    }

    await File.WriteAllTextAsync(Path.Combine(path ?? "../../deployments/config", "cursor"), $"{slot},{hash}");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
