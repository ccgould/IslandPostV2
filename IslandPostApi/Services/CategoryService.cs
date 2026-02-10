using IslandPostApi.Contracts;
using IslandPostApi.Data;
using IslandPostApi.Models;
using IslandPostPOS.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IslandPostApi.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IslandPostDbContext _context;

        public CategoryService(IslandPostDbContext context)
        {
            _context = context;
        }

        public async Task<CategoryDTO> AddCategory(Category categoryEntity)
        {
            _context.Category.Add(categoryEntity);
            await _context.SaveChangesAsync();

            return new CategoryDTO
            {
                IdCategory = categoryEntity.IdCategory,
                Description = categoryEntity.Description,
                IsActive = categoryEntity.IsActive
            };
        }

        public async Task Delete(int id)
        {
            var category = await _context.Category.FindAsync(id);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found.");
            }

            _context.Category.Remove(category);
            await _context.SaveChangesAsync();
        }

        public async Task<CategoryDTO> GetById(int id)
        {
            var category = await _context.Category.FindAsync(id);
            if (category == null)
            {
                return null!;
            }

            return new CategoryDTO
            {
                IdCategory = category.IdCategory,
                Description = category.Description,
                IsActive = category.IsActive
            };
        }

        public async Task<List<CategoryDTO>> GetCategoryAsync()
        {
            var categories = await _context.Category.ToListAsync();
            return categories.Select(c => new CategoryDTO
            {
                IdCategory = c.IdCategory,
                Description = c.Description,
                IsActive = c.IsActive
            }).ToList();
        }

        public async Task<CategoryDTO> Update(CategoryDTO categoryDto)
        {
            var category = await _context.Category.FindAsync(categoryDto.IdCategory);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {categoryDto.IdCategory} not found.");
            }

            category.Description = categoryDto.Description;
            category.IsActive = categoryDto.IsActive;

            _context.Category.Update(category);
            await _context.SaveChangesAsync();

            return new CategoryDTO
            {
                IdCategory = category.IdCategory,
                Description = category.Description,
                IsActive = category.IsActive
            };
        }
    }
}