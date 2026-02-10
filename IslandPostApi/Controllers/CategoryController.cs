using IslandPostApi.Contracts;
using IslandPostApi.Models;
using IslandPostApi.Services;
using IslandPostPOS.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace IslandPostApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(ICategoryService service) : ControllerBase
    {
        [HttpGet("GetAllCategories")]
        public async Task<ActionResult<List<CategoryDTO>>> GetAllCatergories()
        {
            try
            {
                var result = await service.GetCategoryAsync();

                if (result == null)
                {
                    return NotFound("No categories found");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log exception here
                return StatusCode(500, $"Internal server error: {ex.Message}");
                throw;
            }
        }

    [HttpPost("CreateCategory")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryDTO model)
        {
            if (model == null)
            {
                return BadRequest("Category data is required.");
            }

            try
            {
                var categoryEntity = new Category
                {
                    IdCategory = model.IdCategory,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    RegistrationDate = DateTime.UtcNow
                };

                var categoryCreated = await service.AddCategory(categoryEntity);

                var categoryDto = new CategoryDTO
                {
                    IdCategory = categoryCreated.IdCategory,
                    Description = categoryCreated.Description,
                    IsActive = categoryCreated.IsActive
                };

                return CreatedAtAction(nameof(GetCategoryById), new { id = categoryDto.IdCategory }, categoryDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the category.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await service.GetById(id);
            if (category == null)
            {
                return NotFound();
            }

            var categoryDto = new CategoryDTO
            {
                IdCategory = category.IdCategory,
                Description = category.Description,
                IsActive = category.IsActive
            };

            return Ok(categoryDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await service.GetById(id);
                if (category == null)
                {
                    return NotFound($"Category with ID {id} not found.");
                }

                await service.Delete(id);

                return NoContent(); // 204 No Content
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the category.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditCategory(int id, [FromBody] CategoryDTO model)
        {
            if (model == null || id != model.IdCategory)
            {
                return BadRequest("Invalid category data.");
            }

            try
            {
                var category = await service.GetById(id);
                if (category == null)
                {
                    return NotFound($"Category with ID {id} not found.");
                }

                // Update entity properties
                category.Description = model.Description;
                category.IsActive = model.IsActive;

                var updatedCategory = await service.Update(category);

                // Map back to DTO
                var categoryDto = new CategoryDTO
                {
                    IdCategory = updatedCategory.IdCategory,
                    Description = updatedCategory.Description,
                    IsActive = updatedCategory.IsActive
                };

                return Ok(categoryDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the category.");
            }
        }
    }
}
