using IslandPostApi.Contracts;
using IslandPostApi.Models;
using IslandPostPOS.Shared.DTOs;
using IslandPostPOS.Shared.Enumerators;
using Microsoft.AspNetCore.Mvc;

namespace IslandPostApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _service;

        public SalesController(ISaleService service)
        {
            _service = service;
        }

        [HttpGet("GetAllSales")]
        public async Task<ActionResult<List<SaleDTO>>> GetAllSales()
        {
            var sales = await _service.GetAllSalesAsync();
            if (sales == null || !sales.Any())
                return NotFound("No sales found.");

            return Ok(sales);
        }

        [HttpPost("RegisterSale")]
        public async Task<ActionResult<SaleDTO>> RegisterSale([FromBody] Sale model)
        {
            try
            {
                var userIdClaim = HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);

                if (userIdClaim != null)
                    model.IdUsers = int.Parse(userIdClaim.Value);

                var savedSale = await _service.RegisterAsync(model);
                return Ok(savedSale);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("History")]
        public async Task<IActionResult> History(
            [FromQuery] string? saleNumber,
            [FromQuery] string? startDate,
            [FromQuery] string? endDate)
        {
            var sales = await _service.SaleHistoryAsync(saleNumber, startDate, endDate);

            if (sales == null || !sales.Any())
                return NotFound("No sales found for given criteria.");

            return Ok(sales);
        }

        [HttpGet("Detail/{saleNumber}")]
        public async Task<ActionResult<SaleDTO>> Detail(string saleNumber)
        {
            var sale = await _service.DetailAsync(saleNumber);
            if (sale == null)
                return NotFound($"Sale with number {saleNumber} not found.");

            return Ok(sale);
        }

        [HttpPost("ParkSale")]
        public async Task<ActionResult<SaleDTO>> ParkSale([FromBody] Sale model)
        {
            model.Status = SaleStatus.Parked;
            var savedSale = await _service.RegisterAsync(model);
            return Ok(savedSale);
        }

        [HttpPost("FinalizeSale/{id}")]
        public async Task<ActionResult<SaleDTO>> FinalizeSale(int id)
        {
            try
            {
                var finalizedSale = await _service.FinalizeAsync(id);
                return Ok(finalizedSale);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("CancelSale/{id}")]
        public async Task<ActionResult<SaleDTO>> CancelSale(int id)
        {
            try
            {
                var cancelledSale = await _service.CancelAsync(id);
                return Ok(cancelledSale);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}