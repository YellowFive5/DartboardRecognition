#region Usings

using System;
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
        private Point dartboardCenterPoint;
        private Point surfaceProjectionLineCam1Point1;
        private Point surfaceProjectionLineCam1Point2;
        private Point surfaceProjectionLineCam2Point1;
        private Point surfaceProjectionLineCam2Point2;
        private Point? pointOfImpact;
        private int poiToCenterDistance;
        private Image<Bgr, byte> dartboardProjectionFrame;

        public Measureman(MainWindow view, Drawman drawman)
        {
            this.view = view;
            this.drawman = drawman;
        }

        private Point FindMiddlePoint(Point point1, Point point2)
        {
            var mpX = (point1.X + point2.X) / 2;
            var mpY = (point1.Y + point2.Y) / 2;
            return new Point(mpX, mpY);
        }

        private int FindDistance(Point point1, Point point2)
        {
            return (int) Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
        }

        private Point? FindIntersectionPoint(Point line1Point1, Point line1Point2, Point line2Point1,
                                            Point line2Point2)
        {
            float a1 = line1Point2.Y - line1Point1.Y;
            float b1 = line1Point2.X - line1Point1.X;
            var c1 = a1 * line1Point1.X + b1 * line1Point1.Y;

            float a2 = line2Point2.Y - line2Point1.Y;
            float b2 = line2Point2.X - line2Point1.X;
            var c2 = a2 * line2Point1.X + b2 * line2Point1.Y;

            var det = a1 * b2 - a2 * b1;
            if (det == 0)
            {
                return null;
            }
            else
            {
                var x = (int) ((b2 * c1 - b1 * c2) / det);
                var y = (int) ((a1 * c2 - a2 * c1) / det);
                return new Point(x, y);
            }
        }

        public void CalculateSetupLines(Cam cam)
        {
            cam.linedFrame = cam.originFrame.Clone();

            roiRectangle = new Rectangle((int) cam.roiPosXSlider.Value,
                                         (int) cam.roiPosYSlider.Value,
                                         (int) cam.roiWidthSlider.Value,
                                         (int) cam.roiHeightSlider.Value);
            drawman.DrawRectangle(cam.linedFrame, roiRectangle, view.RoiRectColor.MCvScalar, view.RoiRectThickness);

            cam.surfacePoint1 = new Point(0, (int) cam.surfaceSlider.Value);
            cam.surfacePoint2 = new Point(cam.originFrame.Cols, (int) cam.surfaceSlider.Value);
            drawman.DrawLine(cam.linedFrame, cam.surfacePoint1, cam.surfacePoint2, view.SurfaceLineColor.MCvScalar, view.SurfaceLineThickness);

            cam.surfaceCenterPoint1 = new Point
                                      {
                                          X = (int) cam.surfaceCenterSlider.Value,
                                          Y = (int) cam.surfaceSlider.Value
                                      };
            cam.surfaceCenterPoint2 = new Point
                                      {
                                          X = cam.surfaceCenterPoint1.X,
                                          Y = cam.surfaceCenterPoint1.Y - 50
                                      };
            drawman.DrawLine(cam.linedFrame, cam.surfaceCenterPoint1, cam.surfaceCenterPoint2, view.SurfaceLineColor.MCvScalar, view.SurfaceLineThickness);
        }

        public void CalculateRoiRegion(Cam cam)
        {
            cam.roiFrame = cam.originFrame.Clone();
            cam.roiFrame.ROI = roiRectangle;
        }

        public void CalculateDartboardProjection()
        {
            // Draw dartboard projection
            dartboardProjectionFrame = new Image<Bgr, byte>(view.DartboardProjectionFrameWidth, view.DartboardProjectionFrameHeight);
            dartboardCenterPoint = new Point(dartboardProjectionFrame.Width / 2, dartboardProjectionFrame.Height / 2);
            drawman.DrawCircle(dartboardProjectionFrame, dartboardCenterPoint, view.DartboardProjectionCoefficent * 7, view.DartboardProjectionColor, view.DartboardProjectionThickness);
            drawman.DrawCircle(dartboardProjectionFrame, dartboardCenterPoint, view.DartboardProjectionCoefficent * 17, view.DartboardProjectionColor, view.DartboardProjectionThickness);
            drawman.DrawCircle(dartboardProjectionFrame, dartboardCenterPoint, view.DartboardProjectionCoefficent * 95, view.DartboardProjectionColor, view.DartboardProjectionThickness);
            drawman.DrawCircle(dartboardProjectionFrame, dartboardCenterPoint, view.DartboardProjectionCoefficent * 105, view.DartboardProjectionColor, view.DartboardProjectionThickness);
            drawman.DrawCircle(dartboardProjectionFrame, dartboardCenterPoint, view.DartboardProjectionCoefficent * 160, view.DartboardProjectionColor, view.DartboardProjectionThickness);
            drawman.DrawCircle(dartboardProjectionFrame, dartboardCenterPoint, view.DartboardProjectionCoefficent * 170, view.DartboardProjectionColor, view.DartboardProjectionThickness);
            for (var i = 0; i <= 360; i += 9)
            {
                var segmentPoint1 = new Point
                                    {
                                        X = (int)(dartboardCenterPoint.X + Math.Cos(0.314159 * i - 0.15708) * view.DartboardProjectionCoefficent * 170),
                                        Y = (int)(dartboardCenterPoint.Y + Math.Sin(0.314159 * i - 0.15708) * view.DartboardProjectionCoefficent * 170)
                                    };
                var segmentPoint2 = new Point
                                    {
                                        X = (int)(dartboardCenterPoint.X + Math.Cos(0.314159 * i - 0.15708) * view.DartboardProjectionCoefficent * 17),
                                        Y = (int)(dartboardCenterPoint.Y + Math.Sin(0.314159 * i - 0.15708) * view.DartboardProjectionCoefficent * 17)
                                    };
                drawman.DrawLine(dartboardProjectionFrame, segmentPoint1, segmentPoint2, view.DartboardProjectionColor, view.DartboardProjectionThickness);
            }

            // Draw surface projection lines
            surfaceProjectionLineCam1Point1.X = (int) (dartboardCenterPoint.X + Math.Cos(-0.785398) * view.DartboardProjectionCoefficent * 170);
            surfaceProjectionLineCam1Point1.Y = (int)(dartboardCenterPoint.Y + Math.Sin(-0.785398) * view.DartboardProjectionCoefficent * 170);
            surfaceProjectionLineCam1Point2.X = (int) (dartboardCenterPoint.X + Math.Cos(-3.92699) * view.DartboardProjectionCoefficent * 170);
            surfaceProjectionLineCam1Point2.Y = (int)(dartboardCenterPoint.Y + Math.Sin(-3.92699) * view.DartboardProjectionCoefficent * 170);
            drawman.DrawLine(dartboardProjectionFrame, surfaceProjectionLineCam1Point1, surfaceProjectionLineCam1Point2, view.SurfaceProjectionLineColor, view.SurfaceProjectionLineThickness);

            surfaceProjectionLineCam2Point1.X = (int) (dartboardCenterPoint.X + Math.Cos(0.785398) * view.DartboardProjectionCoefficent * 170);
            surfaceProjectionLineCam2Point1.Y = (int)(dartboardCenterPoint.Y + Math.Sin(0.785398) * view.DartboardProjectionCoefficent * 170);
            surfaceProjectionLineCam2Point2.X = (int) (dartboardCenterPoint.X + Math.Cos(3.92699) * view.DartboardProjectionCoefficent * 170);
            surfaceProjectionLineCam2Point2.Y = (int)(dartboardCenterPoint.Y + Math.Sin(3.92699) * view.DartboardProjectionCoefficent * 170);
            drawman.DrawLine(dartboardProjectionFrame, surfaceProjectionLineCam2Point1, surfaceProjectionLineCam2Point2, view.SurfaceProjectionLineColor, view.SurfaceProjectionLineThickness);

            drawman.SaveBitmapToImageBox(dartboardProjectionFrame, view.ImageBox3);
        }

        public void CalculateDartContours(Cam cam)
        {
            cam.roiContourFrame = cam.roiFrame.Clone();
            CvInvoke.FindContours(cam.roiTrasholdFrame, cam.contours, cam.matHierarсhy, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
            //CvInvoke.DrawContours(linedFrame, contours, -1, contourColor, contourThickness, offset: new System.Drawing.Point(0, (int)RoiPosYSlider.Value));

            if (cam.contours.Size <= 0)
            {
                return;
            }

            for (var i = 0; i < cam.contours.Size; i++)
            {
                // Filter contour
                var arclength = CvInvoke.ArcLength(cam.contours[i], true);
                if (arclength < cam.minContourArcLength)
                {
                    continue;
                }

                CalculateMomentsAndCenterPoints(cam, i);

                CalculateContourRectangle(cam, i);

                SetupMiddlePoints();

                CalculateSpikeLine(cam);

                CalculatePoi(cam);

                // Find distance between POI and centerpoint
                poiToCenterDistance = FindDistance(pointOfImpact.Value, cam.surfaceCenterPoint1);

            }
        }

        private void CalculateMomentsAndCenterPoints(Cam cam, int i)
        {
            // Find moments and centerpoint
            contourMoments = CvInvoke.Moments(cam.contours[i]);
            contourCenterPoint = new Point((int) (contourMoments.M10 / contourMoments.M00), (int) cam.roiPosYSlider.Value + (int) (contourMoments.M01 / contourMoments.M00));
            drawman.DrawCircle(cam.linedFrame, contourCenterPoint, 4, new Bgr(Color.Blue).MCvScalar, 3);
        }

        private void CalculateContourRectangle(Cam cam, int i)
        {
            // Find contour rectangle
            var rect = CvInvoke.MinAreaRect(cam.contours[i]);
            var box = CvInvoke.BoxPoints(rect);
            contourBoxPoint1 = new Point((int) box[0].X, (int) cam.roiPosYSlider.Value + (int) box[0].Y);
            contourBoxPoint2 = new Point((int) box[1].X, (int) cam.roiPosYSlider.Value + (int) box[1].Y);
            contourBoxPoint3 = new Point((int) box[2].X, (int) cam.roiPosYSlider.Value + (int) box[2].Y);
            contourBoxPoint4 = new Point((int) box[3].X, (int) cam.roiPosYSlider.Value + (int) box[3].Y);
            drawman.DrawLine(cam.linedFrame, contourBoxPoint1, contourBoxPoint2, view.ContourRectColor, view.ContourRectThickness);
            drawman.DrawLine(cam.linedFrame, contourBoxPoint2, contourBoxPoint3, view.ContourRectColor, view.ContourRectThickness);
            drawman.DrawLine(cam.linedFrame, contourBoxPoint3, contourBoxPoint4, view.ContourRectColor, view.ContourRectThickness);
            drawman.DrawLine(cam.linedFrame, contourBoxPoint4, contourBoxPoint1, view.ContourRectColor, view.ContourRectThickness);
        }

        private void SetupMiddlePoints()
        {
            // Setup vertical contour middlepoints 
            if (FindDistance(contourBoxPoint1, contourBoxPoint2) < FindDistance(contourBoxPoint4, contourBoxPoint1))
            {
                contourBoxMiddlePoint1 = FindMiddlePoint(contourBoxPoint1, contourBoxPoint2);
                contourBoxMiddlePoint2 = FindMiddlePoint(contourBoxPoint4, contourBoxPoint3);
            }
            else
            {
                contourBoxMiddlePoint1 = FindMiddlePoint(contourBoxPoint4, contourBoxPoint1);
                contourBoxMiddlePoint2 = FindMiddlePoint(contourBoxPoint3, contourBoxPoint2);
            }
        }

        private void CalculatePoi(Cam cam)
        {
            // Find point of impact with surface
            pointOfImpact = FindIntersectionPoint(spikeLinePoint1, spikeLinePoint2, cam.surfacePoint1, cam.surfacePoint2);
            if (pointOfImpact != null)
            {
                drawman.DrawCircle(cam.linedFrame, pointOfImpact.Value, view.PointOfImpactRadius, view.PointOfImpactColor, view.PointOfImpactThickness);
            }
        }

        private void CalculateSpikeLine(Cam cam)
        {
            // Find spikeLine to surface
            spikeLinePoint1 = contourBoxMiddlePoint1;
            spikeLinePoint2 = contourBoxMiddlePoint2;
            cam.spikeLineLength = cam.surfacePoint2.Y - contourBoxMiddlePoint2.Y;
            var angle = Math.Atan2(contourBoxMiddlePoint1.Y - contourBoxMiddlePoint2.Y, contourBoxMiddlePoint1.X - contourBoxMiddlePoint2.X);
            spikeLinePoint1.X = (int) (contourBoxMiddlePoint2.X + Math.Cos(angle) * cam.spikeLineLength);
            spikeLinePoint1.Y = (int) (contourBoxMiddlePoint2.Y + Math.Sin(angle) * cam.spikeLineLength);
            drawman.DrawLine(cam.linedFrame, spikeLinePoint1, spikeLinePoint2, view.SpikeLineColor, view.SpikeLineThickness);
        }
    }
}