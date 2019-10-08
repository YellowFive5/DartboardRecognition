#region Usings

using System;
using System.Drawing;
using DartboardRecognition.Windows;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;

#endregion

namespace DartboardRecognition.Services
{
    public class MeasureService
    {
        private readonly CamWindow camWindowView;
        private readonly CamService camService;
        private readonly DrawService drawService;
        private readonly ThrowService throwService;
        private VectorOfPoint processedContour;

        public MeasureService(CamWindow camWindowView,
                              CamService camService)
        {
            this.camWindowView = camWindowView;
            this.camService = camService;
            drawService = ServiceBag.All().DrawService;
            throwService = ServiceBag.All().ThrowService;
        }

        public bool FindDartContour()
        {
            var contourFound = false;

            var allContours = new VectorOfVectorOfPoint();
            var matHierarсhy = new Mat();
            CvInvoke.FindContours(camService.RoiLastThrowFrame,
                                  allContours,
                                  matHierarсhy,
                                  RetrType.External,
                                  ChainApproxMethod.ChainApproxNone);

            if (allContours.Size <= 0)
            {
                return false;
            }

            var dartContour = new VectorOfPoint();
            var dartContourArcLength = 0.0;

            for (var i = 0; i < allContours.Size; i++)
            {
                var tempContour = allContours[i];
                var tempContourArcLength = CvInvoke.ArcLength(tempContour, true);
                if (tempContourArcLength > camWindowView.MinContourArcLength &&
                    tempContourArcLength > dartContourArcLength)
                {
                    contourFound = true;
                    dartContourArcLength = tempContourArcLength;
                    dartContour = tempContour;
                    // CvInvoke.DrawContours(camService.linedFrame, contour, -1, contourColor, contourThickness, offset: new System.Drawing.Point(0, (int)RoiPosYSlider.Value));
                }
            }

            if (dartContourArcLength > 0)
            {
                processedContour = dartContour;
            }

            return contourFound;
        }

        public void ProcessDartContour()
        {
            var contourMoments = CvInvoke.Moments(processedContour);
            var contourCenterPoint = new PointF((float) (contourMoments.M10 / contourMoments.M00),
                                                (float) camService.roiPosYSlider + (float) (contourMoments.M01 / contourMoments.M00));

            // drawService.DrawCircle(camService.LinedFrame,
            //                        contourCenterPoint,
            //                        4,
            //                        new Bgr(Color.Blue).MCvScalar,
            //                        3);

            // Find contour rectangle
            var rect = CvInvoke.MinAreaRect(processedContour);
            var box = CvInvoke.BoxPoints(rect);

            // Maybe expected width processing
            // var a1 = FindDistance(new PointF(box[0].X, box[0].Y), new PointF(box[1].X, box[1].Y));
            // var a2 = FindDistance(new PointF(box[1].X, box[1].Y), new PointF(box[2].X, box[2].Y));
            // var a3 = FindDistance(new PointF(box[2].X, box[2].Y), new PointF(box[3].X, box[3].Y));
            // var a4 = FindDistance(new PointF(box[3].X, box[3].Y), new PointF(box[0].X, box[0].Y));
            // ...

            var contourBoxPoint1 = new PointF(box[0].X, (float) camService.roiPosYSlider + box[0].Y);
            var contourBoxPoint2 = new PointF(box[1].X, (float) camService.roiPosYSlider + box[1].Y);
            var contourBoxPoint3 = new PointF(box[2].X, (float) camService.roiPosYSlider + box[2].Y);
            var contourBoxPoint4 = new PointF(box[3].X, (float) camService.roiPosYSlider + box[3].Y);
            // drawService.DrawLine(camService.LinedFrame, contourBoxPoint1, contourBoxPoint2, camWindowView.CamContourRectColor, camWindowView.CamContourRectThickness);
            // drawService.DrawLine(camService.LinedFrame, contourBoxPoint2, contourBoxPoint3, camWindowView.CamContourRectColor, camWindowView.CamContourRectThickness);
            // drawService.DrawLine(camService.LinedFrame, contourBoxPoint3, contourBoxPoint4, camWindowView.CamContourRectColor, camWindowView.CamContourRectThickness);
            // drawService.DrawLine(camService.LinedFrame, contourBoxPoint4, contourBoxPoint1, camWindowView.CamContourRectColor, camWindowView.CamContourRectThickness);

            // Setup vertical contour middlepoints
            var contourWidth = FindDistance(contourBoxPoint1, contourBoxPoint2);
            var contourHeight = FindDistance(contourBoxPoint4, contourBoxPoint1);
            PointF contourBoxMiddlePoint1;
            PointF contourBoxMiddlePoint2;

            if (contourWidth < contourHeight)
            {
                contourBoxMiddlePoint1 = FindMiddle(contourBoxPoint1, contourBoxPoint2);
                contourBoxMiddlePoint2 = FindMiddle(contourBoxPoint4, contourBoxPoint3);
            }
            else
            {
                contourBoxMiddlePoint1 = FindMiddle(contourBoxPoint4, contourBoxPoint1);
                contourBoxMiddlePoint2 = FindMiddle(contourBoxPoint3, contourBoxPoint2);
            }

            // Find spikeLine to surface
            var spikeLinePoint1 = contourBoxMiddlePoint1;
            var spikeLinePoint2 = contourBoxMiddlePoint2;
            var spikeLineLength = camService.surfacePoint2.Y - contourBoxMiddlePoint2.Y;
            var spikeAngle = FindAngle(contourBoxMiddlePoint2, contourBoxMiddlePoint1);
            spikeLinePoint1.X = (float) (contourBoxMiddlePoint2.X + Math.Cos(spikeAngle) * spikeLineLength);
            spikeLinePoint1.Y = (float) (contourBoxMiddlePoint2.Y + Math.Sin(spikeAngle) * spikeLineLength);
            // drawService.DrawLine(camService.LinedFrame, spikeLinePoint1, spikeLinePoint2, camWindowView.CamSpikeLineColor, camWindowView.CamSpikeLineThickness);

            // Find point of impact with surface
            PointF? camPoi = FindLinesIntersection(spikeLinePoint1, spikeLinePoint2, camService.surfacePoint1, camService.surfacePoint2);
            if (camPoi != null)
            {
                // drawService.DrawCircle(camService.LinedFrame, camPoi.Value, camWindowView.ProjectionPoiRadius, camWindowView.ProjectionPoiColor, camWindowView.ProjectionPoiThickness);
            }

            // Translate cam surface POI to dartboard projection
            var frameWidth = camService.RoiLastThrowFrame.Cols;
            var frameSemiWidth = frameWidth / 2;
            var camFovAngle = 100;
            var camFovSemiAngle = camFovAngle / 2;
            var projectionToCenter = new PointF();
            var surfacePoiToCenterDistance = FindDistance(camService.surfaceCenterPoint1, camPoi.GetValueOrDefault());
            var surfaceLeftToPoiDistance = FindDistance(camService.surfaceLeftPoint1, camPoi.GetValueOrDefault());
            var surfaceRightToPoiDistance = FindDistance(camService.surfaceRightPoint1, camPoi.GetValueOrDefault());
            var projectionCamToCenterDistance = frameSemiWidth / Math.Sin(Math.PI * camFovSemiAngle / 180.0) * Math.Cos(Math.PI * camFovSemiAngle / 180.0);
            var projectionCamToPoiDistance = Math.Sqrt(Math.Pow(projectionCamToCenterDistance, 2) + Math.Pow(surfacePoiToCenterDistance, 2));
            var projectionPoiToCenterDistance = Math.Sqrt(Math.Pow(projectionCamToPoiDistance, 2) - Math.Pow(projectionCamToCenterDistance, 2));
            var poiCamCenterAngle = Math.Asin(projectionPoiToCenterDistance / projectionCamToPoiDistance);

            projectionToCenter.X = (float) (camService.setupPoint.X - Math.Cos(camService.toBullAngle) * projectionCamToCenterDistance);
            projectionToCenter.Y = (float) (camService.setupPoint.Y - Math.Sin(camService.toBullAngle) * projectionCamToCenterDistance);

            if (surfaceLeftToPoiDistance < surfaceRightToPoiDistance)
            {
                poiCamCenterAngle *= -1;
            }

            var projectionPoi = new PointF
                                {
                                    X = (float) (camService.setupPoint.X + Math.Cos(camService.toBullAngle + poiCamCenterAngle) * 2000),
                                    Y = (float) (camService.setupPoint.Y + Math.Sin(camService.toBullAngle + poiCamCenterAngle) * 2000)
                                };

            //drawman.DrawCircle(dartboardProjectionFrame, projectionToCenter, view.ProjectionPoiRadius, view.ProjectionPoiColor, view.ProjectionPoiThickness);
            //drawman.DrawCircle(dartboardProjectionFrame, projectionPoi, view.ProjectionPoiRadius, view.ProjectionPoiColor, view.ProjectionPoiThickness);

            // Draw line from cam through projection POI
            var rayPoint = projectionPoi;
            var angle = FindAngle(camService.setupPoint, rayPoint);
            rayPoint.X = (float) (camService.setupPoint.X + Math.Cos(angle) * 2000);
            rayPoint.Y = (float) (camService.setupPoint.Y + Math.Sin(angle) * 2000);

            //drawman.DrawLine(dartboardProjectionFrame, rayPoint2, rayPoint1, view.ProjectionRayColor, view.ProjectionRayThickness);

            var ray = new Ray(camService.setupPoint, rayPoint, contourWidth);

            throwService.SaveRay(ray);
        }

        public static PointF FindLinesIntersection(PointF line1Point1,
                                                   PointF line1Point2,
                                                   PointF line2Point1,
                                                   PointF line2Point2)
        {
            var tolerance = 0.001;
            var x1 = line1Point1.X;
            var y1 = line1Point1.Y;
            var x2 = line1Point2.X;
            var y2 = line1Point2.Y;
            var x3 = line2Point1.X;
            var y3 = line2Point1.Y;
            var x4 = line2Point2.X;
            var y4 = line2Point2.Y;
            float x;
            float y;

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

            return new PointF(x, y);
        }

        private static PointF FindMiddle(PointF point1,
                                         PointF point2)
        {
            var mpX = (point1.X + point2.X) / 2;
            var mpY = (point1.Y + point2.Y) / 2;
            return new PointF(mpX, mpY);
        }

        public static float FindDistance(PointF point1,
                                         PointF point2)
        {
            return (float) Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
        }

        public static float FindAngle(PointF point1,
                                      PointF point2)
        {
            return (float) Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
        }
    }
}