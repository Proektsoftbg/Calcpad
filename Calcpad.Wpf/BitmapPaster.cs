using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using SD = System.Drawing;

namespace Calcpad.Wpf
{
    internal static class BitmapPaster
    {
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct BITMAPFILEHEADER
        {
            public static readonly short BM = 0x4d42; // BM

            public short bfType;
            public int bfSize;
            public short bfReserved1;
            public short bfReserved2;
            public int bfOffBits;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;
        }

        internal static void PasteImageFromClipboard(string path)
        {
            if (Clipboard.GetData("DeviceIndependentBitmap") is not MemoryStream ms) return;
            var dibBuffer = new byte[ms.Length];
            ms.Read(dibBuffer, 0, dibBuffer.Length);

            BITMAPINFOHEADER infoHeader =
                BinaryStructConverter.FromByteArray<BITMAPINFOHEADER>(dibBuffer);

            var fileHeaderSize = Marshal.SizeOf<BITMAPFILEHEADER>();
            var infoHeaderSize = infoHeader.biSize;
            var fileSize = fileHeaderSize + infoHeader.biSize + infoHeader.biSizeImage;

            BITMAPFILEHEADER fileHeader = new()
            {
                bfType = BITMAPFILEHEADER.BM,
                bfSize = fileSize,
                bfReserved1 = 0,
                bfReserved2 = 0,
                bfOffBits = fileHeaderSize + infoHeaderSize + infoHeader.biClrUsed * 4
            };

            byte[] fileHeaderBytes =
                BinaryStructConverter.ToByteArray(fileHeader);

            MemoryStream msBitmap = new();
            msBitmap.Write(fileHeaderBytes, 0, fileHeaderSize);
            msBitmap.Write(dibBuffer, 0, dibBuffer.Length);
            msBitmap.Seek(0, SeekOrigin.Begin);
            var src = BitmapFrame.Create(msBitmap);
            SD.Bitmap bitmap = new(src.PixelWidth, src.PixelHeight, SD.Imaging.PixelFormat.Format32bppPArgb);
            SD.Imaging.BitmapData data = bitmap.LockBits(new SD.Rectangle(SD.Point.Empty, bitmap.Size), SD.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            src.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bitmap.UnlockBits(data);
            SD.Image image = bitmap;
            image.Save(path, SD.Imaging.ImageFormat.Png);
        }
    }
}
