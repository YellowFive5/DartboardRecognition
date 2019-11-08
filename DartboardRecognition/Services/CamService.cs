﻿#region Usings

using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Media.Imaging;
using Autofac;
using DartboardRecognition.Windows;
using DirectShowLib;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using NLog;

#endregion

namespace DartboardRecognition.Services
{
    public class CamService
    {
        private readonly CamWindow camWindow;
        private readonly DrawService drawService;
        public readonly VideoCapture videoCapture;
        private readonly ConfigService configService;
        private readonly Logger logger;
        public PointF surfacePoint1;
        public PointF surfacePoint2;
        public PointF surfaceCenterPoint1;
        public PointF surfaceCenterPoint2;
        public PointF surfaceLeftPoint1;
        public PointF surfaceLeftPoint2;
        public PointF surfaceRightPoint1;
        public PointF surfaceRightPoint2;
        public double tresholdSlider;
        public double roiPosYSlider;
        public double roiHeightSlider;
        public double surfaceSlider;
        public double surfaceCenterSlider;
        public double surfaceLeftSlider;
        public double surfaceRightSlider;
        public PointF setupPoint;
        public readonly double toBullAngle;
        public readonly int camNumber;
        private Rectangle roiRectangle;
        private readonly bool runtimeCapturing;
        private readonly bool withDetection;
        private readonly double toCamDistance;
        private Image<Bgr, byte> OriginFrame { get; set; }
        private Image<Bgr, byte> LinedFrame { get; set; }
        private Image<Gray, byte> RoiFrame { get; set; }
        private Image<Gray, byte> RoiFrameBackground { get; set; }
        public Image<Gray, byte> RoiLastThrowFrame { get; private set; }

        private readonly int resolutionWidth;
        private readonly int resolutionHeight;
        private readonly int movesExtraction;
        private readonly int movesDart;
        private readonly int movesNoise;
        private readonly int smoothGauss;
        private readonly double moveDetectedSleepTime;

        public CamService(CamWindow camWindow)
        {
            this.camWindow = camWindow;
            logger = MainWindow.ServiceContainer.Resolve<Logger>();
            drawService = MainWindow.ServiceContainer.Resolve<DrawService>();
            configService = MainWindow.ServiceContainer.Resolve<ConfigService>();
            runtimeCapturing = configService.Read<bool>("RuntimeCapturingCheckBox");
            withDetection = configService.Read<bool>("WithDetectionCheckBox");
            surfacePoint1 = new PointF();
            surfacePoint2 = new PointF();
            camNumber = camWindow.camNumber;
            setupPoint = new PointF(configService.Read<float>($"Cam{camNumber}X"),
                                    configService.Read<float>($"Cam{camNumber}Y"));
            resolutionWidth = configService.Read<int>("ResolutionWidth");
            resolutionHeight = configService.Read<int>("ResolutionHeight");
            movesExtraction = configService.Read<int>("MovesExtraction");
            movesDart = configService.Read<int>("MovesDart");
            movesNoise = configService.Read<int>("MovesNoise");
            smoothGauss = configService.Read<int>("SmoothGauss");
            moveDetectedSleepTime = configService.Read<double>("MoveDetectedSleepTime");
            toCamDistance = configService.Read<double>($"ToCam{camNumber}Distance");
            toBullAngle = MeasureService.FindAngle(setupPoint, drawService.projectionCenterPoint);
            videoCapture = new VideoCapture(GetCamIndex(camNumber), VideoCapture.API.DShow);
            videoCapture.SetCaptureProperty(CapProp.FrameWidth, resolutionWidth);
            videoCapture.SetCaptureProperty(CapProp.FrameHeight, resolutionHeight);
            GetSlidersData();
            RefreshImageBoxes();
        }

        private int GetCamIndex(int camNumber)
        {
            var allCams = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice).ToList();
            var camId = configService.Read<string>($"Cam{camNumber}Id");
            var index = allCams.FindIndex(x => x.DevicePath.Contains(camId));
            return index;
        }

        private void GetSlidersData()
        {
            camWindow.Dispatcher.Invoke(new Action(() => tresholdSlider = camWindow.TresholdSlider.Value));
            camWindow.Dispatcher.Invoke(new Action(() => roiPosYSlider = camWindow.RoiPosYSlider.Value));
            camWindow.Dispatcher.Invoke(new Action(() => roiHeightSlider = camWindow.RoiHeightSlider.Value));
            camWindow.Dispatcher.Invoke(new Action(() => surfaceSlider = camWindow.SurfaceSlider.Value));
            camWindow.Dispatcher.Invoke(new Action(() => surfaceCenterSlider = camWindow.SurfaceCenterSlider.Value));
            camWindow.Dispatcher.Invoke(new Action(() => surfaceLeftSlider = camWindow.SurfaceLeftSlider.Value));
            camWindow.Dispatcher.Invoke(new Action(() => surfaceRightSlider = camWindow.SurfaceRightSlider.Value));
        }

        private void DrawSetupLines()
        {
            OriginFrame = videoCapture.QueryFrame().ToImage<Bgr, byte>();
            LinedFrame = OriginFrame.Clone();

            roiRectangle = new Rectangle(0,
                                         (int) roiPosYSlider,
                                         resolutionWidth,
                                         (int) roiHeightSlider);

            drawService.DrawRectangle(LinedFrame,
                                      roiRectangle,
                                      drawService.camRoiRectColor.MCvScalar,
                                      drawService.camRoiRectThickness);

            surfacePoint1 = new PointF(0, (float) surfaceSlider);
            surfacePoint2 = new PointF(resolutionWidth,
                                       (float) surfaceSlider);
            drawService.DrawLine(LinedFrame,
                                 surfacePoint1,
                                 surfacePoint2,
                                 drawService.camSurfaceLineColor.MCvScalar,
                                 drawService.camSurfaceLineThickness);

            surfaceCenterPoint1 = new PointF((float) surfaceCenterSlider,
                                             (float) surfaceSlider);

            surfaceCenterPoint2 = new PointF(surfaceCenterPoint1.X,
                                             surfaceCenterPoint1.Y - 50);
            drawService.DrawLine(LinedFrame,
                                 surfaceCenterPoint1,
                                 surfaceCenterPoint2,
                                 drawService.camSurfaceLineColor.MCvScalar,
                                 drawService.camSurfaceLineThickness);

            surfaceLeftPoint1 = new PointF((float) surfaceLeftSlider,
                                           (float) surfaceSlider);
            surfaceLeftPoint2 = new PointF(surfaceLeftPoint1.X,
                                           surfaceLeftPoint1.Y - 50);
            drawService.DrawLine(LinedFrame,
                                 surfaceLeftPoint1,
                                 surfaceLeftPoint2,
                                 drawService.camSurfaceLineColor.MCvScalar,
                                 drawService.camSurfaceLineThickness);

            surfaceRightPoint1 = new PointF((float) surfaceRightSlider,
                                            (float) surfaceSlider);
            surfaceRightPoint2 = new PointF(surfaceRightPoint1.X,
                                            surfaceRightPoint1.Y - 50);
            drawService.DrawLine(LinedFrame,
                                 surfaceRightPoint1,
                                 surfaceRightPoint2,
                                 drawService.camSurfaceLineColor.MCvScalar,
                                 drawService.camSurfaceLineThickness);
        }

        private void ThresholdRoi(Image<Gray, byte> roiFrame)
        {
            roiFrame.ROI = roiRectangle;
            roiFrame._SmoothGaussian(smoothGauss);
            CvInvoke.Threshold(roiFrame, roiFrame, tresholdSlider, 255, ThresholdType.Binary);
        }

        public void DoCapture(bool withRoiBackgroundRefresh = false)
        {
            logger.Debug($"Doing capture for cam_{camNumber} start. RoiBackgroundRefresh = {withRoiBackgroundRefresh}");

            GetSlidersData();
            DrawSetupLines();

            RoiFrame = OriginFrame.Clone().Convert<Gray, byte>().Not();
            OriginFrame.Dispose();
            ThresholdRoi(RoiFrame);

            if (withRoiBackgroundRefresh)
            {
                OriginFrame = videoCapture.QueryFrame().ToImage<Bgr, byte>(); // todo. Don't know why, but necessary for Extraction process
                RoiFrame = OriginFrame.Clone().Convert<Gray, byte>().Not();
                OriginFrame.Dispose();
                ThresholdRoi(RoiFrame);

                RoiFrameBackground = RoiFrame.Clone();
                RoiLastThrowFrame = RoiFrame.Clone();
                RefreshImageBoxes();
            }

            logger.Debug($"Doing capture for cam_{camNumber} end");
        }

        private void RefreshImageBoxes()
        {
            logger.Debug($"Refreshing imageboxes for cam_{camNumber} start");

            camWindow.Dispatcher.Invoke(new Action(() => camWindow.ImageBox.Source = LinedFrame?.Data != null
                                                                                         ? drawService.ToBitmap(LinedFrame)
                                                                                         : new BitmapImage()));
            camWindow.Dispatcher.Invoke(new Action(() => camWindow.ImageBoxRoi.Source = RoiFrame?.Data != null
                                                                                            ? drawService.ToBitmap(RoiFrame)
                                                                                            : new BitmapImage()));
            camWindow.Dispatcher.Invoke(new Action(() => camWindow.ImageBoxRoiLastThrow.Source = RoiLastThrowFrame?.Data != null
                                                                                                     ? drawService.ToBitmap(RoiLastThrowFrame)
                                                                                                     : new BitmapImage()));
            logger.Debug($"Refreshing imageboxes for cam_{camNumber} end");
        }

        public ResponseType DetectMove()
        {
            var response = ResponseType.Nothing;

            DoCapture();

            if (withDetection)
            {
                var diffImage = CaptureAndDiff();
                var moves = diffImage.CountNonzero()[0];
                diffImage.Dispose();

                logger.Debug($"Moves is '{moves}'");

                if (moves > movesNoise)
                {
                    response = ResponseType.Move;
                }
            }

            if (runtimeCapturing)
            {
                RefreshImageBoxes();
            }

            logger.Debug($"Response is '{response}'");
            return response;
        }

        public ResponseType DetectThrow()
        {
            var response = ResponseType.Nothing;

            Thread.Sleep(TimeSpan.FromSeconds(moveDetectedSleepTime));

            var diffImage = CaptureAndDiff();
            var moves = diffImage.CountNonzero()[0];
            logger.Debug($"Moves is '{moves}'");

            var extractProcess = moves > movesExtraction;
            var throwDetected = !extractProcess && moves > movesDart;

            if (throwDetected)
            {
                RefreshImages(diffImage);
                response = ResponseType.Trow;
            }

            if (extractProcess)
            {
                response = ResponseType.Extraction;
            }

            logger.Debug($"Response is '{response}'");
            return response;
        }

        public void FindThrow()
        {
            logger.Debug($"Find throw for cam_{camNumber} start");

            var diffImage = CaptureAndDiff();
            RefreshImages(diffImage);

            logger.Debug($"Find throw for cam_{camNumber} end");
        }

        private void RefreshImages(Image<Gray, byte> diffImage)
        {
            logger.Debug($"Refreshing images for cam_{camNumber} start");

            RoiFrameBackground = RoiFrame.Clone();
            RoiLastThrowFrame = diffImage.Clone();
            diffImage.Dispose();

            DoCapture();
            RefreshImageBoxes();

            logger.Debug($"Refreshing images for cam_{camNumber} end");
        }

        private Image<Gray, byte> CaptureAndDiff()
        {
            logger.Debug($"Capture and diff for cam_{camNumber} start");

            var newImage = videoCapture.QueryFrame().ToImage<Gray, byte>().Not();
            ThresholdRoi(newImage);
            var diffImage = RoiFrameBackground.AbsDiff(newImage);
            newImage.Dispose();

            logger.Debug($"Capture and diff for cam_{camNumber} end");
            return diffImage;
        }

        public void CalibrateCamSetupPoint()
        {
            var radSectorStep = 0.314159;
            var toCamPixels = 1020 * toCamDistance / 34;
            var calibratedCamSetupPoint = new PointF();
            var i = 1;
            switch (camNumber)
            {
                case 1:
                    i = 2;
                    break;
                case 2:
                    i = 4;
                    break;
                case 3:
                    i = 6;
                    break;
                case 4:
                    i = 8;
                    break;
            }

            calibratedCamSetupPoint.X = (int) (drawService.projectionCenterPoint.X + Math.Cos(-3.14159 + i * radSectorStep) * toCamPixels);
            calibratedCamSetupPoint.Y = (int) (drawService.projectionCenterPoint.Y + Math.Sin(-3.14159 + i * radSectorStep) * toCamPixels);

            camWindow.XTextBox.Text = calibratedCamSetupPoint.X.ToString();
            camWindow.YTextBox.Text = calibratedCamSetupPoint.Y.ToString();

            configService.Write($"Cam{camNumber}X", calibratedCamSetupPoint.X);
            configService.Write($"Cam{camNumber}Y", calibratedCamSetupPoint.Y);
            setupPoint = calibratedCamSetupPoint;
        }
    }
}