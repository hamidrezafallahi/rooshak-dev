using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("api/[controller]")]
public class ProductImagesController : BaseController
{
    private readonly IMediator _mediator;

    public ProductImagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductImageDto>>> GetAll([FromQuery] GetAllProductImagesQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<ProductImageDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetProductImagesByIdQuery { Id=id});
        if (result.IsUnauthorized) return Unauthorized(result);
        return Ok(result);
    }


    [HttpGet("productmainimage/{productId}")]
    public async Task<ActionResult<ProductImageDto>> GetMainImage(int productId)
    {
        var result = await _mediator.Send(new GetMainImageByProductIdQuery(productId));
        if (result.IsUnauthorized) return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<IdDto>> Add([FromForm] AddProductImageCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);

    }

    //[HttpPut]
    //public async Task<ActionResult<IdDto>> Update([FromForm] UpdateProductImageCommand command)
    //{
    //    var result = await _mediator.Send(command);
    //    if (result.IsUnauthorized)
    //        return Unauthorized(result);
    //    return Ok(result);

    //}
    [HttpPut("active")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Active([FromBody] ActiveProductImageCommand command)
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
        var result = await _mediator.Send(new DeleteProductImageCommand { Id = id });
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);

    }
}
