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
    public class FaqsController : BaseController
    {
        private readonly IMediator _mediator;

        public FaqsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/faqs  (عمومی)
        [HttpGet]
        public async Task<ActionResult<ListDto<FaqDto>>> GetAll([FromQuery] GetAllFaqsQuery query)
        {
            var result = await _mediator.Send(query);
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);

            return Ok(result);
        }

        // GET: api/faqs/5  (عمومی)
        [HttpGet("{id:int}")]
        public async Task<ActionResult<FaqDto>> GetById([FromRoute] int id)
        {
            var result = await _mediator.Send(new GetFaqByIdQuery { Id = id });
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);

            return Ok(result);
        }

        // POST: api/faqs
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]
        public async Task<ActionResult<IdDto>> Create([FromBody] CreateFaqCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);

            return Ok(result);
        }

        // PUT: api/faqs
        [HttpPut]
        [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]
        public async Task<ActionResult<IdDto>> Update([FromBody] UpdateFaqCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPut("active")]
        [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]
        public async Task<ActionResult<IdDto>> Active([FromBody] ActiveFaqCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mediator.Send(command);
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]
        public async Task<ActionResult<IdDto>> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteFaqCommand { Id = id });
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);

            return Ok(result);
        }
    }
}
