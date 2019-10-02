#region Usings

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Windows.Threading;
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
        private readonly Dispatcher viewDispatcher;
        public Image<Bgr, byte> processingCapture;
        public readonly VideoCapture videoCapture;
        public Image<Bgr, byte> originFrame;
        public Image<Bgr, byte> linedFrame;
        public Image<Bgr, byte> roiFrame;
        public Image<Gray, byte> roiTrasholdFrame;
        public Image<Bgr, byte> roiContourFrame;
        public Image<Gray, byte> roiTrasholdFrameLastThrow;
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

        public CamService(CamWindow view)
        {
            viewDispatcher = view.Dispatcher;
            surfacePoint1 = new PointF();
            surfacePoint2 = new PointF();
            allContours = new VectorOfVectorOfPoint();
            dartContours = new Stack<VectorOfPoint>();
            matHierarсhy = new Mat();
            switch (view.camNumber)
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
            RefreshLines(view);
        }

        private int GetCamIndex(int camNumber)
        {
            var allCams = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice).ToList();
            var camId = ConfigurationManager.AppSettings[$"Cam{camNumber}Id"];
            var index = allCams.FindIndex(x => x.DevicePath.Contains(camId));
            return index;
        }

        public void RefreshLines(CamWindow view)
        {
            viewDispatcher.Invoke(new Action(() => tresholdMinSlider = view.TresholdMinSlider.Value));
            viewDispatcher.Invoke(new Action(() => tresholdMaxSlider = view.TresholdMaxSlider.Value));
            viewDispatcher.Invoke(new Action(() => roiPosXSlider = view.RoiPosXSlider.Value));
            viewDispatcher.Invoke(new Action(() => roiPosYSlider = view.RoiPosYSlider.Value));
            viewDispatcher.Invoke(new Action(() => roiWidthSlider = view.RoiWidthSlider.Value));
            viewDispatcher.Invoke(new Action(() => roiHeightSlider = view.RoiHeightSlider.Value));
            viewDispatcher.Invoke(new Action(() => surfaceSlider = view.SurfaceSlider.Value));
            viewDispatcher.Invoke(new Action(() => surfaceCenterSlider = view.SurfaceCenterSlider.Value));
            viewDispatcher.Invoke(new Action(() => surfaceLeftSlider = view.SurfaceLeftSlider.Value));
            viewDispatcher.Invoke(new Action(() => surfaceRightSlider = view.SurfaceRightSlider.Value));
        }
    }
}