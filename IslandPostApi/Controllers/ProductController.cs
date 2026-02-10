using IslandPostApi.Contracts;
using IslandPostPOS.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace IslandPostApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService service;

        public ProductController(IProductService service)
        {
            this.service = service;
        }

        // 🔹 Helper method to enrich DTO with URLs
        private ProductDTO EnrichWithUrls(ProductDTO dto)
        {
            if (dto == null) return null;

            dto.PhotoUrl = Url.Action(nameof(GetProductPhoto), nameof(ProductController), new { id = dto.IdProduct }, Request.Scheme);
            dto.ThumbnailUrl = Url.Action(nameof(GetProductThumbnail), nameof(ProductController), new { id = dto.IdProduct }, Request.Scheme);

            return dto;
        }

        private List<ProductDTO> EnrichWithUrls(List<ProductDTO> dtos)
        {
            return dtos?.Select(EnrichWithUrls).ToList() ?? new List<ProductDTO>();
        }

        [HttpGet("GetProducts")]
        public async Task<ActionResult<List<ProductDTO>>> GetProducts(string search)
        {
            var products = await service.SearchForProducts(search);
            if (products == null || !products.Any())
                return NotFound("No products found.");

            return Ok(EnrichWithUrls(products));
        }

        [HttpPost("AddProduct")]
        public async Task<ActionResult<ProductDTO>> AddProduct(ProductDTO product)
        {
            var productCreated = await service.AddAsync(product);

            if (productCreated is null)
                return NotFound("Product could not be created.");

            var enriched = EnrichWithUrls(productCreated);

            return CreatedAtAction(
                nameof(GetProductById),
                new { id = enriched.IdProduct },
                enriched
            );
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDTO>> GetProductById(int id)
        {
            var product = await service.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return Ok(EnrichWithUrls(product));
        }

        [HttpGet("{id}/photo")]
        public async Task<IActionResult> GetProductPhoto(int id)
        {
            var product = await service.GetByIdAsync(id);
            if (product == null || product.Photo == null)
                return NotFound();

            return File(product.Photo, "image/jpeg");
        }

        [HttpGet("{id}/thumbnail")]
        public async Task<IActionResult> GetProductThumbnail(int id)
        {
            var product = await service.GetByIdAsync(id);
            if (product == null || product.ThumbnailUrl == null)
                return NotFound();

            return File(product.ThumbnailUrl, "image/jpeg");
        }

        [HttpPut("EditProduct")]
        public async Task<IActionResult> EditProduct([FromBody] ProductDTO dto)
        {
            var updatedProduct = await service.EditAsync(dto);

            if (updatedProduct == null)
                return NotFound("Product not found.");

            return Ok(EnrichWithUrls(updatedProduct));
        }

        [HttpDelete("DeleteProduct/{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var deleted = await service.DeleteAsync(id);

            if (!deleted)
                return NotFound("Product not found.");

            return NoContent();
        }

        [HttpGet("GetAllProducts")]
        public async Task<ActionResult<List<ProductDTO>>> GetAllProducts()
        {
            var products = await service.GetAllAsync();
            
            if (products == null || !products.Any())
                return NotFound("No products found.");

            return Ok(EnrichWithUrls(products));
        }

        [HttpPost("GetProductsPaged")]
        public async Task<ActionResult<PagedResult<ProductDTO>>> GetProductsPaged(
            int pageNumber,
            int pageSize,
            [FromBody] ProductFilterDTO filter)
        {
            var (items, totalCount) = await service.GetProductsPagedAsync(pageNumber, pageSize, filter);

            if (items == null || !items.Any())
                return NotFound("No products found.");

            var enriched = EnrichWithUrls(items);

            return Ok(new PagedResult<ProductDTO>
            {
                Items = enriched,
                TotalCount = totalCount
            });
        }
    }
}