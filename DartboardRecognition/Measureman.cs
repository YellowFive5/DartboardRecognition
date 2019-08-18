#region Usings

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

#endregion

namespace DartboardRecognition
{
    public partial class Measureman
    {
        private CamWindow camWindowView;
        private Drawman drawman;
        private Dispatcher camWindowDispatcher;
        private ThrowService throwService;
        private Rectangle roiRectangle;
        private Moments contourMoments;
        private PointF contourCenterPoint;
        private PointF contourBoxPoint1;
        private PointF contourBoxPoint2;
        private PointF contourBoxPoint3;
        private PointF contourBoxPoint4;
        private PointF contourBoxMiddlePoint1;
        private PointF contourBoxMiddlePoint2;
        private PointF spikeLinePoint1;
        private PointF spikeLinePoint2;
        private PointF? camPoi;
        private Cam workingCam;
        private PointF projectionPoi;
        private PointF rayPoint;

        public Measureman(CamWindow camWindowView, Drawman drawman, ThrowService throwService)
        {
            this.camWindowView = camWindowView;
            this.drawman = drawman;
            this.throwService = throwService;
            camWindowDispatcher = camWindowView.Dispatcher;
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

            drawman.DrawRectangle(workingCam.linedFrame,
                                  roiRectangle,
                                  camWindowView.CamRoiRectColor.MCvScalar,
                                  camWindowView.CamRoiRectThickness);

            workingCam.surfacePoint1 = new PointF(0, (float) workingCam.surfaceSlider);
            workingCam.surfacePoint2 = new PointF(workingCam.originFrame.Cols,
                                                  (float) workingCam.surfaceSlider);
            drawman.DrawLine(workingCam.linedFrame,
                             workingCam.surfacePoint1,
                             workingCam.surfacePoint2,
                             camWindowView.CamSurfaceLineColor.MCvScalar,
                             camWindowView.CamSurfaceLineThickness);

            workingCam.surfaceCenterPoint1 = new PointF((float) workingCam.surfaceCenterSlider,
                                                        (float) workingCam.surfaceSlider);

            workingCam.surfaceCenterPoint2 = new PointF(workingCam.surfaceCenterPoint1.X,
                                                        workingCam.surfaceCenterPoint1.Y - 50);
            drawman.DrawLine(workingCam.linedFrame,
                             workingCam.surfaceCenterPoint1,
                             workingCam.surfaceCenterPoint2,
                             camWindowView.CamSurfaceLineColor.MCvScalar,
                             camWindowView.CamSurfaceLineThickness);

            workingCam.surfaceLeftPoint1 = new PointF((float) workingCam.surfaceLeftSlider,
                                                      (float) workingCam.surfaceSlider);
            workingCam.surfaceLeftPoint2 = new PointF(workingCam.surfaceLeftPoint1.X,
                                                      workingCam.surfaceLeftPoint1.Y - 50);
            drawman.DrawLine(workingCam.linedFrame,
                             workingCam.surfaceLeftPoint1,
                             workingCam.surfaceLeftPoint2,
                             camWindowView.CamSurfaceLineColor.MCvScalar,
                             camWindowView.CamSurfaceLineThickness);

            workingCam.surfaceRightPoint1 = new PointF((float) workingCam.surfaceRightSlider,
                                                       (float) workingCam.surfaceSlider);
            workingCam.surfaceRightPoint2 = new PointF(workingCam.surfaceRightPoint1.X,
                                                       workingCam.surfaceRightPoint1.Y - 50);
            drawman.DrawLine(workingCam.linedFrame,
                             workingCam.surfaceRightPoint1,
                             workingCam.surfaceRightPoint2,
                             camWindowView.CamSurfaceLineColor.MCvScalar,
                             camWindowView.CamSurfaceLineThickness);
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
            contourCenterPoint = new PointF((float) (contourMoments.M10 / contourMoments.M00),
                                            (float) workingCam.roiPosYSlider + (float) (contourMoments.M01 / contourMoments.M00));
            drawman.DrawCircle(workingCam.linedFrame,
                               contourCenterPoint,
                               4,
                               new Bgr(Color.Blue).MCvScalar,
                               3);

            // Find contour rectangle
            var rect = CvInvoke.MinAreaRect(contour);
            var box = CvInvoke.BoxPoints(rect);

            // Maybe expected width processing
            // var a1 = FindDistance(new PointF(box[0].X, box[0].Y), new PointF(box[1].X, box[1].Y));
            // var a2 = FindDistance(new PointF(box[1].X, box[1].Y), new PointF(box[2].X, box[2].Y));
            // var a3 = FindDistance(new PointF(box[2].X, box[2].Y), new PointF(box[3].X, box[3].Y));
            // var a4 = FindDistance(new PointF(box[3].X, box[3].Y), new PointF(box[0].X, box[0].Y));
            // ...

            contourBoxPoint1 = new PointF(box[0].X, (float) workingCam.roiPosYSlider + box[0].Y);
            contourBoxPoint2 = new PointF(box[1].X, (float) workingCam.roiPosYSlider + box[1].Y);
            contourBoxPoint3 = new PointF(box[2].X, (float) workingCam.roiPosYSlider + box[2].Y);
            contourBoxPoint4 = new PointF(box[3].X, (float) workingCam.roiPosYSlider + box[3].Y);
            drawman.DrawLine(workingCam.linedFrame, contourBoxPoint1, contourBoxPoint2, camWindowView.CamContourRectColor, camWindowView.CamContourRectThickness);
            drawman.DrawLine(workingCam.linedFrame, contourBoxPoint2, contourBoxPoint3, camWindowView.CamContourRectColor, camWindowView.CamContourRectThickness);
            drawman.DrawLine(workingCam.linedFrame, contourBoxPoint3, contourBoxPoint4, camWindowView.CamContourRectColor, camWindowView.CamContourRectThickness);
            drawman.DrawLine(workingCam.linedFrame, contourBoxPoint4, contourBoxPoint1, camWindowView.CamContourRectColor, camWindowView.CamContourRectThickness);

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
            spikeLinePoint1.X = (float) (contourBoxMiddlePoint2.X + Math.Cos(angle) * workingCam.spikeLineLength);
            spikeLinePoint1.Y = (float) (contourBoxMiddlePoint2.Y + Math.Sin(angle) * workingCam.spikeLineLength);
            drawman.DrawLine(workingCam.linedFrame, spikeLinePoint1, spikeLinePoint2, camWindowView.CamSpikeLineColor, camWindowView.CamSpikeLineThickness);
        }

        private void CalculateCamPoi()
        {
            // Find point of impact with surface
            camPoi = FindLinesIntersection(spikeLinePoint1, spikeLinePoint2, workingCam.surfacePoint1, workingCam.surfacePoint2);
            if (camPoi != null)
            {
                drawman.DrawCircle(workingCam.linedFrame, camPoi.Value, camWindowView.ProjectionPoiRadius, camWindowView.ProjectionPoiColor, camWindowView.ProjectionPoiThickness);
            }
        }

        private void TranslateCamPoiToProjection()
        {
            // Translate cam surface POI to dartboard projection
            var frameWidth = workingCam.originFrame.Cols;
            var frameSemiWidth = frameWidth / 2;
            var camFovAngle = 100;
            var camFovSemiAngle = camFovAngle / 2;
            var projectionToCenter = new PointF();
            var surfacePoiToCenterDistance = FindDistance(workingCam.surfaceCenterPoint1, camPoi.GetValueOrDefault());
            var surfaceLeftToPoiDistance = FindDistance(workingCam.surfaceLeftPoint1, camPoi.GetValueOrDefault());
            var surfaceRightToPoiDistance = FindDistance(workingCam.surfaceRightPoint1, camPoi.GetValueOrDefault());
            var projectionCamToCenterDistance = frameSemiWidth / Math.Sin(Math.PI * camFovSemiAngle / 180.0) * Math.Cos(Math.PI * camFovSemiAngle / 180.0);
            var projectionCamToPoiDistance = Math.Sqrt(Math.Pow(projectionCamToCenterDistance, 2) + Math.Pow(surfacePoiToCenterDistance, 2));
            var projectionPoiToCenterDistance = Math.Sqrt(Math.Pow(projectionCamToPoiDistance, 2) - Math.Pow(projectionCamToCenterDistance, 2));
            var poiCamCenterAngle = Math.Asin(projectionPoiToCenterDistance / projectionCamToPoiDistance);

            projectionToCenter.X = (float) (workingCam.setupPoint.X - Math.Cos(workingCam.toBullAngle) * projectionCamToCenterDistance);
            projectionToCenter.Y = (float) (workingCam.setupPoint.Y - Math.Sin(workingCam.toBullAngle) * projectionCamToCenterDistance);

            if (surfaceLeftToPoiDistance < surfaceRightToPoiDistance)
            {
                poiCamCenterAngle *= -1;
            }

            projectionPoi.X = (float) (workingCam.setupPoint.X + Math.Cos(workingCam.toBullAngle + poiCamCenterAngle) * 2000);
            projectionPoi.Y = (float) (workingCam.setupPoint.Y + Math.Sin(workingCam.toBullAngle + poiCamCenterAngle) * 2000);

            //drawman.DrawCircle(dartboardProjectionFrame, projectionToCenter, view.ProjectionPoiRadius, view.ProjectionPoiColor, view.ProjectionPoiThickness);
            //drawman.DrawCircle(dartboardProjectionFrame, projectionPoi, view.ProjectionPoiRadius, view.ProjectionPoiColor, view.ProjectionPoiThickness);
        }

        private void CalculateCamThroughPoiRay()
        {
            // Draw line from cam through projection POI
            rayPoint = projectionPoi;
            var angle = FindAngle(workingCam.setupPoint, rayPoint);
            rayPoint.X = (float) (workingCam.setupPoint.X + Math.Cos(angle) * 2000);
            rayPoint.Y = (float) (workingCam.setupPoint.Y + Math.Sin(angle) * 2000);

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
                moveDetected = moves > 400;
                if (moveDetected)
                {
                    Thread.Sleep(1500);
                    var secondImage = workingCam.videoCapture.QueryFrame().ToImage<Gray, byte>().Not();
                    using (secondImage)
                    {
                        diffImage = DiffImage(secondImage, zeroImage);
                        moves = diffImage.CountNonzero()[0];

                        dartsExtraction = moves > 7000;
                        throwDetected = !dartsExtraction && moves > 550;

                        if (dartsExtraction)
                        {
                            Thread.Sleep(4000);
                            // dispatcher.Invoke(new Action(() => view.PointsBox.Text = ""));
                        }
                        else if (throwDetected)
                        {
                            workingCam.roiTrasholdFrameLastThrow = diffImage;
                            // dispatcher.Invoke(new Action(() => view.PointsBox.Text += $"\n{workingCam} - {moves}"));
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
            CvInvoke.FindContours(workingCam.roiTrasholdFrameLastThrow,
                                  workingCam.allContours,
                                  workingCam.matHierarсhy,
                                  RetrType.External,
                                  ChainApproxMethod.ChainApproxNone);

            if (workingCam.allContours.Size <= 0)
            {
                return false;
            }

            var dartContour = new VectorOfPoint();
            var dartContourArcLength = 0.0;

            for (var i = 0; i < workingCam.allContours.Size; i++)
            {
                var tempContour = workingCam.allContours[i];
                var tempContourArcLength = CvInvoke.ArcLength(tempContour, true);
                if (tempContourArcLength > camWindowView.minContourArcLength &&
                    tempContourArcLength > dartContourArcLength)
                {
                    dartContourFound = true;
                    dartContourArcLength = tempContourArcLength;
                    dartContour = tempContour;
                    // CvInvoke.DrawContours(workingCam.linedFrame, contour, -1, contourColor, contourThickness, offset: new System.Drawing.Point(0, (int)RoiPosYSlider.Value));
                }
            }

            if (dartContourArcLength > 0)
            {
                workingCam.dartContours.Push(dartContour);
            }

            return dartContourFound;
        }
    }
}