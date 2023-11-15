using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

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

        internal static BitmapSource PasteImageFromClipboard()
        {
            if (Clipboard.GetData("DeviceIndependentBitmap") is not MemoryStream ms) return null;
            var dibBuffer = new byte[ms.Length];
            _ = ms.Read(dibBuffer, 0, dibBuffer.Length);

            BITMAPINFOHEADER infoHeader =
                BinaryStructConverter.FromByteArray<BITMAPINFOHEADER>(dibBuffer);

            var fileHeaderSize = Marshal.SizeOf(typeof(BITMAPFILEHEADER));
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

            return BitmapFrame.Create(msBitmap);
        }
    }
}
