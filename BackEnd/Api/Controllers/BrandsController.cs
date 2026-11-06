using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BrandsController : BaseController
{
    private readonly IMediator _mediator;

    public BrandsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpGet]
    public async Task<ActionResult<ListDto<BrandDto>>> GetAll([FromQuery] GetAllBrandsQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }
    [HttpGet("selectOption")]
    public async Task<ActionResult<ListDto<SelectOptionDto>>> Get4selectOption([FromQuery] GetBrands4selectOptionQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("{idOrSlug}")]
    public async Task<ActionResult<BrandDto>> GetByIdOrSlug(string idOrSlug)
    {
        var result = await _mediator.Send(new GetBrandByIdQuery { IdOrSlug = idOrSlug });
        if (result.IsUnauthorized) return Unauthorized(result);
        if (!result.IsSuccess || result.Data is null) return NotFound(result);

        return Ok(result);
    }
    [HttpGet("getids")]
    public async Task<ActionResult<IEnumerable<IdDto>>> GetAllBrandsIds()
    {
        var result = await _mediator.Send(new GetAllBrandsIdQuery());
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }
    [HttpGet("getslugs")]
    public async Task<ActionResult<IEnumerable<SlugDto>>> GetAllBrandsSlugs()
    {
        var result = await _mediator.Send(new GetAllBrandsSlugsQuery());
        if (result.IsUnauthorized) return Unauthorized(result);
        return Ok(result);
    }
    [HttpGet("getproductbybrandid/{brandId}")]
    public async Task<ActionResult<IEnumerable<ProductCardDto>>> GetproductsByBrand(int brandId)
    {
        var result = await _mediator.Send(new GetProductsByBrandIdQuery(brandId));
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }
    [HttpGet("getproductscategoriesbybrandid/{brandId}")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetproductsCategoriesByBrand(int brandId)
    {
        var result = await _mediator.Send(new GetProductsCategoriesByBrandIdQuery(brandId));
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }
    [HttpGet("getProductsSuppliersByBrandId/{brandId}")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetProductsSuppliersByBrandIdQuery(int brandId)
    {
        var result = await _mediator.Send(new GetProductsSuppliersByBrandIdQuery(brandId));
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Create([FromForm] CreateBrandCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }

    

    [HttpPut]
    //[Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Update([FromForm] UpdateBrandCommand command)
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

    public async Task<ActionResult<IdDto>> ActiveBlog([FromBody] ActiveBrandCommand command)
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
        var command = new DeleteBrandCommand
        {
            Id = id,
        };
        var result = await _mediator.Send(command);

        if (result.IsUnauthorized)
            return Unauthorized(result);

        return Ok(result);
    }
}
