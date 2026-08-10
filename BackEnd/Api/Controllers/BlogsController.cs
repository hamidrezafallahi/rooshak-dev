using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
 


[ApiController]
[Route("api/[controller]")]
public class BlogsController : BaseController
{
    private readonly IMediator _mediator;

    public BlogsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpGet]
    public async Task<ActionResult<ListDto<BlogDto>>> GetBlogs([FromQuery] GetBlogsQuery query)
    {
        var result = await _mediator.Send(query);
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);
        return Ok(result);
    }
    [HttpGet("getslugs")]
    public async Task<ActionResult<IEnumerable<SlugDto>>> GetAllBlogsSlugs(
        [FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.Send(new GetAllBlogsSlugsQuery
        {
            IncludeInactive = includeInactive
        });
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);
        return Ok(result);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<BlogDto>> GetBlogBySlug(string slug)
    {
        var result = await _mediator.Send(new GetBlogBySlugQuery { Slug = slug });
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);
        return Ok(result);
    }
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BlogDto>> GetBlogById(int id)
    {
        var result = await _mediator.Send(new GetBlogByIdQuery { Id = id });
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);
        return Ok(result);
    }
    [HttpGet("selectOption")]
    public async Task<ActionResult<ListDto<SelectOptionDto>>> Get4selectOption([FromQuery] GetBlogs4selectOptionQuery query)
    {
        var result = await _mediator.Send(query);
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }

    [HttpPost("validate-content")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]
    public async Task<ActionResult<BlogContentQualityResultDto>> ValidateContent(
        [FromBody] BlogContentQualityRequestDto payload)
    {
        var result = await _mediator.Send(new ValidateBlogContentCommand { Payload = payload });
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> CreateBlog([FromForm] CreateBlogCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var result = await _mediator.Send(command);
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);
        return Ok(result);
    }



    [HttpPut]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> UpdateBlog([FromForm] UpdateBlogCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);
        return Ok(result);
    }

    [HttpPut("active")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> ActiveBlog([FromBody] ActiveBlogCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);

        return Ok(result);
    }



    [HttpDelete("{Id}")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> DeleteBlog(int Id)
    {
        var command = new DeleteBlogCommand
        {
            Id = Id
        };

        var result = await _mediator.Send(command);
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);
        return Ok(result);
    }
}
