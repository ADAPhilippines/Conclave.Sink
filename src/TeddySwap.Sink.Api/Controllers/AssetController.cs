using Microsoft.AspNetCore.Mvc;
using TeddySwap.Common.Models.Request;
using TeddySwap.Common.Models.Response;
using TeddySwap.Sink.Api.Services;
using Asp.Versioning;

namespace TeddySwap.Sink.Api.Controllers;

[ApiVersion(1.0)]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class AssetController : ControllerBase
{
    private readonly ILogger<AssetController> _logger;
    private readonly AssetService _assetService;

    public AssetController(
        ILogger<AssetController> logger,
        AssetService assetService)
    {
        _logger = logger;
        _assetService = assetService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedLeaderboardResponse>> GetAssetsAsync([FromQuery] PaginatedAssetRequest request)
    {
        if (request.Offset < 0 || request.Limit > 100) return BadRequest();

        var res = await _assetService.GetAssetsAsync(request);

        return Ok(res);
    }
}