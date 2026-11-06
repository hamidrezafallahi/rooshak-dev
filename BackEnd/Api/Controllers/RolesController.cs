using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class RolesController : BaseController
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpGet]
    //[Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<List<UserRoleDto>>> GetAll([FromQuery] GetRolesQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }
    
    [HttpGet("{id:int}")]
    //[Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<UserRoleDto?>> GetById(int id)
    {
        var result = await _mediator.Send(new GetRoleByIdQuery { Id = id });
        if (result.IsUnauthorized) return Unauthorized(result);


        return Ok(result);
    }
    [HttpPut("active")]
    [Authorize(Roles = "SuperAdmin")]

    public async Task<ActionResult<IdDto>> Active([FromBody] ActiveRoleCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]

    public async Task<ActionResult<IdDto>> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteRoleCommand { Id = id });
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);

    }
}
