using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProductOfferDiscountsController : BaseController
{
    private readonly IMediator _mediator;

    public ProductOfferDiscountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductOfferDiscountDto>>> GetAll([FromQuery] GetAllProductOfferDiscountQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductOfferDiscountDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetProductOfferDiscountByIdQuery { Id = id });
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }


    [HttpGet("getproductofferdiscountbyproductOfferid/{productOfferId}")]
    public async Task<ActionResult<ProductOfferDiscountDto>> GetByProductOfferId(int productOfferId)
    {
        var result = await _mediator.Send(new GetProductOfferDiscountByProductIdQuery {ProductOfferId=productOfferId });
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("getproductsdiscountbydiscountid/{discountId}")]
    public async Task<ActionResult<IEnumerable<ProductOfferDiscountDto>>> GetByDiscountId(int discountId)
    {
        var result = await _mediator.Send(new GetAllProductOfferDiscountByDiscountIdQuery(discountId));
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }


    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult> Add([FromBody] CreateProductOfferDiscountCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }

    [HttpPut]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Active([FromBody] UpdateProductOfferDiscountCommand command)
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

    public async Task<ActionResult<IdDto>> Active([FromBody] ActiveProductOfferDiscountCommand command)
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
        var result = await _mediator.Send(new DeleteProductOfferDiscountCommand { Id = id });
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);

    }
  
}
