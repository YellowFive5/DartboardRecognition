#region Usings

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using DirectShowLib;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Color = System.Drawing.Color;

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

            var horPoint1 = new PointF(0, (float) frame.Height / 2);
            var horPoint2 = new PointF(frame.Width, (float) frame.Height / 2);
            var horLineColor = new Bgr(Color.Red).MCvScalar;
            var horLineThickness = 5;

            var verPoint1 = new PointF((float) frame.Width / 2, 0);
            var verPoint2 = new PointF((float) frame.Width / 2, frame.Height);
            var verLineColor = new Bgr(Color.Blue).MCvScalar;
            var verLineThickness = 5;

            while (isCapturing)
            {
                frame = videoCapture.QueryFrame().ToImage<Bgr, byte>();
                using (frame)
                {
                    CvInvoke.Line(frame,
                                  new Point((int) horPoint1.X, (int) horPoint1.Y),
                                  new Point((int) horPoint2.X, (int) horPoint2.Y),
                                  horLineColor,
                                  horLineThickness);
                    CvInvoke.Line(frame,
                                  new Point((int) verPoint1.X, (int) verPoint1.Y),
                                  new Point((int) verPoint2.X, (int) verPoint2.Y),
                                  verLineColor,
                                  verLineThickness);

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

        public void FindAllCams()
        {
            var allCams = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice).ToList();
            var str = new StringBuilder();
            for (var i = 0; i < allCams.Count; i++)
            {
                str.AppendLine($"[{i}]-[{allCams[i].Name}]-[{allCams[i].DevicePath}]");
            }

            view.CamsTextBox.Text = allCams.Count == 0
                                        ? "No cameras found"
                                        : str.ToString();
        }
    }
}