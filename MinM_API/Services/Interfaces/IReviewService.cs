using MinM_API.Dtos;
using MinM_API.Dtos.Review;

namespace MinM_API.Services.Interfaces
{
    public interface IReviewService
    {
        Task<ServiceResponse<int>> CreateReview(AddReviewDto request);
        Task<ServiceResponse<int>> EditReview(EditReviewDto request);
        Task<ServiceResponse<List<GetReviewDto>>> GetAllProductReviews();
        Task<ServiceResponse<GetReviewDto>> GetProductReview(string reviewId);
        Task<ServiceResponse<int>> DeleteReview(string reviewId);
    }
}
