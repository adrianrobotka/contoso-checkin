using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ContosoCheckIn
{
    public class Visualization
    {
        private static SolidColorBrush sideFaceFrameBrush = new SolidColorBrush(new Color { R = 0, G = 0, B = 255, A = 255 });
        private static SolidColorBrush mainFaceFrameBrush = new SolidColorBrush(new Color { R = 255, G = 0, B = 0, A = 255 });
        private static Typeface s_typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);

        private static BitmapSource DrawOverlay(BitmapSource baseImage, Action<DrawingContext, double> drawAction)
        {
            double annotationScale = baseImage.PixelHeight / 320;

            DrawingVisual visual = new DrawingVisual();
            DrawingContext drawingContext = visual.RenderOpen();

            drawingContext.DrawImage(baseImage, new Rect(0, 0, baseImage.Width, baseImage.Height));

            drawAction(drawingContext, annotationScale);

            drawingContext.Close();

            RenderTargetBitmap outputBitmap = new RenderTargetBitmap(
                baseImage.PixelWidth, baseImage.PixelHeight,
                baseImage.DpiX, baseImage.DpiY, PixelFormats.Pbgra32);

            outputBitmap.Render(visual);

            return outputBitmap;
        }

        public static BitmapSource DrawFaces(BitmapSource baseImage, Face[] faces)
        {
            if (faces == null || faces.Length == 0)
            {
                return baseImage;
            }
            
            Action<DrawingContext, double> drawAction = (drawingContext, annotationScale) =>
            {
                for (int i = 0; i < faces.Length; i++)
                {
                    var face = faces[i];
                    if (face.FaceRectangle == null) { continue; }

                    Rect faceRect = new Rect(
                        face.FaceRectangle.Left, face.FaceRectangle.Top,
                        face.FaceRectangle.Width, face.FaceRectangle.Height);
                    string textOnHead = "";

                    SolidColorBrush brush;

                    double faceFrameLineFactor = 1;

                    int biggestFaceIndex = Util.GetDominantFaceIndex(faces);

                    if (i == biggestFaceIndex)
                    {
                        brush = mainFaceFrameBrush;
                        textOnHead = "Identifying...";
                        faceFrameLineFactor = 3;
                    }
                    else
                    {
                        brush = sideFaceFrameBrush;
                    }

                    faceRect.Inflate(6 * annotationScale, 6 * annotationScale);

                    double lineThickness = (3 + faceFrameLineFactor) * annotationScale;

                    drawingContext.DrawRectangle(
                        Brushes.Transparent,
                        new Pen(brush, lineThickness),
                        faceRect);

                    if (textOnHead != "")
                    {
                        FormattedText ft = new FormattedText(textOnHead,
                            CultureInfo.CurrentCulture, FlowDirection.LeftToRight, s_typeface,
                            16 * annotationScale, Brushes.Black);

                        var pad = 3 * annotationScale;

                        var ypad = pad;
                        var xpad = pad + 4 * annotationScale;
                        var origin = new Point(
                            faceRect.Left + xpad - lineThickness / 2,
                            faceRect.Top - ft.Height - ypad + lineThickness / 2);
                        var rect = ft.BuildHighlightGeometry(origin).GetRenderBounds(null);
                        rect.Inflate(xpad, ypad);

                        drawingContext.DrawRectangle(brush, null, rect);
                        drawingContext.DrawText(ft, origin);
                    }
                }
            };

            return DrawOverlay(baseImage, drawAction);
        }
    }
}
