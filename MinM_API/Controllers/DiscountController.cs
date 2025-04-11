using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinM_API.Dtos;
using MinM_API.Dtos.Discount;
using MinM_API.Services.Interfaces;

namespace MinM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountController(IDiscountService discountService) : ControllerBase
    {
        [HttpPost]
        [Route("Create")]
        public async Task<ActionResult<ServiceResponse<int>>> AddDiscount(AddDiscountDto discountDto)
        {
            var response = await discountService.AddDiscount(discountDto);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        public async Task<ActionResult<ServiceResponse<List<GetDiscountDto>>>> GetAllDiscounts()
        {
            var response = await discountService.GetAllDiscounts();

            return StatusCode((int)response.StatusCode, response);
        }
    }
}
