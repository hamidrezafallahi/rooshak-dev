using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserTagsController : BaseController
{
    private readonly IMediator _mediator;

    public UserTagsController(IMediator mediator)
    {
        _mediator = mediator;
    }


    [HttpGet]
    public async Task<ActionResult<ListDto<UserTagDto>>> GetAll([FromQuery] GetAllUserTagsQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<UserTagDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetUserTagByIdQuery { Id = id });
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }
 
    [HttpPost]
    public async Task<ActionResult<IdDto>> Create(CreateUserTagCommand command)
    {

        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult<IdDto>> Update([FromBody] UpdateUserTagCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }
    [HttpPut("active")]
    public async Task<ActionResult<IdDto>> Active([FromBody] ActiveUserTagCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<IdDto>> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteUserTagCommand { Id = id });
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);

    }

    
}
