using IslandPostApi.Contracts;
using IslandPostApi.Data;
using IslandPostApi.Mapper;
using IslandPostApi.Models;
using IslandPostPOS.Shared.DTOs;
using IslandPostPOS.Shared.Enumerators;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace IslandPostApi.Services
{
    public class SaleService : ISaleService
    {
        private readonly IslandPostDbContext _context;

        public SaleService(IslandPostDbContext context)
        {
            _context = context;
        }

        public async Task<SaleDTO?> DetailAsync(string saleNumber)
        {
            var sale = await _context.Sales
                .Include(s => s.DetailSales)
                    .ThenInclude(d => d.Product)
                        .ThenInclude(p => p.IdCategoryNavigation)
                .Include(s => s.IdUsersNavigation)
                .Include(s => s.IdTypeDocumentSaleNavigation)
                .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber);

            return sale == null ? null : SaleMapper.ToDto(sale);
        }

        public async Task<List<SaleDTO>> GetAllSalesAsync()
        {
            var sales = await _context.Sales
                .Include(p => p.DetailSales)
                    .ThenInclude(d => d.Product)
                        .ThenInclude(p => p.IdCategoryNavigation)
                .Include(p => p.IdTypeDocumentSaleNavigation)
                .Include(p => p.IdUsersNavigation)
                .ToListAsync();

            return sales.Select(SaleMapper.ToDto).ToList();
        }

        public async Task<SaleDTO> RegisterAsync(Sale entity)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (entity.Status == SaleStatus.Completed)
                {
                    await FinalizeSaleCoreAsync(entity);
                }
                else
                {
                    entity.RegistrationDate = DateTime.UtcNow;
                }

                await _context.Sales.AddAsync(entity);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var savedSale = await _context.Sales
                    .Include(s => s.DetailSales)
                        .ThenInclude(d => d.Product)
                            .ThenInclude(p => p.IdCategoryNavigation)
                    .Include(s => s.IdUsersNavigation)
                    .FirstAsync(s => s.IdSale == entity.IdSale);

                return SaleMapper.ToDto(savedSale);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<SaleDTO> FinalizeAsync(int saleId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var sale = await _context.Sales
                    .Include(s => s.DetailSales)
                        .ThenInclude(d => d.Product)
                            .ThenInclude(p => p.IdCategoryNavigation)
                    .FirstOrDefaultAsync(s => s.IdSale == saleId);

                if (sale == null)
                    throw new Exception("Sale not found.");

                SaleStateMachine.Complete(sale);

                await FinalizeSaleCoreAsync(sale);

                _context.Sales.Update(sale);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return SaleMapper.ToDto(sale);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<SaleDTO>> SaleHistoryAsync(string? saleNumber, string? startDate, string? endDate, SaleStatus? status = null)
        {
            var query = _context.Sales
                .Include(tdv => tdv.IdTypeDocumentSaleNavigation)
                .Include(u => u.IdUsersNavigation)
                .Include(dv => dv.DetailSales)
                    .ThenInclude(d => d.Product)
                        .ThenInclude(p => p.IdCategoryNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                var start_date = DateTime.ParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var end_date = DateTime.ParseExact(endDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                query = query.Where(v =>
                    v.RegistrationDate.HasValue &&
                    v.RegistrationDate.Value.Date >= start_date.Date &&
                    v.RegistrationDate.Value.Date <= end_date.Date);
            }
            else if (!string.IsNullOrEmpty(saleNumber))
            {
                query = query.Where(v => v.SaleNumber == saleNumber);
            }
            else if (status.HasValue)
                query = query.Where(v => v.Status == status.Value);

            var sales = await query.ToListAsync();
            return sales.Select(SaleMapper.ToDto).ToList();
        }

        public async Task<List<SaleReportDTO>> ReportAsync(string startDate, string endDate)
        {
            var start = DateTime.Parse(startDate).Date;
            var end = DateTime.Parse(endDate).Date.AddDays(1).AddTicks(-1);

            return await _context.DetailSale
                .Where(d => d.IdSaleNavigation.RegistrationDate >= start &&
                            d.IdSaleNavigation.RegistrationDate <= end)
                .Select(d => new SaleReportDTO
                {
                    RegistrationDate = d.IdSaleNavigation.RegistrationDate.Value.ToString("yyyy-MM-dd"),
                    SaleNumber = d.IdSaleNavigation.SaleNumber,
                    DocumentType = d.IdSaleNavigation.IdTypeDocumentSaleNavigation.Description,
                    DocumentClient = d.IdSaleNavigation.CustomerDocument,
                    ClientName = d.IdSaleNavigation.ClientName,
                    SubTotalSale = d.IdSaleNavigation.Subtotal,
                    TaxTotalSale = d.IdSaleNavigation.TotalTaxes,
                    TotalSale = d.IdSaleNavigation.Total,
                    Product = d.DescriptionProduct,
                    Quantity = d.Quantity,
                    Price = d.Price,
                    Total = d.Total,
                    PaymentMethod = d.IdSaleNavigation.PaymentMethod,
                    RegisterUser = d.IdSaleNavigation.IdUsersNavigation.Name
                })
                .ToListAsync();
        }

        public async Task<SaleDTO> CancelAsync(int saleId)
        {
            var sale = await _context.Sales
                .Include(s => s.DetailSales)
                    .ThenInclude(d => d.Product)
                        .ThenInclude(p => p.IdCategoryNavigation)
                .FirstOrDefaultAsync(s => s.IdSale == saleId);

            if (sale == null)
                throw new Exception("Sale not found.");

            SaleStateMachine.Cancel(sale);

            sale.RegistrationDate = DateTime.UtcNow;

            _context.Sales.Update(sale);
            await _context.SaveChangesAsync();

            return SaleMapper.ToDto(sale);
        }

        private async Task FinalizeSaleCoreAsync(Sale sale)
        {
            // Deduct inventory and snapshot product info
            foreach (DetailSale dv in sale.DetailSales)
            {
                var product = await _context.Products
                    .Include(p => p.IdCategoryNavigation)
                    .FirstAsync(p => p.IdProduct == dv.IdProduct);

                product.Quantity -= dv.Quantity;
                _context.Products.Update(product);

                // Snapshot product info
                dv.DescriptionProduct = product.Description;
                dv.BrandProduct = product.Brand;
                dv.CategoryProducty = product.IdCategoryNavigation?.Description;
            }
            await _context.SaveChangesAsync();

            // Generate sale number
            var correlative = await _context.CorrelativeNumbers.FirstAsync(n => n.Management == "Sale");
            correlative.LastNumber += 1;
            correlative.DateUpdate = DateTime.Now;

            _context.CorrelativeNumbers.Update(correlative);
            await _context.SaveChangesAsync();

            string ceros = string.Concat(Enumerable.Repeat("0", correlative.QuantityDigits.Value));
            string saleNumber = ceros + correlative.LastNumber.ToString();
            saleNumber = saleNumber.Substring(saleNumber.Length - correlative.QuantityDigits.Value, correlative.QuantityDigits.Value);

            sale.SaleNumber = saleNumber;
            sale.Status = SaleStatus.Completed;
            sale.RegistrationDate = DateTime.UtcNow;
        }

        public async Task<SaleDTO?> GetByIdAsync(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.DetailSales)
                    .ThenInclude(d => d.Product)
                        .ThenInclude(p => p.IdCategoryNavigation)
                .FirstOrDefaultAsync(s => s.IdSale == id);

            return sale == null ? null : SaleMapper.ToDto(sale);
        }

        public async Task<List<SaleDTO>> GetParkedAsync()
        {
            var sales = await _context.Sales
                .Include(s => s.DetailSales)
                    .ThenInclude(d => d.Product)
                        .ThenInclude(p => p.IdCategoryNavigation)
                .Where(s => s.Status == SaleStatus.Parked)
                .ToListAsync();

            return sales.Select(SaleMapper.ToDto).ToList();
        }

        public async Task<SaleDTO> RetrieveAsync(int saleId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var sale = await _context.Sales
                    .Include(s => s.DetailSales)
                        .ThenInclude(d => d.Product)
                            .ThenInclude(p => p.IdCategoryNavigation)
                    .FirstOrDefaultAsync(s => s.IdSale == saleId);

                if (sale == null)
                    throw new Exception("Sale not found.");

                SaleStateMachine.Retrieve(sale);

                _context.Sales.Update(sale);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return SaleMapper.ToDto(sale);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<SaleDTO> ParkAsync(int saleId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var sale = await _context.Sales
                    .Include(s => s.DetailSales)
                        .ThenInclude(d => d.Product)
                            .ThenInclude(p => p.IdCategoryNavigation)
                    .FirstOrDefaultAsync(s => s.IdSale == saleId);

                if (sale == null)
                    throw new Exception("Sale not found.");

                // enforce valid transition
                SaleStateMachine.Park(sale);

                sale.RegistrationDate = DateTime.UtcNow;

                _context.Sales.Update(sale);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return SaleMapper.ToDto(sale);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<SaleReportDTO>> ReportDailyTotalsAsync(string startDate, string endDate)
        {
            var start = DateTime.Parse(startDate).Date;
            var end = DateTime.Parse(endDate).Date.AddDays(1).AddTicks(-1);

            var sales = await _context.Sales
                .Where(s => s.RegistrationDate >=start &&
                            s.RegistrationDate <= end 
                            && s.Status != SaleStatus.Retrieved && s.Status != SaleStatus.Parked)
                .ToListAsync();

            var grouped = sales
                .GroupBy(s => s.RegistrationDate.Value.Date) // group by day only
                .Select(g => new SaleReportDTO
                {
                    RegistrationDate = g.Key.ToString("yyyy-MM-dd"),   // format back to string for DTO
                    Total = g.Sum(x => x.Total)     // sum all sales for that day
                })
                .OrderBy(r => r.RegistrationDate)
                .ToList();

            return grouped;
        }

        public async Task<SalesSummaryDTO> ReportSalesSummaryAsync(DateTime startDate, DateTime endDate)
        {
            // Base query with half-open range
            var sales = await _context.Sales
                .Where(s => s.RegistrationDate >= startDate &&
                            s.RegistrationDate < endDate.AddDays(1))
                .ToListAsync();

            // Daily chart data (across range)
            var dailyTotals = sales
                .GroupBy(s => s.RegistrationDate.Value.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(x => x.Total) })
                .OrderBy(x => x.Date)
                .ToList()
                .Select(x => new SaleReportDTO
                {
                    Date = x.Date.ToString("yyyy-MM-dd"),
                    Total = x.Total
                })
                .ToList();

            // Weekly chart data (current week, per day)
            var diff = (7 + (int)DateTime.Today.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            var startOfWeek = DateTime.Today.AddDays(-diff);
            var endOfWeek = startOfWeek.AddDays(7);

            var weeklyDailyTotalsRaw = await _context.Sales
                .Where(s => s.RegistrationDate >= startOfWeek &&
                            s.RegistrationDate < endOfWeek)
                .GroupBy(s => s.RegistrationDate.Value.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(x => x.Total) })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var weeklyDailyTotals = weeklyDailyTotalsRaw
                .Select(x => new SaleReportDTO
                {
                    DateValue = x.Date,                       // DateTime for chart
                    Date = x.Date.ToString("yyyy-MM-dd"),     // optional label
                    Total = x.Total
                })
                .ToList();

            var weeklyTotal = weeklyDailyTotals.Sum(d => d.Total ?? 0);

            // Monthly chart data (current month, per day)
            var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);

            var monthlyTotalsRaw = await _context.Sales
                .GroupBy(s => new { s.RegistrationDate.Value.Year, s.RegistrationDate.Value.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Sum(x => x.Total)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();


            var monthlyTotals = monthlyTotalsRaw
                .Select(x => new SaleReportDTO
                {
                    DateValue = new DateTime(x.Year, x.Month, 1),   // ✅ first day of month
                    Date = $"{x.Year}-{x.Month:D2}",                // optional label
                    Total = x.Total
                })
                .ToList();



            // KPI cards (calendar-based)
            var todayTotal = await _context.Sales
                .Where(s => s.RegistrationDate >= DateTime.Today &&
                            s.RegistrationDate < DateTime.Today.AddDays(1))
                .SumAsync(s => s.Total);

            var monthlyTotal = monthlyTotals.Sum(d => d.Total ?? 0);

            return new SalesSummaryDTO
            {
                DailyTotals = dailyTotals,
                WeeklyDailyTotals = weeklyDailyTotals,
                MonthlyTotals = monthlyTotals,
                TodayTotal = todayTotal ?? 0,
                WeeklyTotal = weeklyTotal,
                MonthlyTotal = monthlyTotal
            };
        }
    }
}