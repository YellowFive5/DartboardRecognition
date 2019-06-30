#region Usings

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Point = System.Drawing.Point;

#endregion

namespace DartboardRecognition
{
    public partial class Measureman
    {
        private MainWindow view;
        private Drawman drawman;
        private Dispatcher dispatcher;
        private ThrowService throwService;
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
        private Point? camPoi;
        private Cam workingCam;
        private Point projectionPoi;
        private Point rayPoint;

        public Measureman(MainWindow view, Drawman drawman, ThrowService throwService)
        {
            this.view = view;
            this.drawman = drawman;
            this.throwService = throwService;
            dispatcher = view.Dispatcher;
        }

        public void SetupWorkingCam(Cam cam)
        {
            workingCam = cam;
        }

        public void CalculateSetupLines()
        {
            workingCam.linedFrame = workingCam.originFrame.Clone();

            roiRectangle = new Rectangle((int) workingCam.roiPosXSlider,
                                         (int) workingCam.roiPosYSlider,
                                         (int) workingCam.roiWidthSlider,
                                         (int) workingCam.roiHeightSlider);
            drawman.DrawRectangle(workingCam.linedFrame, roiRectangle, view.CamRoiRectColor.MCvScalar, view.CamRoiRectThickness);

            workingCam.surfacePoint1 = new Point(0, (int) workingCam.surfaceSlider);
            workingCam.surfacePoint2 = new Point(workingCam.originFrame.Cols, (int) workingCam.surfaceSlider);
            drawman.DrawLine(workingCam.linedFrame, workingCam.surfacePoint1, workingCam.surfacePoint2, view.CamSurfaceLineColor.MCvScalar, view.CamSurfaceLineThickness);

            workingCam.surfaceCenterPoint1 = new Point
                                             {
                                                 X = (int) workingCam.surfaceCenterSlider,
                                                 Y = (int) workingCam.surfaceSlider
                                             };
            workingCam.surfaceCenterPoint2 = new Point
                                             {
                                                 X = workingCam.surfaceCenterPoint1.X,
                                                 Y = workingCam.surfaceCenterPoint1.Y - 50
                                             };
            drawman.DrawLine(workingCam.linedFrame, workingCam.surfaceCenterPoint1, workingCam.surfaceCenterPoint2, view.CamSurfaceLineColor.MCvScalar, view.CamSurfaceLineThickness);

            workingCam.surfaceLeftPoint1 = new Point
                                           {
                                               X = (int) workingCam.surfaceLeftSlider,
                                               Y = (int) workingCam.surfaceSlider
                                           };
            workingCam.surfaceLeftPoint2 = new Point
                                           {
                                               X = workingCam.surfaceLeftPoint1.X,
                                               Y = workingCam.surfaceLeftPoint1.Y - 50
                                           };
            drawman.DrawLine(workingCam.linedFrame, workingCam.surfaceLeftPoint1, workingCam.surfaceLeftPoint2, view.CamSurfaceLineColor.MCvScalar, view.CamSurfaceLineThickness);

            workingCam.surfaceRightPoint1 = new Point
                                            {
                                                X = (int) workingCam.surfaceRightSlider,
                                                Y = (int) workingCam.surfaceSlider
                                            };
            workingCam.surfaceRightPoint2 = new Point
                                            {
                                                X = workingCam.surfaceRightPoint1.X,
                                                Y = workingCam.surfaceRightPoint1.Y - 50
                                            };
            drawman.DrawLine(workingCam.linedFrame, workingCam.surfaceRightPoint1, workingCam.surfaceRightPoint2, view.CamSurfaceLineColor.MCvScalar, view.CamSurfaceLineThickness);
        }

        public void CalculateRoiRegion()
        {
            workingCam.roiFrame = workingCam.originFrame.Clone();
            workingCam.roiFrame.ROI = roiRectangle;
        }

        public void ProcessDartContour()
        {
            PrepareContour();

            CalculateSpikeLine();

            CalculateCamPoi();

            TranslateCamPoiToProjection();

            CalculateCamThroughPoiRay();

            workingCam.dartContours.Clear();
            workingCam.allContours.Clear();
        }

        private void PrepareContour()
        {
            var contour = workingCam.dartContours.Pop();
            contourMoments = CvInvoke.Moments(contour);
            contourCenterPoint = new Point((int) (contourMoments.M10 / contourMoments.M00), (int) workingCam.roiPosYSlider + (int) (contourMoments.M01 / contourMoments.M00));
            drawman.DrawCircle(workingCam.linedFrame, contourCenterPoint, 4, new Bgr(Color.Blue).MCvScalar, 3);

            // Find contour rectangle
            var rect = CvInvoke.MinAreaRect(contour);
            var box = CvInvoke.BoxPoints(rect);
            contourBoxPoint1 = new Point((int) box[0].X, (int) workingCam.roiPosYSlider + (int) box[0].Y);
            contourBoxPoint2 = new Point((int) box[1].X, (int) workingCam.roiPosYSlider + (int) box[1].Y);
            contourBoxPoint3 = new Point((int) box[2].X, (int) workingCam.roiPosYSlider + (int) box[2].Y);
            contourBoxPoint4 = new Point((int) box[3].X, (int) workingCam.roiPosYSlider + (int) box[3].Y);
            drawman.DrawLine(workingCam.linedFrame, contourBoxPoint1, contourBoxPoint2, view.CamContourRectColor, view.CamContourRectThickness);
            drawman.DrawLine(workingCam.linedFrame, contourBoxPoint2, contourBoxPoint3, view.CamContourRectColor, view.CamContourRectThickness);
            drawman.DrawLine(workingCam.linedFrame, contourBoxPoint3, contourBoxPoint4, view.CamContourRectColor, view.CamContourRectThickness);
            drawman.DrawLine(workingCam.linedFrame, contourBoxPoint4, contourBoxPoint1, view.CamContourRectColor, view.CamContourRectThickness);

            // Setup vertical contour middlepoints
            var contourWidth = FindDistance(contourBoxPoint1, contourBoxPoint2);
            var contourHeight = FindDistance(contourBoxPoint4, contourBoxPoint1);
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
            drawman.DrawLine(workingCam.linedFrame, spikeLinePoint1, spikeLinePoint2, view.CamSpikeLineColor, view.CamSpikeLineThickness);
        }

        private void CalculateCamPoi()
        {
            // Find point of impact with surface
            camPoi = FindLinesIntersection(spikeLinePoint1, spikeLinePoint2, workingCam.surfacePoint1, workingCam.surfacePoint2);
            if (camPoi != null)
            {
                drawman.DrawCircle(workingCam.linedFrame, camPoi.Value, view.ProjectionPoiRadius, view.ProjectionPoiColor, view.ProjectionPoiThickness);
            }
        }

        private void TranslateCamPoiToProjection()
        {
            // Translate cam surface POI to dartboard projection
            var frameWidth = workingCam.originFrame.Cols;
            var frameSemiWidth = frameWidth / 2;
            var camFovAngle = 100;
            var camFovSemiAngle = camFovAngle / 2;
            var projectionToCenter = new Point();
            var surfacePoiToCenterDistance = FindDistance(workingCam.surfaceCenterPoint1, camPoi.GetValueOrDefault());
            var surfaceLeftToPoiDistance = FindDistance(workingCam.surfaceLeftPoint1, camPoi.GetValueOrDefault());
            var surfaceRightToPoiDistance = FindDistance(workingCam.surfaceRightPoint1, camPoi.GetValueOrDefault());
            var projectionCamToCenterDistance = frameSemiWidth / Math.Sin(Math.PI * camFovSemiAngle / 180.0) * Math.Cos(Math.PI * camFovSemiAngle / 180.0);
            var projectionCamToPoiDistance = Math.Sqrt(Math.Pow(projectionCamToCenterDistance, 2) + Math.Pow(surfacePoiToCenterDistance, 2));
            var projectionPoiToCenterDistance = Math.Sqrt(Math.Pow(projectionCamToPoiDistance, 2) - Math.Pow(projectionCamToCenterDistance, 2));
            var poiCamCenterAngle = Math.Asin(projectionPoiToCenterDistance / projectionCamToPoiDistance);

            projectionToCenter.X = (int) (workingCam.setupPoint.X - Math.Cos(workingCam.toBullAngle) * projectionCamToCenterDistance);
            projectionToCenter.Y = (int) (workingCam.setupPoint.Y - Math.Sin(workingCam.toBullAngle) * projectionCamToCenterDistance);

            if (surfaceLeftToPoiDistance < surfaceRightToPoiDistance)
            {
                poiCamCenterAngle *= -1;
            }

            projectionPoi.X = (int) (workingCam.setupPoint.X + Math.Cos(workingCam.toBullAngle + poiCamCenterAngle) * 2000);
            projectionPoi.Y = (int) (workingCam.setupPoint.Y + Math.Sin(workingCam.toBullAngle + poiCamCenterAngle) * 2000);

            //drawman.DrawCircle(dartboardProjectionFrame, projectionToCenter, view.ProjectionPoiRadius, view.ProjectionPoiColor, view.ProjectionPoiThickness);
            //drawman.DrawCircle(dartboardProjectionFrame, projectionPoi, view.ProjectionPoiRadius, view.ProjectionPoiColor, view.ProjectionPoiThickness);
        }

        private void CalculateCamThroughPoiRay()
        {
            // Draw line from cam through projection POI
            rayPoint = projectionPoi;
            var angle = FindAngle(workingCam.setupPoint, rayPoint);
            rayPoint.X = (int) (workingCam.setupPoint.X + Math.Cos(angle) * 2000);
            rayPoint.Y = (int) (workingCam.setupPoint.Y + Math.Sin(angle) * 2000);

            //drawman.DrawLine(dartboardProjectionFrame, rayPoint2, rayPoint1, view.ProjectionRayColor, view.ProjectionRayThickness);

            throwService.SaveRay(rayPoint, workingCam);
        }

        public bool DetectThrow()
        {
            bool dartsExtraction;
            bool moveDetected;
            var throwDetected = false;
            var zeroImage = workingCam.roiTrasholdFrame;
            var firstImage = workingCam.videoCapture.QueryFrame().ToImage<Gray, byte>().Not();
            using (firstImage)
            {
                var diffImage = DiffImage(firstImage, zeroImage);
                var moves = diffImage.CountNonzero()[0];
                moveDetected = moves > 100;
                if (moveDetected)
                {
                    Thread.Sleep(1500);
                    var secondImage = workingCam.videoCapture.QueryFrame().ToImage<Gray, byte>().Not();
                    using (secondImage)
                    {
                        diffImage = DiffImage(secondImage, zeroImage);
                        moves = diffImage.CountNonzero()[0];

                        dartsExtraction = moves > 7000;
                        throwDetected = !dartsExtraction && moves > 500;

                        if (dartsExtraction)
                        {
                            Thread.Sleep(4000);
                            dispatcher.Invoke(new Action(() => view.PointsBox.Text = ""));
                        }
                        else if (throwDetected)
                        {
                            workingCam.roiTrasholdFrameLastThrow = diffImage;
                            dispatcher.Invoke(new Action(() => view.PointsBox.Text += $"\n{workingCam} - {moves}"));
                        }
                    }

                    workingCam.originFrame = workingCam.videoCapture.QueryFrame().ToImage<Bgr, byte>();
                    CalculateSetupLines();
                    CalculateRoiRegion();
                    drawman.TresholdRoiRegion(workingCam);
                }
            }

            return throwDetected;
        }

        private Image<Gray, byte> DiffImage(Image<Gray, byte> image, Image<Gray, byte> originImage)
        {
            image.ROI = roiRectangle;
            image._SmoothGaussian(3);
            image._ThresholdBinary(new Gray(workingCam.tresholdMinSlider),
                                   new Gray(workingCam.tresholdMaxSlider));
            var diffImage = image.AbsDiff(originImage);
            return diffImage;
        }

        public bool FindDartContour()
        {
            var dartContourFound = false;
            CvInvoke.FindContours(workingCam.roiTrasholdFrameLastThrow, workingCam.allContours, workingCam.matHierarсhy, RetrType.External, ChainApproxMethod.ChainApproxNone);

            if (workingCam.allContours.Size <= 0)
            {
                return false;
            }

            for (var i = 0; i < workingCam.allContours.Size; i++)
            {
                var contour = workingCam.allContours[i];
                var arclength = CvInvoke.ArcLength(contour, true);
                if (arclength > view.minContourArcLength &&
                    arclength < view.maxContourArcLength)
                {
                    dartContourFound = true;
                    workingCam.dartContours.Push(contour);
                    // CvInvoke.DrawContours(workingCam.linedFrame, contour, -1, contourColor, contourThickness, offset: new System.Drawing.Point(0, (int)RoiPosYSlider.Value));
                }
            }

            return dartContourFound;
        }
    }
}