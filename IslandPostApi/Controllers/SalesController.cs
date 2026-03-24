using IslandPostApi.Contracts;
using IslandPostApi.Models;
using IslandPostApi.Services;
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

        [HttpGet("{id}")]
        public async Task<ActionResult<SaleDTO>> GetSale(int id)
        {
            var sale = await _service.GetByIdAsync(id);
            if (sale == null) return NotFound();
            return Ok(sale);
        }

        [HttpGet("Parked")]
        public async Task<ActionResult<List<SaleDTO>>> GetParkedSales()
        {
            var sales = await _service.GetParkedAsync();
            return Ok(sales);
        }

        [HttpPost("{saleId}/park")]
        public async Task<ActionResult<SaleDTO>> Park(int saleId)
            => Ok(await _service.ParkAsync(saleId));

        [HttpPost("{saleId}/retrieve")]
        public async Task<ActionResult<SaleDTO>> Retrieve(int saleId)
            => Ok(await _service.RetrieveAsync(saleId));

        [HttpPost("{saleId}/finalize")]
        public async Task<ActionResult<SaleDTO>> Finalize(int saleId)
            => Ok(await _service.FinalizeAsync(saleId));

        [HttpPost("{saleId}/cancel")]
        public async Task<ActionResult<SaleDTO>> Cancel(int saleId)
            => Ok(await _service.CancelAsync(saleId));

        [HttpGet("DailyTotals")]
        public async Task<ActionResult<IEnumerable<SaleReportDTO>>> DailyTotals(string startDate, string endDate)
        {
            try
            {
                var report = await _service.ReportDailyTotalsAsync(startDate, endDate);

                if (report == null || !report.Any())
                    return NotFound("No daily totals found for given criteria.");

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("SalesSummary")]
        public async Task<ActionResult<SalesSummaryDTO>> SalesSummary(DateTime startDate, DateTime endDate)
        {
            try
            {
                var report = await _service.ReportSalesSummaryAsync(startDate, endDate);

                if (report == null || !report.DailyTotals.Any())
                    return NotFound("No sales found for given criteria.");

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}