using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderItemsController : BaseController
    {
        private readonly IMediator _mediator;

        public OrderItemsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet]
        public async Task<ActionResult<ListDto<DisplayOrderItemDto>>> GetBlogs([FromQuery] GetOrderItemsQuery query)
        {
            var result = await _mediator.Send(query);
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }

        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<IEnumerable<OrderItemDto>>> GetItemsByOrderId(int orderId)
        {
            var result = await _mediator.Send(new GetItemsByOrderIdQuery { OrderId = orderId });
            if (result.IsUnauthorized) return Unauthorized(result);

            return Ok(result);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<OrderItemDto>> GetById(int id)
        {
            var result = await _mediator.Send(new GetOrderItemByIdQuery { Id = id });
            if (result.IsUnauthorized) return Unauthorized(result);

            return Ok(result);

        }

        // GET: api/orderitems/totalprice/5
        [HttpGet("totalprice/{orderId}")]
        public async Task<ActionResult<decimal>> GetTotalPrice(int orderId)
        {
            var result = await _mediator.Send(new CalculateTotalPriceQuery { OrderId= orderId });
            if (result.IsUnauthorized) return Unauthorized(result);

            return Ok(result);
        }

        // POST: api/orderitems/{orderId}/items
        [HttpPost("items")]
        public async Task<ActionResult<IdDto>> AddOrderItem( [FromBody] AddOrderItemCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }

        // PUT: api/orderitems/{orderId}/items
        [HttpPut("items")]
        public async Task<ActionResult<IdDto>> UpdateOrderItem([FromBody] UpdateOrderItemCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }
        [HttpPut("active")]
        public async Task<ActionResult<IdDto>> Active([FromBody] ActiveOrderItemCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }
        [HttpDelete("{Id}")]
        public async Task<ActionResult<IdDto>> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteOrderItemCommand { Id = id });
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }
       
    }
}
