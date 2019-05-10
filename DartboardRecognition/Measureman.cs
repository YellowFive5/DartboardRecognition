#region Usings

using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Point = System.Drawing.Point;

#endregion

namespace DartboardRecognition
{
    public class Measureman
    {
        private MainWindow view;
        private Drawman drawman;
        private Rectangle roiRectangle;
        private Moments contourMoments;
        private Point contourCenterPoint;
        private Point contourBoxPoint1;
        private Point contourBoxPoint2;
        private Point contourBoxPoint3;
        private Point contourBoxPoint4;
        private Point contourBoxMiddlePoint1;
        private Point contourBoxMiddlePoint2;
        private Point spikeLinePoint1;
        private Point spikeLinePoint2;
        private Point projectionCenterPoint;
        private Point projectionLineCam1Point1;
        private Point projectionLineCam1Point2;
        private Point projectionLineCam2Point1;
        private Point projectionLineCam2Point2;
        private Point? camPoi;
        private Image<Bgr, byte> dartboardProjectionFrame;
        private int projectionLineCam1Bias = 0;
        private int projectionLineCam2Bias = 0;
        private Point cam1SetupPoint;
        private Point cam2SetupPoint;
        private int minContourArcLength = 250;
        private int maxContourArcLength = 350;
        private Queue<Point> cam1RaysCollection = new Queue<Point>();
        private Queue<Point> cam2RaysCollection = new Queue<Point>();
        private Queue<Point> poiCollection = new Queue<Point>();
        private Queue<Throw> throwsCollection = new Queue<Throw>();
        private Cam workingCam;


        public Measureman(MainWindow view, Drawman drawman)
        {
            this.view = view;
            this.drawman = drawman;
        }

        public void SetupWorkingCam(Cam cam)
        {
            workingCam = cam;
        }

        public void CalculateSetupLines()
        {
            workingCam.linedFrame = workingCam.originFrame.Clone();

            roiRectangle = new Rectangle((int) workingCam.roiPosXSlider.Value,
                                         (int) workingCam.roiPosYSlider.Value,
                                         (int) workingCam.roiWidthSlider.Value,
                                         (int) workingCam.roiHeightSlider.Value);
            drawman.DrawRectangle(workingCam.linedFrame, roiRectangle, view.RoiRectColor.MCvScalar, view.RoiRectThickness);

            workingCam.surfacePoint1 = new Point(0, (int) workingCam.surfaceSlider.Value);
            workingCam.surfacePoint2 = new Point(workingCam.originFrame.Cols, (int) workingCam.surfaceSlider.Value);
            drawman.DrawLine(workingCam.linedFrame, workingCam.surfacePoint1, workingCam.surfacePoint2, view.SurfaceLineColor.MCvScalar, view.SurfaceLineThickness);

            workingCam.surfaceCenterPoint1 = new Point
                                             {
                                                 X = (int) workingCam.surfaceCenterSlider.Value,
                                                 Y = (int) workingCam.surfaceSlider.Value
                                             };
            workingCam.surfaceCenterPoint2 = new Point
                                             {
                                                 X = workingCam.surfaceCenterPoint1.X,
                                                 Y = workingCam.surfaceCenterPoint1.Y - 50
                                             };
            drawman.DrawLine(workingCam.linedFrame, workingCam.surfaceCenterPoint1, workingCam.surfaceCenterPoint2, view.SurfaceLineColor.MCvScalar, view.SurfaceLineThickness);

            workingCam.surfaceLeftPoint1 = new Point
                                           {
                                               X = (int) workingCam.surfaceLeftSlider.Value,
                                               Y = (int) workingCam.surfaceSlider.Value
                                           };
            workingCam.surfaceLeftPoint2 = new Point
                                           {
                                               X = workingCam.surfaceLeftPoint1.X,
                                               Y = workingCam.surfaceLeftPoint1.Y - 50
                                           };
            drawman.DrawLine(workingCam.linedFrame, workingCam.surfaceLeftPoint1, workingCam.surfaceLeftPoint2, view.SurfaceLineColor.MCvScalar, view.SurfaceLineThickness);

            workingCam.surfaceRightPoint1 = new Point
                                            {
                                                X = (int) workingCam.surfaceRightSlider.Value,
                                                Y = (int) workingCam.surfaceSlider.Value
                                            };
            workingCam.surfaceRightPoint2 = new Point
                                            {
                                                X = workingCam.surfaceRightPoint1.X,
                                                Y = workingCam.surfaceRightPoint1.Y - 50
                                            };
            drawman.DrawLine(workingCam.linedFrame, workingCam.surfaceRightPoint1, workingCam.surfaceRightPoint2, view.SurfaceLineColor.MCvScalar, view.SurfaceLineThickness);
        }

        public void CalculateRoiRegion()
        {
            workingCam.roiFrame = workingCam.originFrame.Clone();
            workingCam.roiFrame.ROI = roiRectangle;
        }

        public void CalculateDartboardProjection()
        {
            // Draw dartboard projection
            dartboardProjectionFrame = new Image<Bgr, byte>(view.DartboardProjectionFrameWidth, view.DartboardProjectionFrameHeight);
            projectionCenterPoint = new Point(dartboardProjectionFrame.Width / 2, dartboardProjectionFrame.Height / 2);
            cam1SetupPoint = new Point(0, 0);
            cam2SetupPoint = new Point(dartboardProjectionFrame.Cols, 0);

            drawman.DrawCircle(dartboardProjectionFrame, projectionCenterPoint, view.DartboardProjectionCoefficent * 7, view.DartboardProjectionColor, view.DartboardProjectionThickness);
            drawman.DrawCircle(dartboardProjectionFrame, projectionCenterPoint, view.DartboardProjectionCoefficent * 17, view.DartboardProjectionColor, view.DartboardProjectionThickness);
            drawman.DrawCircle(dartboardProjectionFrame, projectionCenterPoint, view.DartboardProjectionCoefficent * 95, view.DartboardProjectionColor, view.DartboardProjectionThickness);
            drawman.DrawCircle(dartboardProjectionFrame, projectionCenterPoint, view.DartboardProjectionCoefficent * 105, view.DartboardProjectionColor, view.DartboardProjectionThickness);
            drawman.DrawCircle(dartboardProjectionFrame, projectionCenterPoint, view.DartboardProjectionCoefficent * 160, view.DartboardProjectionColor, view.DartboardProjectionThickness);
            drawman.DrawCircle(dartboardProjectionFrame, projectionCenterPoint, view.DartboardProjectionCoefficent * 170, view.DartboardProjectionColor, view.DartboardProjectionThickness);
            for (var i = 0; i <= 360; i += 9)
            {
                var segmentPoint1 = new Point
                                    {
                                        X = (int) (projectionCenterPoint.X + Math.Cos(0.314159 * i - 0.15708) * view.DartboardProjectionCoefficent * 170),
                                        Y = (int) (projectionCenterPoint.Y + Math.Sin(0.314159 * i - 0.15708) * view.DartboardProjectionCoefficent * 170)
                                    };
                var segmentPoint2 = new Point
                                    {
                                        X = (int) (projectionCenterPoint.X + Math.Cos(0.314159 * i - 0.15708) * view.DartboardProjectionCoefficent * 17),
                                        Y = (int) (projectionCenterPoint.Y + Math.Sin(0.314159 * i - 0.15708) * view.DartboardProjectionCoefficent * 17)
                                    };
                drawman.DrawLine(dartboardProjectionFrame, segmentPoint1, segmentPoint2, view.DartboardProjectionColor, view.DartboardProjectionThickness);
            }

            // Draw surface projection lines
            projectionLineCam1Point1.X = (int) (projectionCenterPoint.X + Math.Cos(-0.785398) * view.DartboardProjectionCoefficent * 170) - view.DartboardProjectionCoefficent * projectionLineCam1Bias;
            projectionLineCam1Point1.Y = (int) (projectionCenterPoint.Y + Math.Sin(-0.785398) * view.DartboardProjectionCoefficent * 170) - view.DartboardProjectionCoefficent * projectionLineCam1Bias;
            projectionLineCam1Point2.X = (int) (projectionCenterPoint.X + Math.Cos(2.35619) * view.DartboardProjectionCoefficent * 170) - view.DartboardProjectionCoefficent * projectionLineCam1Bias;
            projectionLineCam1Point2.Y = (int) (projectionCenterPoint.Y + Math.Sin(2.35619) * view.DartboardProjectionCoefficent * 170) - view.DartboardProjectionCoefficent * projectionLineCam1Bias;
            drawman.DrawLine(dartboardProjectionFrame, projectionLineCam1Point1, projectionLineCam1Point2, view.SurfaceProjectionLineColor, view.SurfaceProjectionLineThickness);

            projectionLineCam2Point1.X = (int) (projectionCenterPoint.X + Math.Cos(0.785398) * view.DartboardProjectionCoefficent * 170) + view.DartboardProjectionCoefficent * projectionLineCam2Bias;
            projectionLineCam2Point1.Y = (int) (projectionCenterPoint.Y + Math.Sin(0.785398) * view.DartboardProjectionCoefficent * 170) - view.DartboardProjectionCoefficent * projectionLineCam2Bias;
            projectionLineCam2Point2.X = (int) (projectionCenterPoint.X + Math.Cos(3.92699) * view.DartboardProjectionCoefficent * 170) + view.DartboardProjectionCoefficent * projectionLineCam2Bias;
            projectionLineCam2Point2.Y = (int) (projectionCenterPoint.Y + Math.Sin(3.92699) * view.DartboardProjectionCoefficent * 170) - view.DartboardProjectionCoefficent * projectionLineCam2Bias;
            drawman.DrawLine(dartboardProjectionFrame, projectionLineCam2Point1, projectionLineCam2Point2, view.SurfaceProjectionLineColor, view.SurfaceProjectionLineThickness);

            drawman.SaveToImageBox(dartboardProjectionFrame, view.ImageBox3);
        }

        public void CalculateDartContours()
        {
            workingCam.roiContourFrame = workingCam.roiFrame.Clone();
            CvInvoke.FindContours(workingCam.roiTrasholdFrame, workingCam.contours, workingCam.matHierarсhy, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
            //CvInvoke.DrawContours(linedFrame, contours, -1, contourColor, contourThickness, offset: new System.Drawing.Point(0, (int)RoiPosYSlider.Value));

            if (workingCam.contours.Size <= 0)
            {
                return;
            }

            for (var i = 0; i < workingCam.contours.Size; i++)
            {
                // Filter contour
                var arclength = CvInvoke.ArcLength(workingCam.contours[i], true);
                if (arclength < minContourArcLength || arclength > maxContourArcLength)
                {
                    continue;
                }

                // Find moments and centerpoint
                contourMoments = CvInvoke.Moments(workingCam.contours[i]);
                contourCenterPoint = new Point((int) (contourMoments.M10 / contourMoments.M00), (int) workingCam.roiPosYSlider.Value + (int) (contourMoments.M01 / contourMoments.M00));
                drawman.DrawCircle(workingCam.linedFrame, contourCenterPoint, 4, new Bgr(Color.Blue).MCvScalar, 3);

                // Find contour rectangle
                var rect = CvInvoke.MinAreaRect(workingCam.contours[i]);
                var box = CvInvoke.BoxPoints(rect);
                contourBoxPoint1 = new Point((int) box[0].X, (int) workingCam.roiPosYSlider.Value + (int) box[0].Y);
                contourBoxPoint2 = new Point((int) box[1].X, (int) workingCam.roiPosYSlider.Value + (int) box[1].Y);
                contourBoxPoint3 = new Point((int) box[2].X, (int) workingCam.roiPosYSlider.Value + (int) box[2].Y);
                contourBoxPoint4 = new Point((int) box[3].X, (int) workingCam.roiPosYSlider.Value + (int) box[3].Y);
                drawman.DrawLine(workingCam.linedFrame, contourBoxPoint1, contourBoxPoint2, view.ContourRectColor, view.ContourRectThickness);
                drawman.DrawLine(workingCam.linedFrame, contourBoxPoint2, contourBoxPoint3, view.ContourRectColor, view.ContourRectThickness);
                drawman.DrawLine(workingCam.linedFrame, contourBoxPoint3, contourBoxPoint4, view.ContourRectColor, view.ContourRectThickness);
                drawman.DrawLine(workingCam.linedFrame, contourBoxPoint4, contourBoxPoint1, view.ContourRectColor, view.ContourRectThickness);

                SetupMiddlePoints();

                CalculateSpikeLine();

                CalculateCamPoi();

                var projectionPoi = TranslateCamPoiToProjection();

                var rayPoint2 = CalculateLineThroughPoi(projectionPoi);

                CollectRay(rayPoint2);
            }

            FindProjectionPois();

            drawman.SaveToImageBox(dartboardProjectionFrame, view.ImageBox3);
        }

        private void CollectRay(Point rayPoint2)
        {
            // Save rays to collection
            if (workingCam is Cam1)
            {
                if (!cam1RaysCollection.Contains(rayPoint2))
                {
                    cam1RaysCollection.Enqueue(rayPoint2);
                }
            }
            else
            {
                if (!cam2RaysCollection.Contains(rayPoint2))
                {
                    cam2RaysCollection.Enqueue(rayPoint2);
                }
            }
        }

        private Point CalculateLineThroughPoi(Point projectionPoi)
        {
            // Draw line from cam through projection POI
            Point rayPoint1;
            rayPoint1 = workingCam is Cam1
                            ? cam1SetupPoint
                            : cam2SetupPoint;

            var rayPoint2 = projectionPoi;
            var angle = FindAngle(rayPoint1, rayPoint2);
            rayPoint2.X = (int) (rayPoint1.X + Math.Cos(angle) * 2000);
            rayPoint2.Y = (int) (rayPoint1.Y + Math.Sin(angle) * 2000);

            drawman.DrawLine(dartboardProjectionFrame, rayPoint2, rayPoint1, new Bgr(Color.White).MCvScalar, 2);
            return rayPoint2;
        }

        private Point TranslateCamPoiToProjection()
        {
            // Translate cam surface POI to dartboard projection
            var surfaceLeftToRightDistance = FindDistance(workingCam.surfaceLeftPoint1, workingCam.surfaceRightPoint1);
            var projectionLeftToRightDistance = FindDistance(projectionLineCam1Point1, projectionLineCam1Point2);
            var surfaceProjectionCoeff = (double) projectionLeftToRightDistance / surfaceLeftToRightDistance;
            var surfacePoiToCenterDistance = FindDistance(workingCam.surfaceCenterPoint1, camPoi.GetValueOrDefault());
            var surfaceLeftToPoiDistance = FindDistance(workingCam.surfaceLeftPoint1, camPoi.GetValueOrDefault());
            var surfaceRightToPoiDistance = FindDistance(workingCam.surfaceRightPoint1, camPoi.GetValueOrDefault());
            var projectionPoi = new Point();
            var cam1Cos1 = Math.Cos(-0.785398);
            var cam1Sin1 = Math.Sin(-0.785398);
            var cam1Cos2 = Math.Cos(-3.92699);
            var cam1Sin2 = Math.Sin(-3.92699);
            var cam2Cos1 = Math.Cos(0.785398);
            var cam2Sin1 = Math.Sin(0.785398);
            var cam2Cos2 = Math.Cos(3.92699);
            var cam2Sin2 = Math.Sin(3.92699);
            double cos1;
            double sin1;
            double cos2;
            double sin2;
            Point surfaceProjectionMiddlePoint;

            if (workingCam is Cam1)
            {
                sin1 = cam1Sin1;
                cos1 = cam1Cos1;
                sin2 = cam1Sin2;
                cos2 = cam1Cos2;
                surfaceProjectionMiddlePoint = FindMiddle(projectionLineCam1Point1, projectionLineCam1Point2);
            }
            else
            {
                sin1 = cam2Sin1;
                cos1 = cam2Cos1;
                sin2 = cam2Sin2;
                cos2 = cam2Cos2;
                surfaceProjectionMiddlePoint = FindMiddle(projectionLineCam2Point1, projectionLineCam2Point2);
            }

            if (surfaceLeftToPoiDistance < surfaceRightToPoiDistance)
            {
                projectionPoi.X = (int) (surfaceProjectionMiddlePoint.X + cos1 * surfacePoiToCenterDistance * surfaceProjectionCoeff);
                projectionPoi.Y = (int) (surfaceProjectionMiddlePoint.Y + sin1 * surfacePoiToCenterDistance * surfaceProjectionCoeff);
            }
            else
            {
                projectionPoi.X = (int) (surfaceProjectionMiddlePoint.X + cos2 * surfacePoiToCenterDistance * surfaceProjectionCoeff);
                projectionPoi.Y = (int) (surfaceProjectionMiddlePoint.Y + sin2 * surfacePoiToCenterDistance * surfaceProjectionCoeff);
            }

            drawman.DrawCircle(dartboardProjectionFrame, projectionPoi, 6, new Bgr(Color.Yellow).MCvScalar, 6);
            return projectionPoi;
        }

        private void FindProjectionPois()
        {
            // Find lines intersection to find projection POI and save to collection
            if (cam1RaysCollection.Count != cam2RaysCollection.Count ||
                cam1RaysCollection.Count == 0 ||
                cam2RaysCollection.Count == 0)
            {
                if (cam1RaysCollection.Count == 0 && cam2RaysCollection.Count == 0)
                {
                    poiCollection.Clear();
                }

                return;
            }

            var counter = cam2RaysCollection.Count;
            for (var i = 0; i < counter; i++)
            {
                var poi = FindLinesIntersection(cam1SetupPoint,
                                                cam1RaysCollection.Dequeue(),
                                                cam2SetupPoint,
                                                cam2RaysCollection.Dequeue());
                if (!poiCollection.Contains(poi))
                {
                    poiCollection.Enqueue(poi);
                    var anotherThrow = PrepareThrowData(poi);
                    throwsCollection.Enqueue(anotherThrow);
                    drawman.DrawCircle(dartboardProjectionFrame, poi, 6, new Bgr(Color.Magenta).MCvScalar, 6);
                }
            }
        }

        private Throw PrepareThrowData(Point poi)
        {
            var startRadSector = -1.41372;
            var radSector = 0.314159;
            var angle = FindAngle(projectionCenterPoint, poi);
            var distance = FindDistance(projectionCenterPoint, poi);
            var sector = 0;
            var multiplier = 1;

            if (distance <= view.DartboardProjectionCoefficent * 7)
            {
                sector = 50;
            }

            if (distance > view.DartboardProjectionCoefficent * 7 &&
                distance <= view.DartboardProjectionCoefficent * 17)
            {
                sector = 25;
            }

            if (distance > view.DartboardProjectionCoefficent * 170)
            {
                sector = 0;
            }

            if (distance >= view.DartboardProjectionCoefficent * 95 &&
                distance <= view.DartboardProjectionCoefficent * 105)
            {
                multiplier = 2;
            }

            if (distance >= view.DartboardProjectionCoefficent * 160 &&
                distance <= view.DartboardProjectionCoefficent * 170)
            {
                multiplier = 3;
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
            else if (angle >= 2.984506 && angle < 3.298665)
            {
                sector = 11;
            }
            else if (angle >= 3.298665 && angle < 3.612824)
            {
                sector = 14;
            }
            else if (angle >= 3.612824 && angle < 3.926983)
            {
                sector = 9;
            }
            else if (angle >= 3.926983 && angle < 4.241142)
            {
                sector = 12;
            }
            else if (angle >= 4.241142 && angle < 4.555301)
            {
                sector = 5;
            }
            else if (angle >= 4.555301 && angle < 4.555301) // todo natural?
            {
                sector = 20;
            }

            return new Throw(poi, sector, multiplier, dartboardProjectionFrame);
        }

        private void SetupMiddlePoints()
        {
            // Setup vertical contour middlepoints 
            if (FindDistance(contourBoxPoint1, contourBoxPoint2) < FindDistance(contourBoxPoint4, contourBoxPoint1))
            {
                contourBoxMiddlePoint1 = FindMiddle(contourBoxPoint1, contourBoxPoint2);
                contourBoxMiddlePoint2 = FindMiddle(contourBoxPoint4, contourBoxPoint3);
            }
            else
            {
                contourBoxMiddlePoint1 = FindMiddle(contourBoxPoint4, contourBoxPoint1);
                contourBoxMiddlePoint2 = FindMiddle(contourBoxPoint3, contourBoxPoint2);
            }
        }

        private void CalculateCamPoi()
        {
            // Find point of impact with surface
            camPoi = FindLinesIntersection(spikeLinePoint1, spikeLinePoint2, workingCam.surfacePoint1, workingCam.surfacePoint2);
            if (camPoi != null)
            {
                drawman.DrawCircle(workingCam.linedFrame, camPoi.Value, view.PointOfImpactRadius, view.PointOfImpactColor, view.PointOfImpactThickness);
            }
        }

        private void CalculateSpikeLine()
        {
            // Find spikeLine to surface
            spikeLinePoint1 = contourBoxMiddlePoint1;
            spikeLinePoint2 = contourBoxMiddlePoint2;
            workingCam.spikeLineLength = workingCam.surfacePoint2.Y - contourBoxMiddlePoint2.Y;
            var angle = FindAngle(contourBoxMiddlePoint2, contourBoxMiddlePoint1);
            spikeLinePoint1.X = (int) (contourBoxMiddlePoint2.X + Math.Cos(angle) * workingCam.spikeLineLength);
            spikeLinePoint1.Y = (int) (contourBoxMiddlePoint2.Y + Math.Sin(angle) * workingCam.spikeLineLength);
            drawman.DrawLine(workingCam.linedFrame, spikeLinePoint1, spikeLinePoint2, view.SpikeLineColor, view.SpikeLineThickness);
        }

        private Point FindLinesIntersection(Point line1Point1, Point line1Point2, Point line2Point1,
                                            Point line2Point2)
        {
            var tolerance = 0.001;
            double x1 = line1Point1.X;
            double y1 = line1Point1.Y;
            double x2 = line1Point2.X;
            double y2 = line1Point2.Y;
            double x3 = line2Point1.X;
            double y3 = line2Point1.Y;
            double x4 = line2Point2.X;
            double y4 = line2Point2.Y;
            double x;
            double y;

            if (Math.Abs(x1 - x2) < tolerance)
            {
                var m2 = (y4 - y3) / (x4 - x3);
                var c2 = -m2 * x3 + y3;
                x = x1;
                y = c2 + m2 * x1;
            }
            else if (Math.Abs(x3 - x4) < tolerance)
            {
                var m1 = (y2 - y1) / (x2 - x1);
                var c1 = -m1 * x1 + y1;
                x = x3;
                y = c1 + m1 * x3;
            }
            else
            {
                var m1 = (y2 - y1) / (x2 - x1);
                var c1 = -m1 * x1 + y1;
                var m2 = (y4 - y3) / (x4 - x3);
                var c2 = -m2 * x3 + y3;
                x = (c1 - c2) / (m2 - m1);
                y = c2 + m2 * x;

            }

            return new Point {X = (int) x, Y = (int) y};
        }

        private Point FindMiddle(Point point1, Point point2)
        {
            var mpX = (point1.X + point2.X) / 2;
            var mpY = (point1.Y + point2.Y) / 2;
            return new Point(mpX, mpY);
        }

        private int FindDistance(Point point1, Point point2)
        {
            return (int) Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
        }

        private double FindAngle(Point point1, Point point2)
        {
            return Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
        }
    }
}