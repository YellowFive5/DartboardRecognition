#region Usings

using System;
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
        VideoCapture capture;

        BitmapImage originImageWithLines = new BitmapImage();
        BitmapImage roiTrasholdImage = new BitmapImage();

        Image<Bgr, byte> originFrame;
        Image<Bgr, byte> linedFrame;
        Image<Bgr, byte> roiFrame;
        Image<Gray, byte> roiTrasholdFrame;
        Image<Bgr, byte> roiContourFrame;

        VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
        Mat matHierarhy = new Mat();

        public MainWindow()
        {
            InitializeComponent();
            capture = new VideoCapture(0);
        }

        private void CaptureImage(object sender, EventArgs e)
        {

            using (originFrame = capture.QueryFrame().ToImage<Bgr, byte>())
            {
                if (originFrame != null)
                {
                    #region LinedFrame

                    linedFrame = originFrame.Clone();
                    linedFrame.Draw(new LineSegment2D(
                        new System.Drawing.Point(5, 430),
                        new System.Drawing.Point(650, 430)),
                        new Bgr(0, 0, 255),
                        2);
                    linedFrame.Draw(new System.Drawing.Rectangle(
                            (int)RoiPosXSlider.Value,
                            (int)RoiPosYSlider.Value,
                            (int)RoiWidthSlider.Value,
                            (int)RoiHeightSlider.Value),
                        new Bgr(50, 255, 150),
                        2);

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
                    CvInvoke.FindContours(roiTrasholdFrame, contours, matHierarhy, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
                    CvInvoke.DrawContours(linedFrame, contours, -1, new MCvScalar(0, 0, 255), 2, offset: new System.Drawing.Point(0, (int)RoiPosYSlider.Value));

                    if (contours.Size > 0)
                    {
                        for (int i = 0; i < contours.Size; i++)
                        {
                            var moments = CvInvoke.Moments(contours[i], false);
                            var p = new System.Drawing.Point((int)(moments.M10 / moments.M00), (int)RoiPosYSlider.Value + (int)(moments.M01 / moments.M00));
                            CvInvoke.Circle(linedFrame, p, 4, new MCvScalar(255, 0, 0), 3);

                            var rect = CvInvoke.MinAreaRect(contours[i]);
                            var box = CvInvoke.BoxPoints(rect);
                            var point1 = new System.Drawing.Point((int)box[0].X, (int)RoiPosYSlider.Value + (int)box[0].Y);
                            var point2 = new System.Drawing.Point((int)box[1].X, (int)RoiPosYSlider.Value + (int)box[1].Y);
                            var point3 = new System.Drawing.Point((int)box[2].X, (int)RoiPosYSlider.Value + (int)box[2].Y);
                            var point4 = new System.Drawing.Point((int)box[3].X, (int)RoiPosYSlider.Value + (int)box[3].Y);
                            var color = new MCvScalar(255, 0, 0);
                            var thickness = 3;
                            CvInvoke.Line(linedFrame, point1, point2, color, thickness);
                            CvInvoke.Line(linedFrame, point2, point3, color, thickness);
                            CvInvoke.Line(linedFrame, point3, point4, color, thickness);
                            CvInvoke.Line(linedFrame, point4, point1, color, thickness);
                            var midpointX = (point1.X + point2.X) / 2;
                            var midpointY = (point1.Y + point2.Y) / 2;
                            var midpoint = new System.Drawing.Point(midpointX, midpointY);
                            var midpoint2X = (point3.X + point4.X) / 2;
                            var midpoint2Y = (point3.Y + point4.Y) / 2;
                            var midpoint2 = new System.Drawing.Point(midpoint2X, midpoint2Y);
                            CvInvoke.Line(linedFrame, midpoint, midpoint2, color, thickness);

                            // work but strange
                            var P = new System.Drawing.Point(0, 0);
                            var Q = new System.Drawing.Point(0, 0);
                            //test if line is vertical, otherwise computes line equation
                            //y = ax + b
                            if (midpoint.X == midpoint2.X)
                            {
                                P = new System.Drawing.Point(midpoint.X, 0);
                                Q = new System.Drawing.Point(midpoint.X, linedFrame.Rows);
                            }
                            else
                            {
                                int a = (midpoint2.Y - midpoint.Y) / (midpoint2.X - midpoint.X);
                                int b = midpoint.Y - a * midpoint.X;
                                P = new System.Drawing.Point(0, b);
                                Q = new System.Drawing.Point(linedFrame.Rows, a * linedFrame.Rows + b);
                            }
                            CvInvoke.Line(linedFrame, P, Q, color, thickness);
                        }
                    }

                    #endregion

                    ImageBox.Source = SaveBitmap(linedFrame, originImageWithLines);                   
                    ImageBox2.Source = SaveBitmap(roiTrasholdFrame, roiTrasholdImage);

                    //TransformedBitmap bitmap2 = new TransformedBitmap();
                    //bitmap2.BeginInit();
                    //bitmap2.Source = bitmap.Clone();
                    //bitmap2.Transform = new ScaleTransform(-1, 1, 0, 0);
                    //bitmap2.EndInit();
                    //ImageBox2.Source = bitmap2;
                }
            }
        }
        private BitmapImage SaveBitmap(IImage frame, BitmapImage imageToSave)
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
            this.Dispatcher.Hooks.DispatcherInactive += new EventHandler(CaptureImage);
            StartButton.IsEnabled = !StartButton.IsEnabled;
            CamIndexBox.IsEnabled = !CamIndexBox.IsEnabled;
        }
        private void StopButtonClick(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Hooks.DispatcherInactive -= new EventHandler(CaptureImage);
            ImageBox.Source = null;
            ImageBox2.Source = null;
            ImageBox3.Source = null;
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
            capture = new VideoCapture(camId);
        }
    }
}
