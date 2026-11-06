using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BlogTagsController : BaseController
{
    private readonly IMediator _mediator;

    public BlogTagsController(IMediator mediator)
    {
        _mediator = mediator;
    }


    [HttpGet]
    public async Task<ActionResult<ListDto<BlogTagDto>>> GetAll([FromQuery] GetAllBlogTagsQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<BlogTagDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetBlogTagByIdQuery { Id = id });
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }
 
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Create(CreateBlogTagCommand command)
    {

        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }

    [HttpPut]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Update([FromBody] UpdateBlogTagCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }
    [HttpPut("active")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Active([FromBody] ActiveBlogTagCommand command)
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
        var result = await _mediator.Send(new DeleteBlogTagCommand { Id = id });
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);

    }

    
}
