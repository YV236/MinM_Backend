using MinM_API.Dtos;
using MinM_API.Dtos.Products;

namespace MinM_API.Services.Interfaces
{
    public interface IProductService
    {
        public Task<ServiceResponse<string>> AddProduct(AddProductDto addProductDto);
        public Task<ServiceResponse<int>> UpdateProduct(UpdateProductDto updateProductDto);
        public Task<ServiceResponse<List<GetProductDto>>> GetAllProducts();
        public Task<ServiceResponse<GetProductDto>> GetProductById(string id);
        public Task<ServiceResponse<GetProductDto>> GetProductBySlug(string slug);
        public Task<ServiceResponse<int>> DeleteProduct(string id);
    }
}
