using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : BaseController
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ListDto<ProductDto>>> GetAll([FromQuery] GetAllProductsQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }



    [HttpGet("landings")]
    public async Task<ActionResult<List<ProductByDetailDto>>> Getlandings([FromQuery] GetLandingProductsQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("selectOption")]
    public async Task<ActionResult<ListDto<SelectOptionDto>>> GetParent4selectOption([FromQuery] GetProducts4selectOptionQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }


    [HttpGet("getbydetail")]
    public async Task<ActionResult<IEnumerable<ProductByDetailDto>>> GetAllByDetail()
    {
        var result = await _mediator.Send(new GetAllProductsByDetailQuery());
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }
    [HttpGet("getids")]
    public async Task<ActionResult<IEnumerable<IdDto>>> GetAllProductIds()
    {
        var result = await _mediator.Send(new GetAllProductsIdQuery());
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("getslugs")]
    public async Task<ActionResult<IEnumerable<SlugDto>>> GetAllProductSlugs()
    {
        var result = await _mediator.Send(new GetAllProductsSlugsQuery());
        if (result.IsUnauthorized) return Unauthorized(result);
        return Ok(result);
    }

    [HttpGet("{idOrSlug}")]
    public async Task<ActionResult<ProductByDetailDto>> GetByIdOrSlug(string idOrSlug)
    {
        var result = await _mediator.Send(new GetProductByIdQuery { IdOrSlug = idOrSlug });
        if (result.IsUnauthorized) return Unauthorized(result);
        if (!result.IsSuccess || result.Data is null) return NotFound(result);

        return Ok(result);
    }
    [HttpGet("getspecifications/{id}")]
    public async Task<ActionResult<ProductDto>> GetSpecificationsById(int id)
    {
        var result = await _mediator.Send(new GetProductSpecialsByIdQuery(id));
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("getproductbycategoryid/{categoryId}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetByCategory(int categoryId)
    {
        var result = await _mediator.Send(new GetProductsByCategoryIdQuery(categoryId));
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }
    [HttpGet("discounted")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetDiscounted()
    {
        var result = await _mediator.Send(new GetDiscountedProductsQuery());
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> Search([FromQuery] string keyword)
    {
        var result = await _mediator.Send(new SearchProductsByNameQuery(keyword));
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Create([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }

    [HttpPut]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Update([FromBody] UpdateProductCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }
    [HttpPut("active")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Active([FromBody] ActiveProductCommand command)
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
        var result = await _mediator.Send(new DeleteProductCommand { Id = id });
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);

    }
    
}
