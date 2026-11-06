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
    public class ShippingMethodsController : BaseController
    {
        private readonly IMediator _mediator;

        public ShippingMethodsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/shippingmethods
        [HttpGet]
        public async Task<ActionResult<ListDto<ShippingMethodDto>>> GetAll([FromQuery] GetAllShippingMethodsQuery query)
        {
            var result = await _mediator.Send(query);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }

        

        // GET: api/shippingmethods/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ShippingMethodDto>> GetByCode([FromRoute] int id)
        {
            var result = await _mediator.Send(new GetShippingMethodByIdQuery
            {
                Id = id
            });
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }

        // POST: api/shippingmethods
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

        public async Task<ActionResult<IdDto>> Create([FromBody] CreateShippingMethodCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }

        // PUT: api/shippingmethods/5
        [HttpPut]
        [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

        public async Task<ActionResult<IdDto>> Update([FromBody] UpdateShippingMethodCommand command)
        {

            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }
       
        [HttpPut("set-default")]
        public async Task<ActionResult<IdDto>> SetDefaultShippingMethod(int shippingMethodId)
        {
            var command = new SetDefaultShippingMethodCommand
            {
                ShippingMethodId = shippingMethodId
            };
            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);
        }
        [HttpPut("active")]
        [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

        public async Task<ActionResult<IdDto>> Active([FromBody] ActiveShippingMethodCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mediator.Send(command);
            if (result.IsUnauthorized)
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]

        public async Task<ActionResult<IdDto>> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteShippingMethodCommand { Id = id });
            if (result.IsUnauthorized)
                return Unauthorized(result);
            return Ok(result);

        }
    }
}
