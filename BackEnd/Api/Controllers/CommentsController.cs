using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : BaseController
{
    private readonly IMediator _mediator;

    public CommentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetAll([FromQuery] GetAllCommentsQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }
    // ===== 1. گرفتن کامنت‌ها برای یک Target =====
    [HttpGet("{targetType}/{targetId}")]
    public async Task<ActionResult<List<CommentDto>>> GetComments(string targetType, int targetId)
    {
        if (!Enum.TryParse<EnumTargetType>(targetType, true, out var targetEnum))
            return BadRequest("نوع مورد انتخاب‌شده برای نظر معتبر نیست.");

        var query = new GetCommentsByTargetQuery
        {
            TargetType = targetEnum,
            TargetId = targetId
        };

        var result = await _mediator.Send(query);
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }
  // ===== 1. گرفتن کامنت‌ها برای یک Target =====

[HttpGet("{id}")]
    public async Task<ActionResult<CommentDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetCommentByIdQuery { Id = id });
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
    }
    // ===== 1. ایجاد کامنت =====
    [HttpPost]
    public async Task<ActionResult<IdDto>> CreateComment([FromBody] CreateCommentCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);

        if (result.IsUnauthorized)
            return Unauthorized(result);

        return Ok(result);
    }

    // ===== 2. پاسخ به یک کامنت (اختیاری) =====
    [HttpPost("reply")]
    public async Task<ActionResult<IdDto>> ReplyComment([FromBody] ReplyCommentCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);

        if (result.IsUnauthorized)
            return Unauthorized(result);

        return Ok(result);
    }

    // ===== 3. تایید کامنت =====
    [HttpPut("approve")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> ApproveComment([FromBody] ApproveCommentCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);

        if (result.IsUnauthorized)
            return Unauthorized(result);

        return Ok(result);
    }

    // ===== 4. ویرایش کامنت =====
    [HttpPut]
    public async Task<ActionResult<IdDto>> EditComment(  [FromBody] EditCommentCommand command)
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

    public async Task<ActionResult<IdDto>> Active([FromBody] ActiveCommentCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);

        return Ok(result);
    }
    [HttpDelete("{Id}")]
    public async Task<ActionResult<IdDto>> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteCommentCommand { Id = id });
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }
 
 
    

  
}
