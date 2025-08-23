using MinM_API.Dtos;
using MinM_API.Dtos.Review;
using System.Security.Claims;

namespace MinM_API.Services.Interfaces
{
    public interface IReviewService
    {
        Task<ServiceResponse<int>> CreateReview(AddReviewDto request, ClaimsPrincipal user);
        Task<ServiceResponse<int>> EditReview(EditReviewDto request, ClaimsPrincipal user);
        Task<ServiceResponse<List<GetReviewDto>>> GetAllProductReviews(string productId);
        Task<ServiceResponse<GetReviewDto>> GetProductReview(string reviewId);
        Task<ServiceResponse<int>> DeleteReview(string reviewId, ClaimsPrincipal user);
    }
}
