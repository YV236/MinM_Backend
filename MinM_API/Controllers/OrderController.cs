using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinM_API.Dtos;
using MinM_API.Dtos.Order;
using MinM_API.Services.Interfaces;

namespace MinM_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController(IOrderService orderService) : ControllerBase
    {
        [HttpPost]
        [Route("Create")]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> CreateOrder(AddOrderDto addOrderDto)
        {
            var response = await orderService.CreateOrder(addOrderDto, User);

            return StatusCode((int)response.StatusCode, response);
        }
    }
}
