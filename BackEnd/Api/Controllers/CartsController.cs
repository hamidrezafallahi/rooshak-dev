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
[Authorize]
    public class CartsController : BaseController
    {
        private readonly IMediator _mediator;

        public CartsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<CartDto>>> GetAll([FromQuery] GetAllCartsQuery query)
        {
            var result = await _mediator.Send(query);
            if (result.IsUnauthorized) return Unauthorized(result);

            return Ok(result);
        }

        // GET: api/carts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CartDto>> GetById(int id)
        {
            var result = await _mediator.Send(new GetCartByIdQuery { Id = id });
            if (result.IsUnauthorized) return Unauthorized(result);

            return Ok(result);

        }

        [HttpGet("detailcart/{id}")]
        public async Task<ActionResult<CartDto>> GetDetailCartById([FromQuery] GetDetailCartByIdQuery query)
        {
          

            var result = await _mediator.Send(query);
            if (result.IsUnauthorized) return Unauthorized(result);

            return Ok(result);
        }


        [HttpGet("byUserId")]
        //[Authorize(Roles ="Admin")]
        public async Task<ActionResult<IEnumerable<CartDto>>> GetCartsByUserId([FromQuery] GetCartsByUserIdQuery query)
        {
 
            var result = await _mediator.Send(query);
            if (result.IsUnauthorized) return Unauthorized(result);

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateCart([FromBody] CreateCartCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }


        [HttpPost("sync")]
        public async Task<ActionResult> SyncCart([FromBody] SyncCartCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPost("clearcart")]
        public async Task<ActionResult> ClearCart([FromBody] ClearCartCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }



        // PUT: api/carts/{id}
        [HttpPut]
        public async Task<ActionResult<bool>> UpdateCart(UpdateCartCommand command)
        {
 

            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPut("active")]
        public async Task<ActionResult<IdDto>> Active([FromBody] ActiveCartCommand command)
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
            var result = await _mediator.Send(new DeleteCartCommand { Id = id });
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }
        
    }
}
