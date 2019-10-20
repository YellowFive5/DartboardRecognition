#region Usings

using System;
using System.Configuration;
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
        public double tresholdMinSlider;
        public double tresholdMaxSlider;
        public double roiPosXSlider;
        public double roiPosYSlider;
        public double roiWidthSlider;
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

        private const int ResolutionWidth = 1920;
        private const int ResolutionHeight = 1080;
        private const int MovesExtraction = 7000;
        private const int MovesDart = 1000;
        private const int MovesNoise = 900;
        private const int SmoothGauss = 5;

        public CamService(CamWindow camWindow, bool runtimeCapturing, bool withDetection)
        {
            this.camWindow = camWindow;
            this.runtimeCapturing = runtimeCapturing;
            this.withDetection = withDetection;
            drawService = MainWindow.ServiceContainer.Resolve<DrawService>();
            configService = MainWindow.ServiceContainer.Resolve<ConfigService>();
            surfacePoint1 = new PointF();
            surfacePoint2 = new PointF();
            var coeff = (2200 - drawService.ProjectionFrameSide) / 2;

            switch (camWindow.camNumber)
            {
                case 1:
                    camNumber = 1;
                    setupPoint = new PointF(305 - coeff,
                                            512 - coeff);
                    break;
                case 2:
                    camNumber = 2;
                    setupPoint = new PointF(798 - coeff,
                                            153 - coeff);
                    break;
                case 3:
                    camNumber = 3;
                    setupPoint = new PointF(1414 - coeff,
                                            133 - coeff);
                    break;
                case 4:
                    camNumber = 4;
                    setupPoint = new PointF(1939 - coeff,
                                            475 - coeff);
                    break;
                default:
                    throw new Exception("Out of cameras range");
            }

            toBullAngle = MeasureService.FindAngle(setupPoint, drawService.ProjectionCenterPoint);
            videoCapture = new VideoCapture(GetCamIndex(camNumber));
            videoCapture.SetCaptureProperty(CapProp.FrameWidth, ResolutionWidth);
            videoCapture.SetCaptureProperty(CapProp.FrameHeight, ResolutionHeight);
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
            camWindow.Dispatcher.Invoke(new Action(() => tresholdMinSlider = camWindow.TresholdMinSlider.Value));
            camWindow.Dispatcher.Invoke(new Action(() => tresholdMaxSlider = camWindow.TresholdMaxSlider.Value));
            camWindow.Dispatcher.Invoke(new Action(() => roiPosXSlider = camWindow.RoiPosXSlider.Value));
            camWindow.Dispatcher.Invoke(new Action(() => roiPosYSlider = camWindow.RoiPosYSlider.Value));
            camWindow.Dispatcher.Invoke(new Action(() => roiWidthSlider = camWindow.RoiWidthSlider.Value));
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

            roiRectangle = new Rectangle((int) roiPosXSlider,
                                         (int) roiPosYSlider,
                                         (int) roiWidthSlider,
                                         (int) roiHeightSlider);

            drawService.DrawRectangle(LinedFrame,
                                      roiRectangle,
                                      drawService.CamRoiRectColor.MCvScalar,
                                      drawService.CamRoiRectThickness);

            surfacePoint1 = new PointF(0, (float) surfaceSlider);
            surfacePoint2 = new PointF(ResolutionWidth,
                                       (float) surfaceSlider);
            drawService.DrawLine(LinedFrame,
                                 surfacePoint1,
                                 surfacePoint2,
                                 drawService.CamSurfaceLineColor.MCvScalar,
                                 drawService.CamSurfaceLineThickness);

            surfaceCenterPoint1 = new PointF((float) surfaceCenterSlider,
                                             (float) surfaceSlider);

            surfaceCenterPoint2 = new PointF(surfaceCenterPoint1.X,
                                             surfaceCenterPoint1.Y - 50);
            drawService.DrawLine(LinedFrame,
                                 surfaceCenterPoint1,
                                 surfaceCenterPoint2,
                                 drawService.CamSurfaceLineColor.MCvScalar,
                                 drawService.CamSurfaceLineThickness);

            surfaceLeftPoint1 = new PointF((float) surfaceLeftSlider,
                                           (float) surfaceSlider);
            surfaceLeftPoint2 = new PointF(surfaceLeftPoint1.X,
                                           surfaceLeftPoint1.Y - 50);
            drawService.DrawLine(LinedFrame,
                                 surfaceLeftPoint1,
                                 surfaceLeftPoint2,
                                 drawService.CamSurfaceLineColor.MCvScalar,
                                 drawService.CamSurfaceLineThickness);

            surfaceRightPoint1 = new PointF((float) surfaceRightSlider,
                                            (float) surfaceSlider);
            surfaceRightPoint2 = new PointF(surfaceRightPoint1.X,
                                            surfaceRightPoint1.Y - 50);
            drawService.DrawLine(LinedFrame,
                                 surfaceRightPoint1,
                                 surfaceRightPoint2,
                                 drawService.CamSurfaceLineColor.MCvScalar,
                                 drawService.CamSurfaceLineThickness);
        }

        public void DoCapture()
        {
            // RoiFrame?.Dispose(); todo check and delete

            GetSlidersData();
            DrawSetupLines();

            RoiFrame = OriginFrame.Clone().Convert<Gray, byte>().Not();
            OriginFrame.Dispose();

            RoiFrame.ROI = roiRectangle;
            RoiFrame._SmoothGaussian(SmoothGauss);
            RoiFrame._ThresholdBinary(new Gray(tresholdMinSlider),
                                      new Gray(tresholdMaxSlider));
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

                var moveDetected = moves > MovesNoise;

                if (moveDetected)
                {
                    var now = DateTime.Now;
                    while (DateTime.Now - now < TimeSpan.FromSeconds(1))
                    {
                    }

                    diffImage = CaptureAndDiff(zeroImage);
                    zeroImage.Dispose();
                    moves = diffImage.CountNonzero()[0];

                    var extractProcess = moves > MovesExtraction;
                    var throwDetected = !extractProcess && moves > MovesDart;

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
            newImage._SmoothGaussian(SmoothGauss);
            newImage._ThresholdBinary(new Gray(tresholdMinSlider),
                                      new Gray(tresholdMaxSlider));

            var diffImage = oldImage.Data != null
                                ? oldImage.AbsDiff(newImage)
                                : new Image<Gray, byte>(newImage.Width, newImage.Height, new Gray(255)); // todo check this
            newImage.Dispose();

            return diffImage;
        }
    }
}