using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Office2019.Drawing.SVG;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.IO;
using D = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using P = DocumentFormat.OpenXml.Drawing.Pictures;

namespace Calcpad.OpenXml
{
    internal static class ImageWriter
    {
        private static readonly Random Random = new();
        internal static OpenXmlElement AddImage(string src, string url, string name, int width, int height, MainDocumentPart mainPart, string align)
        {
            ImageProcessor imageProcessor;
            if (src.StartsWith("<svg", StringComparison.OrdinalIgnoreCase))
                imageProcessor = new SvgImageProcessor(src);
            else if (src.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
                imageProcessor = new Base64ImageProcessor(src);
            else if (src.Contains('/'))
            {
                if (!Uri.IsWellFormedUriString(src, UriKind.Absolute))
                    src = url + src;

                var slash = '/';
                if (src.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
                {
                    src = src[8..].Replace('/', '\\');
                    imageProcessor = new FileImageProcessor(src);
                    slash = '\\';
                }
                else if (Path.Exists(src))
                    imageProcessor = new FileImageProcessor(src);
                else
                    imageProcessor = new UrlImageProcessor(src);

                if (string.IsNullOrWhiteSpace(name))
                    name = src[(src.LastIndexOf(slash) + 1)..];

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
                (int w, int h) = imageProcessor.ImageSize;
                if (width == 0)
                    width = w;
                if (height == 0)
                    height = h;
                if (height != h)
                    width = (int)(width * height / (double)h);
                if (width != w)
                    height = (int)(height * width / (double)w);
            }
            return AddImageReference(name, width, height, mainPart.GetIdOfPart(imagePart), align);
        }

        private static Paragraph AddImageReference(string name, int width, int height, string imageId, string align)
        {
            const long px2emu = 9525;
            long w = width * px2emu, h = height * px2emu;
            if (align == "left" || align == "right")
                return AddFloatingImage(name, w, h, imageId, align);

            return new Paragraph(
                new Run(
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
                                Id = (UInt32Value)(uint)Random.Next(0, int.MaxValue),
                                Name = name
                            },
                            new DW.NonVisualGraphicFrameDrawingProperties(
                                new D.GraphicFrameLocks() { NoChangeAspect = true }),
                            AddPicture(name, w, h, imageId)
                        )
                        {
                            DistanceFromTop = 0U,
                            DistanceFromBottom = 0U,
                            DistanceFromLeft = 0U,
                            DistanceFromRight = 0U,
                        }
                    )
                )
            );
        }

        private static Paragraph AddFloatingImage(string name, long w, long h, string imageId, string align)
        {
            return new Paragraph(
                new ParagraphProperties(
                    new SpacingBetweenLines()
                    {
                        Before = "0",
                        After = "0",
                        Line = "0",
                        LineRule = LineSpacingRuleValues.Exact
                    }
                ),
                new Run(
                    new RunProperties(new FontSize() { Val = "2" }),
                    new Text("⚓"),
                    new Drawing(
                        new DW.Anchor(
                            new DW.SimplePosition() { X = 0L, Y = 0L },
                            new DW.HorizontalPosition(
                                new DW.HorizontalAlignment(align)
                            )
                            {
                                RelativeFrom =
                                    DW.HorizontalRelativePositionValues.Margin
                            },
                            new DW.VerticalPosition(new DW.PositionOffset("0"))
                            {
                                RelativeFrom =
                                DW.VerticalRelativePositionValues.Paragraph
                            },
                            new DW.Extent() { Cx = w, Cy = h },
                            new DW.EffectExtent()
                            {
                                LeftEdge = 0L,
                                TopEdge = 0L,
                                RightEdge = 0L,
                                BottomEdge = 0L
                            },
                            new DW.WrapSquare() { WrapText = DW.WrapTextValues.Largest },
                            new DW.DocProperties()
                            {
                                Id = (UInt32Value)(uint)Random.Next(0, int.MaxValue),
                                Name = name
                            },
                            new DW.NonVisualGraphicFrameDrawingProperties(
                                    new D.GraphicFrameLocks() { NoChangeAspect = true }
                            ),
                            AddPicture(name, w, h, imageId)
                        )
                        {
                            DistanceFromTop = (UInt32Value)0U,
                            DistanceFromBottom = (UInt32Value)0U,
                            DistanceFromLeft = (UInt32Value)114300U,
                            DistanceFromRight = (UInt32Value)114300U,
                            SimplePos = false,
                            RelativeHeight = (UInt32Value)251658240U,
                            BehindDoc = true,
                            Locked = false,
                            LayoutInCell = true,
                            AllowOverlap = true
                        }
                    )
                )
            );
        }

        private static D.Graphic AddPicture(string name, long w, long h, string imageId)
        {
            var g = new D.Graphic(
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
                { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
            );
            if (Path.GetExtension(name).Equals(".svg", StringComparison.OrdinalIgnoreCase))
            {
                var pic = g.GraphicData.ChildElements.First<P.Picture>();
                var blip = pic.BlipFill.Blip;
                var list = blip.ChildElements.First<D.BlipExtensionList>();
                list.AddChild(new D.BlipExtension(new SVGBlip { Embed = imageId })
                {
                    Uri = "{96DAC541-7B7A-43D3-8B79-37D633B846F1}"
                });
            }
            return g;
        }
    }
}
