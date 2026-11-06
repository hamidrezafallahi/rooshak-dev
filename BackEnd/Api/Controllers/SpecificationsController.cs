using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/[controller]")]
public class SpecificationsController : BaseController
{
    private readonly IMediator _mediator;

    public SpecificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpGet]
    public async Task<ActionResult<ListDto<SpecificationsDto>>> GetAll([FromQuery] GetProductSpecificationsQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsUnauthorized) return Unauthorized(result);


        return Ok(result);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<SpecificationsDto>> GetSpecificationsById(int id)
    {
        var result = await _mediator.Send(new GetProductSpecificationByIdQuery { Id = id });
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> CreateSpecifications([FromBody] CreateProductSpecificationCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }



    [HttpPut]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> UpdateSpecifications([FromBody] UpdateProductSpecificationCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }

    [HttpPut("active")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Active([FromBody] ActiveProductSpecificationCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteProductSpecificationCommand { Id = id });
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);

    }



    
}
