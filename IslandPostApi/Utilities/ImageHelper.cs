using System.Drawing;
using System.Drawing.Imaging;

namespace IslandPostApi.Utilities;

public static class ImageHelper
{
    /// <summary>
    /// Generates a thumbnail image from raw photo bytes.
    /// </summary>
    /// <param name="photoBytes">Original image as byte array</param>
    /// <param name="maxWidth">Max width of thumbnail</param>
    /// <param name="maxHeight">Max height of thumbnail</param>
    /// <returns>Thumbnail as byte array (JPEG)</returns>
    public static byte[] GenerateThumbnail(byte[] photoBytes, int maxWidth = 200, int maxHeight = 200)
    {
        if (photoBytes == null || photoBytes.Length == 0)
            throw new ArgumentException("Photo bytes cannot be null or empty");

        using var inputStream = new MemoryStream(photoBytes);
        using var originalImage = Image.FromStream(inputStream);

        // Calculate new size while preserving aspect ratio
        var ratioX = (double)maxWidth / originalImage.Width;
        var ratioY = (double)maxHeight / originalImage.Height;
        var ratio = Math.Min(ratioX, ratioY);

        var newWidth = (int)(originalImage.Width * ratio);
        var newHeight = (int)(originalImage.Height * ratio);

        using var thumbnail = new Bitmap(newWidth, newHeight);
        using (var graphics = Graphics.FromImage(thumbnail))
        {
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
        }

        using var outputStream = new MemoryStream();
        thumbnail.Save(outputStream, ImageFormat.Jpeg);
        return outputStream.ToArray();
    }
}
