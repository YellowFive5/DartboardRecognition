#region Usings

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Emgu.CV.CvEnum;
using Point = System.Drawing.Point;

#endregion

namespace DartboardRecognition
{
    public class Cam
    {
        protected Image<Bgr, byte> testCaptureThrow1;
        protected Image<Bgr, byte> testCaptureThrow2;
        protected Image<Bgr, byte> testCaptureThrow3;
        public Image<Bgr, byte> processingCapture;
        public VideoCapture videoCapture;
        public Image<Bgr, byte> originFrame;
        public Image<Bgr, byte> linedFrame;
        public Image<Bgr, byte> roiFrame;
        public Image<Gray, byte> roiTrasholdFrame;
        public Image<Bgr, byte> roiContourFrame;
        public Point surfacePoint1;
        public Point surfacePoint2;
        public Point surfaceCenterPoint1;
        public Point surfaceCenterPoint2;
        public Point surfaceLeftPoint1;
        public Point surfaceLeftPoint2;
        public Point surfaceRightPoint1;
        public Point surfaceRightPoint2;
        public int spikeLineLength;
        public Stack<VectorOfPoint> workingContours;
        public readonly VectorOfVectorOfPoint allContours;
        public readonly Mat matHierarсhy;
        protected TextBox camIndexBox;
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

        protected Cam()
        {
            surfacePoint1 = new Point();
            surfacePoint2 = new Point();
            allContours = new VectorOfVectorOfPoint();
            workingContours = new Stack<VectorOfPoint>();
            matHierarсhy = new Mat();
        }

        public void SetProcessingCapture(int throwIndex)
        {
            switch (throwIndex)
            {
                case 1:
                    processingCapture = testCaptureThrow1;
                    break;
                case 2:
                    processingCapture = testCaptureThrow2;
                    break;
                case 3:
                    processingCapture = testCaptureThrow3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Only 3 throws avaliable");
            }
        }
    }

    public class Cam1 : Cam
    {
        public Cam1(MainWindow view)
        {
            testCaptureThrow1 = new Image<Bgr, byte>(@"..\..\..\..\TestPhoto\Ethalon images\cam1_1.png");
            testCaptureThrow2 = new Image<Bgr, byte>(@"..\..\..\..\TestPhoto\Ethalon images\cam1_2.png");
            testCaptureThrow3 = new Image<Bgr, byte>(@"..\..\..\..\TestPhoto\Ethalon images\cam1_3.png");
            camIndexBox = view.Cam1IndexBox;
            tresholdMinSlider = view.Cam1TresholdMinSlider.Value;
            tresholdMaxSlider = view.Cam1TresholdMaxSlider.Value;
            roiPosXSlider = view.Cam1RoiPosXSlider.Value;
            roiPosYSlider = view.Cam1RoiPosYSlider.Value;
            roiWidthSlider = view.Cam1RoiWidthSlider.Value;
            roiHeightSlider = view.Cam1RoiHeightSlider.Value;
            surfaceSlider = view.Cam1SurfaceSlider.Value;
            surfaceCenterSlider = view.Cam1SurfaceCenterSlider.Value;
            surfaceLeftSlider = view.Cam1SurfaceLeftSlider.Value;
            surfaceRightSlider = view.Cam1SurfaceRightSlider.Value;
            videoCapture = new VideoCapture(int.Parse(camIndexBox.Text));
            videoCapture.SetCaptureProperty(CapProp.FrameWidth, 1920);
            videoCapture.SetCaptureProperty(CapProp.FrameHeight, 1080);
        }
    }

    public class Cam2 : Cam
    {
        public Cam2(MainWindow view)
        {
            testCaptureThrow1 = new Image<Bgr, byte>(@"..\..\..\..\TestPhoto\Ethalon images\cam2_1.png");
            testCaptureThrow2 = new Image<Bgr, byte>(@"..\..\..\..\TestPhoto\Ethalon images\cam2_2.png");
            testCaptureThrow3 = new Image<Bgr, byte>(@"..\..\..\..\TestPhoto\Ethalon images\cam2_3.png");
            camIndexBox = view.Cam2IndexBox;
            tresholdMinSlider = view.Cam2TresholdMinSlider.Value;
            tresholdMaxSlider = view.Cam2TresholdMaxSlider.Value;
            roiPosXSlider = view.Cam2RoiPosXSlider.Value;
            roiPosYSlider = view.Cam2RoiPosYSlider.Value;
            roiWidthSlider = view.Cam2RoiWidthSlider.Value;
            roiHeightSlider = view.Cam2RoiHeightSlider.Value;
            surfaceSlider = view.Cam2SurfaceSlider.Value;
            surfaceCenterSlider = view.Cam2SurfaceCenterSlider.Value;
            surfaceLeftSlider = view.Cam2SurfaceLeftSlider.Value;
            surfaceRightSlider = view.Cam2SurfaceRightSlider.Value;
            videoCapture = new VideoCapture(int.Parse(camIndexBox.Text));
            videoCapture.SetCaptureProperty(CapProp.FrameWidth, 1920);
            videoCapture.SetCaptureProperty(CapProp.FrameHeight, 1080);
        }
    }
}