using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AddressController : BaseController
{
    private readonly IMediator _mediator;

    public AddressController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpGet]
    //[Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<List<UserAddressDto>>> GetAll([FromQuery] GetAddressesQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserAddressDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetUserAddressByIdQuery { Id = id });
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }
    [HttpGet("ByUserId/{userId}")]
    public async Task<ActionResult<UserAddressDto>> GetByUserId(int userId)
    {
        var result = await _mediator.Send(new GetUserAddressByUserIdQuery { UserId = userId });
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }
    [HttpGet("ByUser")]
    public async Task<ActionResult<UserAddressDto>> GetUseraddress()
    {
        var result = await _mediator.Send(new GetUserAddressByUserQuery());
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }
    [HttpGet("default")]
    public async Task<ActionResult<UserAddressDto>> GetUserDefaultAddress()
    {
        var query = new GetDefaultUserAddressByUserIdQuery();
        var result = await _mediator.Send(query);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }

    // 📌 افزودن آدرس جدید
    [HttpPost]
    public async Task<ActionResult<IdDto>> AddAddress([FromBody] AddUserAddressCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }

    // 📌 ویرایش آدرس
    [HttpPut]
    public async Task<ActionResult<IdDto>> UpdateAddress([FromBody] UpdateUserAddressCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }
  
    

    // 📌 ست کردن به‌عنوان آدرس پیش‌فرض
    [HttpPut("set-default")]
    public async Task<ActionResult<IdDto>> SetDefaultAddress([FromBody] SetDefaultUserAddressCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);
    }

    [HttpPut("active")]
    public async Task<ActionResult<IdDto>> Active([FromBody] ActiveUserAddressCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<IdDto>> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteUserAddressCommand { Id = id });
        if (result.IsUnauthorized)
            return Unauthorized(result);
        return Ok(result);

    }
}
