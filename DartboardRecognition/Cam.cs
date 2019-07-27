#region Usings

using System;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Collections.Generic;
using System.Windows.Threading;
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
        public Stack<VectorOfPoint> dartContours;
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
        protected Dispatcher dispatcher;

        protected Cam()
        {
            surfacePoint1 = new Point();
            surfacePoint2 = new Point();
            allContours = new VectorOfVectorOfPoint();
            dartContours = new Stack<VectorOfPoint>();
            matHierarсhy = new Mat();
        }

        public virtual void RefreshLines(CamWindow view)
        {
        }
    }

    public class Cam1 : Cam
    {
        public Cam1(CamWindow view)
        {
            dispatcher = view.Dispatcher;
            RefreshLines(view);
            dispatcher.Invoke(new Action(() => videoCapture = new VideoCapture(int.Parse(view.IndexBox.Text))));
            videoCapture.SetCaptureProperty(CapProp.FrameWidth, 1920);
            videoCapture.SetCaptureProperty(CapProp.FrameHeight, 1080);
            toBullAngle = 0.785398;
        }

        public override void RefreshLines(CamWindow view)
        {
            dispatcher.Invoke(new Action(() => tresholdMinSlider = view.TresholdMinSlider.Value));
            dispatcher.Invoke(new Action(() => tresholdMaxSlider = view.TresholdMaxSlider.Value));
            dispatcher.Invoke(new Action(() => roiPosXSlider = view.RoiPosXSlider.Value));
            dispatcher.Invoke(new Action(() => roiPosYSlider = view.RoiPosYSlider.Value));
            dispatcher.Invoke(new Action(() => roiWidthSlider = view.RoiWidthSlider.Value));
            dispatcher.Invoke(new Action(() => roiHeightSlider = view.RoiHeightSlider.Value));
            dispatcher.Invoke(new Action(() => surfaceSlider = view.SurfaceSlider.Value));
            dispatcher.Invoke(new Action(() => surfaceCenterSlider = view.SurfaceCenterSlider.Value));
            dispatcher.Invoke(new Action(() => surfaceLeftSlider = view.SurfaceLeftSlider.Value));
            dispatcher.Invoke(new Action(() => surfaceRightSlider = view.SurfaceRightSlider.Value));
            dispatcher.Invoke(new Action(() => setupPoint = view.SetupPoint));
        }
    }

    public class Cam2 : Cam
    {
        public Cam2(CamWindow view)
        {
            dispatcher = view.Dispatcher;
            RefreshLines(view);
            dispatcher.Invoke(new Action(() => videoCapture = new VideoCapture(int.Parse(view.IndexBox.Text))));
            videoCapture.SetCaptureProperty(CapProp.FrameWidth, 1920);
            videoCapture.SetCaptureProperty(CapProp.FrameHeight, 1080);
            toBullAngle = 2.35619;
        }

        public override void RefreshLines(CamWindow view)
        {
            dispatcher.Invoke(new Action(() => tresholdMinSlider = view.TresholdMinSlider.Value));
            dispatcher.Invoke(new Action(() => tresholdMaxSlider = view.TresholdMaxSlider.Value));
            dispatcher.Invoke(new Action(() => roiPosXSlider = view.RoiPosXSlider.Value));
            dispatcher.Invoke(new Action(() => roiPosYSlider = view.RoiPosYSlider.Value));
            dispatcher.Invoke(new Action(() => roiWidthSlider = view.RoiWidthSlider.Value));
            dispatcher.Invoke(new Action(() => roiHeightSlider = view.RoiHeightSlider.Value));
            dispatcher.Invoke(new Action(() => surfaceSlider = view.SurfaceSlider.Value));
            dispatcher.Invoke(new Action(() => surfaceCenterSlider = view.SurfaceCenterSlider.Value));
            dispatcher.Invoke(new Action(() => surfaceLeftSlider = view.SurfaceLeftSlider.Value));
            dispatcher.Invoke(new Action(() => surfaceRightSlider = view.SurfaceRightSlider.Value));
            dispatcher.Invoke(new Action(() => setupPoint = view.SetupPoint));
        }
    }
}