using Azure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinM_API.Dtos;
using MinM_API.Dtos.Order;
using MinM_API.Extension;
using MinM_API.Models;
using MinM_API.Services.Interfaces;
using System.Security.Claims;

namespace MinM_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController(IOrderService orderService) : ControllerBase
    {
        [HttpPost]
        [Route("create-authenticated")]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> CreateOrder(AddOrderDto addOrderDto)
        {

            var response = await orderService.CreateOrder(addOrderDto, User);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost]
        [Route("create-guest")]
        public async Task<ActionResult<ServiceResponse<int>>> CreateUnauthorizedOrder(AddOrderDto addOrderDto)
        {
            var response = await orderService.CreateUnauthorizedOrder(addOrderDto);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [Route("cancel/{orderId}")]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> CancelOrder([FromRoute] string orderId)
        {
            var response = await orderService.CancelOrder(User, orderId);

            return StatusCode((int)response.StatusCode, response);
        }


        [HttpPut]
        [Route("paid/{orderId}")]
        public async Task<ActionResult<ServiceResponse<int>>> SetOrderAsPaid([FromRoute] string orderId)
        {
            var response = await orderService.SetOrderAsPaid(orderId);

            return StatusCode((int)response.StatusCode, response);
        }


        [HttpPut]
        [Route("change/{orderId}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> ChangeOrderStatus([FromRoute] string orderId, [FromBody] Status status)
        {
            var response = await orderService.ChangeOrderStatus(orderId, status);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [Route("all")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<List<OrderDto>>>> GetAllOrders()
        {
            var response = await orderService.GetAllOrders();

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [Route("my")]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<List<OrderDto>>>> GetAllUserOrders()
        {
            var response = await orderService.GetAllUserOrders(User);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [Route("my/{orderId}")]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<List<OrderDto>>>> GetUserOrder([FromRoute] string orderId)
        {
            var response = await orderService.GetUserOrder(orderId);

            return StatusCode((int)response.StatusCode, response);
        }
    }
}
