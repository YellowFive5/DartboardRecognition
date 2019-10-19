#region Usings

using System;
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
        public Bgr CamRoiRectColor { get; } = new Bgr(Color.LawnGreen);
        public int CamRoiRectThickness { get; } = 5;
        public Bgr CamSurfaceLineColor { get; } = new Bgr(Color.Red);
        public int CamSurfaceLineThickness { get; } = 5;
        public MCvScalar CamContourRectColor { get; } = new Bgr(Color.Blue).MCvScalar;
        public int CamContourRectThickness { get; } = 5;
        public MCvScalar CamSpikeLineColor { get; } = new Bgr(Color.White).MCvScalar;
        public int CamSpikeLineThickness { get; } = 4;
        public MCvScalar ProjectionPoiColor { get; } = new Bgr(Color.Yellow).MCvScalar;
        public int ProjectionPoiRadius { get; } = 6;
        public int ProjectionPoiThickness { get; } = 6;
        public MCvScalar CamContourColor { get; } = new Bgr(Color.Violet).MCvScalar;
        public int CamContourThickness { get; } = 2;
        private MCvScalar ProjectionGridColor { get; } = new Bgr(Color.DarkGray).MCvScalar;
        public MCvScalar ProjectionSurfaceLineColor { get; } = new Bgr(Color.Red).MCvScalar;
        public int ProjectionSurfaceLineThickness { get; } = 2;
        public MCvScalar ProjectionRayColor { get; } = new Bgr(Color.DeepSkyBlue).MCvScalar;
        public int ProjectionRayThickness { get; } = 2;
        private MCvScalar PoiColor { get; } = new Bgr(Color.MediumVioletRed).MCvScalar;
        private int PoiRadius { get; } = 6;
        private int PoiThickness { get; } = 6;
        private int ProjectionGridThickness { get; } = 2;
        private Bgr ProjectionDigitsColor { get; } = new Bgr(Color.White);
        private double ProjectionDigitsScale { get; } = 2;
        private int ProjectionDigitsThickness { get; } = 2;
        public int ProjectionCoefficent { get; } = 3;
        public PointF ProjectionCenterPoint { get; }
        private int ProjectionLineCam1Bias { get; } = 0;
        private int ProjectionLineCam2Bias { get; } = 0;
        public int ProjectionFrameSide { get; } = 1300;
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
            var startAngle = 0;
            var sectorAngle = 0.314159;
            DrawString(DartboardProjectionFrameBackground,
                       "6",
                       (int) (ProjectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 0) * ProjectionCoefficent * 180),
                       (int) (ProjectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 0) * ProjectionCoefficent * 180),
                       ProjectionDigitsScale,
                       ProjectionDigitsColor,
                       ProjectionDigitsThickness);
            DrawString(DartboardProjectionFrameBackground,
                       "10",
                       (int) (ProjectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 1 + 0.05) * ProjectionCoefficent * 180),
                       (int) (ProjectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 1 + 0.05) * ProjectionCoefficent * 180),
                       ProjectionDigitsScale,
                       ProjectionDigitsColor,
                       ProjectionDigitsThickness);
            DrawString(DartboardProjectionFrameBackground,
                       "15",
                       (int) (ProjectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 2 + 0.05) * ProjectionCoefficent * 180),
                       (int) (ProjectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 2 + 0.05) * ProjectionCoefficent * 180),
                       ProjectionDigitsScale,
                       ProjectionDigitsColor,
                       ProjectionDigitsThickness);
            DrawString(DartboardProjectionFrameBackground,
                       "2",
                       (int) (ProjectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 3 + 0.05) * ProjectionCoefficent * 190),
                       (int) (ProjectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 3 + 0.05) * ProjectionCoefficent * 190),
                       ProjectionDigitsScale,
                       ProjectionDigitsColor,
                       ProjectionDigitsThickness);
            DrawString(DartboardProjectionFrameBackground,
                       "17",
                       (int) (ProjectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 4 + 0.05) * ProjectionCoefficent * 190),
                       (int) (ProjectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 4 + 0.05) * ProjectionCoefficent * 190),
                       ProjectionDigitsScale,
                       ProjectionDigitsColor,
                       ProjectionDigitsThickness);
            DrawString(DartboardProjectionFrameBackground,
                       "3",
                       (int) (ProjectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 5 + 0.05) * ProjectionCoefficent * 190),
                       (int) (ProjectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 5 + 0.05) * ProjectionCoefficent * 190),
                       ProjectionDigitsScale,
                       ProjectionDigitsColor,
                       ProjectionDigitsThickness);
            DrawString(DartboardProjectionFrameBackground,
                       "19",
                       (int) (ProjectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 6 + 0.05) * ProjectionCoefficent * 200),
                       (int) (ProjectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 6 + 0.05) * ProjectionCoefficent * 200),
                       ProjectionDigitsScale,
                       ProjectionDigitsColor,
                       ProjectionDigitsThickness);
            DrawString(DartboardProjectionFrameBackground,
                       "7",
                       (int) (ProjectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 7) * ProjectionCoefficent * 200),
                       (int) (ProjectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 7) * ProjectionCoefficent * 200),
                       ProjectionDigitsScale,
                       ProjectionDigitsColor,
                       ProjectionDigitsThickness);
            DrawString(DartboardProjectionFrameBackground,
                       "16",
                       (int) (ProjectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 8) * ProjectionCoefficent * 210),
                       (int) (ProjectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 8) * ProjectionCoefficent * 210),
                       ProjectionDigitsScale,
                       ProjectionDigitsColor,
                       ProjectionDigitsThickness);
            DrawString(DartboardProjectionFrameBackground,
                       "8",
                       (int) (ProjectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 9) * ProjectionCoefficent * 200),
                       (int) (ProjectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 9) * ProjectionCoefficent * 200),
                       ProjectionDigitsScale,
                       ProjectionDigitsColor,
                       ProjectionDigitsThickness);
            DrawString(DartboardProjectionFrameBackground,
                       "11",
                       (int) (ProjectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 10) * ProjectionCoefficent * 200),
                       (int) (ProjectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 10) * ProjectionCoefficent * 200),
                       ProjectionDigitsScale,
                       ProjectionDigitsColor,
                       ProjectionDigitsThickness);
            DrawString(DartboardProjectionFrameBackground,
                       "14",
                       (int) (ProjectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 11 - 0.05) * ProjectionCoefficent * 200),
                       (int) (ProjectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 11 - 0.05) * ProjectionCoefficent * 200),
                       ProjectionDigitsScale,
                       ProjectionDigitsColor,
                       ProjectionDigitsThickness);
            DrawString(DartboardProjectionFrameBackground,
                       "9",
                       (int) (ProjectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 12 - 0.05) * ProjectionCoefficent * 200),
                       (int) (ProjectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 12 - 0.05) * ProjectionCoefficent * 200),
                       ProjectionDigitsScale,
                       ProjectionDigitsColor,
                       ProjectionDigitsThickness);
            DrawString(DartboardProjectionFrameBackground,
                       "12",
                       (int) (ProjectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 13 - 0.05) * ProjectionCoefficent * 200),
                       (int) (ProjectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 13 - 0.05) * ProjectionCoefficent * 200),
                       ProjectionDigitsScale,
                       ProjectionDigitsColor,
                       ProjectionDigitsThickness);
            DrawString(DartboardProjectionFrameBackground,
                       "5",
                       (int) (ProjectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 14 - 0.05) * ProjectionCoefficent * 190),
                       (int) (ProjectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 14 - 0.05) * ProjectionCoefficent * 190),
                       ProjectionDigitsScale,
                       ProjectionDigitsColor,
                       ProjectionDigitsThickness);
            DrawString(DartboardProjectionFrameBackground,
                       "20",
                       (int) (ProjectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 15 - 0.05) * ProjectionCoefficent * 180),
                       (int) (ProjectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 15 - 0.05) * ProjectionCoefficent * 180),
                       ProjectionDigitsScale,
                       ProjectionDigitsColor,
                       ProjectionDigitsThickness);
            DrawString(DartboardProjectionFrameBackground,
                       "1",
                       (int) (ProjectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 16 - 0.05) * ProjectionCoefficent * 180),
                       (int) (ProjectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 16 - 0.05) * ProjectionCoefficent * 180),
                       ProjectionDigitsScale,
                       ProjectionDigitsColor,
                       ProjectionDigitsThickness);
            DrawString(DartboardProjectionFrameBackground,
                       "18",
                       (int) (ProjectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 17 - 0.05) * ProjectionCoefficent * 180),
                       (int) (ProjectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 17 - 0.05) * ProjectionCoefficent * 180),
                       ProjectionDigitsScale,
                       ProjectionDigitsColor,
                       ProjectionDigitsThickness);
            DrawString(DartboardProjectionFrameBackground,
                       "4",
                       (int) (ProjectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 18 - 0.05) * ProjectionCoefficent * 180),
                       (int) (ProjectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 18 - 0.05) * ProjectionCoefficent * 180),
                       ProjectionDigitsScale,
                       ProjectionDigitsColor,
                       ProjectionDigitsThickness);
            DrawString(DartboardProjectionFrameBackground,
                       "13",
                       (int) (ProjectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 19) * ProjectionCoefficent * 180),
                       (int) (ProjectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 19) * ProjectionCoefficent * 180),
                       ProjectionDigitsScale,
                       ProjectionDigitsColor,
                       ProjectionDigitsThickness);

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