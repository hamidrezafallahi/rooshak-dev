using Api.Controllers;
using Application.Dtos;
using Application.Queries;
using Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminPaymentsController : BaseController
    {
        private readonly IMediator _mediator;

        public AdminPaymentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("payments")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<ServiceResult<ListDto<PaymentDto>>>> GetPayments([FromQuery] GetAllPaymentsQuery query)
        {
            var result = await _mediator.Send(query);
            if (!result.IsSuccess && result.Error == "Unauthorized")
                return Unauthorized(result);

            return Ok(result);
        }
    }
}
