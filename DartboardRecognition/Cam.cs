#region Usings

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Collections.Generic;
using System.Windows.Controls;
using Emgu.CV.CvEnum;
using Point = System.Drawing.Point;

#endregion

namespace DartboardRecognition
{
    public class Cam
    {
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
    }

    public class Cam1 : Cam
    {
        public Cam1(MainWindow view)
        {
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
            videoCapture = new VideoCapture(int.Parse(view.Cam1IndexBox.Text));
            videoCapture.SetCaptureProperty(CapProp.FrameWidth, 1920);
            videoCapture.SetCaptureProperty(CapProp.FrameHeight, 1080);
        }
    }

    public class Cam2 : Cam
    {
        public Cam2(MainWindow view)
        {
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
            videoCapture = new VideoCapture(int.Parse(view.Cam2IndexBox.Text));
            videoCapture.SetCaptureProperty(CapProp.FrameWidth, 1920);
            videoCapture.SetCaptureProperty(CapProp.FrameHeight, 1080);
        }
    }
}