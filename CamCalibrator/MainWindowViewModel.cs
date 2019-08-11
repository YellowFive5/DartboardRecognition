#region Usings

using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;

#endregion

namespace CamCalibrator
{
    public class MainWindowViewModel
    {
        private MainWindow view;
        private bool isCapturing;

        public MainWindowViewModel()
        {
        }

        public MainWindowViewModel(MainWindow view)
        {
            this.view = view;
        }

        public void StartCapture()
        {
            ToggleControls();
            isCapturing = true;

            var camId = int.Parse(view.CamNumberBox.Text);
            Task.Factory.StartNew(() => CaptureWork(camId));
        }

        public void StopCapture()
        {
            ToggleControls();
            isCapturing = false;
        }

        private void ToggleControls()
        {
            view.StartButton.IsEnabled = !view.StartButton.IsEnabled;
            view.StopButton.IsEnabled = !view.StopButton.IsEnabled;
            view.CamNumberBox.IsEnabled = !view.CamNumberBox.IsEnabled;
        }

        private void CaptureWork(int camId)
        {
            var videoCapture = new VideoCapture(camId);
            videoCapture.SetCaptureProperty(CapProp.FrameWidth, 1920);
            videoCapture.SetCaptureProperty(CapProp.FrameHeight, 1080);

            var frame = videoCapture.QueryFrame().ToImage<Bgr, byte>();
            BitmapImage imageToSave;

            var horPoint1 = new Point(0, frame.Height / 2);
            var horPoint2 = new Point(frame.Width, frame.Height / 2);
            var horLineColor = new Bgr(Color.Red).MCvScalar;
            var horLineThickness = 5;

            var verPoint1 = new Point(frame.Width / 2, 0);
            var verPoint2 = new Point(frame.Width / 2, frame.Height);
            var verLineColor = new Bgr(Color.Blue).MCvScalar;
            var verLineThickness = 5;

            while (isCapturing)
            {
                frame = videoCapture.QueryFrame().ToImage<Bgr, byte>();
                using (frame)
                {
                    CvInvoke.Line(frame, horPoint1, horPoint2, horLineColor, horLineThickness);
                    CvInvoke.Line(frame, verPoint1, verPoint2, verLineColor, verLineThickness);

                    using (var stream = new MemoryStream())
                    {
                        imageToSave = new BitmapImage();
                        frame.Bitmap.Save(stream, ImageFormat.Bmp);
                        imageToSave.BeginInit();
                        imageToSave.StreamSource = new MemoryStream(stream.ToArray());
                        imageToSave.EndInit();
                        imageToSave.Freeze();
                    }

                    view.Dispatcher.Invoke(new Action(() => view.ImageBox.Source = imageToSave));
                }
            }

            videoCapture.Dispose();
            view.Dispatcher.Invoke(new Action(() => view.ImageBox.Source = new BitmapImage()));
        }
    }
}