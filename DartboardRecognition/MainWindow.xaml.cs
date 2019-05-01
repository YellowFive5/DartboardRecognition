#region Usings

using System;
using System.ComponentModel;
using System.Configuration;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;

#endregion

namespace DartboardRecognition
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        VideoCapture videoCapture;

        EventHandler cam1Handler;
        EventHandler cam2Handler;

        BitmapImage originImageWithLines = new BitmapImage();
        BitmapImage roiTrasholdImage = new BitmapImage();
        BitmapImage dartboardImage = new BitmapImage();

        Image<Bgr, byte> originFrame;
        Image<Bgr, byte> linedFrame;
        Image<Bgr, byte> roiFrame;
        Image<Gray, byte> roiTrasholdFrame;
        Image<Bgr, byte> roiContourFrame;
        Image<Bgr, byte> dartboardProjectionFrame;

        System.Drawing.Point surfacePoint1 = new System.Drawing.Point();
        System.Drawing.Point surfacePoint2 = new System.Drawing.Point();
        Bgr surfaceLineColor = new Bgr(System.Drawing.Color.Red);
        int surfaceLineThickness = 5;

        Bgr roiRectColor = new Bgr(System.Drawing.Color.LawnGreen);
        int roiRectThickness = 5;

        MCvScalar contourColor = new Bgr(System.Drawing.Color.Violet).MCvScalar;
        int countourThickness = 2;

        MCvScalar contourRectColor = new Bgr(System.Drawing.Color.Blue).MCvScalar;
        int contourRectThickness = 5;

        int spikeLineLength;
        MCvScalar spikeLineColor = new Bgr(System.Drawing.Color.White).MCvScalar;
        int spikeLineThickness = 4;

        MCvScalar pointOfImpactColor = new Bgr(System.Drawing.Color.Yellow).MCvScalar;
        int pointOfImpactRadius = 6;
        int pointOfImpactThickness = 6;

        MCvScalar dartboardProjectionColor = new Bgr(System.Drawing.Color.White).MCvScalar;
        int dartboardProjectionFrameHeigth = 1200;
        int dartboardProjectionFrameWidth = 1200;
        int dartboardProjectionCoefficent = 3;
        int dartboardProjectionThickness = 2;

        MCvScalar surfaceProjectionLineColor = new Bgr(System.Drawing.Color.Red).MCvScalar;
        int surfaceProjectionLineThickness = 2;


        int minContourArcLength = 250;

        VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
        Mat matHierarсhy = new Mat();

        Image<Bgr, byte> cam1_1 = new Image<Bgr, byte>(@"C:\Users\YellowFive\Dropbox\MY\[Darts recognition]\MyC#\TestPhoto\Ethalon images\cam1_1.png");
        Image<Bgr, byte> cam1_2 = new Image<Bgr, byte>(@"C:\Users\YellowFive\Dropbox\MY\[Darts recognition]\MyC#\TestPhoto\Ethalon images\cam1_2.png");
        Image<Bgr, byte> cam1_3 = new Image<Bgr, byte>(@"C:\Users\YellowFive\Dropbox\MY\[Darts recognition]\MyC#\TestPhoto\Ethalon images\cam1_3.png");
        Image<Bgr, byte> cam2_1 = new Image<Bgr, byte>(@"C:\Users\YellowFive\Dropbox\MY\[Darts recognition]\MyC#\TestPhoto\Ethalon images\cam2_1.png");
        Image<Bgr, byte> cam2_2 = new Image<Bgr, byte>(@"C:\Users\YellowFive\Dropbox\MY\[Darts recognition]\MyC#\TestPhoto\Ethalon images\cam2_2.png");
        Image<Bgr, byte> cam2_3 = new Image<Bgr, byte>(@"C:\Users\YellowFive\Dropbox\MY\[Darts recognition]\MyC#\TestPhoto\Ethalon images\cam2_3.png");

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
            videoCapture = new VideoCapture(0);
        }

        private void LoadSettings()
        {
            TresholdMinSlider.Value = double.Parse(ConfigurationManager.AppSettings["TresholdMinSlider"]);
            TresholdMaxSlider.Value = double.Parse(ConfigurationManager.AppSettings["TresholdMaxSlider"]);
            RoiPosXSlider.Value = double.Parse(ConfigurationManager.AppSettings["RoiPosXSlider"]);
            RoiPosYSlider.Value = double.Parse(ConfigurationManager.AppSettings["RoiPosYSlider"]);
            RoiWidthSlider.Value = double.Parse(ConfigurationManager.AppSettings["RoiWidthSlider"]);
            RoiHeightSlider.Value = double.Parse(ConfigurationManager.AppSettings["RoiHeightSlider"]);
            SurfaceSlider.Value = double.Parse(ConfigurationManager.AppSettings["SurfaceSlider"]);
            CamIndexBox.Text = ConfigurationManager.AppSettings["CamIndexBox"];
        }
        private void SaveSettings()
        {
            Configuration configManager = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetExecutingAssembly().Location);
            configManager.AppSettings.Settings.Remove("TresholdMinSlider");
            configManager.AppSettings.Settings.Add("TresholdMinSlider", TresholdMinSlider.Value.ToString());
            configManager.AppSettings.Settings.Remove("TresholdMaxSlider");
            configManager.AppSettings.Settings.Add("TresholdMaxSlider", TresholdMaxSlider.Value.ToString());
            configManager.AppSettings.Settings.Remove("RoiPosXSlider");
            configManager.AppSettings.Settings.Add("RoiPosXSlider", RoiPosXSlider.Value.ToString());
            configManager.AppSettings.Settings.Remove("RoiPosYSlider");
            configManager.AppSettings.Settings.Add("RoiPosYSlider", RoiPosYSlider.Value.ToString());
            configManager.AppSettings.Settings.Remove("RoiWidthSlider");
            configManager.AppSettings.Settings.Add("RoiWidthSlider", RoiWidthSlider.Value.ToString());
            configManager.AppSettings.Settings.Remove("RoiHeightSlider");
            configManager.AppSettings.Settings.Add("RoiHeightSlider", RoiHeightSlider.Value.ToString());
            configManager.AppSettings.Settings.Remove("SurfaceSlider");
            configManager.AppSettings.Settings.Add("SurfaceSlider", SurfaceSlider.Value.ToString());
            configManager.AppSettings.Settings.Remove("CamIndexBox");
            configManager.AppSettings.Settings.Add("CamIndexBox", CamIndexBox.Text);
            configManager.Save(ConfigurationSaveMode.Modified);
        }
        private void OnClosing(object sender, CancelEventArgs e)
        {
            SaveSettings();
        }
        private void CaptureImage(object sender, EventArgs e, Image<Bgr, byte> img, int imageBox)
        {
            originFrame = img.Clone();
            //using (originFrame = capture.QueryFrame().ToImage<Bgr, byte>())
            //{
            using (originFrame)
            {
                if (originFrame != null)
                {
                    #region DrawLines

                    surfacePoint1 = new System.Drawing.Point(0, (int)SurfaceSlider.Value);
                    surfacePoint2 = new System.Drawing.Point(originFrame.Cols, (int)SurfaceSlider.Value);
                    linedFrame = originFrame.Clone();
                    linedFrame.Draw(new LineSegment2D(surfacePoint1, surfacePoint2), surfaceLineColor, surfaceLineThickness);
                    linedFrame.Draw(new System.Drawing.Rectangle(
                                                    (int)RoiPosXSlider.Value,
                                                    (int)RoiPosYSlider.Value,
                                                    (int)RoiWidthSlider.Value,
                                                    (int)RoiHeightSlider.Value),
                                    roiRectColor,
                                    roiRectThickness);

                    #endregion

                    #region FindROIRegion

                    roiFrame = originFrame.Clone();
                    roiFrame.ROI = new System.Drawing.Rectangle(
                        (int)RoiPosXSlider.Value,
                        (int)RoiPosYSlider.Value,
                        (int)RoiWidthSlider.Value,
                        (int)RoiHeightSlider.Value);

                    #endregion

                    #region TresholdROIRegion

                    roiTrasholdFrame = roiFrame.Clone().Convert<Gray, byte>().Not();
                    roiTrasholdFrame._ThresholdBinary(
                        new Gray(TresholdMinSlider.Value),
                        new Gray(TresholdMaxSlider.Value));

                    #endregion

                    #region FindDartContours

                    roiContourFrame = roiFrame.Clone();
                    CvInvoke.FindContours(roiTrasholdFrame, contours, matHierarсhy, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
                    //CvInvoke.DrawContours(linedFrame, contours, -1, contourColor, countourThickness, offset: new System.Drawing.Point(0, (int)RoiPosYSlider.Value));

                    if (contours.Size > 0)
                    {
                        for (int i = 0; i < contours.Size; i++)
                        {
                            // Filter contour
                            var arclenth = CvInvoke.ArcLength(contours[i],true);
                            if (arclenth < minContourArcLength)
                            {
                                continue;
                            }

                            // Find moments and centerpoint
                            var moments = CvInvoke.Moments(contours[i], false);
                            var centerPoint = new System.Drawing.Point((int)(moments.M10 / moments.M00), (int)RoiPosYSlider.Value + (int)(moments.M01 / moments.M00));
                            CvInvoke.Circle(linedFrame, centerPoint, 4, new Bgr(System.Drawing.Color.Blue).MCvScalar, 3);

                            // Find contour rectangle
                            var rect = CvInvoke.MinAreaRect(contours[i]);
                            var box = CvInvoke.BoxPoints(rect);
                            var point1 = new System.Drawing.Point((int)box[0].X, (int)RoiPosYSlider.Value + (int)box[0].Y);
                            var point2 = new System.Drawing.Point((int)box[1].X, (int)RoiPosYSlider.Value + (int)box[1].Y);
                            var point3 = new System.Drawing.Point((int)box[2].X, (int)RoiPosYSlider.Value + (int)box[2].Y);
                            var point4 = new System.Drawing.Point((int)box[3].X, (int)RoiPosYSlider.Value + (int)box[3].Y);
                            CvInvoke.Line(linedFrame, point1, point2, contourRectColor, contourRectThickness);
                            CvInvoke.Line(linedFrame, point2, point3, contourRectColor, contourRectThickness);
                            CvInvoke.Line(linedFrame, point3, point4, contourRectColor, contourRectThickness);
                            CvInvoke.Line(linedFrame, point4, point1, contourRectColor, contourRectThickness);

                            // Setup vertical contour middlepoints 
                            var middlePoint1 = new System.Drawing.Point();
                            var middlePoint2 = new System.Drawing.Point();
                            if (DistancePtP(point1,point2) < DistancePtP(point4,point1))
                            {
                                middlePoint1 = MiddlePoint(point1, point2);
                                middlePoint2 = MiddlePoint(point4, point3);
                            }
                            else
                            {
                                middlePoint1 = MiddlePoint(point4, point1);
                                middlePoint2 = MiddlePoint(point3, point2);
                            }

                            // Find spikeLine to surface
                            var spikePoint1 = middlePoint1;
                            var spikePoint2 = middlePoint2;
                            spikeLineLength = surfacePoint2.Y - middlePoint2.Y; 
                            var angle = Math.Atan2(middlePoint1.Y - middlePoint2.Y, middlePoint1.X - middlePoint2.X);
                            spikePoint1.X = (int)(middlePoint2.X + Math.Cos(angle) * spikeLineLength);
                            spikePoint1.Y = (int)(middlePoint2.Y + Math.Sin(angle) * spikeLineLength);
                            CvInvoke.Line(linedFrame, spikePoint1, spikePoint2, spikeLineColor, spikeLineThickness);

                            // Find point of impact with surface
                            var pointOfImpact = IntersectPoint(spikePoint1, spikePoint2, surfacePoint1, surfacePoint2);
                            if (pointOfImpact != null)
                            {
                                CvInvoke.Circle(linedFrame, pointOfImpact.Value, pointOfImpactRadius, pointOfImpactColor, pointOfImpactThickness);
                            }
                        }
                    }
                    #endregion

                    #region SaveImageToImagebox

                    switch (imageBox)
                    {
                        case 1:
                            ImageBox1.Source = SaveBitmap(linedFrame, originImageWithLines);
                            ImageBox1Roi.Source = SaveBitmap(roiTrasholdFrame, roiTrasholdImage);
                            break;
                        case 2:
                            ImageBox2.Source = SaveBitmap(linedFrame, originImageWithLines);
                            ImageBox2Roi.Source = SaveBitmap(roiTrasholdFrame, roiTrasholdImage);
                            break;
                        default:
                            break;
                    }

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

                    ImageBox3.Source = SaveBitmap(dartboardProjectionFrame,dartboardImage);

                    #endregion
                }
            }
        }
        private static BitmapImage SaveBitmap(IImage frame, BitmapImage imageToSave)
        {
            using (var stream = new MemoryStream())
            {
                imageToSave = new BitmapImage();
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

            Image<Bgr, byte> cam1Image = null;
            Image<Bgr, byte> cam2Image = null;
            if (Throw1RadioButton.IsChecked.Value)
            {
                cam1Image = cam1_1;
                cam2Image = cam2_1;
            }
            if (Throw2RadioButton.IsChecked.Value)
            {
                cam1Image = cam1_2;
                cam2Image = cam2_2;
            }
            if (Throw3RadioButton.IsChecked.Value)
            {
                cam1Image = cam1_3;
                cam2Image = cam2_3;
            }
            cam1Handler = (s, e2) => CaptureImage(s, e2, cam1Image, 1);
            cam2Handler = (s, e2) => CaptureImage(s, e2, cam2Image, 2);
            this.Dispatcher.Hooks.DispatcherInactive += cam1Handler;
            this.Dispatcher.Hooks.DispatcherInactive += cam2Handler;
        }
        private void StopButtonClick(object sender, RoutedEventArgs e)
        {
            ToggleControls();

            this.Dispatcher.Hooks.DispatcherInactive -= cam1Handler;
            this.Dispatcher.Hooks.DispatcherInactive -= cam2Handler;

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
            CamIndexBox.IsEnabled = !CamIndexBox.IsEnabled;
            Throw1RadioButton.IsEnabled = !Throw1RadioButton.IsEnabled;
            Throw2RadioButton.IsEnabled = !Throw2RadioButton.IsEnabled;
            Throw3RadioButton.IsEnabled = !Throw3RadioButton.IsEnabled;
        }
        private void CamIndexBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int camId = 0;
            Int32.TryParse(CamIndexBox.Text, out camId);
            videoCapture = new VideoCapture(camId);
        }
        private static System.Drawing.Point MiddlePoint(System.Drawing.Point point1, System.Drawing.Point point2)
        {
            var mpX = (point1.X + point2.X) / 2;
            var mpY = (point1.Y + point2.Y) / 2;
            return new System.Drawing.Point(mpX, mpY);
        }
        private static int DistancePtP(System.Drawing.Point point1, System.Drawing.Point point2)
        {
            return (int)Math.Sqrt((Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2)));
        }
        private static System.Drawing.Point? IntersectPoint(System.Drawing.Point line1point1,
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
