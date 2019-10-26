#region Usings

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Media.Imaging;
using Autofac;
using DartboardRecognition.Windows;
using DirectShowLib;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

#endregion

namespace DartboardRecognition.Services
{
    public class CamService
    {
        private readonly CamWindow camWindow;
        private readonly DrawService drawService;
        public readonly VideoCapture videoCapture;
        private readonly ConfigService configService;
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
            // RoiFrame?.Dispose(); todo check and delete

            GetSlidersData();
            DrawSetupLines();

            RoiFrame = OriginFrame.Clone().Convert<Gray, byte>().Not();
            OriginFrame.Dispose();

            RoiFrame.ROI = roiRectangle;
            RoiFrame._SmoothGaussian(smoothGauss);
            RoiFrame._ThresholdBinary(new Gray(tresholdSlider),
                                      new Gray(255));
        }

        public void RefreshImageBoxes()
        {
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
        }

        public ResponseType Detect()
        {
            if (withDetection)
            {
                var zeroImage = RoiFrame;
                var diffImage = CaptureAndDiff(zeroImage);
                var moves = diffImage.CountNonzero()[0];
                diffImage.Dispose();

                var moveDetected = moves > movesNoise;

                if (moveDetected)
                {
                    var now = DateTime.Now;
                    while (DateTime.Now - now < TimeSpan.FromSeconds(moveDetectedSleepTime)) // todo something with thread
                    {
                    }

                    diffImage = CaptureAndDiff(zeroImage);
                    zeroImage.Dispose();
                    moves = diffImage.CountNonzero()[0];

                    var extractProcess = moves > movesExtraction;
                    var throwDetected = !extractProcess && moves > movesDart;

                    if (extractProcess)
                    {
                        return ResponseType.Extraction;
                    }

                    if (throwDetected)
                    {
                        RoiLastThrowFrame = diffImage.Clone();
                        diffImage.Dispose();

                        DoCapture();
                        RefreshImageBoxes();

                        return ResponseType.Trow;
                    }
                }
            }

            if (runtimeCapturing)
            {
                DoCapture();
                RefreshImageBoxes();
            }

            return ResponseType.Nothing;
        }

        public void FindThrow()
        {
            var zeroImage = RoiFrame;
            var diffImage = CaptureAndDiff(zeroImage);
            RoiLastThrowFrame = diffImage.Clone();
            diffImage.Dispose();
            DoCapture();
            RefreshImageBoxes();
        }

        private Image<Gray, byte> CaptureAndDiff(Image<Gray, byte> oldImage)
        {
            var newImage = videoCapture.QueryFrame().ToImage<Gray, byte>().Not();
            newImage.ROI = roiRectangle;
            newImage._SmoothGaussian(smoothGauss);
            newImage._ThresholdBinary(new Gray(tresholdSlider),
                                      new Gray(255));

            var diffImage = oldImage.Data != null
                                ? oldImage.AbsDiff(newImage)
                                : new Image<Gray, byte>(newImage.Width, newImage.Height, new Gray(255)); // todo check this
            newImage.Dispose();

            return diffImage;
        }
    }
}