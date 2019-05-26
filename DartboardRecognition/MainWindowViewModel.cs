#region Usings

using System;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using DartboardRecognition.Annotations;
using Emgu.CV.Structure;

#endregion

namespace DartboardRecognition
{
    public class MainWindowViewModel : IMainWindowViewModel
    {
        private MainWindow view;
        private Drawman drawman;
        private Cam cam1;
        private Cam cam2;
        private Measureman measureman1;
        private Measureman measureman2;
        private CancellationToken cancelToken;
        private CancellationTokenSource cts;
        private BitmapImage cam1ImageBox;
        private BitmapImage cam2ImageBox;
        private BitmapImage cam1ImageBoxRoi;
        private BitmapImage cam2ImageBoxRoi;

        public BitmapImage Cam1ImageBox
        {
            get => cam1ImageBox;
            set
            {
                cam1ImageBox = value;
                OnPropertyChanged($"{nameof(Cam1ImageBox)}");
            }
        }

        public BitmapImage Cam2ImageBox
        {
            get => cam2ImageBox;
            set
            {
                cam2ImageBox = value;
                OnPropertyChanged($"{nameof(Cam2ImageBox)}");
            }
        }

        public BitmapImage Cam1ImageBoxRoi
        {
            get => cam1ImageBoxRoi;
            set
            {
                cam1ImageBoxRoi = value;
                OnPropertyChanged($"{nameof(Cam1ImageBoxRoi)}");
            }
        }

        public BitmapImage Cam2ImageBoxRoi
        {
            get => cam2ImageBoxRoi;
            set
            {
                cam2ImageBoxRoi = value;
                OnPropertyChanged($"{nameof(Cam2ImageBoxRoi)}");
            }
        }

        public MainWindowViewModel()
        {
        }

        public MainWindowViewModel(MainWindow view)
        {
            this.view = view;
            LoadSettings();
        }

        public void StartCapture()
        {
            cam1 = new Cam1(view);
            cam2 = new Cam2(view);
            drawman = new Drawman();
            measureman1 = new Measureman(view, drawman);
            measureman2 = new Measureman(view, drawman);
            Cam1ImageBox = new BitmapImage();
            Cam2ImageBox = new BitmapImage();
            Cam1ImageBoxRoi = new BitmapImage();
            Cam2ImageBoxRoi = new BitmapImage();
            cts = new CancellationTokenSource();
            cancelToken = cts.Token;

            measureman1.CalculateDartboardProjection();

            var t = new Task(CaptureImageCam1);
            var t2 = new Task(CaptureImageCam2);
            t.Start();
            t2.Start();
        }

        public void StopCapture()
        {
            cts.Cancel();
        }

        private void LoadSettings()
        {
            view.Cam1TresholdMinSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1TresholdMinSlider"]);
            view.Cam1TresholdMaxSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1TresholdMaxSlider"]);
            view.Cam1RoiPosXSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1RoiPosXSlider"]);
            view.Cam1RoiPosYSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1RoiPosYSlider"]);
            view.Cam1RoiWidthSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1RoiWidthSlider"]);
            view.Cam1RoiHeightSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1RoiHeightSlider"]);
            view.Cam1SurfaceSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1SurfaceSlider"]);
            view.Cam1IndexBox.Text = ConfigurationManager.AppSettings["Cam1IndexBox"];
            view.Cam1SurfaceCenterSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1SurfaceCenterSlider"]);
            view.Cam1SurfaceLeftSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1SurfaceLeftSlider"]);
            view.Cam1SurfaceRightSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1SurfaceRightSlider"]);

            view.Cam2TresholdMinSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2TresholdMinSlider"]);
            view.Cam2TresholdMaxSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2TresholdMaxSlider"]);
            view.Cam2RoiPosXSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2RoiPosXSlider"]);
            view.Cam2RoiPosYSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2RoiPosYSlider"]);
            view.Cam2RoiWidthSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2RoiWidthSlider"]);
            view.Cam2RoiHeightSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2RoiHeightSlider"]);
            view.Cam2SurfaceSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2SurfaceSlider"]);
            view.Cam2IndexBox.Text = ConfigurationManager.AppSettings["Cam2IndexBox"];
            view.Cam2SurfaceCenterSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2SurfaceCenterSlider"]);
            view.Cam2SurfaceLeftSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2SurfaceLeftSlider"]);
            view.Cam2SurfaceRightSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2SurfaceRightSlider"]);
        }

        public void SaveSettings()
        {
            var configManager = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetExecutingAssembly().Location);
            configManager.AppSettings.Settings.Clear();
            configManager.AppSettings.Settings.Add("Cam1TresholdMinSlider", view.Cam1TresholdMinSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1TresholdMaxSlider", view.Cam1TresholdMaxSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1RoiPosXSlider", view.Cam1RoiPosXSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1RoiPosYSlider", view.Cam1RoiPosYSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1RoiWidthSlider", view.Cam1RoiWidthSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1RoiHeightSlider", view.Cam1RoiHeightSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1SurfaceSlider", view.Cam1SurfaceSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1IndexBox", view.Cam1IndexBox.Text);
            configManager.AppSettings.Settings.Add("Cam1SurfaceCenterSlider", view.Cam1SurfaceCenterSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1SurfaceLeftSlider", view.Cam1SurfaceLeftSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1SurfaceRightSlider", view.Cam1SurfaceRightSlider.Value.ToString());

            configManager.AppSettings.Settings.Add("Cam2TresholdMinSlider", view.Cam2TresholdMinSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2TresholdMaxSlider", view.Cam2TresholdMaxSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2RoiPosXSlider", view.Cam2RoiPosXSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2RoiPosYSlider", view.Cam2RoiPosYSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2RoiWidthSlider", view.Cam2RoiWidthSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2RoiHeightSlider", view.Cam2RoiHeightSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2SurfaceSlider", view.Cam2SurfaceSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2IndexBox", view.Cam2IndexBox.Text);
            configManager.AppSettings.Settings.Add("Cam2SurfaceCenterSlider", view.Cam2SurfaceCenterSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2SurfaceLeftSlider", view.Cam2SurfaceLeftSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2SurfaceRightSlider", view.Cam2SurfaceRightSlider.Value.ToString());

            configManager.Save(ConfigurationSaveMode.Modified);
        }

        private void CaptureImageCam1()
        {
            while (!cancelToken.IsCancellationRequested)
            {
                cam1.originFrame = cam1.videoCapture.QueryFrame().ToImage<Bgr, byte>();
                using (cam1.originFrame)
                {
                    if (cam1.originFrame == null)
                    {
                        return;
                    }

                    measureman1.SetupWorkingCam(cam1);
                    measureman1.CalculateSetupLines();
                    measureman1.CalculateRoiRegion();
                    drawman.TresholdRoiRegion(cam1);

                    if (measureman1.ThrowDetected())
                    {
                        measureman1.CalculateDartContour();
                    }

                    Cam1ImageBox.Dispatcher.BeginInvoke(new Action(() => Cam1ImageBox = drawman.ConvertToBitmap(cam1.linedFrame)));
                    Cam1ImageBoxRoi.Dispatcher.BeginInvoke(new Action(() => Cam1ImageBoxRoi = drawman.ConvertToBitmap(cam1.roiTrasholdFrame)));
                }
            }

            cam1.videoCapture.Dispose();
            Cam1ImageBox.Dispatcher.BeginInvoke(new Action(() => Cam1ImageBox = new BitmapImage()));
            Cam1ImageBoxRoi.Dispatcher.BeginInvoke(new Action(() => Cam1ImageBoxRoi = new BitmapImage()));
        }

        private void CaptureImageCam2()
        {
            while (!cancelToken.IsCancellationRequested)
            {
                cam2.originFrame = cam2.videoCapture.QueryFrame().ToImage<Bgr, byte>();
                using (cam2.originFrame)
                {
                    if (cam2.originFrame == null)
                    {
                        return;
                    }

                    measureman2.SetupWorkingCam(cam2);
                    measureman2.CalculateSetupLines();
                    measureman2.CalculateRoiRegion();
                    drawman.TresholdRoiRegion(cam2);

                    if (measureman2.ThrowDetected())
                    {
                        measureman2.CalculateDartContour();
                    }

                    Cam2ImageBox.Dispatcher.BeginInvoke(new Action(() => Cam2ImageBox = drawman.ConvertToBitmap(cam2.linedFrame)));
                    Cam2ImageBoxRoi.Dispatcher.BeginInvoke(new Action(() => Cam2ImageBoxRoi = drawman.ConvertToBitmap(cam2.roiTrasholdFrame)));
                }
            }

            cam2.videoCapture.Dispose();
            Cam2ImageBox.Dispatcher.BeginInvoke(new Action(() => Cam2ImageBox = new BitmapImage()));
            Cam2ImageBoxRoi.Dispatcher.BeginInvoke(new Action(() => Cam2ImageBoxRoi = new BitmapImage()));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}