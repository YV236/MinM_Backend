using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinM_API.Dtos;
using MinM_API.Dtos.Review;
using MinM_API.Services.Interfaces;

namespace MinM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController(IReviewService reviewService) : ControllerBase
    {
        [HttpPost]
        [Route("create")]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> CreateReview(AddReviewDto request)
        {
            var response = await reviewService.CreateReview(request, User);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost]
        [Route("edit")]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> EditReview(EditReviewDto request)
        {
            var response = await reviewService.EditReview(request, User);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [Route("all/{productId}")]
        public async Task<ActionResult<ServiceResponse<List<GetReviewDto>>>> GetAllProductReviews([FromRoute] string productId)
        {
            var response = await reviewService.GetAllProductReviews(productId);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [Route("{reviewId}")]
        public async Task<ActionResult<ServiceResponse<GetReviewDto>>> GetProductReview([FromRoute] string reviewId)
        {
            var response = await reviewService.GetProductReview(reviewId);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete]
        [Route("{reviewId}")]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> DeleteReview([FromRoute] string reviewId)
        {
            var response = await reviewService.DeleteReview(reviewId);

            return StatusCode((int)response.StatusCode, response);
        }
    }
}
