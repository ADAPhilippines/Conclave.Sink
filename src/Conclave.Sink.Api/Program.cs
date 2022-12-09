using Blockfrost.Api.Extensions;
using Conclave.Common.Models;
using Conclave.Sink.Data;
using Microsoft.EntityFrameworkCore;
using ConclaveAccountService = Conclave.Sink.Api.Services.AccountService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ConclaveSinkDbContext>(options =>
{
    options.EnableSensitiveDataLogging(true);
    options.UseNpgsql(builder.Configuration.GetConnectionString("ConclaveSink"));
});
var blockfrost = builder.Configuration.GetSection("Blockfrost");
builder.Services.AddBlockfrost(blockfrost.GetValue<string>("Network"), blockfrost.GetValue<string>("ProjectId"));
builder.Services.AddScoped<ConclaveAccountService>();
builder.Services.Configure<ConclaveSettings>(builder.Configuration.GetSection("Conclave"));
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
