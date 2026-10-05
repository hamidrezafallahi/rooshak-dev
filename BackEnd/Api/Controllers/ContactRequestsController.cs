using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    /// <summary>
    /// درخواست های ثبت شده از فرم عمومی سایت.
    /// بسته به سایت، این فرم «همکاری با ما» یا «مشاوره خرید» است.
    /// ثبت درخواست عمومی است؛ ویرایش و حذف فقط برای ادمین.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ContactRequestsController : BaseController
    {
        private const string StaffRoles = "SuperAdmin,Admin,Support";

        private readonly IMediator _mediator;

        public ContactRequestsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/contactRequests
        [HttpGet]
        public async Task<ActionResult<ListDto<ContactRequestDto>>> GetAll([FromQuery] GetAllContactRequestsQuery query)
        {
            var result = await _mediator.Send(query);
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);

            return Ok(result);
        }

        // GET: api/contactRequests/5
        [HttpGet("{id:int}")]
        [Authorize(Roles = StaffRoles)]
        public async Task<ActionResult<ContactRequestDto>> GetById([FromRoute] int id)
        {
            var result = await _mediator.Send(new GetContactRequestByIdQuery { Id = id });
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);

            return Ok(result);
        }

        // POST: api/contactRequests  (عمومی - بدون نیاز به ورود)
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<IdDto>> Create([FromBody] CreateContactRequestCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        // PUT: api/contactRequests
        [HttpPut]
        [Authorize(Roles = StaffRoles)]
        public async Task<ActionResult<IdDto>> Update([FromBody] UpdateContactRequestCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = StaffRoles)]
        public async Task<ActionResult<IdDto>> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteContactRequestCommand { Id = id });
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);

            return Ok(result);
        }
    }
}
