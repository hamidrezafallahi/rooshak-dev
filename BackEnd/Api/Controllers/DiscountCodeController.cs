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
    public class DiscountCodesController : BaseController
    {
        private readonly IMediator _mediator;

        public DiscountCodesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/discountcodes
        [HttpGet]
        public async Task<ActionResult<ListDto<DiscountCodeDto>>> GetAll([FromQuery] GetAllDiscountCodesQuery query)
        {
            var result = await _mediator.Send(query);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<DiscountCodeDto>> GetById(int id)
        {
            var result = await _mediator.Send(new GetDiscountCodeByIdQuery { Id = id });
            if (result.IsUnauthorized) return Unauthorized(result);

            return Ok(result);
        }

        // GET: api/discountcodes/getcode/5
        [HttpGet("getcode/{code}")]
        public async Task<ActionResult<DiscountCodeDto?>> GetByCode([FromRoute] string code)
        {
            var result = await _mediator.Send(new GetDiscountCodeByCodeQuery
            {
                Code = code
            });
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }

        // POST: api/discountcodes
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

        public async Task<ActionResult<IdDto>> Create([FromBody] CreateDiscountCodeCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }

        // PUT: api/discountcodes/5
        [HttpPut]
        [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

        public async Task<ActionResult<IdDto>> Update([FromBody] UpdateDiscountCodeCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }
        [HttpPut("active")]
        [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

        public async Task<ActionResult<IdDto>> Active([FromBody] ActiveDiscountCodeCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }
        [HttpDelete("{Id}")]
        [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

        public async Task<ActionResult<IdDto>> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteDiscountCodeCommand { Id = id });
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }

         
    }
}
