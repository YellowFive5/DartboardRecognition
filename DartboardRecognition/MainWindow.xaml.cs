#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;

#endregion

namespace DartboardRecognition
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Cam Cam1;
        Cam Cam2;
        Dictionary<string, FrameworkElement> controlsCollection = new Dictionary<string, FrameworkElement>();

        Bgr surfaceLineColor = new Bgr(System.Drawing.Color.Red);
        int surfaceLineThickness = 5;

        Bgr roiRectColor = new Bgr(System.Drawing.Color.LawnGreen);
        int roiRectThickness = 5;

        MCvScalar contourColor = new Bgr(System.Drawing.Color.Violet).MCvScalar;
        int countourThickness = 2;

        MCvScalar contourRectColor = new Bgr(System.Drawing.Color.Blue).MCvScalar;
        int contourRectThickness = 5;

        MCvScalar spikeLineColor = new Bgr(System.Drawing.Color.White).MCvScalar;
        int spikeLineThickness = 4;

        MCvScalar pointOfImpactColor = new Bgr(System.Drawing.Color.Yellow).MCvScalar;
        int pointOfImpactRadius = 6;
        int pointOfImpactThickness = 6;

        private Image<Bgr, byte> dartboardProjectionFrame;
        MCvScalar dartboardProjectionColor = new Bgr(System.Drawing.Color.White).MCvScalar;
        int dartboardProjectionFrameHeigth = 1200;
        int dartboardProjectionFrameWidth = 1200;
        int dartboardProjectionCoefficent = 3;
        int dartboardProjectionThickness = 2;

        MCvScalar surfaceProjectionLineColor = new Bgr(System.Drawing.Color.Red).MCvScalar;
        int surfaceProjectionLineThickness = 2;

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
            PrepareControlsCollection();
            Cam1 = new Cam(controlsCollection, 1);
            Cam2 = new Cam(controlsCollection, 2);
        }
        private void PrepareControlsCollection()
        {
            controlsCollection.Add("Cam1TresholdMinSlider", Cam1TresholdMinSlider);
            controlsCollection.Add("Cam1TresholdMaxSlider", Cam1TresholdMaxSlider);
            controlsCollection.Add("Cam1RoiPosXSlider", Cam1RoiPosXSlider);
            controlsCollection.Add("Cam1RoiPosYSlider", Cam1RoiPosYSlider);
            controlsCollection.Add("Cam1RoiWidthSlider", Cam1RoiWidthSlider);
            controlsCollection.Add("Cam1RoiHeightSlider", Cam1RoiHeightSlider);
            controlsCollection.Add("Cam1SurfaceSlider", Cam1SurfaceSlider);
            controlsCollection.Add("Cam1IndexBox", Cam1IndexBox);
            controlsCollection.Add("Cam2TresholdMinSlider", Cam2TresholdMinSlider);
            controlsCollection.Add("Cam2TresholdMaxSlider", Cam2TresholdMaxSlider);
            controlsCollection.Add("Cam2RoiPosXSlider", Cam2RoiPosXSlider);
            controlsCollection.Add("Cam2RoiPosYSlider", Cam2RoiPosYSlider);
            controlsCollection.Add("Cam2RoiWidthSlider", Cam2RoiWidthSlider);
            controlsCollection.Add("Cam2RoiHeightSlider", Cam2RoiHeightSlider);
            controlsCollection.Add("Cam2SurfaceSlider", Cam2SurfaceSlider);
            controlsCollection.Add("Cam2IndexBox", Cam2IndexBox);
            controlsCollection.Add("ImageBox1", ImageBox1);
            controlsCollection.Add("ImageBox1Roi", ImageBox1Roi);
            controlsCollection.Add("ImageBox2", ImageBox2);
            controlsCollection.Add("ImageBox2Roi", ImageBox2Roi);
        }
        private void LoadSettings()
        {
            Cam1TresholdMinSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1TresholdMinSlider"]);
            Cam1TresholdMaxSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1TresholdMaxSlider"]);
            Cam1RoiPosXSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1RoiPosXSlider"]);
            Cam1RoiPosYSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1RoiPosYSlider"]);
            Cam1RoiWidthSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1RoiWidthSlider"]);
            Cam1RoiHeightSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1RoiHeightSlider"]);
            Cam1SurfaceSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1SurfaceSlider"]);
            Cam1IndexBox.Text = ConfigurationManager.AppSettings["Cam1IndexBox"];
            Cam2TresholdMinSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2TresholdMinSlider"]);
            Cam2TresholdMaxSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2TresholdMaxSlider"]);
            Cam2RoiPosXSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2RoiPosXSlider"]);
            Cam2RoiPosYSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2RoiPosYSlider"]);
            Cam2RoiWidthSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2RoiWidthSlider"]);
            Cam2RoiHeightSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2RoiHeightSlider"]);
            Cam2SurfaceSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2SurfaceSlider"]);
            Cam2IndexBox.Text = ConfigurationManager.AppSettings["Cam2IndexBox"];
        }
        private void SaveSettings()
        {
            Configuration configManager = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetExecutingAssembly().Location);
            configManager.AppSettings.Settings.Clear();
            configManager.AppSettings.Settings.Add("Cam1TresholdMinSlider", Cam1TresholdMinSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1TresholdMaxSlider", Cam1TresholdMaxSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1RoiPosXSlider", Cam1RoiPosXSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1RoiPosYSlider", Cam1RoiPosYSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1RoiWidthSlider", Cam1RoiWidthSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1RoiHeightSlider", Cam1RoiHeightSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1SurfaceSlider", Cam1SurfaceSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1IndexBox", Cam1IndexBox.Text);
            configManager.AppSettings.Settings.Add("Cam2TresholdMinSlider", Cam2TresholdMinSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2TresholdMaxSlider", Cam2TresholdMaxSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2RoiPosXSlider", Cam2RoiPosXSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2RoiPosYSlider", Cam2RoiPosYSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2RoiWidthSlider", Cam2RoiWidthSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2RoiHeightSlider", Cam2RoiHeightSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2SurfaceSlider", Cam2SurfaceSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2IndexBox", Cam2IndexBox.Text);
            configManager.Save(ConfigurationSaveMode.Modified);
        }
        private void OnClosing(object sender, CancelEventArgs e)
        {
            SaveSettings();
        }
        private void CaptureImage(object sender, EventArgs e, Cam cam)
        {

            cam.originFrame = cam.processingCapture.Clone();
            //using (cam.originFrame = cam.videoCapture.QueryFrame().ToImage<Bgr, byte>())
            //{
            using (cam.originFrame)
            {
                if (cam.originFrame != null)
                {
                    #region DrawLines

                    cam.surfacePoint1 = new System.Drawing.Point(0, (int)cam.surfaceSlider.Value);
                    cam.surfacePoint2 = new System.Drawing.Point(cam.originFrame.Cols, (int)cam.surfaceSlider.Value);
                    cam.linedFrame = cam.originFrame.Clone();
                    cam.linedFrame.Draw(new LineSegment2D(cam.surfacePoint1, cam.surfacePoint2), surfaceLineColor, surfaceLineThickness);
                    cam.linedFrame.Draw(new System.Drawing.Rectangle(
                                                    (int)cam.roiPosXSlider.Value,
                                                    (int)cam.roiPosYSlider.Value,
                                                    (int)cam.roiWidthSlider.Value,
                                                    (int)cam.roiHeightSlider.Value),
                                    roiRectColor,
                                    roiRectThickness);

                    #endregion

                    #region FindROIRegion

                    cam.roiFrame = cam.originFrame.Clone();
                    cam.roiFrame.ROI = new System.Drawing.Rectangle(
                        (int)cam.roiPosXSlider.Value,
                        (int)cam.roiPosYSlider.Value,
                        (int)cam.roiWidthSlider.Value,
                        (int)cam.roiHeightSlider.Value);

                    #endregion

                    #region TresholdROIRegion

                    cam.roiTrasholdFrame = cam.roiFrame.Clone().Convert<Gray, byte>().Not();
                    cam.roiTrasholdFrame._ThresholdBinary(
                        new Gray(cam.tresholdMinSlider.Value),
                        new Gray(cam.tresholdMaxSlider.Value));

                    #endregion

                    #region FindDartContours

                    cam.roiContourFrame = cam.roiFrame.Clone();
                    CvInvoke.FindContours(cam.roiTrasholdFrame, cam.contours, cam.matHierarсhy, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
                    //CvInvoke.DrawContours(linedFrame, contours, -1, contourColor, countourThickness, offset: new System.Drawing.Point(0, (int)RoiPosYSlider.Value));

                    if (cam.contours.Size > 0)
                    {
                        for (int i = 0; i < cam.contours.Size; i++)
                        {
                            // Filter contour
                            var arclenth = CvInvoke.ArcLength(cam.contours[i],true);
                            if (arclenth < cam.minContourArcLength)
                            {
                                continue;
                            }

                            // Find moments and centerpoint
                            var moments = CvInvoke.Moments(cam.contours[i], false);
                            var centerPoint = new System.Drawing.Point((int)(moments.M10 / moments.M00), (int)cam.roiPosYSlider.Value + (int)(moments.M01 / moments.M00));
                            CvInvoke.Circle(cam.linedFrame, centerPoint, 4, new Bgr(System.Drawing.Color.Blue).MCvScalar, 3);

                            // Find contour rectangle
                            var rect = CvInvoke.MinAreaRect(cam.contours[i]);
                            var box = CvInvoke.BoxPoints(rect);
                            var point1 = new System.Drawing.Point((int)box[0].X, (int)cam.roiPosYSlider.Value + (int)box[0].Y);
                            var point2 = new System.Drawing.Point((int)box[1].X, (int)cam.roiPosYSlider.Value + (int)box[1].Y);
                            var point3 = new System.Drawing.Point((int)box[2].X, (int)cam.roiPosYSlider.Value + (int)box[2].Y);
                            var point4 = new System.Drawing.Point((int)box[3].X, (int)cam.roiPosYSlider.Value + (int)box[3].Y);
                            CvInvoke.Line(cam.linedFrame, point1, point2, contourRectColor, contourRectThickness);
                            CvInvoke.Line(cam.linedFrame, point2, point3, contourRectColor, contourRectThickness);
                            CvInvoke.Line(cam.linedFrame, point3, point4, contourRectColor, contourRectThickness);
                            CvInvoke.Line(cam.linedFrame, point4, point1, contourRectColor, contourRectThickness);

                            // Setup vertical contour middlepoints 
                            var middlePoint1 = new System.Drawing.Point();
                            var middlePoint2 = new System.Drawing.Point();
                            if (FindDistance(point1,point2) < FindDistance(point4,point1))
                            {
                                middlePoint1 = FindMiddlePoint(point1, point2);
                                middlePoint2 = FindMiddlePoint(point4, point3);
                            }
                            else
                            {
                                middlePoint1 = FindMiddlePoint(point4, point1);
                                middlePoint2 = FindMiddlePoint(point3, point2);
                            }

                            // Find spikeLine to surface
                            var spikePoint1 = middlePoint1;
                            var spikePoint2 = middlePoint2;
                            cam.spikeLineLength = cam.surfacePoint2.Y - middlePoint2.Y; 
                            var angle = Math.Atan2(middlePoint1.Y - middlePoint2.Y, middlePoint1.X - middlePoint2.X);
                            spikePoint1.X = (int)(middlePoint2.X + Math.Cos(angle) * cam.spikeLineLength);
                            spikePoint1.Y = (int)(middlePoint2.Y + Math.Sin(angle) * cam.spikeLineLength);
                            CvInvoke.Line(cam.linedFrame, spikePoint1, spikePoint2, spikeLineColor, spikeLineThickness);

                            // Find point of impact with surface
                            var pointOfImpact = FindIntersectionPoint(spikePoint1, spikePoint2, cam.surfacePoint1, cam.surfacePoint2);
                            if (pointOfImpact != null)
                            {
                                CvInvoke.Circle(cam.linedFrame, pointOfImpact.Value, pointOfImpactRadius, pointOfImpactColor, pointOfImpactThickness);
                            }
                        }
                    }
                    #endregion

                    #region SaveImageToImagebox

                    cam.imageBox.Source = SaveBitmap(cam.linedFrame);
                    cam.imageBoxRoi.Source = SaveBitmap(cam.roiTrasholdFrame);


                    #endregion

                    #region DrawDartboard

                    // Draw dartboard projection
                    dartboardProjectionFrame = new Image<Bgr, byte>(dartboardProjectionFrameWidth, dartboardProjectionFrameHeigth);
                    var dartboardCenterPoint = new System.Drawing.Point(dartboardProjectionFrame.Width/2,dartboardProjectionFrame.Height/2);
                    CvInvoke.Circle(dartboardProjectionFrame, dartboardCenterPoint, dartboardProjectionCoefficent*7, dartboardProjectionColor, dartboardProjectionThickness);
                    CvInvoke.Circle(dartboardProjectionFrame, dartboardCenterPoint, dartboardProjectionCoefficent*17, dartboardProjectionColor, dartboardProjectionThickness);
                    CvInvoke.Circle(dartboardProjectionFrame, dartboardCenterPoint, dartboardProjectionCoefficent*95, dartboardProjectionColor, dartboardProjectionThickness);
                    CvInvoke.Circle(dartboardProjectionFrame, dartboardCenterPoint, dartboardProjectionCoefficent*105, dartboardProjectionColor, dartboardProjectionThickness);
                    CvInvoke.Circle(dartboardProjectionFrame, dartboardCenterPoint, dartboardProjectionCoefficent*160, dartboardProjectionColor, dartboardProjectionThickness);
                    CvInvoke.Circle(dartboardProjectionFrame, dartboardCenterPoint, dartboardProjectionCoefficent*170, dartboardProjectionColor, dartboardProjectionThickness);
                    for (int i = 0; i <= 360; i+=9)
                    {
                        var segmentPoint1 = new System.Drawing.Point();
                        segmentPoint1.X = (int)(dartboardCenterPoint.X + Math.Cos(0.314159 * i - 0.15708) * dartboardProjectionCoefficent * 170);
                        segmentPoint1.Y = (int)(dartboardCenterPoint.Y + Math.Sin(0.314159 * i - 0.15708) * dartboardProjectionCoefficent * 170);
                        var segmentPoint2 = new System.Drawing.Point();
                        segmentPoint2.X = (int)(dartboardCenterPoint.X + Math.Cos(0.314159 * i - 0.15708) * dartboardProjectionCoefficent * 17);
                        segmentPoint2.Y = (int)(dartboardCenterPoint.Y + Math.Sin(0.314159 * i - 0.15708) * dartboardProjectionCoefficent * 17);
                        CvInvoke.Line(dartboardProjectionFrame, segmentPoint1, segmentPoint2, dartboardProjectionColor, dartboardProjectionThickness);
                    }

                    // Draw surface projection lines
                    var surfaceProjectionLineCam1Point1 = new System.Drawing.Point();
                    var surfaceProjectionLineCam1Point2 = new System.Drawing.Point();
                    var surfaceProjectionLineCam2Point1 = new System.Drawing.Point();
                    var surfaceProjectionLineCam2Point2 = new System.Drawing.Point();

                    surfaceProjectionLineCam1Point1.X = (int)(dartboardCenterPoint.X + Math.Cos(-0.785398) * dartboardProjectionCoefficent * 170);
                    surfaceProjectionLineCam1Point1.Y = (int)(dartboardCenterPoint.Y + Math.Sin(-0.785398) * dartboardProjectionCoefficent * 170);
                    surfaceProjectionLineCam1Point2.X = (int)(dartboardCenterPoint.X + Math.Cos(-3.92699) * dartboardProjectionCoefficent * 170);
                    surfaceProjectionLineCam1Point2.Y = (int)(dartboardCenterPoint.Y + Math.Sin(-3.92699) * dartboardProjectionCoefficent * 170);
                    CvInvoke.Line(dartboardProjectionFrame, surfaceProjectionLineCam1Point1, surfaceProjectionLineCam1Point2, surfaceProjectionLineColor, surfaceProjectionLineThickness);

                    surfaceProjectionLineCam2Point1.X = (int)(dartboardCenterPoint.X + Math.Cos(0.785398) * dartboardProjectionCoefficent * 170);
                    surfaceProjectionLineCam2Point1.Y = (int)(dartboardCenterPoint.Y + Math.Sin(0.785398) * dartboardProjectionCoefficent * 170);
                    surfaceProjectionLineCam2Point2.X = (int)(dartboardCenterPoint.X + Math.Cos(3.92699) * dartboardProjectionCoefficent * 170);
                    surfaceProjectionLineCam2Point2.Y = (int)(dartboardCenterPoint.Y + Math.Sin(3.92699) * dartboardProjectionCoefficent * 170);
                    CvInvoke.Line(dartboardProjectionFrame, surfaceProjectionLineCam2Point1, surfaceProjectionLineCam2Point2, surfaceProjectionLineColor, surfaceProjectionLineThickness);

                    ImageBox3.Source = SaveBitmap(dartboardProjectionFrame);

                    #endregion
                }
            }
        }
        private static BitmapImage SaveBitmap(IImage frame)
        {
            using (var stream = new MemoryStream())
            {
                var imageToSave = new BitmapImage();
                frame.Bitmap.Save(stream, ImageFormat.Bmp);
                imageToSave.BeginInit();
                imageToSave.StreamSource = new MemoryStream(stream.ToArray());
                imageToSave.EndInit();
                return imageToSave;
            }
        }
        private void CaptureButtonClick(object sender, RoutedEventArgs e)
        {
            ToggleControls();

            if (Throw1RadioButton.IsChecked.Value)
            {
                Cam1.SetProcessingCapture(1);
                Cam2.SetProcessingCapture(1);
            }
            if (Throw2RadioButton.IsChecked.Value)
            {
                Cam1.SetProcessingCapture(2);
                Cam2.SetProcessingCapture(2);
            }
            if (Throw3RadioButton.IsChecked.Value)
            {
                Cam1.SetProcessingCapture(3);
                Cam2.SetProcessingCapture(3);
            }

            Cam1.camHandler = (s, e2) => CaptureImage(s, e2, Cam1);
            Cam2.camHandler = (s, e2) => CaptureImage(s, e2, Cam2);
            Dispatcher.Hooks.DispatcherInactive += Cam1.camHandler;
            Dispatcher.Hooks.DispatcherInactive += Cam2.camHandler;
        }
        private void StopButtonClick(object sender, RoutedEventArgs e)
        {
            ToggleControls();

            Dispatcher.Hooks.DispatcherInactive -= Cam1.camHandler;
            Dispatcher.Hooks.DispatcherInactive -= Cam2.camHandler;

            ClearImageBoxes();
        }
        private void ClearImageBoxes()
        {
            ImageBox1.Source = null;
            ImageBox1Roi.Source = null;
            ImageBox2.Source = null;
            ImageBox2Roi.Source = null;
            ImageBox3.Source = null;
        }
        private void ToggleControls()
        {
            StartButton.IsEnabled = !StartButton.IsEnabled;
            StopButton.IsEnabled = !StopButton.IsEnabled;
            Cam1IndexBox.IsEnabled = !Cam1IndexBox.IsEnabled;
            Cam2IndexBox.IsEnabled = !Cam2IndexBox.IsEnabled;
            Throw1RadioButton.IsEnabled = !Throw1RadioButton.IsEnabled;
            Throw2RadioButton.IsEnabled = !Throw2RadioButton.IsEnabled;
            Throw3RadioButton.IsEnabled = !Throw3RadioButton.IsEnabled;
        }
        private void CamIndexBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //int camId = 0;
            //Int32.TryParse(CamIndexBox.Text, out camId);
            //videoCapture = new VideoCapture(camId);
        }
        private static System.Drawing.Point FindMiddlePoint(System.Drawing.Point point1, System.Drawing.Point point2)
        {
            var mpX = (point1.X + point2.X) / 2;
            var mpY = (point1.Y + point2.Y) / 2;
            return new System.Drawing.Point(mpX, mpY);
        }
        private static int FindDistance(System.Drawing.Point point1, System.Drawing.Point point2)
        {
            return (int)Math.Sqrt((Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2)));
        }
        private static System.Drawing.Point? FindIntersectionPoint(System.Drawing.Point line1point1,
                                                            System.Drawing.Point line1point2,
                                                            System.Drawing.Point line2point1,
                                                            System.Drawing.Point line2point2)
        {
            float A1 = line1point2.Y - line1point1.Y;
            float B1 = line1point2.X - line1point1.X;
            float C1 = A1 * line1point1.X + B1 * line1point1.Y;

            float A2 = line2point2.Y - line2point1.Y;
            float B2 = line2point2.X - line2point1.X;
            float C2 = A2 * line2point1.X + B2 * line2point1.Y;

            float det = A1 * B2 - A2 * B1;
            if (det == 0)
            {
                return null;
            }
            else
            {
                int x = (int)((B2 * C1 - B1 * C2) / det);
                int y = (int)((A1 * C2 - A2 * C1) / det);
                return new System.Drawing.Point(x, y);
            }
        }

    }
}
