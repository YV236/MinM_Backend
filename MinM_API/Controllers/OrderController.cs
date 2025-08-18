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
        public async Task<ActionResult<ServiceResponse<int>>> CanceleOrder([FromRoute] string orderId)
        {
            var respone = await orderService.CancelOrder(User, orderId);

            return StatusCode((int)respone.StatusCode, respone);
        }


        [HttpPut]
        [Route("paid/{orderId}")]
        public async Task<ActionResult<ServiceResponse<int>>> SetOrderAsPaid([FromRoute] string orderId)
        {
            var respone = await orderService.SetOrderAsPaid(orderId);

            return StatusCode((int)respone.StatusCode, respone);
        }


        [HttpPut]
        [Route("cancel/{orderId}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> ChangeOrderStatus([FromRoute] string orderId, [FromBody] Status status)
        {
            var respone = await orderService.ChangeOrderStatus(orderId, status);

            return StatusCode((int)respone.StatusCode, respone);
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
            var reposne = await orderService.GetAllUserOrders(User);

            return StatusCode((int)reposne.StatusCode, reposne);
        }

        [HttpGet]
        [Route("my/{orderId}")]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<List<OrderDto>>>> GetUserOrders([FromRoute] string orderId)
        {
            var reposne = await orderService.GetUserOrders(User, orderId);

            return StatusCode((int)reposne.StatusCode, reposne);
        }
    }
}
