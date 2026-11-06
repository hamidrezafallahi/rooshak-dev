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
    public class PaymentMethodsController : BaseController
    {
        private readonly IMediator _mediator;

        public PaymentMethodsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/paymentmethods
        [HttpGet]
        public async Task<ActionResult<ListDto<PaymentMethodDto>>> GetAll([FromQuery] GetAllPaymentMethodsQuery query)
        {
            var result = await _mediator.Send(query);
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }
        [HttpGet("selectOption")]
        public async Task<ActionResult<ListDto<SelectOptionDto>>> Get4selectOption([FromQuery] GetPaymentMethods4selectOptionQuery query)
        {
            var result = await _mediator.Send(query);
            if (result.IsUnauthorized) return Unauthorized(result);

            return Ok(result);
        }

        // GET: api/paymentmethods/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentMethodDto>> GetById([FromRoute] int id)
        {
            var result = await _mediator.Send(new GetPaymentMethodByIdQuery
            {
                Id = id
            });
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }

        // POST: api/paymentmethods
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

        public async Task<ActionResult<IdDto>> Create([FromBody] CreatePaymentMethodCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }

        // PUT: api/paymentmethods/5
        [HttpPut]
        [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

        public async Task<ActionResult<IdDto>> Update([FromBody] UpdatePaymentMethodCommand command)
        {
 

            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }

        [HttpPut("active")]
        [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

        public async Task<ActionResult<IdDto>> Active([FromBody] ActivePaymentMethodCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }

        // DELETE: api/paymentmethods/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

        public async Task<ActionResult<IdDto>> Delete(int id)
        {
            var command = new DeletePaymentMethodCommand
            {
                Id = id
            };

            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }
    }
}
