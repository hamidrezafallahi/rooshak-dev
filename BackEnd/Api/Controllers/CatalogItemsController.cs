using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CatalogItemsController : BaseController
{
    private readonly IMediator _mediator;

    public CatalogItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Policy = Application.Common.CatalogApiKey.PolicyName)]
    [Consumes("application/json")]
    public Task<ActionResult<CatalogItemIdsDto>> CreateJson([FromBody] CreateCatalogItemCommand command)
        => Create(command);

    [HttpPost]
    [Authorize(Policy = Application.Common.CatalogApiKey.PolicyName)]
    [Consumes("multipart/form-data")]
    public Task<ActionResult<CatalogItemIdsDto>> CreateForm([FromForm] CreateCatalogItemCommand command)
        => Create(command);

    private async Task<ActionResult<CatalogItemIdsDto>> Create(CreateCatalogItemCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpPut("active")]
    [Authorize(Policy = Application.Common.CatalogApiKey.PolicyName)]
    public async Task<ActionResult<CatalogItemActiveDto>> SetActive(
        [FromBody] SetCatalogItemActiveCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);

        return Ok(result);
    }
}
