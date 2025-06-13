using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinM_API.Dtos;
using MinM_API.Dtos.Discount;
using MinM_API.Services.Interfaces;

namespace MinM_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiscountController(IDiscountService discountService) : ControllerBase
    {
        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> AddDiscount(AddDiscountDto discountDto)
        {
            var response = await discountService.AddDiscount(discountDto);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [Route("Update")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> UpdateDiscount(UpdateDiscountDto discountDto)
        {
            var response = await discountService.UpdateDiscount(discountDto);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [Route("GetAll")]
        public async Task<ActionResult<ServiceResponse<List<GetDiscountDto>>>> GetAllDiscounts()
        {
            var response = await discountService.GetAllDiscounts();

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<ServiceResponse<GetDiscountDto>>> GetDiscountById(string id)
        {
            var response = await discountService.GetDiscountById(id);

            return StatusCode((int)response.StatusCode, response);
        }
    }
}
