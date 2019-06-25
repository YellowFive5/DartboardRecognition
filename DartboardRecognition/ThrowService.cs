using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;

namespace DartboardRecognition
{
    public class ThrowService
    {
        private MainWindow view;
        private Drawman drawman;
        private Point projectionCenterPoint;
        private Stack<Point> cam1RayPoint;
        private Stack<Point> cam2RayPoint;
        private Queue<Throw> throwsCollection;
        private Point projectionLineCam1Point1;
        private Point projectionLineCam1Point2;
        private Point projectionLineCam2Point1;
        private Point projectionLineCam2Point2;
        public Image<Bgr, byte> DartboardProjectionFrame { get; }

        public ThrowService(MainWindow view, Drawman drawman, CancellationToken cancelToken)
        {
            this.view = view;
            this.drawman = drawman;
            DartboardProjectionFrame = new Image<Bgr, byte>(view.ProjectionFrameWidth, view.ProjectionFrameHeight);
            projectionCenterPoint = new Point(DartboardProjectionFrame.Width / 2, DartboardProjectionFrame.Height / 2);
            cam1RayPoint = new Stack<Point>();
            cam2RayPoint = new Stack<Point>();
            throwsCollection = new Queue<Throw>();
        }

        public void AwaitForThrow(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                var anotherThrowDetected = cam1RayPoint.Count == 1 && cam2RayPoint.Count == 1;
                if (anotherThrowDetected)
                {
                    CalculateAndSaveThrow();
                }
            }
        }

        private void CalculateAndSaveThrow()
        {
            var poi = Measureman.FindLinesIntersection(view.Cam1SetupPoint,
                                                       cam1RayPoint.Pop(),
                                                       view.Cam2SetupPoint,
                                                       cam2RayPoint.Pop());

            var anotherThrow = PrepareThrowData(poi);
            throwsCollection.Enqueue(anotherThrow);

            drawman.DrawCircle(DartboardProjectionFrame, poi, view.PoiRadius, view.PoiColor, view.PoiThickness);
            // view.PointsBox.Text += $"{anotherThrow.Sector} x {anotherThrow.Multiplier} = {anotherThrow.TotalPoints}\n";
        }

        private Throw PrepareThrowData(Point poi)
        {
            var startRadSector = -1.41372;
            var radSectorStep = 0.314159;
            var angle = Measureman.FindAngle(projectionCenterPoint, poi);
            var distance = Measureman.FindDistance(projectionCenterPoint, poi);
            var sector = 0;
            var multiplier = 1;

            if (distance <= view.ProjectionCoefficent * 7)
            {
                sector = 50;
            }

            if (distance > view.ProjectionCoefficent * 7 &&
                distance <= view.ProjectionCoefficent * 17)
            {
                sector = 25;
            }

            if (distance > view.ProjectionCoefficent * 170)
            {
                sector = 0;
            }

            if (distance >= view.ProjectionCoefficent * 95 &&
                distance <= view.ProjectionCoefficent * 105)
            {
                multiplier = 3;
            }

            if (distance >= view.ProjectionCoefficent * 160 &&
                distance <= view.ProjectionCoefficent * 170)
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

            return new Throw(poi, sector, multiplier, DartboardProjectionFrame);
        }

        public void SaveRay(Point rayPoint, Cam cam)
        {
            if (cam is Cam1)
            {
                cam1RayPoint.Push(rayPoint);
            }
            else
            {
                cam2RayPoint.Push(rayPoint);
            }
        }

        public Image<Bgr, byte> PrepareDartboardProjectionImage()
        {
            // Draw dartboard projection
            drawman.DrawCircle(DartboardProjectionFrame, projectionCenterPoint, view.ProjectionCoefficent * 7, view.ProjectionGridColor, view.ProjectionGridThickness);
            drawman.DrawCircle(DartboardProjectionFrame, projectionCenterPoint, view.ProjectionCoefficent * 17, view.ProjectionGridColor, view.ProjectionGridThickness);
            drawman.DrawCircle(DartboardProjectionFrame, projectionCenterPoint, view.ProjectionCoefficent * 95, view.ProjectionGridColor, view.ProjectionGridThickness);
            drawman.DrawCircle(DartboardProjectionFrame, projectionCenterPoint, view.ProjectionCoefficent * 105, view.ProjectionGridColor, view.ProjectionGridThickness);
            drawman.DrawCircle(DartboardProjectionFrame, projectionCenterPoint, view.ProjectionCoefficent * 160, view.ProjectionGridColor, view.ProjectionGridThickness);
            drawman.DrawCircle(DartboardProjectionFrame, projectionCenterPoint, view.ProjectionCoefficent * 170, view.ProjectionGridColor, view.ProjectionGridThickness);
            for (var i = 0; i <= 360; i += 9)
            {
                var segmentPoint1 = new Point
                                    {
                                        X = (int) (projectionCenterPoint.X + Math.Cos(0.314159 * i - 0.15708) * view.ProjectionCoefficent * 170),
                                        Y = (int) (projectionCenterPoint.Y + Math.Sin(0.314159 * i - 0.15708) * view.ProjectionCoefficent * 170)
                                    };
                var segmentPoint2 = new Point
                                    {
                                        X = (int) (projectionCenterPoint.X + Math.Cos(0.314159 * i - 0.15708) * view.ProjectionCoefficent * 17),
                                        Y = (int) (projectionCenterPoint.Y + Math.Sin(0.314159 * i - 0.15708) * view.ProjectionCoefficent * 17)
                                    };
                drawman.DrawLine(DartboardProjectionFrame, segmentPoint1, segmentPoint2, view.ProjectionGridColor, view.ProjectionGridThickness);
            }

            // Draw surface projection lines
            projectionLineCam1Point1.X = (int) (projectionCenterPoint.X + Math.Cos(-0.785398) * view.ProjectionCoefficent * 170) - view.ProjectionCoefficent * view.projectionLineCam1Bias;
            projectionLineCam1Point1.Y = (int) (projectionCenterPoint.Y + Math.Sin(-0.785398) * view.ProjectionCoefficent * 170) - view.ProjectionCoefficent * view.projectionLineCam1Bias;
            projectionLineCam1Point2.X = (int) (projectionCenterPoint.X + Math.Cos(2.35619) * view.ProjectionCoefficent * 170) - view.ProjectionCoefficent * view.projectionLineCam1Bias;
            projectionLineCam1Point2.Y = (int) (projectionCenterPoint.Y + Math.Sin(2.35619) * view.ProjectionCoefficent * 170) - view.ProjectionCoefficent * view.projectionLineCam1Bias;
            //drawman.DrawLine(dartboardProjectionFrame, projectionLineCam1Point1, projectionLineCam1Point2, view.ProjectionSurfaceLineColor, view.ProjectionSurfaceLineThickness);

            projectionLineCam2Point1.X = (int) (projectionCenterPoint.X + Math.Cos(0.785398) * view.ProjectionCoefficent * 170) + view.ProjectionCoefficent * view.projectionLineCam2Bias;
            projectionLineCam2Point1.Y = (int) (projectionCenterPoint.Y + Math.Sin(0.785398) * view.ProjectionCoefficent * 170) - view.ProjectionCoefficent * view.projectionLineCam2Bias;
            projectionLineCam2Point2.X = (int) (projectionCenterPoint.X + Math.Cos(3.92699) * view.ProjectionCoefficent * 170) + view.ProjectionCoefficent * view.projectionLineCam2Bias;
            projectionLineCam2Point2.Y = (int) (projectionCenterPoint.Y + Math.Sin(3.92699) * view.ProjectionCoefficent * 170) - view.ProjectionCoefficent * view.projectionLineCam2Bias;
            //drawman.DrawLine(dartboardProjectionFrame, projectionLineCam2Point1, projectionLineCam2Point2, view.ProjectionSurfaceLineColor, view.ProjectionSurfaceLineThickness);

            // Draw digits
            var startAngle = 0;
            var sectorAngle = 0.314159;
            drawman.DrawString(DartboardProjectionFrame,
                               "6",
                               (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 0) * view.ProjectionCoefficent * 180),
                               (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 0) * view.ProjectionCoefficent * 180),
                               view.ProjectionDigitsScale,
                               view.ProjectionDigitsColor,
                               view.ProjectionDigitsThickness);
            drawman.DrawString(DartboardProjectionFrame,
                               "10",
                               (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 1 + 0.05) * view.ProjectionCoefficent * 180),
                               (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 1 + 0.05) * view.ProjectionCoefficent * 180),
                               view.ProjectionDigitsScale,
                               view.ProjectionDigitsColor,
                               view.ProjectionDigitsThickness);
            drawman.DrawString(DartboardProjectionFrame,
                               "15",
                               (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 2 + 0.05) * view.ProjectionCoefficent * 180),
                               (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 2 + 0.05) * view.ProjectionCoefficent * 180),
                               view.ProjectionDigitsScale,
                               view.ProjectionDigitsColor,
                               view.ProjectionDigitsThickness);
            drawman.DrawString(DartboardProjectionFrame,
                               "2",
                               (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 3 + 0.05) * view.ProjectionCoefficent * 190),
                               (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 3 + 0.05) * view.ProjectionCoefficent * 190),
                               view.ProjectionDigitsScale,
                               view.ProjectionDigitsColor,
                               view.ProjectionDigitsThickness);
            drawman.DrawString(DartboardProjectionFrame,
                               "17",
                               (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 4 + 0.05) * view.ProjectionCoefficent * 190),
                               (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 4 + 0.05) * view.ProjectionCoefficent * 190),
                               view.ProjectionDigitsScale,
                               view.ProjectionDigitsColor,
                               view.ProjectionDigitsThickness);
            drawman.DrawString(DartboardProjectionFrame,
                               "3",
                               (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 5 + 0.05) * view.ProjectionCoefficent * 190),
                               (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 5 + 0.05) * view.ProjectionCoefficent * 190),
                               view.ProjectionDigitsScale,
                               view.ProjectionDigitsColor,
                               view.ProjectionDigitsThickness);
            drawman.DrawString(DartboardProjectionFrame,
                               "19",
                               (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 6 + 0.05) * view.ProjectionCoefficent * 200),
                               (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 6 + 0.05) * view.ProjectionCoefficent * 200),
                               view.ProjectionDigitsScale,
                               view.ProjectionDigitsColor,
                               view.ProjectionDigitsThickness);
            drawman.DrawString(DartboardProjectionFrame,
                               "7",
                               (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 7) * view.ProjectionCoefficent * 200),
                               (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 7) * view.ProjectionCoefficent * 200),
                               view.ProjectionDigitsScale,
                               view.ProjectionDigitsColor,
                               view.ProjectionDigitsThickness);
            drawman.DrawString(DartboardProjectionFrame,
                               "16",
                               (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 8) * view.ProjectionCoefficent * 210),
                               (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 8) * view.ProjectionCoefficent * 210),
                               view.ProjectionDigitsScale,
                               view.ProjectionDigitsColor,
                               view.ProjectionDigitsThickness);
            drawman.DrawString(DartboardProjectionFrame,
                               "8",
                               (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 9) * view.ProjectionCoefficent * 200),
                               (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 9) * view.ProjectionCoefficent * 200),
                               view.ProjectionDigitsScale,
                               view.ProjectionDigitsColor,
                               view.ProjectionDigitsThickness);
            drawman.DrawString(DartboardProjectionFrame,
                               "11",
                               (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 10) * view.ProjectionCoefficent * 200),
                               (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 10) * view.ProjectionCoefficent * 200),
                               view.ProjectionDigitsScale,
                               view.ProjectionDigitsColor,
                               view.ProjectionDigitsThickness);
            drawman.DrawString(DartboardProjectionFrame,
                               "14",
                               (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 11 - 0.05) * view.ProjectionCoefficent * 200),
                               (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 11 - 0.05) * view.ProjectionCoefficent * 200),
                               view.ProjectionDigitsScale,
                               view.ProjectionDigitsColor,
                               view.ProjectionDigitsThickness);
            drawman.DrawString(DartboardProjectionFrame,
                               "9",
                               (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 12 - 0.05) * view.ProjectionCoefficent * 200),
                               (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 12 - 0.05) * view.ProjectionCoefficent * 200),
                               view.ProjectionDigitsScale,
                               view.ProjectionDigitsColor,
                               view.ProjectionDigitsThickness);
            drawman.DrawString(DartboardProjectionFrame,
                               "12",
                               (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 13 - 0.05) * view.ProjectionCoefficent * 200),
                               (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 13 - 0.05) * view.ProjectionCoefficent * 200),
                               view.ProjectionDigitsScale,
                               view.ProjectionDigitsColor,
                               view.ProjectionDigitsThickness);
            drawman.DrawString(DartboardProjectionFrame,
                               "5",
                               (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 14 - 0.05) * view.ProjectionCoefficent * 190),
                               (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 14 - 0.05) * view.ProjectionCoefficent * 190),
                               view.ProjectionDigitsScale,
                               view.ProjectionDigitsColor,
                               view.ProjectionDigitsThickness);
            drawman.DrawString(DartboardProjectionFrame,
                               "20",
                               (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 15 - 0.05) * view.ProjectionCoefficent * 180),
                               (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 15 - 0.05) * view.ProjectionCoefficent * 180),
                               view.ProjectionDigitsScale,
                               view.ProjectionDigitsColor,
                               view.ProjectionDigitsThickness);
            drawman.DrawString(DartboardProjectionFrame,
                               "1",
                               (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 16 - 0.05) * view.ProjectionCoefficent * 180),
                               (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 16 - 0.05) * view.ProjectionCoefficent * 180),
                               view.ProjectionDigitsScale,
                               view.ProjectionDigitsColor,
                               view.ProjectionDigitsThickness);
            drawman.DrawString(DartboardProjectionFrame,
                               "18",
                               (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 17 - 0.05) * view.ProjectionCoefficent * 180),
                               (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 17 - 0.05) * view.ProjectionCoefficent * 180),
                               view.ProjectionDigitsScale,
                               view.ProjectionDigitsColor,
                               view.ProjectionDigitsThickness);
            drawman.DrawString(DartboardProjectionFrame,
                               "4",
                               (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 18 - 0.05) * view.ProjectionCoefficent * 180),
                               (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 18 - 0.05) * view.ProjectionCoefficent * 180),
                               view.ProjectionDigitsScale,
                               view.ProjectionDigitsColor,
                               view.ProjectionDigitsThickness);
            drawman.DrawString(DartboardProjectionFrame,
                               "13",
                               (int) (projectionCenterPoint.X + Math.Cos(startAngle + sectorAngle * 19) * view.ProjectionCoefficent * 180),
                               (int) (projectionCenterPoint.Y + Math.Sin(startAngle + sectorAngle * 19) * view.ProjectionCoefficent * 180),
                               view.ProjectionDigitsScale,
                               view.ProjectionDigitsColor,
                               view.ProjectionDigitsThickness);

            return DartboardProjectionFrame;
        }
    }
}