using Conclave.Common.Models;
using Conclave.Sink.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Conclave.Sink.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ConclaveSinkDbContext _dbContext;
    public WeatherForecastController(ConclaveSinkDbContext dbContext, ILogger<WeatherForecastController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<ActionResult<Block>> Get()
    {
        return Ok(await _dbContext.Blocks.OrderByDescending(block => block.BlockNumber).Take(10).ToListAsync());
    }
}
