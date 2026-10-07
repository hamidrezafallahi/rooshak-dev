using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThemeSettingsController : BaseController
    {
        private const string AdminRoles = "SuperAdmin,Admin,ContentEditor";
        private readonly IMediator _mediator;

        public ThemeSettingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/themesettings/active  (عمومی؛ فرانت رنگ‌های سایت را از اینجا می‌گیرد)
        [HttpGet("active")]
        public async Task<ActionResult<ThemeSettingDto>> GetActive()
        {
            var result = await _mediator.Send(new GetActiveThemeSettingQuery());
            return Ok(result);
        }

        // GET: api/themesettings
        // عمومی مثل بقیه‌ی لیست‌های پنل: صفحه‌ی ادمین لیست را سمت سرور و بدون توکن می‌گیرد
        // (getAll در فرانت)، پس Authorize اینجا باعث خطای «بارگذاری لیست ناموفق بود» می‌شد.
        // ویرایش/ایجاد/حذف همچنان فقط برای ادمین است.
        [HttpGet]
        public async Task<ActionResult<ListDto<ThemeSettingDto>>> GetAll([FromQuery] GetAllThemeSettingsQuery query)
        {
            var result = await _mediator.Send(query);
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);

            return Ok(result);
        }

        // GET: api/themesettings/5  (ادمین)
        [HttpGet("{id:int}")]
        [Authorize(Roles = AdminRoles)]
        public async Task<ActionResult<ThemeSettingDto>> GetById([FromRoute] int id)
        {
            var result = await _mediator.Send(new GetThemeSettingByIdQuery { Id = id });
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = AdminRoles)]
        public async Task<ActionResult<IdDto>> Create([FromBody] CreateThemeSettingCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPut]
        [Authorize(Roles = AdminRoles)]
        public async Task<ActionResult<IdDto>> Update([FromBody] UpdateThemeSettingCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPut("active")]
        [Authorize(Roles = AdminRoles)]
        public async Task<ActionResult<IdDto>> Active([FromBody] ActiveThemeSettingCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mediator.Send(command);
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = AdminRoles)]
        public async Task<ActionResult<IdDto>> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteThemeSettingCommand { Id = id });
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);

            return Ok(result);
        }
    }
}
