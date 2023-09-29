using System;
using System.IO;
using System.Net.Http;
using DocumentFormat.OpenXml.Packaging;
using SkiaSharp;

namespace Calcpad.OpenXml
{
    internal abstract class ImageProcessor
    {
        protected readonly string src;
        public abstract ImagePart GetImagePart(MainDocumentPart mainPart);
        public abstract Tuple<int, int> ImageSize { get; }

        protected ImageProcessor(string imageSrc)
        {
            src = imageSrc;
        }

        protected static string GetImageDataType(string data)
        {
            var start = data.IndexOf('/') + 1;
            var end = data.IndexOf(';');
            if (start <= 0 || end <= 0)
#if BG
                throw new Exception("Невалидни данни на изображение.");
#else
                throw new Exception("Invalid image data.");
#endif
            return data[start..end];
        }

        protected static byte[] GetImageData(string data)
        {
            var start = data.IndexOf(',') + 1;
            if (start <= 0)
#if BG
                throw new Exception("Невалидни данни на изображение.");
#else
                throw new Exception("Invalid image data.");
#endif
            var base64 = data[start..];
            return Convert.FromBase64String(base64);
        }

        protected static ImagePartType GetImagePartType(string ext)
        {
            return ext.ToLowerInvariant() switch
            {
                "png" => ImagePartType.Png,
                "gif" => ImagePartType.Gif,
                "jpg" => ImagePartType.Jpeg,
                "jpeg" => ImagePartType.Jpeg,
                "bmp" => ImagePartType.Bmp,
                "tif" => ImagePartType.Tiff,
                "tiff" => ImagePartType.Tiff,
                "wmf" => ImagePartType.Wmf,
                "emf" => ImagePartType.Emf,
                "ico" => ImagePartType.Icon,
                "pcx" => ImagePartType.Pcx,
                "svg" => ImagePartType.Svg, 
#if BG
                _ => throw new Exception($"Неподдържан тип изображение: \"{ext}\".")
#else
                _ => throw new Exception($"Unsupported image type: \"{ext}\".")
#endif
            };
        }
    }

    internal class Base64ImageProcessor : ImageProcessor
    {
        private readonly byte[] _imageData;

        public Base64ImageProcessor(string imageSrc) : base(imageSrc)
        {
            _imageData = GetImageData(imageSrc);
        }

        public override ImagePart GetImagePart(MainDocumentPart mainPart)
        {
            var ext = GetImageDataType(src);
            var imageType = GetImagePartType(ext);
            var imagePart = mainPart.AddImagePart(imageType);
            using var stream = new MemoryStream(_imageData);
            imagePart.FeedData(stream);
            return imagePart;
        }

        public override Tuple<int, int> ImageSize
        {
            get
            {
                using var stream = new MemoryStream(_imageData);
                using var img = SKBitmap.Decode(stream);
                return new(img.Width, img.Height);
            }
        }
    }

    internal class UrlImageProcessor : ImageProcessor
    {
        public UrlImageProcessor(string imageSrc) : base(imageSrc) { }

        public override ImagePart GetImagePart(MainDocumentPart mainPart)
        {
            var ext = src[(src.LastIndexOf('.') + 1)..];
            var imageType = GetImagePartType(ext);
            var imagePart = mainPart.AddImagePart(imageType);
            var uri = new Uri(src);
            if (uri.IsFile)
            {
                using var stream = new FileStream(uri.LocalPath, FileMode.Open, FileAccess.Read);
                imagePart.FeedData(stream);
            }
            else
            {
                using var webClient = new HttpClient();
                using var stream = webClient.GetStreamAsync(src).Result;
                imagePart.FeedData(stream);
            }
            return imagePart;
        }

        public override Tuple<int, int> ImageSize
        {
            get
            {
                var isBitmap = string.Equals(Path.GetExtension(src), ".bmp", StringComparison.OrdinalIgnoreCase);    
                var uri = new Uri(src);
                if (uri.IsFile)
                {
                    if (isBitmap )
                    {
                        using var bmp = SKBitmap.Decode(src);
                        return new(bmp.Width, bmp.Height);
                    }
                    using var data = SKData.Create(src);
                    using var img = SKImage.FromEncodedData(data);
                    return new(img.Width, img.Height);
                }
                else
                {
                    using var webClient = new HttpClient();
                    using var stream = webClient.GetStreamAsync(src).Result;
                    if (isBitmap)
                    {
                        using var bmp = SKBitmap.Decode(stream);
                        return new(bmp.Width, bmp.Height);
                    }
                    using var data = SKData.Create(stream);
                    using var img = SKImage.FromEncodedData(data);
                    return new(img.Width, img.Height);
                }
            }
        }
    }

    internal class FileImageProcessor : ImageProcessor
    {
        private readonly FileInfo _fileInfo;

        public FileImageProcessor(string imageSrc) : base(imageSrc)
        {
            _fileInfo = new FileInfo(imageSrc);
            if (!_fileInfo.Exists)
#if BG
                throw new Exception($"Невалиден файл с изображение: {imageSrc}.");
#else
                throw new Exception($"Invalid image file: {imageSrc}.");
#endif
        }
        public override ImagePart GetImagePart(MainDocumentPart mainPart)
        {
            var ext = _fileInfo.Extension[1..];
            var imageType = GetImagePartType(ext);
            var imagePart = mainPart.AddImagePart(imageType);
            using var stream = new FileStream(src, FileMode.Open, FileAccess.Read);
            imagePart.FeedData(stream);
            return imagePart;
        }

        public override Tuple<int, int> ImageSize
        {
            get
            {
                if (string.Equals(Path.GetExtension(src), ".bmp",StringComparison.OrdinalIgnoreCase))
                {
                    using var bmp = SKBitmap.Decode(src);
                    return new(bmp.Width, bmp.Height);
                }
                using var data = SKData.Create(src);
                using var img = SKImage.FromEncodedData(data);
                return new(img.Width, img.Height);
            }
        }
    }

    internal class SvgImageProcessor : ImageProcessor
    {
        public SvgImageProcessor(string imageSrc) : base(imageSrc) { }

        public override ImagePart GetImagePart(MainDocumentPart mainPart)
        {
            var imagePart = mainPart.AddImagePart(ImagePartType.Svg);
            var bytes = System.Text.Encoding.UTF8.GetBytes(src);    
            using var stream = new MemoryStream(bytes); 
            imagePart.FeedData(stream);
            return imagePart;
        }

        public override Tuple<int, int> ImageSize => new(0, 0);
    }
}
