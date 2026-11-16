using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    /// <summary>اسلایدر میانی صفحه‌ی اصلی؛ کاملاً جدا از بنر هیرو (api/landing).</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HomeSlidesController(IMediator _mediator) : BaseController
    {
        private const string AdminRoles = "SuperAdmin,Admin,ContentEditor";

        // GET: api/homeslides/public  (عمومی؛ اسلایدهای فعال به ترتیب)
        [HttpGet("public")]
        public async Task<ActionResult<List<HomeSlideDto>>> GetPublic() =>
            Ok(await _mediator.Send(new GetPublicHomeSlidesQuery()));

        // GET: api/homeslides  (لیست پنل؛ مثل بقیه‌ی لیست‌ها بدون توکن از سمت سرور گرفته می‌شود)
        [HttpGet]
        public async Task<ActionResult<ListDto<HomeSlideDto>>> GetAll([FromQuery] GetAllHomeSlidesQuery query)
        {
            var result = await _mediator.Send(query);
            if (result.IsUnauthorized) return Unauthorized(result);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<HomeSlideDto>> GetById([FromRoute] int id) =>
            Ok(await _mediator.Send(new GetHomeSlideByIdQuery { Id = id }));

        [HttpPost]
        [Authorize(Roles = AdminRoles)]
        public async Task<ActionResult<IdDto>> Create([FromForm] CreateHomeSlideCommand command) =>
            Respond(await _mediator.Send(command));

        [HttpPut]
        [Authorize(Roles = AdminRoles)]
        public async Task<ActionResult<IdDto>> Update([FromForm] UpdateHomeSlideCommand command) =>
            Respond(await _mediator.Send(command));

        [HttpPut("active")]
        [Authorize(Roles = AdminRoles)]
        public async Task<ActionResult<IdDto>> Active([FromBody] ActiveHomeSlideCommand command) =>
            Respond(await _mediator.Send(command));

        [HttpDelete("{id:int}")]
        [Authorize(Roles = AdminRoles)]
        public async Task<ActionResult<IdDto>> Delete(int id) =>
            Respond(await _mediator.Send(new DeleteHomeSlideCommand { Id = id }));

        private ActionResult<IdDto> Respond(Common.ServiceResult<IdDto> result)
        {
            if (result.IsUnauthorized) return Unauthorized(result);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }
    }

    /// <summary>متن‌های دوزبانه‌ی صفحه‌ی درباره ما / داستان ما.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AboutSectionsController(IMediator _mediator) : BaseController
    {
        private const string AdminRoles = "SuperAdmin,Admin,ContentEditor";

        [HttpGet("public")]
        public async Task<ActionResult<List<AboutSectionDto>>> GetPublic() =>
            Ok(await _mediator.Send(new GetPublicAboutSectionsQuery()));

        [HttpGet]
        public async Task<ActionResult<ListDto<AboutSectionDto>>> GetAll([FromQuery] GetAllAboutSectionsQuery query)
        {
            var result = await _mediator.Send(query);
            if (result.IsUnauthorized) return Unauthorized(result);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AboutSectionDto>> GetById([FromRoute] int id) =>
            Ok(await _mediator.Send(new GetAboutSectionByIdQuery { Id = id }));

        [HttpPut]
        [Authorize(Roles = AdminRoles)]
        public async Task<ActionResult<IdDto>> Update([FromBody] UpdateAboutSectionCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized) return Unauthorized(result);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("active")]
        [Authorize(Roles = AdminRoles)]
        public async Task<ActionResult<IdDto>> Active([FromBody] ActiveAboutSectionCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized) return Unauthorized(result);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }
    }

    /// <summary>فونت‌های آپلودی ادمین (فایل در uploads/fonts، مشخصات در جدول CustomFonts).</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CustomFontsController(IMediator _mediator) : BaseController
    {
        private const string AdminRoles = "SuperAdmin,Admin,ContentEditor";

        // GET: api/customfonts/public  (عمومی؛ فرانت در زمان build و هر ساعت می‌گیرد و @font-face می‌سازد)
        [HttpGet("public")]
        public async Task<ActionResult<List<CustomFontDto>>> GetPublic() =>
            Ok(await _mediator.Send(new GetPublicCustomFontsQuery()));

        [HttpGet]
        public async Task<ActionResult<ListDto<CustomFontDto>>> GetAll([FromQuery] GetAllCustomFontsQuery query)
        {
            var result = await _mediator.Send(query);
            if (result.IsUnauthorized) return Unauthorized(result);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CustomFontDto>> GetById([FromRoute] int id) =>
            Ok(await _mediator.Send(new GetCustomFontByIdQuery { Id = id }));

        [HttpPost]
        [Authorize(Roles = AdminRoles)]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<ActionResult<IdDto>> Create([FromForm] CreateCustomFontCommand command) =>
            Respond(await _mediator.Send(command));

        [HttpPut]
        [Authorize(Roles = AdminRoles)]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<ActionResult<IdDto>> Update([FromForm] UpdateCustomFontCommand command) =>
            Respond(await _mediator.Send(command));

        [HttpPut("active")]
        [Authorize(Roles = AdminRoles)]
        public async Task<ActionResult<IdDto>> Active([FromBody] ActiveCustomFontCommand command) =>
            Respond(await _mediator.Send(command));

        [HttpDelete("{id:int}")]
        [Authorize(Roles = AdminRoles)]
        public async Task<ActionResult<IdDto>> Delete(int id) =>
            Respond(await _mediator.Send(new DeleteCustomFontCommand { Id = id }));

        private ActionResult<IdDto> Respond(Common.ServiceResult<IdDto> result)
        {
            if (result.IsUnauthorized) return Unauthorized(result);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }
    }
}
