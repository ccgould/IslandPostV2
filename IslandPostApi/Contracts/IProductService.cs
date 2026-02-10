using IslandPostPOS.Shared.DTOs;

namespace IslandPostApi.Contracts
{
    public interface IProductService
    {
        Task<List<ProductDTO>> GetAllAsync();
        Task<ProductDTO> AddAsync(ProductDTO entity);
        Task<bool> DeleteAsync(int idProduct);
        Task<ProductDTO> EditAsync(ProductDTO dto);
        Task<ProductDTO?> GetByIdAsync(int id);
        Task<List<ProductDTO>> SearchForProducts(string search);
        Task<(List<ProductDTO> Items, int TotalCount)> GetProductsPagedAsync(
            int pageNumber, int pageSize, ProductFilterDTO filter);
    }
}
