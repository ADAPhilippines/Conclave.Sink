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

    [HttpGet("policy/{policyId}/address/{address}")]
    public async Task<ActionResult<PaginatedLeaderBoardResponse>> GetAssetsAsync([FromRoute] string policyId, [FromRoute] string address, [FromQuery] PaginatedRequest request)
    {
        if (request.Offset < 0 || request.Limit > 100 || string.IsNullOrEmpty(policyId) || string.IsNullOrEmpty(address)) return BadRequest();

        var res = await _assetService.GetNftOwnerAsync(request.Offset, request.Limit, address, policyId);

        return Ok(res);
    }

    [HttpGet("metadata/policy/{policyId}/name/{name}")]
    public async Task<ActionResult<AssetMetadataResponse>> GetAssetMetadataAsync([FromRoute] string policyId, [FromRoute] string name)
    {
        if (string.IsNullOrEmpty(policyId) || string.IsNullOrEmpty(name)) return BadRequest();

        var res = await _assetService.GetNftMetadataAsync(policyId, name);

        return Ok(res);
    }
}