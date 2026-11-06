using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class LandingController : BaseController
{
    private readonly IMediator _mediator;

    public LandingController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpGet]
    public async Task<ActionResult<ListDto<LandingSliderDto>>> GetBlogs([FromQuery] GetAllLandingSlidesQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }
     

    [HttpGet("{id}")]
    public async Task<ActionResult<LandingSliderDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetLandingSlideByIdQuery { Id = id });
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("slide")]

    public async Task<ActionResult<IEnumerable<LandingSliderDto>>> getSliders()
    {
        var result = await _mediator.Send(new GetLandingSlideQuery());
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> CreateSlide([FromForm] CreateSlideCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }




    [HttpPut]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> UpdateSlide([FromForm] UpdateSlideCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }
    [HttpPut("set-default")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> SetHeroBanner([FromBody] SetHeroBannerCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }

    [HttpPut("active")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> ActiveSlide([FromBody] ActiveSlideCommand command)
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

        var command = new DeleteSlideCommand { Id = id };
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }





}
