using Microsoft.EntityFrameworkCore;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.Season;
using MinM_API.Extension;
using MinM_API.Mappers;
using MinM_API.Models;
using MinM_API.Services.Interfaces;
using System.Net;

namespace MinM_API.Services.Implementations
{
    public class SeasonService(DataContext context, ILogger<SeasonService> logger) : ISeasonService
    {
        public async Task<ServiceResponse<int>> AddSeason(AddSeasonDto addSeasonDto)
        {
            try
            {
                var season = new Season()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = addSeasonDto.Name,
                    Slug = SlugExtension.GenerateSlug(addSeasonDto.Name),
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                };

                var productList = await context.Products
                   .Where(p => addSeasonDto.ProductIds.Contains(p.Id))
                   .ToListAsync();

                if (productList == null)
                {
                    logger.LogInformation("Fail: No products found in database");
                    return ResponseFactory.Error(0, "Product not found", HttpStatusCode.NotFound);
                }

                foreach (var product in productList)
                {
                    product.Season = season;
                    product.SeasonId = season.Id;
                    product.IsSeasonal = true;
                }

                await context.SaveChangesAsync();

                return ResponseFactory.Success(1, "Successful season creation");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while adding season. Name: {SeasonName}", addSeasonDto.Name);
                return ResponseFactory.Error(0, "Internal error");
            }
        }

        public Task<ServiceResponse<List<GetSeasonDto>>> GetAllSeasons()
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<GetSeasonDto>> GetSeasonById(string id)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<GetSeasonDto>> GetSeasonBySlug(string slug)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<int>> UpdateSeason(UpdateSeasonDto updateSeasonDto)
        {
            throw new NotImplementedException();
        }
    }
}
