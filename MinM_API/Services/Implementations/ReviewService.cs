using MinM_API.Dtos;
using MinM_API.Dtos.Review;
using MinM_API.Services.Interfaces;

namespace MinM_API.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        public Task<ServiceResponse<int>> CreateReview(AddReviewDto request)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<int>> DeleteReview(string reviewId)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<List<GetReviewDto>>> GetAllProductReviews()
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<GetReviewDto>> GetProductReview(string reviewId)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<int>> EditReview(EditReviewDto request)
        {
            throw new NotImplementedException();
        }
    }
}
