
using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.Commands.IdentityManagerCommands;

[ApiController]
[Route("api/[controller]")]
public class IdentityController : BaseController
{
    private readonly IMediator _mediator;

    public IdentityController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ===== Public Actions =====
    [HttpPost("Register")]
    [AllowAnonymous]
    public async Task<ActionResult<IdDto>> Register([FromForm] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginDto>> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

   [HttpPost("refresh-token")]
public async Task<IActionResult> RefreshToken(
    [FromBody] RefreshTokenCommand? request)
{
    var command = new RefreshTokenCommand
    {
        AccessToken = request?.AccessToken ?? Request.Cookies["candyAccess"] ?? "",
        RefreshToken = request?.RefreshToken ?? Request.Cookies["candyRefresh"] ?? "",
        Ip = request?.Ip,
        UserAgent = request?.UserAgent
    };

    var result = await _mediator.Send(command);

    return Ok(result);
}

    [HttpPut("changepasswordbyadmin")]
    [Authorize(Roles = "SuperAdmin,Admin")]

    public async Task<ActionResult<bool>> ChangePassword([FromBody] ChangeUserPasswordByAdminCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }

    [HttpPut("changepassword")]
    public async Task<ActionResult<bool>> ChangePassword([FromBody] ChangeUserPasswordCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }

    [HttpPut("role")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<bool>> ChangeRole([FromBody] SetUserRoleCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
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
        if (result.IsUnauthorized)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]
    //[Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IdDto>> Delete(int id)
    {
        var command = new DeleteUserCommand { Id = id };
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }
   




}
