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

        Image<Bgr, byte> originFrame;
        Image<Bgr, byte> linedFrame;
        Image<Bgr, byte> roiFrame;
        Image<Gray, byte> roiTrasholdFrame;
        Image<Bgr, byte> roiContourFrame;

        System.Drawing.Point surfacePoint1 = new System.Drawing.Point();
        System.Drawing.Point surfacePoint2 = new System.Drawing.Point();
        Bgr surfaceLineColor = new Bgr(0, 0, 255);
        int surfaceLineThickness = 5;

        Bgr roiRectColor = new Bgr(50, 255, 150);
        int roiRectThickness = 5;

        MCvScalar contourColor = new MCvScalar(0, 0, 255);
        int countourThickness = 2;

        MCvScalar contourRectColor = new MCvScalar(255, 0, 0);
        int contourRectThickness = 5;

        int spikeLineLength;
        MCvScalar spikeLineColor = new MCvScalar(255, 255, 255);
        int spikeLineThickness = 4;

        MCvScalar pointOfImpactColor = new MCvScalar(0, 255, 255);
        int pointOfImpactRadius = 6;
        int pointOfImpactThickness = 6;

        int minContourArcLength = 250;

        VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
        Mat matHierarhy = new Mat();

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
            TrasholdMinSlider.Value = double.Parse(ConfigurationManager.AppSettings["TrasholdMinSlider"]);
            TrasholdMaxSlider.Value = double.Parse(ConfigurationManager.AppSettings["TrasholdMaxSlider"]);
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
            configManager.AppSettings.Settings.Remove("TrasholdMinSlider");
            configManager.AppSettings.Settings.Add("TrasholdMinSlider", TrasholdMinSlider.Value.ToString());
            configManager.AppSettings.Settings.Remove("TrasholdMaxSlider");
            configManager.AppSettings.Settings.Add("TrasholdMaxSlider", TrasholdMaxSlider.Value.ToString());
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
                    #region LinedFrame

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

                    #region ROIFrame

                    roiFrame = originFrame.Clone();
                    roiFrame.ROI = new System.Drawing.Rectangle(
                        (int)RoiPosXSlider.Value,
                        (int)RoiPosYSlider.Value,
                        (int)RoiWidthSlider.Value,
                        (int)RoiHeightSlider.Value);

                    #endregion

                    #region ROITrasholdFrame

                    roiTrasholdFrame = roiFrame.Clone().Convert<Gray, byte>().Not();
                    roiTrasholdFrame._ThresholdBinary(
                        new Gray(TrasholdMinSlider.Value),
                        new Gray(TrasholdMaxSlider.Value));

                    #endregion

                    #region ROIContourFrame

                    roiContourFrame = roiFrame.Clone();
                    CvInvoke.FindContours(roiTrasholdFrame, contours, matHierarhy, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
                    //CvInvoke.DrawContours(linedFrame, contours, -1, contourColor, countourThickness, offset: new System.Drawing.Point(0, (int)RoiPosYSlider.Value));

                    if (contours.Size > 0)
                    {
                        for (int i = 0; i < contours.Size; i++)
                        {
                            var arclenth = CvInvoke.ArcLength(contours[i],true);
                            if (arclenth < minContourArcLength)
                            {
                                continue;
                            }
                            var moments = CvInvoke.Moments(contours[i], false);
                            var centerPoint = new System.Drawing.Point((int)(moments.M10 / moments.M00), (int)RoiPosYSlider.Value + (int)(moments.M01 / moments.M00));
                            CvInvoke.Circle(linedFrame, centerPoint, 4, new MCvScalar(255, 0, 0), 3);

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

                            var spikePoint1 = middlePoint1;
                            var spikePoint2 = middlePoint2;
                            spikeLineLength = surfacePoint2.Y - middlePoint2.Y; 
                            var angle = Math.Atan2(middlePoint1.Y - middlePoint2.Y, middlePoint1.X - middlePoint2.X);
                            spikePoint1.X = (int)(middlePoint2.X + Math.Cos(angle) * spikeLineLength);
                            spikePoint1.Y = (int)(middlePoint2.Y + Math.Sin(angle) * spikeLineLength);
                            CvInvoke.Line(linedFrame, spikePoint1, spikePoint2, spikeLineColor, spikeLineThickness);

                            var pointOfImpact = IntersectPoint(spikePoint1, spikePoint2, surfacePoint1, surfacePoint2);
                            if (pointOfImpact != null)
                            {
                                CvInvoke.Circle(linedFrame, pointOfImpact.Value, pointOfImpactRadius, pointOfImpactColor, pointOfImpactThickness);
                            }
                        }
                    }
                    #endregion

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

            StartButton.IsEnabled = !StartButton.IsEnabled;
            CamIndexBox.IsEnabled = !CamIndexBox.IsEnabled;
        }
        private void StopButtonClick(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Hooks.DispatcherInactive -= cam1Handler;
            this.Dispatcher.Hooks.DispatcherInactive -= cam2Handler;

            ImageBox1.Source = null;
            ImageBox1Roi.Source = null;
            ImageBox2.Source = null;
            ImageBox2Roi.Source = null;
            if (!StartButton.IsEnabled)
            {
                StartButton.IsEnabled = !StartButton.IsEnabled;
            }
            if (!CamIndexBox.IsEnabled)
            {
                CamIndexBox.IsEnabled = !CamIndexBox.IsEnabled;
            }
        }
        private void CamIndexBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int camId = 0;
            Int32.TryParse(CamIndexBox.Text, out camId);
            videoCapture = new VideoCapture(camId);
        }
        private static System.Drawing.Point MiddlePoint(System.Drawing.Point point1,System.Drawing.Point point2)
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
