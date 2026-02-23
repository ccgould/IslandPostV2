using IslandPostApi.Models;
using IslandPostPOS.Shared.DTOs;
using IslandPostPOS.Shared.Enumerators;

namespace IslandPostApi.Contracts;

public interface ISaleService
{
    Task<List<SaleDTO>> GetAllSalesAsync();
    Task<SaleDTO> RegisterAsync(Sale entity);

    Task<List<SaleDTO>> SaleHistoryAsync(string SaleNumber, string StarDate, string EndDate, SaleStatus? status = null);
    Task<SaleDTO> DetailAsync(string SaleNumber);
    Task<List<SaleReportDTO>> ReportAsync(string StarDate, string EndDate);
    Task<SaleDTO> CancelAsync(int saleId);
    Task<SaleDTO> FinalizeAsync(int saleId);
}
