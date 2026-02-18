using CsvHelper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

class Program
{
    static void Main(string[] args)
    {
        string csvPath = "C:\\Users\\manag\\Documents\\BARCODE_LIST_GEN.csv";
        if (!File.Exists(csvPath))
        {
            Console.WriteLine($"File not found: {csvPath}");
            return;
        }

        TemplateConfig cfg = new TemplateConfig
        {
            StampBoxWidth = 294,
            StampBoxHeight = 361,
            StampBoxMarginTop = 100,
            StampBoxMarginRight = 100,
            BarcodeMargin = 10,
            TextGap = 5,
            SkuFontSize = 12,
            UpcFontSize = 14 // UPC digits font size
        };

        using (var reader = new StreamReader(csvPath))
        using (var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            var records = csv.GetRecords<SkuRecord>().ToList();

            foreach (var record in records)
            {
                string productName = record.ProductName ?? string.Empty;
                string sku = record.SKU ?? string.Empty;
                string barcode11 = record.Barcode?.Trim() ?? "";

                if (barcode11.Length != 11 || !barcode11.All(char.IsDigit))
                {
                    Console.WriteLine($"Skipping invalid barcode: {barcode11}");
                    continue;
                }

                string upc12 = barcode11 + CalculateUpcCheckDigit(barcode11);

                // Generate barcode image without text
                var writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.UPC_A,
                    Options = new EncodingOptions
                    {
                        Width = 600,
                        Height = 200,
                        Margin = 10,
                        PureBarcode = true // disable ZXing text
                    }
                };

                Bitmap barcodeBitmap = writer.Write(upc12);

                // Load postcard template safely as Bitmap
                using (Image img = Image.FromFile("Postcard Back - RBC.jpeg"))
                using (Bitmap template = new Bitmap(img))
                using (Graphics g = Graphics.FromImage(template))
                {
                    Rectangle stampBox = new Rectangle(
                        template.Width - cfg.StampBoxWidth - cfg.StampBoxMarginRight,
                        cfg.StampBoxMarginTop,
                        cfg.StampBoxWidth,
                        cfg.StampBoxHeight
                    );

                    // Position barcode to the left of stamp box
                    int x = stampBox.X - barcodeBitmap.Width - cfg.BarcodeMargin;
                    int y = stampBox.Y + (stampBox.Height - barcodeBitmap.Height) / 2;

                    g.DrawImage(barcodeBitmap, new Point(x, y));

                    // SKU text above barcode
                    using (Font font = new Font(SystemFonts.DefaultFont.FontFamily, cfg.SkuFontSize))
                    {
                        string text = $"SKU: {sku}";
                        SizeF textSize = g.MeasureString(text, font);
                        float textX = x + (barcodeBitmap.Width - textSize.Width) / 2;
                        float textY = y - textSize.Height - cfg.TextGap;
                        g.DrawString(text, font, Brushes.Black, new PointF(textX, textY));
                    }

                    // UPC digits below barcode (large font)
                    using (Font font = new Font(SystemFonts.DefaultFont.FontFamily, cfg.UpcFontSize, FontStyle.Bold))
                    {
                        SizeF textSize = g.MeasureString(upc12, font);
                        float textX = x + (barcodeBitmap.Width - textSize.Width) / 2;
                        float textY = y + barcodeBitmap.Height + cfg.TextGap;
                        g.DrawString(upc12, font, Brushes.Black, new PointF(textX, textY));
                    }

                    // Save composite
                    string outputDir = Path.GetDirectoryName(csvPath);
                    string filename = Path.Combine(outputDir,"Barcode Exports", $"{sku}_postcard.png");
                    template.Save(filename, ImageFormat.Png);
                    Console.WriteLine($"Generated: {filename}");
                }
            }
        }
    }

    static int CalculateUpcCheckDigit(string upc11)
    {
        int sumOdd = 0, sumEven = 0;
        for (int i = 0; i < upc11.Length; i++)
        {
            int digit = upc11[i] - '0';
            if ((i + 1) % 2 == 1)
                sumOdd += digit;
            else
                sumEven += digit;
        }

        int total = sumOdd * 3 + sumEven;
        int remainder = total % 10;
        return remainder == 0 ? 0 : 10 - remainder;
    }
}

public class SkuRecord
{
    public string SKU { get; set; }
    public string ProductName { get; set; }
    public string Barcode { get; set; } // 11-digit barcode without check digit
}

public class TemplateConfig
{
    public int StampBoxWidth { get; set; }
    public int StampBoxHeight { get; set; }
    public int StampBoxMarginTop { get; set; }
    public int StampBoxMarginRight { get; set; }
    public int BarcodeMargin { get; set; }
    public int TextGap { get; set; }
    public int SkuFontSize { get; set; }
    public int UpcFontSize { get; set; }
}