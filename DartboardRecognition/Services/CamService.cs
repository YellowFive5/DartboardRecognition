#region Usings

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
        private Image<Bgr, byte> OriginFrame { get; set; }
        private Image<Bgr, byte> LinedFrame { get; set; }
        private Image<Gray, byte> RoiFrame { get; set; }
        public Image<Gray, byte> RoiLastThrowFrame { get; private set; }

        private readonly int resolutionWidth;
        private readonly int resolutionHeight;
        private readonly int movesExtraction;
        private readonly int movesDart;
        private readonly int movesNoise;
        private readonly int smoothGauss;
        private readonly double moveDetectedSleepTime;

        public CamService(CamWindow camWindow, bool runtimeCapturing, bool withDetection)
        {
            this.camWindow = camWindow;
            this.runtimeCapturing = runtimeCapturing;
            this.withDetection = withDetection;
            logger = MainWindow.ServiceContainer.Resolve<Logger>();
            drawService = MainWindow.ServiceContainer.Resolve<DrawService>();
            configService = MainWindow.ServiceContainer.Resolve<ConfigService>();
            surfacePoint1 = new PointF();
            surfacePoint2 = new PointF();
            camNumber = camWindow.camNumber;
            var projectionCoefficient = (2200 - drawService.projectionFrameSide) / 2;
            setupPoint = new PointF(configService.Read<float>($"Cam{camNumber}X") - projectionCoefficient,
                                    configService.Read<float>($"Cam{camNumber}Y") - projectionCoefficient);
            resolutionWidth = configService.Read<int>("ResolutionWidth");
            resolutionHeight = configService.Read<int>("ResolutionHeight");
            movesExtraction = configService.Read<int>("MovesExtraction");
            movesDart = configService.Read<int>("MovesDart");
            movesNoise = configService.Read<int>("MovesNoise");
            smoothGauss = configService.Read<int>("SmoothGauss");
            moveDetectedSleepTime = configService.Read<double>("MoveDetectedSleepTime");
            toBullAngle = MeasureService.FindAngle(setupPoint, drawService.projectionCenterPoint);
            videoCapture = new VideoCapture(GetCamIndex(camNumber), VideoCapture.API.DShow);
            videoCapture.SetCaptureProperty(CapProp.FrameWidth, resolutionWidth);
            videoCapture.SetCaptureProperty(CapProp.FrameHeight, resolutionHeight);
            GetSlidersData();
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
            OriginFrame = videoCapture.QueryFrame()?.ToImage<Bgr, byte>();

            LinedFrame = OriginFrame?.Clone();

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

        public void DoCapture()
        {
            logger.Debug($"Doing capture for cam_{camNumber} start");

            RoiFrame?.Dispose();

            GetSlidersData();
            DrawSetupLines();

            RoiFrame = OriginFrame.Clone().Convert<Gray, byte>().Not();
            OriginFrame.Dispose();

            RoiFrame.ROI = roiRectangle;
            RoiFrame._SmoothGaussian(smoothGauss);
            RoiFrame._ThresholdBinary(new Gray(tresholdSlider),
                                      new Gray(255));

            logger.Debug($"Doing capture for cam_{camNumber} end");
        }

        public void RefreshImageBoxes()
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
            OriginFrame?.Dispose();
            LinedFrame?.Dispose();

            logger.Debug($"Refreshing imageboxes for cam_{camNumber} end");
        }

        public ResponseType Detect()
        {
            if (withDetection)
            {
                var zeroImage = RoiFrame.Clone();
                var diffImage = CaptureAndDiff(zeroImage);
                var moves = diffImage.CountNonzero()[0];
                logger.Debug($"Moves:{moves}");
                diffImage.Dispose();

                var moveDetected = moves > movesNoise;
                logger.Debug($"Move detected:{moveDetected}");

                if (moveDetected)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(moveDetectedSleepTime));

                    diffImage = CaptureAndDiff(zeroImage);
                    zeroImage.Dispose();
                    moves = diffImage.CountNonzero()[0];
                    logger.Debug($"Moves:{moves}");

                    var extractProcess = moves > movesExtraction;
                    logger.Debug($"Extract process:{extractProcess}");

                    var throwDetected = !extractProcess && moves > movesDart;
                    logger.Debug($"Throw detected:{throwDetected}");

                    if (extractProcess)
                    {
                        logger.Debug($"Return response type:{ResponseType.Extraction}");
                        return ResponseType.Extraction;
                    }

                    if (throwDetected)
                    {
                        RoiLastThrowFrame = diffImage.Clone();
                        diffImage.Dispose();

                        DoCapture();
                        RefreshImageBoxes();

                        logger.Debug($"Return response type:{ResponseType.Trow}");
                        return ResponseType.Trow;
                    }
                }
            }

            if (runtimeCapturing)
            {
                DoCapture();
                RefreshImageBoxes();
            }

            logger.Debug($"Return response type:{ResponseType.Nothing}");
            return ResponseType.Nothing;
        }

        public void FindThrow()
        {
            logger.Debug($"Find throw for cam_{camNumber} start");

            var zeroImage = RoiFrame.Clone();
            var diffImage = CaptureAndDiff(zeroImage);
            RoiLastThrowFrame = diffImage.Clone();
            diffImage.Dispose();
            DoCapture();
            RefreshImageBoxes();

            logger.Debug($"Find throw for cam_{camNumber} end");
        }

        private Image<Gray, byte> CaptureAndDiff(Image<Gray, byte> oldImage)
        {
            logger.Debug($"Capture and diff for cam_{camNumber} start");

            var newImage = videoCapture.QueryFrame().ToImage<Gray, byte>().Not();
            newImage.ROI = roiRectangle;
            newImage._SmoothGaussian(smoothGauss);
            newImage._ThresholdBinary(new Gray(tresholdSlider),
                                      new Gray(255));
            var diffImage = oldImage.AbsDiff(newImage);
            newImage.Dispose();

            logger.Debug($"Capture and diff for cam_{camNumber} end");
            return diffImage;
        }
    }
}