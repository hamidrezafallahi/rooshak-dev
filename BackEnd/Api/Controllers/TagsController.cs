using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("api/[controller]")]
public class TagsController : BaseController
{
    private readonly IMediator _mediator;

    public TagsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    //[Authorize(Roles = "SuperAdmin,Admin,Customer")]
    public async Task<ActionResult<ListDto<TagDto>>> GetAll([FromQuery] GetTagsQuery query)
    {
        var result = await _mediator.Send(query);
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("selectOption")]
    public async Task<ActionResult<ListDto<SelectOptionDto>>> Get4SelectOption([FromQuery] GetTags4selectOptionQuery query)
    {
        var result = await _mediator.Send(query);
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }


    [HttpGet("{slug}")]

    public async Task<ActionResult<TagDto>> GetById(string slug)
    {
        var result = await _mediator.Send(new GetTagBySlugQuery { Slug= slug });
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("ByProductOfferId/{id}")]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetTagByProductId(int id)
    {
        var result = await _mediator.Send(new GetTagsByProductOfferIdQuery { ProductId = id });
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("getids")]
    public async Task<ActionResult<IEnumerable<IdDto>>> GetAllTagIds()
    {
        var result = await _mediator.Send(new GetAllTagIdsQuery());
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("getslugs")]
    public async Task<ActionResult<IEnumerable<SlugDto>>> GetAllTagsSlugs()
    {
        var result = await _mediator.Send(new GetAllTagsSlugsQuery());
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);
        return Ok(result);
    }

    /// <summary>Tags that currently hold priced products — powers the exhibition index.</summary>
    [HttpGet("families")]
    public async Task<ActionResult<IEnumerable<TagFamilyDto>>> GetFamilies()
    {
        var result = await _mediator.Send(new GetTagFamiliesQuery());
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }

    /// <summary>Public price list of one tag ("family") for the exhibition QR pages.</summary>
    [HttpGet("{idOrSlug}/pricelist")]
    public async Task<ActionResult<TagPriceListDto>> GetPriceList(string idOrSlug)
    {
        var result = await _mediator.Send(new GetTagPriceListQuery { IdOrSlug = idOrSlug });
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);
        if (!result.IsSuccess || result.Data is null) return NotFound(result);

        return Ok(result);
    }














    //[HttpGet("/getTagsByproductId/{productId}")]
    //public async Task<ActionResult<ProductTagDto>> GetById(int productId)
    //{
    //    var Tags = await _mediator.Send(new GetTagsByProductIdQuery(productId));
    //    if (Tags == null)
    //        return NotFound();

    //    return Ok(Tags);
    //}

    //[HttpGet("getProductsByTagId/{tagId}")]
    //public async Task<ActionResult<IEnumerable<ProductTagDto>>> GetByProductId(int tagId)
    //{
    //    var products = await _mediator.Send(new GetProductSByTagIdQuery(tagId));
    //    return Ok(products);
    //}




    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Create(CreateTagCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);
        return Ok(result);
    }


    [HttpPut]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Update(UpdateTagCommand command)
    {

        var result = await _mediator.Send(command);
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);
        return Ok(result);
    }
    [HttpPut("active")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Active([FromBody] ActiveTagCommand command)
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
        var result = await _mediator.Send(new DeleteTagCommand { Id = id });
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);
        return Ok(result);

    }
    



}
