using Api.Controllers;
using Application.Commands;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Admin API for a product's 3D model (GLB / USDZ) and the phone captures used to build it.
/// The public storefront reads the model from the product detail response (<c>model3D</c>).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]
public class ProductModels3DController : BaseController
{
    private readonly IMediator _mediator;

    public ProductModels3DController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{productId:int}")]
    public async Task<IActionResult> Get(int productId)
    {
        var result = await _mediator.Send(new GetProductModel3DAdminQuery { ProductId = productId });
        return Ok(result);
    }

    [HttpPut("{productId:int}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upsert(int productId, [FromForm] UpsertProductModel3DCommand command)
    {
        command.ProductId = productId;
        var result = await _mediator.Send(command);
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost("{productId:int}/rescale")]
    public async Task<IActionResult> Rescale(int productId, [FromBody] RescaleProductModel3DCommand command)
    {
        command.ProductId = productId;
        var result = await _mediator.Send(command);
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);
        return Ok(result);
    }

    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> Delete(int productId)
    {
        var result = await _mediator.Send(new DeleteProductModel3DCommand { ProductId = productId });
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost("{productId:int}/scan-sources")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddScanSources(int productId, [FromForm] AddProductScanSourcesCommand command)
    {
        command.ProductId = productId;
        var result = await _mediator.Send(command);
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);
        return Ok(result);
    }

    [HttpDelete("scan-sources/{id:int}")]
    public async Task<IActionResult> DeleteScanSource(int id)
    {
        var result = await _mediator.Send(new DeleteProductScanSourceCommand { Id = id });
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);
        return Ok(result);
    }
}
