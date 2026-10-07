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
    public class AnnouncementBarsController : BaseController
    {
        private const string AdminRoles = "SuperAdmin,Admin,ContentEditor";
        private readonly IMediator _mediator;

        public AnnouncementBarsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/announcementbars/current  (عمومی؛ نوار قابل نمایش همین لحظه یا Data = null)
        [HttpGet("current")]
        public async Task<ActionResult<AnnouncementBarDto>> GetCurrent()
        {
            var result = await _mediator.Send(new GetCurrentAnnouncementBarQuery());
            return Ok(result);
        }

        // GET: api/announcementbars
        // عمومی مثل بقیه‌ی لیست‌های پنل: صفحه‌ی ادمین لیست را سمت سرور و بدون توکن می‌گیرد
        // (getAll در فرانت)، پس Authorize اینجا باعث خطای «بارگذاری لیست ناموفق بود» می‌شد.
        // ویرایش/ایجاد/حذف همچنان فقط برای ادمین است.
        [HttpGet]
        public async Task<ActionResult<ListDto<AnnouncementBarDto>>> GetAll([FromQuery] GetAllAnnouncementBarsQuery query)
        {
            var result = await _mediator.Send(query);
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);

            return Ok(result);
        }

        // GET: api/announcementbars/5  (ادمین)
        [HttpGet("{id:int}")]
        [Authorize(Roles = AdminRoles)]
        public async Task<ActionResult<AnnouncementBarDto>> GetById([FromRoute] int id)
        {
            var result = await _mediator.Send(new GetAnnouncementBarByIdQuery { Id = id });
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = AdminRoles)]
        public async Task<ActionResult<IdDto>> Create([FromForm] CreateAnnouncementBarCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut]
        [Authorize(Roles = AdminRoles)]
        public async Task<ActionResult<IdDto>> Update([FromForm] UpdateAnnouncementBarCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mediator.Send(command);
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("active")]
        [Authorize(Roles = AdminRoles)]
        public async Task<ActionResult<IdDto>> Active([FromBody] ActiveAnnouncementBarCommand command)
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
            var result = await _mediator.Send(new DeleteAnnouncementBarCommand { Id = id });
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);

            return Ok(result);
        }
    }
}
