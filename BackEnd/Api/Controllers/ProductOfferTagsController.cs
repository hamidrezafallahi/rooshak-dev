using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProductOfferTagsController : BaseController
{
    private readonly IMediator _mediator;

    public ProductOfferTagsController(IMediator mediator)
    {
        _mediator = mediator;
    }


    [HttpGet]
    public async Task<ActionResult<ListDto<ProductOfferTagsDto>>> GetAll([FromQuery] GetAllProductOfferTagQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetProductOfferTagByIdQuery { Id = id });
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("productId/{productId}")]
    public async Task<ActionResult<IEnumerable<ProductOfferTagDto>>> GetByProductId(int productId)
    {
        var result = await _mediator.Send(new GetProductOfferTagByProductIdQuery(productId));
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }


    [HttpGet("tag/{tagId}")]
    public async Task<ActionResult<IEnumerable<ProductCardBySupplierDto>>> GetByTagId(int tagId)
    {
        var result = await _mediator.Send(new GetAllProductOfferTagByTagIdQuery(tagId));
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }
    [HttpGet("getids")]
    public async Task<ActionResult<IEnumerable<IdDto>>> GetAllProductTagIds()
    {
        var result = await _mediator.Send(new GetAllProductOfferTagIdsQuery());
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult> Create(CreateProductOfferTagCommand command)
    {

        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }

    [HttpPut]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult> Update([FromBody]UpdateProductOfferTagCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }
    [HttpPut("active")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Active([FromBody] ActiveProductOfferTagCommand command)
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
        var result = await _mediator.Send(new DeleteProductOfferTagCommand { Id = id });
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);

    }

    
}
