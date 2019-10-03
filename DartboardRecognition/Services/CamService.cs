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
        public Image<Bgr, byte> OriginFrame { get; private set; }
        public Image<Bgr, byte> LinedFrame { get; private set; }
        public Image<Bgr, byte> RoiFrame { get; private set; }
        public Image<Gray, byte> RoiTrasholdFrame { get; private set; }
        public Image<Bgr, byte> RoiContourFrame { get; private set; }
        public Image<Gray, byte> RoiTrasholdFrameLastThrow { get; private set; }
        private int SmoothGauss { get; } = 5;
        private int MovesNoise { get; } = 900;
        private int MovesDart { get; } = 1000;
        private int MovesExtraction { get; } = 7000;

        public CamService(CamWindow camWindow, DrawService drawService)
        {
            this.camWindow = camWindow;
            this.drawService = drawService;
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
            GetSetupLines();
        }

        private int GetCamIndex(int camNumber)
        {
            var allCams = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice).ToList();
            var camId = ConfigurationManager.AppSettings[$"Cam{camNumber}Id"];
            var index = allCams.FindIndex(x => x.DevicePath.Contains(camId));
            return index;
        }

        private void GetSetupLines()
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
            GetSetupLines();
            DrawSetupLines();

            RoiFrame = OriginFrame.Clone();
            RoiFrame.ROI = roiRectangle;

            RoiTrasholdFrame = RoiFrame.Clone().Convert<Gray, byte>().Not();
            RoiTrasholdFrame._SmoothGaussian(SmoothGauss);
            RoiTrasholdFrame._ThresholdBinary(new Gray(tresholdMinSlider),
                                              new Gray(tresholdMaxSlider));
            OriginFrame.Dispose();
        }

        public void RefreshImageBoxes()
        {
            camWindow.Dispatcher.Invoke(new Action(() => camWindow.ImageBox.Source = LinedFrame?.Data != null
                                                                                         ? drawService.ToBitmap(LinedFrame)
                                                                                         : new BitmapImage()));
            camWindow.Dispatcher.Invoke(new Action(() => camWindow.ImageBoxRoi.Source = RoiTrasholdFrame?.Data != null
                                                                                            ? drawService.ToBitmap(RoiTrasholdFrame)
                                                                                            : new BitmapImage()));
            camWindow.Dispatcher.Invoke(new Action(() => camWindow.ImageBoxRoiLastThrow.Source = RoiTrasholdFrameLastThrow?.Data != null
                                                                                                     ? drawService.ToBitmap(RoiTrasholdFrameLastThrow)
                                                                                                     : new BitmapImage()));
            DisposeAllImages();
        }

        public bool DetectThrow()
        {
            bool dartsExtractProcess;
            bool moveDetected;
            var throwDetected = false;

            var zeroImage = RoiTrasholdFrame.Clone();
            var firstImage = videoCapture.QueryFrame().ToImage<Gray, byte>().Not();

            var diffImage = DiffImage(firstImage, zeroImage);
            var moves = diffImage.CountNonzero()[0];
            moveDetected = moves > MovesNoise;

            if (moveDetected)
            {
                // Thread.Sleep(1500);
                var secondImage = videoCapture.QueryFrame().ToImage<Gray, byte>().Not();
                diffImage = DiffImage(secondImage, zeroImage);
                moves = diffImage.CountNonzero()[0];

                dartsExtractProcess = moves > MovesExtraction;
                throwDetected = !dartsExtractProcess && moves > MovesDart;

                if (dartsExtractProcess)
                {
                    Thread.Sleep(4000);
                }
                else if (throwDetected)
                {
                    RoiTrasholdFrameLastThrow = diffImage.Clone();

                    DoCaptures();
                    RefreshImageBoxes();
                }

                secondImage.Dispose();
            }

            zeroImage.Dispose();
            firstImage.Dispose();
            diffImage.Dispose();

            return throwDetected;
        }

        private Image<Gray, byte> DiffImage(Image<Gray, byte> image, Image<Gray, byte> originImage)
        {
            image.ROI = roiRectangle;
            image._SmoothGaussian(3);
            image._ThresholdBinary(new Gray(tresholdMinSlider),
                                   new Gray(tresholdMaxSlider));
            var diffImage = image.AbsDiff(originImage);
            return diffImage;
        }

        private void DisposeAllImages()
        {
            OriginFrame?.Dispose();
            LinedFrame?.Dispose();
            RoiFrame?.Dispose();
            RoiTrasholdFrame?.Dispose();
            RoiContourFrame?.Dispose();
            RoiTrasholdFrameLastThrow?.Dispose();
        }
    }
}