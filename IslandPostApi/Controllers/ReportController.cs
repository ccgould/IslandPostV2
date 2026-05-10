using IslandPostApi.Contracts;
using IslandPostPOS.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace IslandPostApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController(ISaleService service) : ControllerBase
    {
        [HttpGet("ReportPdf")]
        public async Task<IActionResult> ReportPdf(DateTime startDate, DateTime endDate)
        {
            // startDate = 2026-04-01 00:00:00
            // endDate   = 2026-04-01 23:59:59

            var report = await service.ReportAsync(startDate, endDate);

            // Return DTO or PDF depending on your design
            return Ok(report);
        }
    }
}
