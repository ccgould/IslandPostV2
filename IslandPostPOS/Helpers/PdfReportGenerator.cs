using IslandPostPOS.Shared.DTOs;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using System.Collections.Generic;
using System.IO;

public class PdfReportGenerator
{
    public byte[] GenerateReport(EndOfShiftReportDTO report)
    {
        using var document = new PdfDocument();
        var page = document.Pages.Add();
        var graphics = page.Graphics;

        // Title
        var fontTitle = new PdfStandardFont(PdfFontFamily.Helvetica, 16, PdfFontStyle.Bold);
        graphics.DrawString("End of Shift Report", fontTitle, PdfBrushes.Black, new PointF(200, 20));

        // 1. Sales Summary
        var summaryGrid = new PdfGrid();
        summaryGrid.Columns.Add(4);     // define 4 columns
        summaryGrid.Headers.Add(1);     // add 1 header row

        summaryGrid.Headers[0].Cells[0].Value = "Subtotal";
        summaryGrid.Headers[0].Cells[1].Value = "Taxes";
        summaryGrid.Headers[0].Cells[2].Value = "Total Sales";
        summaryGrid.Headers[0].Cells[3].Value = "Transactions";

        var row = summaryGrid.Rows.Add();
        row.Cells[0].Value = report.Summary.TotalSubtotal.ToString("C");
        row.Cells[1].Value = report.Summary.TotalTaxes.ToString("C");
        row.Cells[2].Value = report.Summary.TotalSales.ToString("C");
        row.Cells[3].Value = report.Summary.TransactionCount.ToString();


        summaryGrid.Draw(page, new PointF(0, 60));

        // 2. Payment Breakdown
        var paymentGrid = new PdfGrid();
        paymentGrid.Columns.Add(2);
        paymentGrid.Headers.Add(1);

        paymentGrid.Headers[0].Cells[0].Value = "Method";
        paymentGrid.Headers[0].Cells[1].Value = "Amount";

        foreach (var p in report.PaymentBreakdown)
        {
            var row1 = paymentGrid.Rows.Add();
            row1.Cells[0].Value = p.Method;
            row1.Cells[1].Value = p.Amount.ToString("C");
        }

        // Draw payment grid and capture layout result
        PdfLayoutResult paymentResult = paymentGrid.Draw(page, new PointF(0, 150));

        // 3. Cashier Breakdown
        var cashierGrid = new PdfGrid();
        cashierGrid.Columns.Add(3);
        cashierGrid.Headers.Add(1);

        cashierGrid.Headers[0].Cells[0].Value = "Cashier Name";
        cashierGrid.Headers[0].Cells[1].Value = "Transactions";
        cashierGrid.Headers[0].Cells[2].Value = "Sales";

        foreach (var c in report.CashierBreakdown)
        {
            var row2 = cashierGrid.Rows.Add();
            row2.Cells[0].Value = c.CashierName;
            row2.Cells[1].Value = c.TransactionsHandled.ToString();
            row2.Cells[2].Value = c.CashierSales.ToString("C");
        }

        // Position cashier grid just below payment grid
        PdfLayoutResult cashierResult = cashierGrid.Draw(page,
            new PointF(0, paymentResult.Bounds.Bottom + 20));



        // 4. Top Products by Quantity
        AddProductGrid(document, "Top Products by Quantity", report.TopProductsByQuantity);

        // 5. Top Products by Revenue
        AddProductGrid(document, "Top Products by Revenue", report.TopProductsByRevenue);

        // 6. All Sales from Today
        AddSalesGrid(document, "All Sales", report.AllSalesToday);

        // Save to byte array
        using var stream = new MemoryStream();
        document.Save(stream);
        return stream.ToArray();
    }

    private void AddProductGrid(PdfDocument doc, string title, List<ProductReportDTO> products)
    {
        var page = doc.Pages.Add();
        var graphics = page.Graphics;
        var font = new PdfStandardFont(PdfFontFamily.Helvetica, 14, PdfFontStyle.Bold);
        graphics.DrawString(title, font, PdfBrushes.Black, new PointF(0, 20));

        var grid = new PdfGrid();
        grid.Columns.Add(6);      // define 6 columns
        grid.Headers.Add(1);      // add 1 header row

        grid.Headers[0].Cells[0].Value = "Product ID";
        grid.Headers[0].Cells[1].Value = "Brand";
        grid.Headers[0].Cells[2].Value = "Description";
        grid.Headers[0].Cells[3].Value = "Category";
        grid.Headers[0].Cells[4].Value = "Quantity";
        grid.Headers[0].Cells[5].Value = "Revenue";

        foreach (var p in products)
        {
            var row = grid.Rows.Add();
            row.Cells[0].Value = p.ProductId.ToString();
            row.Cells[1].Value = p.Brand;
            row.Cells[2].Value = p.Description;
            row.Cells[3].Value = p.Category;
            row.Cells[4].Value = p.Quantity.ToString();
            row.Cells[5].Value = p.Revenue.ToString("C");
        }

        grid.Draw(page, new PointF(0, 60));
    }

    private void AddSalesGrid(PdfDocument doc, string title, List<SaleDTO> sales)
    {
        var page = doc.Pages.Add();
        var graphics = page.Graphics;
        var font = new PdfStandardFont(PdfFontFamily.Helvetica, 14, PdfFontStyle.Bold);
        graphics.DrawString(title, font, PdfBrushes.Black, new PointF(0, 20));

        var grid = new PdfGrid();
        grid.Columns.Add(6);     // define 6 columns
        grid.Headers.Add(1);     // add 1 header row

        grid.Headers[0].Cells[0].Value = "Sale #";
        grid.Headers[0].Cells[1].Value = "Client";
        grid.Headers[0].Cells[2].Value = "Subtotal";
        grid.Headers[0].Cells[3].Value = "Taxes";
        grid.Headers[0].Cells[4].Value = "Total";
        grid.Headers[0].Cells[5].Value = "Date";

        foreach (var s in sales)
        {
            var row = grid.Rows.Add();
            row.Cells[0].Value = s.SaleNumber;
            row.Cells[1].Value = s.ClientName;
            row.Cells[2].Value = s.Subtotal.Value.ToString("C");
            row.Cells[3].Value = s.TotalTaxes.Value.ToString("C");
            row.Cells[4].Value = s.Total.Value.ToString("C");
            row.Cells[5].Value = s.RegistrationDate.Value.ToString("g");
        }

        grid.Draw(page, new PointF(0, 60));
    }
}