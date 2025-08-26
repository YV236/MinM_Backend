using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.Category;
using MinM_API.Dtos.Review;
using MinM_API.Extension;
using MinM_API.Mappers;
using MinM_API.Models;
using MinM_API.Services.Interfaces;
using System.Security.Claims;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using MinM_API.Repositories.Interfaces;

namespace MinM_API.Services.Implementations
{
    public class ReviewService(DataContext context, ProductMapper mapper,
        ILogger<ReviewService> logger, IUserRepository userRepository) : IReviewService
    {
        public async Task<ServiceResponse<int>> CreateReview(AddReviewDto request, ClaimsPrincipal user)
        {
            try
            {
                var getUser = await userRepository.FindUser(user, context);

                if (getUser is null)
                {
                    return ResponseFactory.Error(0, "User not found", HttpStatusCode.NotFound);
                }

                var review = new Review
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = getUser.Id,
                    ProductId = request.ProductId,
                    Rating = request.Rating,
                    Comment = request.Comment,
                    CreatedAt = DateTime.UtcNow,
                };

                context.Reviews.Add(review);
                await context.SaveChangesAsync();

                return ResponseFactory.Success(1, "Review added successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while adding review.");
                return ResponseFactory.Error(0, "Internal error");
            }
        }

        public async Task<ServiceResponse<int>> EditReview(EditReviewDto request, ClaimsPrincipal user)
        {
            try
            {
                var getUser = await userRepository.FindUser(user, context);

                if (getUser is null)
                {
                    return ResponseFactory.Error(0, "User not found", HttpStatusCode.NotFound);
                }

                var review = await context.Reviews.FirstOrDefaultAsync(r => r.Id == request.Id && r.UserId == getUser.Id);

                if (review is null)
                {
                    return ResponseFactory.Error(0, "Review not found", HttpStatusCode.NotFound);
                }

                review.Rating = request.Rating;
                review.Comment = request.Comment;

                await context.SaveChangesAsync();

                return ResponseFactory.Success(1, "Review edited successfully");
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Fail: Error while editing review.");
                return ResponseFactory.Error(0, "Internal error");
            }
        }

        public async Task<ServiceResponse<List<GetReviewDto>>> GetAllProductReviews(string productId)
        {
            try
            {
                var productReviews = await context.Reviews.Where(r => r.ProductId == productId).ToListAsync();

                if (productReviews.Count == 0)
                {
                    return ResponseFactory.Error<List<GetReviewDto>>(null, "Reviews not found", HttpStatusCode.NotFound);
                }

                var result = new List<GetReviewDto>();

                foreach (var productReview in productReviews)
                {
                    result.Add(new GetReviewDto
                    {
                        Id = productReview.Id,
                        Rating = productReview.Rating,
                        Comment = productReview.Comment,
                    });
                }

                return ResponseFactory.Success(result, "Successful extraction of product reviews");
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Fail: Error while retrieving reviews.");
                return ResponseFactory.Error<List<GetReviewDto>>(null, "Internal error");
            }
        }

        public async Task<ServiceResponse<GetReviewDto>> GetProductReview(string reviewId)
        {
            try
            {
                var productReview = await context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId);

                if (productReview is null)
                {
                    return ResponseFactory.Error<GetReviewDto>(null, "Review not found", HttpStatusCode.NotFound);
                }

                var result = new GetReviewDto
                {
                    Id = productReview.Id,
                    Rating = productReview.Rating,
                    Comment = productReview.Comment,
                };

                return ResponseFactory.Success(result, "Successful extraction of product reviews");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while retrieving reviews.");
                return ResponseFactory.Error<GetReviewDto>(null, "Internal error");
            }
        }

        public async Task<ServiceResponse<int>> DeleteReview(string reviewId, ClaimsPrincipal user)
        {
            try
            {
                var getUser = await userRepository.FindUser(user, context);

                if (getUser is null)
                {
                    return ResponseFactory.Error(0, "User not found", HttpStatusCode.NotFound);
                }

                var productReview = await context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == getUser.Id);

                if (productReview is null)
                {
                    return ResponseFactory.Error(0, "Review not found", HttpStatusCode.NotFound);
                }

                context.Reviews.Remove(productReview);
                await context.SaveChangesAsync();

                return ResponseFactory.Success(1, "Successful delete of product review");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while retrieving reviews.");
                return ResponseFactory.Error(0, "Internal error");
            }
        }
    }
}
