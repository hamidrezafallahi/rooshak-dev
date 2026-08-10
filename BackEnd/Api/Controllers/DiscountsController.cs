using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Domain.Entities;

[ApiController]
[Route("api/[controller]")]
public class DiscountsController : BaseController
{
    private readonly IMediator _mediator;

    public DiscountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET: api/discounts
    [HttpGet]
    public async Task<ActionResult<ListDto<DiscountDto>>> GetAll([FromQuery] GetAllDiscountsQuery query)
    {
        var result = await _mediator.Send(query);
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("getids")]
    public async Task<ActionResult<IEnumerable<IdDto>>> GetAllDiscountIds()
    {
        var result = await _mediator.Send(new GetAllDiscountsIdQuery());
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("getslugs")]
    public async Task<ActionResult<IEnumerable<SlugDto>>> GetAllDiscountSlugs()
    {
        var result = await _mediator.Send(new GetAllDiscountsSlugsQuery());
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("selectOption")]
    public async Task<ActionResult<ListDto<SelectOptionDto>>> Get4selectOption([FromQuery] GetDiscounts4selectOptionQuery query)
    {
        var result = await _mediator.Send(query);
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }

    // GET: api/discounts/active
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<DiscountDto>>> GetActive()
    {
        var result = await _mediator.Send(new GetActiveDiscountsQuery());
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DiscountDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetDiscountByIdQuery { Id = id });
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }

    // GET: api/discounts/product/5
    [HttpGet("productOffer/{productOfferId}")]
    public async Task<ActionResult<Discount>> GetByProductOfferId([FromQuery] GetDiscountByProductOfferIdQuery query)
    {
        var result = await _mediator.Send(query);
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);
        return Ok(result);
    }

    // GET: api/discounts/validate/5
    [HttpGet("validate/{discountId}")]
    public async Task<ActionResult<ValidDiscountDto>> IsValid([FromQuery] IsDiscountValidQuery query)
    {
        var result = await _mediator.Send(query);
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }

    // POST: api/discounts
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Create([FromBody] CreateDiscountCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var result = await _mediator.Send(command);
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);
        return Ok(result);
    }

    // PUT: api/discounts/{id}
    [HttpPut]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Update([FromBody] UpdateDiscountCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);
        return Ok(result);
    }
    [HttpPut("active")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Active([FromBody] ActiveDiscountCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);

        return Ok(result);
    }
    // DELETE: api/discounts/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Delete(int id)
    {
        var command = new DeleteDiscountCommand { Id = id};
        var result = await _mediator.Send(command);
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);
        return Ok(result);
    }
}
