#region Usings

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace DartboardRecognition
{
    public class Cam
    {
        public Image<Bgr, byte> testCaptureThrow1;
        public Image<Bgr, byte> testCaptureThrow2;
        public Image<Bgr, byte> testCaptureThrow3;
        public Image<Bgr, byte> processingCapture;
        public EventHandler camHandler;
        private VideoCapture videoCapture;
        public Image<Bgr, byte> originFrame;
        public Image<Bgr, byte> linedFrame;
        public Image<Bgr, byte> roiFrame;
        public Image<Gray, byte> roiTrasholdFrame;
        public Image<Bgr, byte> roiContourFrame;
        public System.Drawing.Point surfacePoint1;
        public System.Drawing.Point surfacePoint2;
        public int spikeLineLength;
        public int minContourArcLength = 250;
        public VectorOfVectorOfPoint contours;
        public Mat matHierarсhy;
        public Image imageBox;
        public Image imageBoxRoi;        
        private TextBox camIndexBox;
        public Slider tresholdMinSlider;
        public Slider tresholdMaxSlider;
        public Slider roiPosXSlider;
        public Slider roiPosYSlider;
        public Slider roiWidthSlider;
        public Slider roiHeightSlider;
        public Slider surfaceSlider;  
        
        public Cam(Dictionary<string, FrameworkElement> controlsCollection, int index)
        {
            surfacePoint1 = new System.Drawing.Point();
            surfacePoint2 = new System.Drawing.Point();
            contours = new VectorOfVectorOfPoint();
            matHierarсhy = new Mat();

            if (index == 1)
            {
                testCaptureThrow1 = new Image<Bgr, byte>(@"C:\Users\YellowFive\Dropbox\MY\[Darts recognition]\MyC#\TestPhoto\Ethalon images\cam1_1.png");
                testCaptureThrow2 = new Image<Bgr, byte>(@"C:\Users\YellowFive\Dropbox\MY\[Darts recognition]\MyC#\TestPhoto\Ethalon images\cam1_2.png");
                testCaptureThrow3 = new Image<Bgr, byte>(@"C:\Users\YellowFive\Dropbox\MY\[Darts recognition]\MyC#\TestPhoto\Ethalon images\cam1_3.png");
                camIndexBox = (TextBox)controlsCollection["Cam1IndexBox"];
                tresholdMinSlider = (Slider)controlsCollection["Cam1TresholdMinSlider"];
                tresholdMaxSlider = (Slider)controlsCollection["Cam1TresholdMaxSlider"];
                roiPosXSlider = (Slider)controlsCollection["Cam1RoiPosXSlider"];
                roiPosYSlider = (Slider)controlsCollection["Cam1RoiPosYSlider"];
                roiWidthSlider = (Slider)controlsCollection["Cam1RoiWidthSlider"];
                roiHeightSlider = (Slider)controlsCollection["Cam1RoiHeightSlider"];
                surfaceSlider = (Slider)controlsCollection["Cam1SurfaceSlider"];
                imageBox = (Image)controlsCollection["ImageBox1"];
                imageBoxRoi = (Image)controlsCollection["ImageBox1Roi"];
            }
            else
            {
                testCaptureThrow1 = new Image<Bgr, byte>(@"C:\Users\YellowFive\Dropbox\MY\[Darts recognition]\MyC#\TestPhoto\Ethalon images\cam2_1.png");
                testCaptureThrow2 = new Image<Bgr, byte>(@"C:\Users\YellowFive\Dropbox\MY\[Darts recognition]\MyC#\TestPhoto\Ethalon images\cam2_2.png");
                testCaptureThrow3 = new Image<Bgr, byte>(@"C:\Users\YellowFive\Dropbox\MY\[Darts recognition]\MyC#\TestPhoto\Ethalon images\cam2_3.png");
                camIndexBox = (TextBox)controlsCollection["Cam2IndexBox"];
                tresholdMinSlider = (Slider)controlsCollection["Cam2TresholdMinSlider"];
                tresholdMaxSlider = (Slider)controlsCollection["Cam2TresholdMaxSlider"];
                roiPosXSlider = (Slider)controlsCollection["Cam2RoiPosXSlider"];
                roiPosYSlider = (Slider)controlsCollection["Cam2RoiPosYSlider"];
                roiWidthSlider = (Slider)controlsCollection["Cam2RoiWidthSlider"];
                roiHeightSlider = (Slider)controlsCollection["Cam2RoiHeightSlider"];
                surfaceSlider = (Slider)controlsCollection["Cam2SurfaceSlider"];
                imageBox = (Image)controlsCollection["ImageBox2"];
                imageBoxRoi = (Image)controlsCollection["ImageBox2Roi"];
            }
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
                    break;
            }
        }
    }
}
