using IslandPostApi.Models;
using IslandPostPOS.Shared.DTOs;

namespace IslandPostApi.Contracts;

public interface ISaleService
{
    Task<List<SaleDTO>> GetAllSalesAsync();
    Task<SaleDTO> RegisterAsync(Sale entity);

    Task<List<SaleDTO>> SaleHistoryAsync(string SaleNumber, string StarDate, string EndDate);
    Task<SaleDTO> DetailAsync(string SaleNumber);
    Task<List<SaleReportDTO>> ReportAsync(string StarDate, string EndDate);
}
