using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IslandPostPOS.Models;
using IslandPostPOS.Services;
using IslandPostPOS.Shared.DTOs;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Services.Maps;
using static IslandPostPOS.Services.ReceiptService;

namespace IslandPostPOS.ViewModels
{
    public partial class SalesHistoryViewModel : ObservableObject
    {
        public APIService Service { get; set; }
        [ObservableProperty] private DateTimeOffset? startDate;
        [ObservableProperty] private DateTimeOffset? endDate;
        [ObservableProperty] private string? saleNumber;
        private readonly ReceiptService receiptService;

        public SalesHistoryViewModel(APIService service, ReceiptService receiptService)
        {
            Service = service;
            this.receiptService = receiptService;
        }

        [RelayCommand]
        private async Task Search()
        {
            string? start = StartDate?.Date.ToString("yyyy-MM-dd");
            string? end = EndDate?.Date.ToString("yyyy-MM-dd");

            await Service.SearchAndUpdateSalesHistoryAsync(
                SaleNumber,
                start,
                end
            );
        }

        [RelayCommand]
        private async Task ExportReport()
        {
            try
            {
                // Save to Documents folder
                string path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    $"EndOfShiftReport_{StartDate.Value.Date.ToString("MMM_dd_yyyy")}_{EndDate.Value.Date.ToString("MMM_dd_yyyy")}.pdf"
                );

                var start = StartDate.Value.Date; // beginning of the day
                var end = EndDate.Value.Date.AddDays(1).AddTicks(-1); // end of the day

                await Service.SaveReportPdfAsync(start, end, path);

                // Optionally open the PDF
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                
            }

        }

        [RelayCommand]
        public async Task EmailReceipt(SaleDTO sale)
        {
            var receipt = new ReceiptData
            {
                company_name = "Island Post Paradise Island",
                company_address = "Royal Beach Club Paradise Island",
                company_email = "islandpostphotospi@gmail.com",
                receipt_no = sale.SaleNumber,
                receipt_date = sale.RegistrationDate.Value.ToString("yyyy-MM-dd hh:mm:ss tt"),
                footer = "Thank you for shopping with us!",
                tax = "10",
                logo = "https://craftmypdf-upload.s3-ap-southeast-1.amazonaws.com/3e9/b0d7c504-4ee7-4173-aa2d-12b16ec10814.png",
                currency = "$"
            };

            foreach (var item in sale.DetailSales)
            {
                receipt.items.Add(new ReceiptItem
                {
                    description = item.DescriptionProduct,
                    qty = item.Quantity.Value,
                    unitprice = item.Price ?? 0
                });
            }

            var pdfResult = await receiptService.GenerateReceiptAsync(receipt);

            // Option 1: open the hosted file in browser
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = pdfResult.file,
                UseShellExecute = true
            });

            // Option 2: download and save locally
            using var client = new HttpClient();
            var bytes = await client.GetByteArrayAsync(pdfResult.file);
            await File.WriteAllBytesAsync("receipt.pdf", bytes);

        }

    }
}
