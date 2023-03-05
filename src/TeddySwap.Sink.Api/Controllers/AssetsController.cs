using Microsoft.AspNetCore.Mvc;
using TeddySwap.Common.Models.Request;
using TeddySwap.Common.Models.Response;
using TeddySwap.Sink.Api.Services;
using Asp.Versioning;

namespace TeddySwap.Sink.Api.Controllers;

[ApiVersion(1.0)]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class AssetsController : ControllerBase
{
    private readonly ILogger<AssetsController> _logger;
    private readonly AssetService _assetService;

    public AssetsController(
        ILogger<AssetsController> logger,
        AssetService assetService)
    {
        _logger = logger;
        _assetService = assetService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedLeaderBoardResponse>> GetAssetsAsync([FromQuery] PaginatedAssetRequest request)
    {
        if (request.Offset < 0 || request.Limit > 100) return BadRequest();

        var res = await _assetService.GetAssetsAsync(request);

        return Ok(res);
    }

    [HttpGet("metadata")]
    public async Task<ActionResult<PaginatedLeaderBoardResponse>> GetAssetsWithMetadataAsync([FromQuery] PaginatedAssetRequest request)
    {
        if (request.Offset < 0 || request.Limit > 100) return BadRequest();

        var res = await _assetService.GetAssetsWithMetadataAsync(request);

        return Ok(res);
    }
}