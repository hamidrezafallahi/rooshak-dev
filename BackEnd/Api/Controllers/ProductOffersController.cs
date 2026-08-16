using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProductOffersController : BaseController
{
    private readonly IMediator _mediator;

    public ProductOffersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ================= Queries =================
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll([FromQuery] GetProductOffersQuery query)
    {
        var result = await _mediator.Send(query);
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }


    [HttpGet("selectOption")]
    public async Task<ActionResult<ListDto<SelectOptionDto>>> Get4SelectOption([FromQuery] GetProductOffers4selectOptionQuery query)
    {
        var result = await _mediator.Send(query);
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }



    [HttpGet("suppliers")]

    public async Task<ActionResult<List<BrandDto>>> GetAllSuppliers([FromQuery] GetSuppliersQuery query)
    {
        var result = await _mediator.Send(query);
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }
    [HttpGet("suppliersIds")]
    public async Task<ActionResult<IEnumerable<IdDto>>> GetAllSupplierIds()
    {
        var result = await _mediator.Send(new GetSuppliersIdsQuery());
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }



    [HttpGet("by-product/{productId}")]
    public async Task<ActionResult<List<ProductOfferDto>>> GetByProduct(int productId)
    {
        var result = await _mediator.Send(
            new GetProductOffersByProductIdQuery { ProductId=productId }
        );
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("by-seller/{sellerId}")]
    public async Task<ActionResult<List<ProductOfferDto>>> GetMyOffers(int sellerId)
    {
        var result = await _mediator.Send(
            new GetProductOffersBySellerIdQuery { SellerId=sellerId }
        );
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductOfferDetailDto>> GetById(int id)
    {
        var result = await _mediator.Send(
            new GetProductOfferByIdQuery { Id= id }
        );
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("getSuppliersByCategoryId")]
    public async Task<ActionResult<SupplierListDto>> GetSuppliersByCategoryId([FromQuery] GetSuppliersByCategoryIdQuery query  )
    {
        var result = await _mediator.Send(query);
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }

    // ================= Commands =================

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Create(CreateProductOfferCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);


        return Ok(result);
    }

    [HttpPut]
    [Authorize(Policy = Application.Common.CatalogApiKey.PolicyName)]
    public async Task<ActionResult<IdDto>> Update([FromBody] UpdateProductOfferCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);

        return Ok(result);
    }
    [HttpPut("active")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Active([FromBody] ActiveProductOfferCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteProductOfferCommand { Id = id });
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);
        return Ok(result);

    }

    
}
