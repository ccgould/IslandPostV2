using IslandPostApi.Models;
using IslandPostPOS.Shared.DTOs;

namespace IslandPostApi.Contracts
{
    public interface ICategoryService
    {
        Task<CategoryDTO> AddCategory(Category categoryEntity);
        Task Delete(int id);
        Task<CategoryDTO> GetById(int id);
        Task<List<CategoryDTO>> GetCategoryAsync();
        Task<CategoryDTO> Update(CategoryDTO category);
    }
}
