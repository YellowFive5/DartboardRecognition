#region Usings

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Windows.Controls;
using Point = System.Drawing.Point;

#endregion

namespace DartboardRecognition
{
    public class Cam
    {
        private readonly Image<Bgr, byte> testCaptureThrow1;
        private readonly Image<Bgr, byte> testCaptureThrow2;
        private readonly Image<Bgr, byte> testCaptureThrow3;
        public Image<Bgr, byte> processingCapture;
        public EventHandler camHandler;
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
        public int spikeLineLength;
        public int minContourArcLength = 250;
        public readonly VectorOfVectorOfPoint contours;
        public readonly Mat matHierarсhy;
        public readonly Image imageBox;
        public readonly Image imageBoxRoi;
        private TextBox camIndexBox;
        public readonly Slider tresholdMinSlider;
        public readonly Slider tresholdMaxSlider;
        public readonly Slider roiPosXSlider;
        public readonly Slider roiPosYSlider;
        public readonly Slider roiWidthSlider;
        public readonly Slider roiHeightSlider;
        public readonly Slider surfaceSlider;
        public readonly Slider surfaceCenterSlider;

        public Cam(MainWindow view, int index)
        {
            surfacePoint1 = new Point();
            surfacePoint2 = new Point();
            contours = new VectorOfVectorOfPoint();
            matHierarсhy = new Mat();

            if (index == 1)
            {
                testCaptureThrow1 = new Image<Bgr, byte>(@"C:\Users\YellowFive\Dropbox\MY\[Darts recognition]\MyC#\TestPhoto\Ethalon images\cam1_1.png");
                testCaptureThrow2 = new Image<Bgr, byte>(@"C:\Users\YellowFive\Dropbox\MY\[Darts recognition]\MyC#\TestPhoto\Ethalon images\cam1_2.png");
                testCaptureThrow3 = new Image<Bgr, byte>(@"C:\Users\YellowFive\Dropbox\MY\[Darts recognition]\MyC#\TestPhoto\Ethalon images\cam1_3.png");
                camIndexBox = view.Cam1IndexBox;
                tresholdMinSlider = view.Cam1TresholdMinSlider;
                tresholdMaxSlider = view.Cam1TresholdMaxSlider;
                roiPosXSlider = view.Cam1RoiPosXSlider;
                roiPosYSlider = view.Cam1RoiPosYSlider;
                roiWidthSlider = view.Cam1RoiWidthSlider;
                roiHeightSlider = view.Cam1RoiHeightSlider;
                surfaceSlider = view.Cam1SurfaceSlider;
                imageBox = view.ImageBox1;
                imageBoxRoi = view.ImageBox1Roi;
                surfaceCenterSlider = view.Cam1SurfaceCenterSlider;
            }
            else
            {
                testCaptureThrow1 = new Image<Bgr, byte>(@"C:\Users\YellowFive\Dropbox\MY\[Darts recognition]\MyC#\TestPhoto\Ethalon images\cam2_1.png");
                testCaptureThrow2 = new Image<Bgr, byte>(@"C:\Users\YellowFive\Dropbox\MY\[Darts recognition]\MyC#\TestPhoto\Ethalon images\cam2_2.png");
                testCaptureThrow3 = new Image<Bgr, byte>(@"C:\Users\YellowFive\Dropbox\MY\[Darts recognition]\MyC#\TestPhoto\Ethalon images\cam2_3.png");
                camIndexBox = view.Cam2IndexBox;
                tresholdMinSlider = view.Cam2TresholdMinSlider;
                tresholdMaxSlider = view.Cam2TresholdMaxSlider;
                roiPosXSlider = view.Cam2RoiPosXSlider;
                roiPosYSlider = view.Cam2RoiPosYSlider;
                roiWidthSlider = view.Cam2RoiWidthSlider;
                roiHeightSlider = view.Cam2RoiHeightSlider;
                surfaceSlider = view.Cam2SurfaceSlider;
                imageBox = view.ImageBox2;
                imageBoxRoi = view.ImageBox2Roi;
                surfaceCenterSlider = view.Cam2SurfaceCenterSlider;
            }

            videoCapture = new VideoCapture(int.Parse(camIndexBox.Text));
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
}