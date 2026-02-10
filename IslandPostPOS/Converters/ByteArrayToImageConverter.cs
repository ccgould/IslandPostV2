using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace IslandPostPOS.Converters
{
    public class ByteArrayToImageConverter : IValueConverter
    {
        // Thread-safe cache: key = hash of byte[] content
        private static readonly ConcurrentDictionary<int, BitmapImage> _cache = new();

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is byte[] bytes && bytes.Length > 0)
            {
                try
                {
                    var image = new BitmapImage();
                    using var ms = new MemoryStream(bytes);
                    ms.Position = 0;
                    image.SetSource(ms.AsRandomAccessStream());
                    return image;
                }
                catch
                {
                    return new BitmapImage(new Uri("ms-appx:///Assets/No-Image-Placeholder.png"));
                }
            }

            // If null or empty, return placeholder
            return new BitmapImage(new Uri("ms-appx:///Assets/No-Image-Placeholder.png"));

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}