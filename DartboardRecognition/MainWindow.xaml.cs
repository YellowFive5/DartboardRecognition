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
        VideoCapture capture = new VideoCapture(1);

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
                            var p = new System.Drawing.Point((int)(moments.M10 / moments.M00), (int)RoiPosYSlider.Value+(int)(moments.M01 / moments.M00));
                            CvInvoke.Circle(linedFrame, p,4, new MCvScalar(255, 0, 0),3);
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
        private BitmapImage SaveBitmap(object frame, BitmapImage imageToSave)
        {
            //todo Boxing/unboxing?

            Image<Bgr, byte> bgrImage;
            Image<Gray, byte> grayscaleImage;

            using (var stream = new MemoryStream())
            {
                imageToSave = new BitmapImage();

                if (frame is Image<Bgr, byte>)
                {
                    bgrImage = (Image<Bgr, byte>)frame;
                    bgrImage.Bitmap.Save(stream, ImageFormat.Bmp);
                }
                else
                {
                    grayscaleImage = (Image<Gray, byte>)frame;
                    grayscaleImage.Bitmap.Save(stream, ImageFormat.Bmp);
                }

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
        }
    }
}
