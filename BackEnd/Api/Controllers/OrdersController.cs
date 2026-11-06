using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : BaseController
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/orders/user/5
        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<OrderReadModel>>> GetOrdersByUserId()
        {
            var query = new GetOrdersByUserIdQuery();
            var result = await _mediator.Send(query);
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }

        // GET: api/orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderReadModel>> GetOrderWithItems(int id)
        {
            var result = await _mediator.Send(new GetOrderWithItemsQuery(id));
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }

        // GET: api/orders/open
        [HttpGet("open")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOpenOrders()
        {
            var result = await _mediator.Send(new GetOpenOrdersQuery());
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }
        [HttpPost("CheckoutCart")]
        public async Task<ActionResult<OrderIdDto>> handler(CheckoutCartCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }
       
        // POST: api/orders/confirm
        [HttpPost("confirm")]
        public async Task<ActionResult<IdDto>> ConfirmOrder([FromBody] ConfirmOrderCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }

        // POST: api/orders/pay
        [HttpPost("pay")]
        public async Task<ActionResult<IdDto>> PayOrder([FromBody] PayOrderCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }

        // POST: api/orders/cancel
        [HttpPost("cancel")]
        public async Task<ActionResult<IdDto>> CancelOrder([FromBody] CancelOrderCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }

      

        // POST: api/orders/ship
        [HttpPost("ship")]
        public async Task<ActionResult<IdDto>> ShipOrder([FromBody] ShipOrderCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }

        // POST: api/orders/deliver
        [HttpPost("deliver")]
        public async Task<ActionResult<IdDto>> DeliverOrder([FromBody] DeliverOrderCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }
 
        [HttpPost("apply-discount")]
        public async Task<ActionResult<IdDto>> ApplyDiscountCode([FromBody] ApplyDiscountCodeCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPost("remove-discount")]
        public async Task<ActionResult<IdDto>> RemoveDiscountCode( [FromBody] RemoveDiscountCodeCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }
        [HttpPut("items")]
        public async Task<ActionResult<IdDto>> AddOrderItem([FromBody] AddOrderItemCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPut("active")]
        public async Task<ActionResult<IdDto>> Active([FromBody] ActiveOrderCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }
        [HttpDelete("deleteByjob")]
        public async Task<ActionResult<IdDto>> DeleteOrderAfterTime(int orderId)
        {
            var command = new DeleteOrderCommand { Id = orderId };
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }

        [HttpDelete("{orderId}/items/{productId}")]
        public async Task<ActionResult<IdDto>> RemoveOrderItem(int orderId, int productId)
        {
            var command = new RemoveOrderItemCommand
            {
                OrderId = orderId,
                ProductId = productId,
            };

            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }
        [HttpDelete("{Id}")]
        public async Task<ActionResult<IdDto>> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteOrderCommand { Id = id });
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }

    }
}
