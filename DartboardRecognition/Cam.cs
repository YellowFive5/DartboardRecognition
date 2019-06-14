#region Usings

using System;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Collections.Generic;
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
        public Image<Gray, byte> roiTrasholdFrameLastThrow;
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
        public Point setupPoint;
        public double toBullAngle;

        protected Cam()
        {
            surfacePoint1 = new Point();
            surfacePoint2 = new Point();
            allContours = new VectorOfVectorOfPoint();
            workingContours = new Stack<VectorOfPoint>();
            matHierarсhy = new Mat();
        }

        public virtual void RefreshLines(MainWindow view)
        {
        }
    }

    public class Cam1 : Cam
    {
        public Cam1(MainWindow view)
        {
            RefreshLines(view);
            view.Dispatcher.Invoke(new Action(() => videoCapture = new VideoCapture(int.Parse(view.Cam1IndexBox.Text))));
            videoCapture.SetCaptureProperty(CapProp.FrameWidth, 1920);
            videoCapture.SetCaptureProperty(CapProp.FrameHeight, 1080);
            toBullAngle = 0.785398;
        }

        public override void RefreshLines(MainWindow view)
        {
            view.Dispatcher.Invoke(new Action(() => tresholdMinSlider = view.Cam1TresholdMinSlider.Value));
            view.Dispatcher.Invoke(new Action(() => tresholdMaxSlider = view.Cam1TresholdMaxSlider.Value));
            view.Dispatcher.Invoke(new Action(() => roiPosXSlider = view.Cam1RoiPosXSlider.Value));
            view.Dispatcher.Invoke(new Action(() => roiPosYSlider = view.Cam1RoiPosYSlider.Value));
            view.Dispatcher.Invoke(new Action(() => roiWidthSlider = view.Cam1RoiWidthSlider.Value));
            view.Dispatcher.Invoke(new Action(() => roiHeightSlider = view.Cam1RoiHeightSlider.Value));
            view.Dispatcher.Invoke(new Action(() => surfaceSlider = view.Cam1SurfaceSlider.Value));
            view.Dispatcher.Invoke(new Action(() => surfaceCenterSlider = view.Cam1SurfaceCenterSlider.Value));
            view.Dispatcher.Invoke(new Action(() => surfaceLeftSlider = view.Cam1SurfaceLeftSlider.Value));
            view.Dispatcher.Invoke(new Action(() => surfaceRightSlider = view.Cam1SurfaceRightSlider.Value));
            view.Dispatcher.Invoke(new Action(() => setupPoint = view.Cam1SetupPoint));
        }
    }

    public class Cam2 : Cam
    {
        public Cam2(MainWindow view)
        {
            RefreshLines(view);
            view.Dispatcher.Invoke(new Action(() => videoCapture = new VideoCapture(int.Parse(view.Cam2IndexBox.Text))));
            videoCapture.SetCaptureProperty(CapProp.FrameWidth, 1920);
            videoCapture.SetCaptureProperty(CapProp.FrameHeight, 1080);
            toBullAngle = 2.35619;
        }

        public override void RefreshLines(MainWindow view)
        {
            view.Dispatcher.Invoke(new Action(() => tresholdMinSlider = view.Cam2TresholdMinSlider.Value));
            view.Dispatcher.Invoke(new Action(() => tresholdMaxSlider = view.Cam2TresholdMaxSlider.Value));
            view.Dispatcher.Invoke(new Action(() => roiPosXSlider = view.Cam2RoiPosXSlider.Value));
            view.Dispatcher.Invoke(new Action(() => roiPosYSlider = view.Cam2RoiPosYSlider.Value));
            view.Dispatcher.Invoke(new Action(() => roiWidthSlider = view.Cam2RoiWidthSlider.Value));
            view.Dispatcher.Invoke(new Action(() => roiHeightSlider = view.Cam2RoiHeightSlider.Value));
            view.Dispatcher.Invoke(new Action(() => surfaceSlider = view.Cam2SurfaceSlider.Value));
            view.Dispatcher.Invoke(new Action(() => surfaceCenterSlider = view.Cam2SurfaceCenterSlider.Value));
            view.Dispatcher.Invoke(new Action(() => surfaceLeftSlider = view.Cam2SurfaceLeftSlider.Value));
            view.Dispatcher.Invoke(new Action(() => surfaceRightSlider = view.Cam2SurfaceRightSlider.Value));
            view.Dispatcher.Invoke(new Action(() => setupPoint = view.Cam2SetupPoint));
        }
    }
}