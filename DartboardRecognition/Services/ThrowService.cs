#region Usings

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media.Imaging;
using DartboardRecognition.Windows;
using Emgu.CV;
using Emgu.CV.Structure;

#endregion

namespace DartboardRecognition.Services
{
    public class ThrowService
    {
        private readonly MainWindow mainWindow;
        private readonly DrawService drawService;
        private PointF projectionCenterPoint;
        private readonly List<Ray> rays;
        private PointF projectionLineCam1Point1;
        private PointF projectionLineCam1Point2;
        private PointF projectionLineCam2Point1;
        private PointF projectionLineCam2Point2;
        private readonly Queue<Throw> throwsCollection;
        private Image<Bgr, byte> DartboardProjectionFrameBackground { get; }
        private Image<Bgr, byte> DartboardProjectionWorkingFrame { get; set; }

        public ThrowService(MainWindow mainWindow, DrawService drawService)
        {
            this.mainWindow = mainWindow;
            this.drawService = drawService;
            DartboardProjectionFrameBackground = new Image<Bgr, byte>(mainWindow.ProjectionFrameWidth,
                                                                      mainWindow.ProjectionFrameHeight);
            DartboardProjectionWorkingFrame = DartboardProjectionFrameBackground.Clone();
            projectionCenterPoint = new PointF((float) DartboardProjectionFrameBackground.Width / 2,
                                               (float) DartboardProjectionFrameBackground.Height / 2);
            rays = new List<Ray>();
            throwsCollection = new Queue<Throw>();
        }

        public void CalculateAndSaveThrow()
        {
            DartboardProjectionWorkingFrame = DartboardProjectionFrameBackground.Clone();

            var firstBestRay = rays.OrderByDescending(i => i.ContourWidth).First();
            rays.Remove(firstBestRay);
            var secondBestRay = rays.OrderByDescending(i => i.ContourWidth).First();
            rays.Clear();

            var poi = MeasureService.FindLinesIntersection(firstBestRay.CamPoint,
                                                           firstBestRay.RayPoint,
                                                           secondBestRay.CamPoint,
                                                           secondBestRay.RayPoint);
            var anotherThrow = PrepareThrowData(poi);
            throwsCollection.Enqueue(anotherThrow);

            drawService.DrawCircle(DartboardProjectionWorkingFrame, poi, mainWindow.PoiRadius, mainWindow.PoiColor, mainWindow.PoiThickness);

            mainWindow.Dispatcher.Invoke(new Action(() => mainWindow.DartboardProjectionImageBox.Source = drawService.ToBitmap(DartboardProjectionWorkingFrame)));
            // mainWindow.Dispatcher.Invoke(new Action(() => mainWindow.PointsBox.Text = ""));
            // mainWindow.Dispatcher.Invoke(new Action(() => mainWindow.PointsBox.Text += $"{anotherThrow.Sector} x {anotherThrow.Multiplier} = {anotherThrow.TotalPoints}\n"));
        }


        private Throw PrepareThrowData(PointF poi)
        {
            var startRadSector = -1.41372;
            var radSectorStep = 0.314159;
            var angle = MeasureService.FindAngle(projectionCenterPoint, poi);
            var distance = MeasureService.FindDistance(projectionCenterPoint, poi);
            var sector = 0;
            var multiplier = 1;

            if (distance >= mainWindow.ProjectionCoefficent * 95 &&
                distance <= mainWindow.ProjectionCoefficent * 105)
            {
                multiplier = 3;
            }

            if (distance >= mainWindow.ProjectionCoefficent * 160 &&
                distance <= mainWindow.ProjectionCoefficent * 170)
            {
                multiplier = 2;
            }

            // Find sector
            if (angle >= -1.41372 && angle < -1.099561)
            {
                sector = 1;
            }
            else if (angle >= -1.099561 && angle < -0.785402)
            {
                sector = 18;
            }
            else if (angle >= -0.785402 && angle < -0.471243)
            {
                sector = 4;
            }
            else if (angle >= -0.471243 && angle < -0.157084)
            {
                sector = 13;
            }
            else if (angle >= -0.157084 && angle < 0.157075)
            {
                sector = 6;
            }
            else if (angle >= 0.157075 && angle < 0.471234)
            {
                sector = 10;
            }
            else if (angle >= 0.471234 && angle < 0.785393)
            {
                sector = 15;
            }
            else if (angle >= 0.785393 && angle < 1.099552)
            {
                sector = 2;
            }
            else if (angle >= 1.099552 && angle < 1.413711)
            {
                sector = 17;
            }
            else if (angle >= 1.413711 && angle < 1.72787)
            {
                sector = 3;
            }
            else if (angle >= 1.72787 && angle < 2.042029)
            {
                sector = 19;
            }
            else if (angle >= 2.042029 && angle < 2.356188)
            {
                sector = 7;
            }
            else if (angle >= 2.356188 && angle < 2.670347)
            {
                sector = 16;
            }
            else if (angle >= 2.670347 && angle < 2.984506)
            {
                sector = 8;
            }
            else if (angle >= 2.984506 && angle < 3.14159 ||
                     angle >= -3.14159 && angle < -3.29868)
            {
                sector = 11;
            }
            else if (angle >= -3.29868 && angle < -2.67036)
            {
                sector = 14;
            }
            else if (angle >= -2.67036 && angle < -2.3562)
            {
                sector = 9;
            }
            else if (angle >= -2.3562 && angle < -2.04204)
            {
                sector = 12;
            }
            else if (angle >= -2.04204 && angle < -1.72788)
            {
                sector = 5;
            }
            else if (angle >= -1.72788 && angle < -1.41372)
            {
                sector = 20;
            }

            if (distance <= mainWindow.ProjectionCoefficent * 7)
            {
                sector = 50;
            }

            if (distance > mainWindow.ProjectionCoefficent * 7 &&
                distance <= mainWindow.ProjectionCoefficent * 17)
            {
                sector = 25;
            }

            if (distance > mainWindow.ProjectionCoefficent * 170)
            {
                sector = 0;
            }

            return new Throw(poi, sector, multiplier, DartboardProjectionFrameBackground);
        }

        public void SaveRay(Ray ray)
        {
            rays.Add(ray);
        }

        public BitmapImage PrepareDartboardProjectionImage()
        {
            // Draw dartboard projection
            drawService.DrawCircle(DartboardProjectionFrameBackground, projectionCenterPoint, mainWindow.ProjectionCoefficent * 7, mainWindow.ProjectionGridColor, mainWindow.ProjectionGridThickness);
            drawService.DrawCircle(DartboardProjectionFrameBackground, projectionCenterPoint, mainWindow.ProjectionCoefficent * 17, mainWindow.ProjectionGridColor, mainWindow.ProjectionGridThickness);
            drawService.DrawCircle(DartboardProjectionFrameBackground, projectionCenterPoint, mainWindow.ProjectionCoefficent * 95, mainWindow.ProjectionGridColor, mainWindow.ProjectionGridThickness);
            drawService.DrawCircle(DartboardProjectionFrameBackground, projectionCenterPoint, mainWindow.ProjectionCoefficent * 105, mainWindow.ProjectionGridColor, mainWindow.ProjectionGridThickness);
            drawService.DrawCircle(DartboardProjectionFrameBackground, projectionCenterPoint, mainWindow.ProjectionCoefficent * 160, mainWindow.ProjectionGridColor, mainWindow.ProjectionGridThickness);
            drawService.DrawCircle(DartboardProjectionFrameBackground, projectionCenterPoint, mainWindow.ProjectionCoefficent * 170, mainWindow.ProjectionGridColor, mainWindow.ProjectionGridThickness);
            for (var i = 0; i <= 360; i += 9)
            {
                var segmentPoint1 = new PointF((float) (projectionCenterPoint.X + Math.Cos(0.314159 * i - 0.15708) * mainWindow.ProjectionCoefficent * 170),
                                               (float) (projectionCenterPoint.Y + Math.Sin(0.314159 * i - 0.15708) * mainWindow.ProjectionCoefficent * 170));
                var segmentPoint2 = new PointF((float) (projectionCenterPoint.X + Math.Cos(0.314159 * i - 0.15708) * mainWindow.ProjectionCoefficent * 17),
                                               (float) (projectionCenterPoint.Y + Math.Sin(0.314159 * i - 0.15708) * mainWindow.ProjectionCoefficent * 17));
                drawService.DrawLine(DartboardProjectionFrameBackground, segmentPoint1, segmentPoint2, mainWindow.ProjectionGridColor, mainWindow.ProjectionGridThickness);
            }

            // Draw surface projection lines
            projectionLineCam1Point1.X = (float) (projectionCenterPoint.X + Math.Cos(-0.785398) * mainWindow.ProjectionCoefficent * 170) - mainWindow.ProjectionCoefficent * mainWindow.ProjectionLineCam1Bias;
            projectionLineCam1Point1.Y = (float) (projectionCenterPoint.Y + Math.Sin(-0.785398) * mainWindow.ProjectionCoefficent * 170) - mainWindow.ProjectionCoefficent * mainWindow.ProjectionLineCam1Bias;
            projectionLineCam1Point2.X = (float) (projectionCenterPoint.X + Math.Cos(2.35619) * mainWindow.ProjectionCoefficent * 170) - mainWindow.ProjectionCoefficent * mainWindow.ProjectionLineCam1Bias;
            projectionLineCam1Point2.Y = (float) (projectionCenterPoint.Y + Math.Sin(2.35619) * mainWindow.ProjectionCoefficent * 170) - mainWindow.ProjectionCoefficent * mainWindow.ProjectionLineCam1Bias;
            //drawman.DrawLine(dartboardProjectionFrame, projectionLineCam1Point1, projectionLineCam1Point2, view.ProjectionSurfaceLineColor, view.ProjectionSurfaceLineThickness);

            projectionLineCam2Point1.X = (float) (projectionCenterPoint.X + Math.Cos(0.785398) * mainWindow.ProjectionCoefficent * 170) + mainWindow.ProjectionCoefficent * mainWindow.ProjectionLineCam2Bias;
            projectionLineCam2Point1.Y = (float) (projectionCenterPoint.Y + Math.Sin(0.785398) * mainWindow.ProjectionCoefficent * 170) - mainWindow.ProjectionCoefficent * mainWindow.ProjectionLineCam2Bias;
            projectionLineCam2Point2.X = (float) (projectionCenterPoint.X + Math.Cos(3.92699) * mainWindow.ProjectionCoefficent * 170) + mainWindow.ProjectionCoefficent * mainWindow.ProjectionLineCam2Bias;
            projectionLineCam2Point2.Y = (float) (projectionCenterPoint.Y + Math.Sin(3.92699) * mainWindow.ProjectionCoefficent * 170) - mainWindow.ProjectionCoefficent * mainWindow.ProjectionLineCam2Bias;
            //drawman.DrawLine(dartboardProjectionFrame, projectionLineCam2Point1, projectionLineCam2Point2, view.ProjectionSurfaceLineColor, view.ProjectionSurfaceLineThickness);

            // Draw digits
            var startAngle = 0;
            var sectorAngle = 0.314159;
            drawService.DrawString(DartboardProjectionFrameBackground,
                                   "6",
                                   (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 0) * mainWindow.ProjectionCoefficent * 180),
                                   (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 0) * mainWindow.ProjectionCoefficent * 180),
                                   mainWindow.ProjectionDigitsScale,
                                   mainWindow.ProjectionDigitsColor,
                                   mainWindow.ProjectionDigitsThickness);
            drawService.DrawString(DartboardProjectionFrameBackground,
                                   "10",
                                   (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 1 + 0.05) * mainWindow.ProjectionCoefficent * 180),
                                   (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 1 + 0.05) * mainWindow.ProjectionCoefficent * 180),
                                   mainWindow.ProjectionDigitsScale,
                                   mainWindow.ProjectionDigitsColor,
                                   mainWindow.ProjectionDigitsThickness);
            drawService.DrawString(DartboardProjectionFrameBackground,
                                   "15",
                                   (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 2 + 0.05) * mainWindow.ProjectionCoefficent * 180),
                                   (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 2 + 0.05) * mainWindow.ProjectionCoefficent * 180),
                                   mainWindow.ProjectionDigitsScale,
                                   mainWindow.ProjectionDigitsColor,
                                   mainWindow.ProjectionDigitsThickness);
            drawService.DrawString(DartboardProjectionFrameBackground,
                                   "2",
                                   (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 3 + 0.05) * mainWindow.ProjectionCoefficent * 190),
                                   (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 3 + 0.05) * mainWindow.ProjectionCoefficent * 190),
                                   mainWindow.ProjectionDigitsScale,
                                   mainWindow.ProjectionDigitsColor,
                                   mainWindow.ProjectionDigitsThickness);
            drawService.DrawString(DartboardProjectionFrameBackground,
                                   "17",
                                   (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 4 + 0.05) * mainWindow.ProjectionCoefficent * 190),
                                   (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 4 + 0.05) * mainWindow.ProjectionCoefficent * 190),
                                   mainWindow.ProjectionDigitsScale,
                                   mainWindow.ProjectionDigitsColor,
                                   mainWindow.ProjectionDigitsThickness);
            drawService.DrawString(DartboardProjectionFrameBackground,
                                   "3",
                                   (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 5 + 0.05) * mainWindow.ProjectionCoefficent * 190),
                                   (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 5 + 0.05) * mainWindow.ProjectionCoefficent * 190),
                                   mainWindow.ProjectionDigitsScale,
                                   mainWindow.ProjectionDigitsColor,
                                   mainWindow.ProjectionDigitsThickness);
            drawService.DrawString(DartboardProjectionFrameBackground,
                                   "19",
                                   (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 6 + 0.05) * mainWindow.ProjectionCoefficent * 200),
                                   (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 6 + 0.05) * mainWindow.ProjectionCoefficent * 200),
                                   mainWindow.ProjectionDigitsScale,
                                   mainWindow.ProjectionDigitsColor,
                                   mainWindow.ProjectionDigitsThickness);
            drawService.DrawString(DartboardProjectionFrameBackground,
                                   "7",
                                   (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 7) * mainWindow.ProjectionCoefficent * 200),
                                   (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 7) * mainWindow.ProjectionCoefficent * 200),
                                   mainWindow.ProjectionDigitsScale,
                                   mainWindow.ProjectionDigitsColor,
                                   mainWindow.ProjectionDigitsThickness);
            drawService.DrawString(DartboardProjectionFrameBackground,
                                   "16",
                                   (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 8) * mainWindow.ProjectionCoefficent * 210),
                                   (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 8) * mainWindow.ProjectionCoefficent * 210),
                                   mainWindow.ProjectionDigitsScale,
                                   mainWindow.ProjectionDigitsColor,
                                   mainWindow.ProjectionDigitsThickness);
            drawService.DrawString(DartboardProjectionFrameBackground,
                                   "8",
                                   (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 9) * mainWindow.ProjectionCoefficent * 200),
                                   (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 9) * mainWindow.ProjectionCoefficent * 200),
                                   mainWindow.ProjectionDigitsScale,
                                   mainWindow.ProjectionDigitsColor,
                                   mainWindow.ProjectionDigitsThickness);
            drawService.DrawString(DartboardProjectionFrameBackground,
                                   "11",
                                   (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 10) * mainWindow.ProjectionCoefficent * 200),
                                   (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 10) * mainWindow.ProjectionCoefficent * 200),
                                   mainWindow.ProjectionDigitsScale,
                                   mainWindow.ProjectionDigitsColor,
                                   mainWindow.ProjectionDigitsThickness);
            drawService.DrawString(DartboardProjectionFrameBackground,
                                   "14",
                                   (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 11 - 0.05) * mainWindow.ProjectionCoefficent * 200),
                                   (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 11 - 0.05) * mainWindow.ProjectionCoefficent * 200),
                                   mainWindow.ProjectionDigitsScale,
                                   mainWindow.ProjectionDigitsColor,
                                   mainWindow.ProjectionDigitsThickness);
            drawService.DrawString(DartboardProjectionFrameBackground,
                                   "9",
                                   (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 12 - 0.05) * mainWindow.ProjectionCoefficent * 200),
                                   (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 12 - 0.05) * mainWindow.ProjectionCoefficent * 200),
                                   mainWindow.ProjectionDigitsScale,
                                   mainWindow.ProjectionDigitsColor,
                                   mainWindow.ProjectionDigitsThickness);
            drawService.DrawString(DartboardProjectionFrameBackground,
                                   "12",
                                   (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 13 - 0.05) * mainWindow.ProjectionCoefficent * 200),
                                   (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 13 - 0.05) * mainWindow.ProjectionCoefficent * 200),
                                   mainWindow.ProjectionDigitsScale,
                                   mainWindow.ProjectionDigitsColor,
                                   mainWindow.ProjectionDigitsThickness);
            drawService.DrawString(DartboardProjectionFrameBackground,
                                   "5",
                                   (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 14 - 0.05) * mainWindow.ProjectionCoefficent * 190),
                                   (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 14 - 0.05) * mainWindow.ProjectionCoefficent * 190),
                                   mainWindow.ProjectionDigitsScale,
                                   mainWindow.ProjectionDigitsColor,
                                   mainWindow.ProjectionDigitsThickness);
            drawService.DrawString(DartboardProjectionFrameBackground,
                                   "20",
                                   (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 15 - 0.05) * mainWindow.ProjectionCoefficent * 180),
                                   (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 15 - 0.05) * mainWindow.ProjectionCoefficent * 180),
                                   mainWindow.ProjectionDigitsScale,
                                   mainWindow.ProjectionDigitsColor,
                                   mainWindow.ProjectionDigitsThickness);
            drawService.DrawString(DartboardProjectionFrameBackground,
                                   "1",
                                   (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 16 - 0.05) * mainWindow.ProjectionCoefficent * 180),
                                   (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 16 - 0.05) * mainWindow.ProjectionCoefficent * 180),
                                   mainWindow.ProjectionDigitsScale,
                                   mainWindow.ProjectionDigitsColor,
                                   mainWindow.ProjectionDigitsThickness);
            drawService.DrawString(DartboardProjectionFrameBackground,
                                   "18",
                                   (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 17 - 0.05) * mainWindow.ProjectionCoefficent * 180),
                                   (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 17 - 0.05) * mainWindow.ProjectionCoefficent * 180),
                                   mainWindow.ProjectionDigitsScale,
                                   mainWindow.ProjectionDigitsColor,
                                   mainWindow.ProjectionDigitsThickness);
            drawService.DrawString(DartboardProjectionFrameBackground,
                                   "4",
                                   (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 18 - 0.05) * mainWindow.ProjectionCoefficent * 180),
                                   (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 18 - 0.05) * mainWindow.ProjectionCoefficent * 180),
                                   mainWindow.ProjectionDigitsScale,
                                   mainWindow.ProjectionDigitsColor,
                                   mainWindow.ProjectionDigitsThickness);
            drawService.DrawString(DartboardProjectionFrameBackground,
                                   "13",
                                   (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 19) * mainWindow.ProjectionCoefficent * 180),
                                   (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 19) * mainWindow.ProjectionCoefficent * 180),
                                   mainWindow.ProjectionDigitsScale,
                                   mainWindow.ProjectionDigitsColor,
                                   mainWindow.ProjectionDigitsThickness);

            DartboardProjectionWorkingFrame = DartboardProjectionFrameBackground.Clone();

            return drawService.ToBitmap(DartboardProjectionFrameBackground);
        }
    }
}