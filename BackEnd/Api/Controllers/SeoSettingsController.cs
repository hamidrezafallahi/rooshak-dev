using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SeoSettingsController : BaseController
{
    private readonly IMediator _mediator;

    public SeoSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ListDto<SeoSettingDto>>> GetAll([FromQuery] GetSeoSettingsQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsUnauthorized)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("resolve")]
    public async Task<ActionResult<SeoResolvedDto>> Resolve([FromQuery] ResolveSeoSettingQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsUnauthorized)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SeoSettingDto>> GetById([FromRoute] int id)
    {
        var result = await _mediator.Send(new GetSeoSettingByIdQuery { Id = id });
        if (result.IsUnauthorized)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]
    public async Task<ActionResult<IdDto>> Create([FromBody] CreateSeoSettingCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpPut]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]
    public async Task<ActionResult<IdDto>> Update([FromBody] UpdateSeoSettingCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpPut("active")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]
    public async Task<ActionResult<IdDto>> Active([FromBody] ActiveSeoSettingCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]
    public async Task<ActionResult<IdDto>> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteSeoSettingCommand { Id = id });
        if (result.IsUnauthorized)
            return Unauthorized(result);

        return Ok(result);
    }
}
