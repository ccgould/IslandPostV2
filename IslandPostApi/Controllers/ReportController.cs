using IslandPostApi.Contracts;
using IslandPostPOS.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace IslandPostApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController(ISaleService service) : ControllerBase
    {
        [HttpGet("Report")]
        public async Task<ActionResult<SaleReportDTO>> Report(string startDate, string endDate)
        {
            try
            {
                var report = await service.ReportAsync(startDate, endDate);

                if (report == null || !report.Any())
                    return NotFound("No report data found for given criteria.");

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
