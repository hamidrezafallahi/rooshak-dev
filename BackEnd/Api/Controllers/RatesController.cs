using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class RatesController : BaseController
{
    private readonly IMediator _mediator;

    public RatesController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpGet]
    public async Task<ActionResult<ListDto<RateDto>>> GetAll([FromQuery] GetAllRateQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<RateDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetRateByIdQuery {Id=id });
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }
    [HttpGet("average")]
    public async Task<ActionResult<ListDto<AverageRateDto>>> GetAverage([FromQuery] GetAverageRateQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<IdDto>> Rate([FromBody] AddOrUpdateRateCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var result = await _mediator.Send(command);

        if (result.IsUnauthorized)
            return Unauthorized(result);

        return Ok(result);
    }
    [HttpPut]
    public async Task<ActionResult<IdDto>> Update([FromBody] AddOrUpdateRateCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }
    // دریافت میانگین امتیاز

    [HttpPut("active")]
    public async Task<ActionResult<IdDto>> Active([FromBody] ActiveRateCommand command)
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
        var result = await _mediator.Send(new DeleteRateCommand { Id = id });
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);

    }
}
