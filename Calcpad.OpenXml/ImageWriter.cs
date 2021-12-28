using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using D = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using P = DocumentFormat.OpenXml.Drawing.Pictures;

namespace Calcpad.OpenXml
{
    internal static class ImageWriter
    {
        private static readonly Random _random = new();
        internal static OpenXmlElement AddImage(string src, string url, string name, int width, int height, MainDocumentPart mainPart)
        {
            ImageProcessor imageProcessor;
            if (src.StartsWith("data:image/"))
                imageProcessor = new Base64ImageProcessor(src);
            else if (src.Contains('/'))
            {
                if (!Uri.IsWellFormedUriString(src, UriKind.Absolute))
                    src = url + src;

                imageProcessor = new UrlImageProcessor(src);
                if (string.IsNullOrWhiteSpace(name))
                    name = src[(src.LastIndexOf('/') + 1)..];
            }
            else
            {
                imageProcessor = new FileImageProcessor(src);
                if (string.IsNullOrWhiteSpace(name))
                    name = src[(src.LastIndexOf('\\') + 1)..];
            }
            var imagePart = imageProcessor.GetImagePart(mainPart);
            if (width == 0 || height == 0)
            {
                var imageSize = imageProcessor.ImageSize;
                var w = imageSize.Item1;
                var h = imageSize.Item2;
                if (width == 0)
                    width = w;
                if (height == 0)
                    height = h;
                if (height != h)
                    width = (int)(width * height / (double)h);
                if (width != w)
                    height = (int)(height * width / (double)w);
            }
            return AddImageReference(name, width, height, mainPart.GetIdOfPart(imagePart));
        }

        private static OpenXmlElement AddImageReference(string name, int width, int height, string imageId)
        {
            const long px2emu = 9525;
            long w = width * px2emu, h = height * px2emu;
            // Define the reference of the image.
            var id = (uint)(_random.Next(0, int.MaxValue));
            var element =
                new Drawing(
                    new DW.Inline(
                        new DW.Extent() { Cx = w, Cy = h },
                        new DW.EffectExtent()
                        {
                            LeftEdge = 0L,
                            TopEdge = 0L,
                            RightEdge = 0L,
                            BottomEdge = 0L
                        },
                        new DW.DocProperties()
                        {
                            Id = (UInt32Value)id,
                            Name = name
                        },
                        new DW.NonVisualGraphicFrameDrawingProperties(
                            new D.GraphicFrameLocks() { NoChangeAspect = true }),
                        new D.Graphic(
                            new D.GraphicData(
                                new P.Picture(
                                    new P.NonVisualPictureProperties(
                                        new P.NonVisualDrawingProperties()
                                        {
                                            Id = (UInt32Value)0U,
                                            Name = name
                                        },
                                        new P.NonVisualPictureDrawingProperties()),
                                    new P.BlipFill(
                                        new D.Blip(
                                            new D.BlipExtensionList(
                                                new D.BlipExtension()
                                                {
                                                    Uri = "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                                })
                                        )
                                        {
                                            Embed = imageId,
                                            CompressionState =
                                            D.BlipCompressionValues.Print
                                        },
                                        new D.Stretch(
                                            new D.FillRectangle())),
                                    new P.ShapeProperties(
                                        new D.Transform2D(
                                            new D.Offset() { X = 0L, Y = 0L },
                                            new D.Extents() { Cx = w, Cy = h }),
                                        new D.PresetGeometry(
                                            new D.AdjustValueList()
                                        )
                                        { Preset = D.ShapeTypeValues.Rectangle }))
                            )
                            { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                    )
                    {
                        DistanceFromTop = 0U,
                        DistanceFromBottom = 0U,
                        DistanceFromLeft = 0U,
                        DistanceFromRight = 0U,
                        //EditId = "50D07946"
                    }
            );
            return new Run(element);
        }
    }
}
