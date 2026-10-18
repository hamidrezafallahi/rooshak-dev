using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.IdentityManagerCommands;
[ApiController]
[Route("api/[controller]")]
public class UsersController : BaseController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ===== Read Actions =====
    [HttpGet("{idOrSlug}")]
    public async Task<ActionResult<UserDto>> GetByIdOrSlug(string idOrSlug)
    {
        var result = await _mediator.Send(new GetUserByIdQuery { IdOrSlug = idOrSlug });
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);
        if (!result.IsSuccess || result.Data is null) return NotFound(result);

        // Public supplier pages read this endpoint anonymously; contact details are staff-only.
        if (!(User.IsInRole("SuperAdmin") || User.IsInRole("Admin") || User.IsInRole("ContentEditor")))
        {
            result.Data.Email = null!;
            result.Data.PhoneNumber = null!;
        }

        return Ok(result);
    }

    [HttpGet("getslugs")]
    public async Task<ActionResult<IEnumerable<SlugDto>>> GetAllUsersSlugs()
    {
        var result = await _mediator.Send(new GetAllUsersSlugsQuery());
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]
    public async Task<ActionResult<List<UserDto>>> GetAll([FromQuery] GetUsersQuery query)
    {
        var result = await _mediator.Send(query);
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }
    [HttpGet("selectOption")]
    public async Task<ActionResult<ListDto<SelectOptionDto>>> GetParent4selectOption([FromQuery] GetUsers4selectOptionQuery query)
    {
        var result = await _mediator.Send(query);
        if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

        return Ok(result);
    }

    // ===== Update Actions =====
    [HttpPut("profile")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<UserDto>> UpdateProfile(UpdateUserProfileCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);
        return Ok(result);
    }
    [HttpPut("active")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

    public async Task<ActionResult<IdDto>> Active([FromBody] ActiveUserCommand command)
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
        var result = await _mediator.Send(new DeleteUserCommand { Id = id });
        if (!result.IsSuccess && result.Error == "Unauthorized")
            return Unauthorized(result);
        return Ok(result);

    }
}
