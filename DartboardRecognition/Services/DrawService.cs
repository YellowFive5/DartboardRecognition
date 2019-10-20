#region Usings

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using DartboardRecognition.Windows;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Point = System.Drawing.Point;

#endregion

namespace DartboardRecognition.Services
{
    public class DrawService
    {
        private readonly MainWindow mainWindow;
        public Bgr CamRoiRectColor = new Bgr(Color.LawnGreen);
        public readonly int CamRoiRectThickness = 5;
        public Bgr CamSurfaceLineColor = new Bgr(Color.Red);
        public readonly int CamSurfaceLineThickness = 5;
        public MCvScalar CamContourRectColor = new Bgr(Color.Blue).MCvScalar;
        public int CamContourRectThickness = 5;
        public MCvScalar CamSpikeLineColor = new Bgr(Color.White).MCvScalar;
        public int CamSpikeLineThickness = 4;
        public MCvScalar ProjectionPoiColor = new Bgr(Color.Yellow).MCvScalar;
        public int ProjectionPoiRadius = 6;
        public int ProjectionPoiThickness = 6;
        public MCvScalar CamContourColor = new Bgr(Color.Violet).MCvScalar;
        public int CamContourThickness = 2;
        private readonly MCvScalar ProjectionGridColor = new Bgr(Color.DarkGray).MCvScalar;
        public MCvScalar ProjectionSurfaceLineColor = new Bgr(Color.Red).MCvScalar;
        public int ProjectionSurfaceLineThickness = 2;
        private MCvScalar ProjectionRayColor = new Bgr(Color.DeepSkyBlue).MCvScalar;
        public int ProjectionRayThickness = 2;
        private MCvScalar PoiColor = new Bgr(Color.MediumVioletRed).MCvScalar;
        private readonly int PoiRadius = 6;
        private readonly int PoiThickness = 6;
        private readonly int ProjectionGridThickness = 2;
        private Bgr ProjectionDigitsColor = new Bgr(Color.White);
        private readonly double ProjectionDigitsScale = 2;
        private readonly int ProjectionDigitsThickness = 2;
        public readonly int ProjectionCoefficent = 3;
        public PointF ProjectionCenterPoint;
        private int ProjectionLineCam1Bias = 0;
        private int ProjectionLineCam2Bias = 0;
        public readonly int ProjectionFrameSide = 1300;
        private Image<Bgr, byte> DartboardProjectionFrameBackground { get; }
        private Image<Bgr, byte> DartboardProjectionWorkingFrame { get; set; }

        public DrawService(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            DartboardProjectionFrameBackground = new Image<Bgr, byte>(ProjectionFrameSide,
                                                                      ProjectionFrameSide);
            ProjectionCenterPoint = new PointF((float) DartboardProjectionFrameBackground.Width / 2,
                                               (float) DartboardProjectionFrameBackground.Height / 2);
        }

        public void DrawLine(Image<Bgr, byte> image,
                             PointF point1,
                             PointF point2,
                             MCvScalar color,
                             int thickness)
        {
            CvInvoke.Line(image,
                          new Point((int) point1.X, (int) point1.Y),
                          new Point((int) point2.X, (int) point2.Y),
                          color,
                          thickness);
        }

        public void DrawRectangle(Image<Bgr, byte> image,
                                  Rectangle rectangle,
                                  MCvScalar color,
                                  int thickness)
        {
            CvInvoke.Rectangle(image, rectangle, color, thickness);
        }

        public void DrawCircle(Image<Bgr, byte> image,
                               PointF centerpoint,
                               int radius,
                               MCvScalar color,
                               int thickness)
        {
            CvInvoke.Circle(image,
                            new Point((int) centerpoint.X, (int) centerpoint.Y),
                            radius,
                            color,
                            thickness);
        }

        public void DrawString(Image<Bgr, byte> image,
                               string text,
                               int pointX,
                               int pointY,
                               double scale,
                               Bgr color,
                               int thickness)
        {
            image.Draw(text,
                       new Point(pointX, pointY),
                       FontFace.HersheySimplex,
                       scale,
                       color,
                       thickness);
        }

        public void PrintThrow(Throw thrw)
        {
            mainWindow.Dispatcher.Invoke(new Action(() => mainWindow.PointsBox.Text = thrw.ToString()));
        }

        public void ProjectionDrawThrow(PointF poi, bool exclusiveDraw = true)
        {
            if (exclusiveDraw)
            {
                DartboardProjectionWorkingFrame = DartboardProjectionFrameBackground.Clone();
            }

            DrawCircle(DartboardProjectionWorkingFrame, poi, PoiRadius, PoiColor, PoiThickness);

            mainWindow.Dispatcher.Invoke(new Action(() => mainWindow.DartboardProjectionImageBox.Source = ToBitmap(DartboardProjectionWorkingFrame)));
        }

        public void ProjectionDrawLine(PointF point1, PointF point2, bool exclusiveDraw = true)
        {
            if (exclusiveDraw)
            {
                DartboardProjectionWorkingFrame = DartboardProjectionFrameBackground.Clone();
            }

            DrawLine(DartboardProjectionWorkingFrame, point1, point2, ProjectionRayColor, PoiThickness);

            mainWindow.Dispatcher.Invoke(new Action(() => mainWindow.DartboardProjectionImageBox.Source = ToBitmap(DartboardProjectionWorkingFrame)));
        }

        public void DrawProjection()
        {
            // Draw dartboard projection
            DrawCircle(DartboardProjectionFrameBackground, ProjectionCenterPoint, ProjectionCoefficent * 7, ProjectionGridColor, ProjectionGridThickness);
            DrawCircle(DartboardProjectionFrameBackground, ProjectionCenterPoint, ProjectionCoefficent * 17, ProjectionGridColor, ProjectionGridThickness);
            DrawCircle(DartboardProjectionFrameBackground, ProjectionCenterPoint, ProjectionCoefficent * 95, ProjectionGridColor, ProjectionGridThickness);
            DrawCircle(DartboardProjectionFrameBackground, ProjectionCenterPoint, ProjectionCoefficent * 105, ProjectionGridColor, ProjectionGridThickness);
            DrawCircle(DartboardProjectionFrameBackground, ProjectionCenterPoint, ProjectionCoefficent * 160, ProjectionGridColor, ProjectionGridThickness);
            DrawCircle(DartboardProjectionFrameBackground, ProjectionCenterPoint, ProjectionCoefficent * 170, ProjectionGridColor, ProjectionGridThickness);
            for (var i = 0; i <= 360; i += 9)
            {
                var segmentPoint1 = new PointF((float) (ProjectionCenterPoint.X + Math.Cos(0.314159 * i - 0.15708) * ProjectionCoefficent * 170),
                                               (float) (ProjectionCenterPoint.Y + Math.Sin(0.314159 * i - 0.15708) * ProjectionCoefficent * 170));
                var segmentPoint2 = new PointF((float) (ProjectionCenterPoint.X + Math.Cos(0.314159 * i - 0.15708) * ProjectionCoefficent * 17),
                                               (float) (ProjectionCenterPoint.Y + Math.Sin(0.314159 * i - 0.15708) * ProjectionCoefficent * 17));
                DrawLine(DartboardProjectionFrameBackground, segmentPoint1, segmentPoint2, ProjectionGridColor, ProjectionGridThickness);
            }

            // Draw digits
            var sectors = new List<int>()
                          {
                              11, 14, 9, 12, 5,
                              20, 1, 18, 4, 13,
                              6, 10, 15, 2, 17,
                              3, 19, 7, 16, 8
                          };
            var startRadSector = -3.14159;
            var radSectorStep = 0.314159;
            var radSector = startRadSector;
            foreach (var sector in sectors)
            {
                DrawString(DartboardProjectionFrameBackground,
                           sector.ToString(),
                           (int) (ProjectionCenterPoint.X - 40 + Math.Cos(radSector) * ProjectionCoefficent * 190),
                           (int) (ProjectionCenterPoint.Y + 20 + Math.Sin(radSector) * ProjectionCoefficent * 190),
                           ProjectionDigitsScale,
                           ProjectionDigitsColor,
                           ProjectionDigitsThickness);
                radSector += radSectorStep;
            }

            DartboardProjectionWorkingFrame = DartboardProjectionFrameBackground.Clone();

            mainWindow.Dispatcher.Invoke(new Action(() => mainWindow.DartboardProjectionImageBox.Source = ToBitmap(DartboardProjectionWorkingFrame)));
        }

        public BitmapImage ToBitmap(IImage image)
        {
            var imageToSave = new BitmapImage();

            using (var stream = new MemoryStream())
            {
                image.Bitmap.Save(stream, ImageFormat.Bmp);
                imageToSave.BeginInit();
                imageToSave.StreamSource = new MemoryStream(stream.ToArray());
                imageToSave.EndInit();
            }

            return imageToSave;
        }

        private void SaveToFile(BitmapSource image, string path = null)
        {
            var pathString = path ?? "image.png";
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using (var fileStream = new FileStream(pathString, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
    }
}