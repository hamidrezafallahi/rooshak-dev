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
    public class PaymentsController : BaseController
    {
        private readonly IMediator _mediator;
        public PaymentsController(IMediator mediator) => _mediator = mediator;


        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]

        public async Task<ActionResult<ListDto<PaymentDto>>> GetAll([FromQuery] GetAllPaymentsQuery query)
        {
            var result = await _mediator.Send(query);
            if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]

        public async Task<ActionResult> GetPaymentById(int id)
        {
            var result = await _mediator.Send(new GetPaymentByIdQuery { Id=id});
            if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

            return Ok(result);
        }

        [HttpGet("order/{orderId}")]
        [Authorize(Roles = "SuperAdmin,Admin")]

        public async Task<ActionResult> GetPaymentsByOrderId(int orderId)
        {
            var result = await _mediator.Send(new GetPaymentsByOrderIdQuery { OrderId=orderId});
            if (!result.IsSuccess && result.Error == "Unauthorized") return Unauthorized(result);

            return Ok(result);
        }

        [HttpPost]

        public async Task<ActionResult> CreatePayment([FromBody] CreatePaymentCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);
            return Ok(result);
        }

        [HttpPost("request")]
        [Authorize]
        public async Task<ActionResult<PaymentStartDto>> StartPayment([FromBody] RequestPaymentCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);
            return Ok(result);
        }

        [HttpPost("verify")]
        [Authorize]
        public async Task<ActionResult<PaymentVerifyResponseDto>> VerifyPayment([FromBody] VerifyPaymentCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);
            return Ok(result);
        }

        [HttpPut("markaspaid")]
        public async Task<ActionResult> MarkAsPaid([FromBody] MarkPaymentAsPaidCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);
            return Ok(result);
        }

        [HttpPut("markasfailed")]
        public async Task<ActionResult> MarkAsFailed([FromBody] MarkPaymentAsFailedCommand command )
        {
            var result = await _mediator.Send(command);
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);
            return Ok(result);
        }

        [HttpPut("cancel")]
        public async Task<ActionResult> CancelPayment([FromBody]  CancelPaymentCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);
            return Ok(result);
        }
        [HttpPut("active")]
        public async Task<ActionResult<IdDto>> Active([FromBody] ActivePaymentCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mediator.Send(command);
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);

            return Ok(result);
        }

       

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePayment(int id)
        {
            var result = await _mediator.Send(new DeletePaymentCommand
            {
                PaymentId = id,
            });
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);
            return Ok(result);
        }

       
    }
}
