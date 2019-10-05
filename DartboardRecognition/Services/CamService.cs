#region Usings

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Media.Imaging;
using DartboardRecognition.Windows;
using DirectShowLib;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

#endregion

namespace DartboardRecognition.Services
{
    public class CamService
    {
        private readonly CamWindow camWindow;
        private readonly DrawService drawService;
        public readonly VideoCapture videoCapture;
        public PointF surfacePoint1;
        public PointF surfacePoint2;
        public PointF surfaceCenterPoint1;
        public PointF surfaceCenterPoint2;
        public PointF surfaceLeftPoint1;
        public PointF surfaceLeftPoint2;
        public PointF surfaceRightPoint1;
        public PointF surfaceRightPoint2;
        public float spikeLineLength;
        public readonly Stack<VectorOfPoint> dartContours;
        public readonly VectorOfVectorOfPoint allContours;
        public readonly Mat matHierarсhy;
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
        public Image<Gray, byte> OldRoiFrame { get; private set; }
        public Image<Bgr, byte> OriginFrame { get; private set; }
        public Image<Bgr, byte> LinedFrame { get; private set; }
        public Image<Gray, byte> RoiFrame { get; private set; }
        public Image<Bgr, byte> RoiContourFrame { get; private set; }
        public Image<Gray, byte> RoiLastThrowFrame { get; private set; }
        private int SmoothGauss { get; } = 5;
        private int MovesNoise { get; } = 900;
        private int MovesDart { get; } = 1000;
        private int MovesExtraction { get; } = 7000;

        public CamService(CamWindow camWindow)
        {
            this.camWindow = camWindow;
            drawService = ServiceBag.All().DrawService;
            surfacePoint1 = new PointF();
            surfacePoint2 = new PointF();
            allContours = new VectorOfVectorOfPoint();
            dartContours = new Stack<VectorOfPoint>();
            matHierarсhy = new Mat();
            switch (camWindow.camNumber)
            {
                case 1:
                    toBullAngle = 0.785398;
                    camNumber = 1;
                    setupPoint = new PointF(10, 10); //todo
                    break;
                case 2:
                    toBullAngle = 2.35619;
                    camNumber = 2;
                    setupPoint = new PointF(1200 - 10, 10); //todo
                    break;
                case 3:
                    toBullAngle = 2.35619; //todo
                    camNumber = 3;
                    setupPoint = new PointF(1200 - 10, 10); //todo
                    break;
                case 4:
                    toBullAngle = 2.35619; //todo
                    camNumber = 4;
                    setupPoint = new PointF(1200 - 10, 10); //todo
                    break;
                default:
                    throw new Exception("Out of cameras range");
            }

            videoCapture = new VideoCapture(GetCamIndex(camNumber));
            videoCapture.SetCaptureProperty(CapProp.FrameWidth, 1920);
            videoCapture.SetCaptureProperty(CapProp.FrameHeight, 1080);
            GetSlidersData();
        }

        private int GetCamIndex(int camNumber)
        {
            var allCams = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice).ToList();
            var camId = ConfigurationManager.AppSettings[$"Cam{camNumber}Id"];
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
            OriginFrame = videoCapture.QueryFrame().ToImage<Bgr, byte>();

            LinedFrame = OriginFrame.Clone();

            roiRectangle = new Rectangle((int) roiPosXSlider,
                                         (int) roiPosYSlider,
                                         (int) roiWidthSlider,
                                         (int) roiHeightSlider);

            drawService.DrawRectangle(LinedFrame,
                                      roiRectangle,
                                      camWindow.CamRoiRectColor.MCvScalar,
                                      camWindow.CamRoiRectThickness);

            surfacePoint1 = new PointF(0, (float) surfaceSlider);
            surfacePoint2 = new PointF(OriginFrame.Cols,
                                       (float) surfaceSlider);
            drawService.DrawLine(LinedFrame,
                                 surfacePoint1,
                                 surfacePoint2,
                                 camWindow.CamSurfaceLineColor.MCvScalar,
                                 camWindow.CamSurfaceLineThickness);

            surfaceCenterPoint1 = new PointF((float) surfaceCenterSlider,
                                             (float) surfaceSlider);

            surfaceCenterPoint2 = new PointF(surfaceCenterPoint1.X,
                                             surfaceCenterPoint1.Y - 50);
            drawService.DrawLine(LinedFrame,
                                 surfaceCenterPoint1,
                                 surfaceCenterPoint2,
                                 camWindow.CamSurfaceLineColor.MCvScalar,
                                 camWindow.CamSurfaceLineThickness);

            surfaceLeftPoint1 = new PointF((float) surfaceLeftSlider,
                                           (float) surfaceSlider);
            surfaceLeftPoint2 = new PointF(surfaceLeftPoint1.X,
                                           surfaceLeftPoint1.Y - 50);
            drawService.DrawLine(LinedFrame,
                                 surfaceLeftPoint1,
                                 surfaceLeftPoint2,
                                 camWindow.CamSurfaceLineColor.MCvScalar,
                                 camWindow.CamSurfaceLineThickness);

            surfaceRightPoint1 = new PointF((float) surfaceRightSlider,
                                            (float) surfaceSlider);
            surfaceRightPoint2 = new PointF(surfaceRightPoint1.X,
                                            surfaceRightPoint1.Y - 50);
            drawService.DrawLine(LinedFrame,
                                 surfaceRightPoint1,
                                 surfaceRightPoint2,
                                 camWindow.CamSurfaceLineColor.MCvScalar,
                                 camWindow.CamSurfaceLineThickness);
        }

        public void DoCaptures()
        {
            OldRoiFrame?.Dispose();

            GetSlidersData();
            DrawSetupLines();

            RoiFrame = OriginFrame.Clone().Convert<Gray, byte>().Not();
            OriginFrame.Dispose();

            RoiFrame.ROI = roiRectangle;
            RoiFrame._SmoothGaussian(SmoothGauss);
            RoiFrame._ThresholdBinary(new Gray(tresholdMinSlider),
                                      new Gray(tresholdMaxSlider));

            OldRoiFrame = RoiFrame.Clone();
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
            DisposeAllImages();
        }

        public ResponseType Detect()
        {
            var zeroImage = RoiFrame;
            var diffImage = CaptureAndDiff(zeroImage);
            var moves = diffImage.CountNonzero()[0];
            diffImage.Dispose();

            var moveDetected = moves > MovesNoise;

            if (moveDetected)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.8));

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

                    DoCaptures();
                    RefreshImageBoxes();

                    return ResponseType.Trow;
                }
            }

            return ResponseType.Nothing;
        }

        public void FindThrow()
        {
            var zeroImage = OldRoiFrame;
            var diffImage = CaptureAndDiff(zeroImage);
            RoiLastThrowFrame = diffImage.Clone();
            diffImage.Dispose();
            DoCaptures();
            RefreshImageBoxes();
        }

        private Image<Gray, byte> CaptureAndDiff(Image<Gray, byte> oldImage)
        {
            var newImage = videoCapture.QueryFrame().ToImage<Gray, byte>().Not();
            newImage.ROI = roiRectangle;
            newImage._SmoothGaussian(SmoothGauss);
            newImage._ThresholdBinary(new Gray(tresholdMinSlider),
                                      new Gray(tresholdMaxSlider));

            var diffImage = oldImage.AbsDiff(newImage);
            newImage.Dispose();

            return diffImage;
        }

        private void DisposeAllImages()
        {
            OriginFrame?.Dispose();
            LinedFrame?.Dispose();
            RoiFrame?.Dispose();
            RoiContourFrame?.Dispose();
        }
    }
}