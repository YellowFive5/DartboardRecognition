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

        public void CalculateLines(Cam cam)
        {
            cam.linedFrame = cam.originFrame.Clone();

            var roiRectangle = new Rectangle((int) cam.roiPosXSlider.Value,
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
            var roiRectangle = new Rectangle((int) cam.roiPosXSlider.Value,
                                             (int) cam.roiPosYSlider.Value,
                                             (int) cam.roiWidthSlider.Value,
                                             (int) cam.roiHeightSlider.Value);
            cam.roiFrame = cam.originFrame.Clone();
            cam.roiFrame.ROI = roiRectangle;
        }

        public void CalculateDartContours(Cam cam)
        {
            cam.roiContourFrame = cam.roiFrame.Clone();
            CvInvoke.FindContours(cam.roiTrasholdFrame, cam.contours, cam.matHierarсhy, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
            //CvInvoke.DrawContours(linedFrame, contours, -1, contourColor, contourThickness, offset: new System.Drawing.Point(0, (int)RoiPosYSlider.Value));

            if (cam.contours.Size > 0)
            {
                for (var i = 0; i < cam.contours.Size; i++)
                {
                    // Filter contour
                    var arclength = CvInvoke.ArcLength(cam.contours[i], true);
                    if (arclength < cam.minContourArcLength)
                    {
                        continue;
                    }

                    // Find moments and centerpoint
                    var moments = CvInvoke.Moments(cam.contours[i]);
                    var centerPoint = new Point((int) (moments.M10 / moments.M00), (int) cam.roiPosYSlider.Value + (int) (moments.M01 / moments.M00));
                    drawman.DrawCircle(cam.linedFrame, centerPoint, 4, new Bgr(Color.Blue).MCvScalar, 3);

                    // Find contour rectangle
                    var rect = CvInvoke.MinAreaRect(cam.contours[i]);
                    var box = CvInvoke.BoxPoints(rect);
                    var point1 = new Point((int) box[0].X, (int) cam.roiPosYSlider.Value + (int) box[0].Y);
                    var point2 = new Point((int) box[1].X, (int) cam.roiPosYSlider.Value + (int) box[1].Y);
                    var point3 = new Point((int) box[2].X, (int) cam.roiPosYSlider.Value + (int) box[2].Y);
                    var point4 = new Point((int) box[3].X, (int) cam.roiPosYSlider.Value + (int) box[3].Y);
                    drawman.DrawLine(cam.linedFrame, point1, point2, view.ContourRectColor, view.ContourRectThickness);
                    drawman.DrawLine(cam.linedFrame, point2, point3, view.ContourRectColor, view.ContourRectThickness);
                    drawman.DrawLine(cam.linedFrame, point3, point4, view.ContourRectColor, view.ContourRectThickness);
                    drawman.DrawLine(cam.linedFrame, point4, point1, view.ContourRectColor, view.ContourRectThickness);

                    // Setup vertical contour middlepoints 
                    Point middlePoint1;
                    Point middlePoint2;
                    if (FindDistance(point1, point2) < FindDistance(point4, point1))
                    {
                        middlePoint1 = FindMiddlePoint(point1, point2);
                        middlePoint2 = FindMiddlePoint(point4, point3);
                    }
                    else
                    {
                        middlePoint1 = FindMiddlePoint(point4, point1);
                        middlePoint2 = FindMiddlePoint(point3, point2);
                    }

                    FindSpikelineAndPoi(cam, middlePoint1, middlePoint2);
                }
            }
        }

        private void FindSpikelineAndPoi(Cam cam, Point middlePoint1, Point middlePoint2)
        {
            // Find spikeLine to surface
            var spikePoint1 = middlePoint1;
            var spikePoint2 = middlePoint2;
            cam.spikeLineLength = cam.surfacePoint2.Y - middlePoint2.Y;
            var angle = Math.Atan2(middlePoint1.Y - middlePoint2.Y, middlePoint1.X - middlePoint2.X);
            spikePoint1.X = (int) (middlePoint2.X + Math.Cos(angle) * cam.spikeLineLength);
            spikePoint1.Y = (int) (middlePoint2.Y + Math.Sin(angle) * cam.spikeLineLength);
            drawman.DrawLine(cam.linedFrame, spikePoint1, spikePoint2, view.SpikeLineColor, view.SpikeLineThickness);

            // Find point of impact with surface
            var pointOfImpact = FindIntersectionPoint(spikePoint1, spikePoint2, cam.surfacePoint1, cam.surfacePoint2);
            if (pointOfImpact != null)
            {
                drawman.DrawCircle(cam.linedFrame, pointOfImpact.Value, view.PointOfImpactRadius, view.PointOfImpactColor, view.PointOfImpactThickness);
            }
        }
    }
}