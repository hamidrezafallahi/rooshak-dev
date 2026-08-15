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
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]
    public async Task<ActionResult<CatalogItemIdsDto>> Create([FromBody] CreateCatalogItemCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);

        return Ok(result);
    }
}
