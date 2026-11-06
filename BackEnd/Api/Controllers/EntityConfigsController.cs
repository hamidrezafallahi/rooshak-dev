using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
 

[ApiController]
[Route("api/[controller]")]
public class EntityConfigsController : BaseController
{
    private readonly IMediator _mediator;

    public EntityConfigsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    // ===== Get All =====
    [HttpGet]
    public async Task<ActionResult<ListDto<EntityConfigDto>>> GetEntityConfigs([FromQuery] GetEntityConfigsQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }


    // ===== Get menu =====
    [HttpGet("menu")]
    public async Task<ActionResult<List<MenuDto>>> GetMenu()
    {
        var result = await _mediator.Send(new GetMenuQuery());
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }

    // ===== Get By Id =====
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EntityConfigDto>> GetEntityConfigById(int id)
    {
        var result = await _mediator.Send(new GetEntityConfigByIdQuery { Id = id });
        if (result.IsUnauthorized) return Unauthorized(result);
        return Ok(result);
    }

    // ===== Get By Entity (literal segment before {url} catch-all) =====
    [HttpGet("entityFormConfig/{entity}")]
    public async Task<ActionResult<EntityConfigDto>> GetEntityConfigByurl(string entity)
    {
        var result = await _mediator.Send(new GetFormQuery { EntityName = entity });
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }

    // ===== Get By EntityName =====
    [HttpGet("{url}")]
    public async Task<ActionResult<EntityConfigDto>> GetEntityConfigByName(string url)
    {
        var result = await _mediator.Send(new GetEntityConfigByNameQuery { EntityName = url });
        if (result.IsUnauthorized) return Unauthorized(result);
        return Ok(result);
    }
    // ===== Create =====
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> CreateEntityConfig([FromBody] CreateEntityConfigCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }

  

    // ===== Update =====
    [HttpPut]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> UpdateEntityConfig([FromBody] UpdateEntityConfigCommand command)
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

    public async Task<ActionResult<IdDto>> Active([FromBody] ActiveEntityConfigCommand command)
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
        var command = new DeleteEntityConfigCommand { Id = id };
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }
   
}
