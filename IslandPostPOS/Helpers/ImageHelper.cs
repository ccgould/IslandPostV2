using System;
using System.IO;

namespace IslandPostPOS.Helpers;

public static class ImageHelper
{
    /// <summary>
    /// Reads an image file from disk and converts it to a Base64 string.
    /// </summary>
    public static string ImageFileToBase64(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Image file not found.", filePath);

        byte[] imageBytes = File.ReadAllBytes(filePath);
        return Convert.ToBase64String(imageBytes);
    }

    /// <summary>
    /// Converts a Base64 string back into a byte array.
    /// </summary>
    public static byte[] Base64ToBytes(string base64String)
    {
        if (string.IsNullOrWhiteSpace(base64String))
            throw new ArgumentException("Base64 string is null or empty.", nameof(base64String));

        return Convert.FromBase64String(base64String);
    }

    /// <summary>
    /// Saves a Base64 string as an image file on disk.
    /// </summary>
    public static void SaveBase64ToImageFile(string base64String, string outputPath)
    {
        byte[] imageBytes = Base64ToBytes(base64String);
        File.WriteAllBytes(outputPath, imageBytes);
    }

    /// <summary>
    /// Converts a byte array (e.g., from DB) into a Base64 string.
    /// </summary>
    public static string BytesToBase64(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            throw new ArgumentException("Byte array is null or empty.", nameof(bytes));

        return Convert.ToBase64String(bytes);
    }
}
