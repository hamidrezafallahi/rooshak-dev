using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
    public class CartItemsController : BaseController
    {
        private readonly IMediator _mediator;

        public CartItemsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet]
        public async Task<ActionResult<List<CartItemDto>>> GetAll([FromQuery] GetAllCartItemsQuery query)
        {
            var result = await _mediator.Send(query);
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
        }

        // GET: api/cartitems/cart/{cartId}
        [HttpGet("cart/{cartId}")]
        public async Task<ActionResult<IEnumerable<CartItemDto>>> GetItemsByCartId([FromQuery] GetCartItemsByCartIdQuery query)
        {

            var result = await _mediator.Send(query);
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);

        }

        // GET: api/cartitems/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CartItemDto>> GetById(int id)
        {
            var result = await _mediator.Send(new GetCartItemByIdQuery { Id = id });
        if (result.IsUnauthorized) return Unauthorized(result);

        return Ok(result);
 
        }

      

        [HttpPost]

        public async Task<ActionResult<CartItemDto>> AddOrUpdateItem([FromBody] AddOrUpdateCartItemCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }





        [HttpPost("decrease")]
        public async Task<ActionResult> DecreaseItem([FromBody] DecreaseCartItemCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }
    [HttpPut]
    public async Task<ActionResult<IdDto>> Active([FromBody] UpdateCartItemCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);
        if (result.IsUnauthorized)
            return Unauthorized(result);

        return Ok(result);
    }


    // DELETE: api/cartitems/{id}
    [HttpDelete("removeproductfromcart/{id}")]
        public async Task<ActionResult> DecreaseItem(int id)
        {
            var result = await _mediator.Send(new DeleteCartItemCommand { Id = id });
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPut("active")]
 
    public async Task<ActionResult<IdDto>> Active([FromBody] ActiveCartItemCommand command)
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
            var result = await _mediator.Send(new DeleteCartItemCommand { Id = id });
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }
    }
