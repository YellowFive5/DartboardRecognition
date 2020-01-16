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

#endregion

namespace CamCalibrator
{
    public class MainWindowViewModel
    {
        private readonly MainWindow view;
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
            var width = 0;
            var height = 0;
            if (view._1920X1080.IsChecked.Value)
            {
                width = 1920;
                height = 1080;
            }
            else if (view._1600X900.IsChecked.Value)
            {
                width = 1600;
                height = 900;
            }
            else if (view._1360X768.IsChecked.Value)
            {
                width = 1360;
                height = 768;
            }
            else if (view._1280X720.IsChecked.Value)
            {
                width = 1280;
                height = 720;
            }
            else if (view._640X360.IsChecked.Value)
            {
                width = 640;
                height = 360;
            }

            var camId = int.Parse(view.CamNumberBox.Text);
            Task.Factory.StartNew(() => CaptureWork(camId, width, height));
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
            view._1920X1080.IsEnabled = !view._1920X1080.IsEnabled;
            view._1600X900.IsEnabled = !view._1600X900.IsEnabled;
            view._1360X768.IsEnabled = !view._1360X768.IsEnabled;
            view._1280X720.IsEnabled = !view._1280X720.IsEnabled;
            view._640X360.IsEnabled = !view._640X360.IsEnabled;
        }

        private void CaptureWork(int camId, int width, int height)
        {
            var videoCapture = new VideoCapture(camId, VideoCapture.API.DShow);

            videoCapture.SetCaptureProperty(CapProp.FrameWidth, width);
            videoCapture.SetCaptureProperty(CapProp.FrameHeight, height);

            var frame = videoCapture.QueryFrame().ToImage<Bgr, byte>();
            BitmapImage imageToSave;

            var horPoint1 = new PointF(0, (float) frame.Height / 2);
            var horPoint2 = new PointF(frame.Width, (float) frame.Height / 2);
            var horLineColor = new Bgr(Color.Red).MCvScalar;
            var horLineThickness = frame.Height / 200;

            var verPoint1 = new PointF((float) frame.Width / 2, 0);
            var verPoint2 = new PointF((float) frame.Width / 2, frame.Height);
            var verLineColor = new Bgr(Color.Blue).MCvScalar;
            var verLineThickness = frame.Height / 200;

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