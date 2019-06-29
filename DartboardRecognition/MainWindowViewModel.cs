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
        private Measureman measureman1;
        private Measureman measureman2;
        private ThrowService throwService;
        private CancellationToken cancelToken;
        private CancellationTokenSource cts;
        private BitmapImage cam1ImageBox;
        private BitmapImage cam2ImageBox;
        private BitmapImage cam1RoiImageBox;
        private BitmapImage cam2RoiImageBox;
        private BitmapImage cam1RoiLastThrowImageBox;
        private BitmapImage cam2RoiLastThrowImageBox;
        private BitmapImage dartboardProjectionImageBox;

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

        public BitmapImage Cam1RoiImageBox
        {
            get => cam1RoiImageBox;
            set
            {
                cam1RoiImageBox = value;
                OnPropertyChanged($"{nameof(Cam1RoiImageBox)}");
            }
        }

        public BitmapImage Cam2RoiImageBox
        {
            get => cam2RoiImageBox;
            set
            {
                cam2RoiImageBox = value;
                OnPropertyChanged($"{nameof(Cam2RoiImageBox)}");
            }
        }

        public BitmapImage Cam1RoiLastThrowImageBox
        {
            get => cam1RoiLastThrowImageBox;
            set
            {
                cam1RoiLastThrowImageBox = value;
                OnPropertyChanged($"{nameof(Cam1RoiLastThrowImageBox)}");
            }
        }

        public BitmapImage Cam2RoiLastThrowImageBox
        {
            get => cam2RoiLastThrowImageBox;
            set
            {
                cam2RoiLastThrowImageBox = value;
                OnPropertyChanged($"{nameof(Cam2RoiLastThrowImageBox)}");
            }
        }

        public BitmapImage DartboardProjectionImageBox
        {
            get => dartboardProjectionImageBox;
            set
            {
                dartboardProjectionImageBox = value;
                OnPropertyChanged($"{nameof(DartboardProjectionImageBox)}");
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
            drawman = new Drawman();
            cts = new CancellationTokenSource();
            cancelToken = cts.Token;
            throwService = new ThrowService(view, drawman, cancelToken, this);
            measureman1 = new Measureman(view, drawman, throwService);
            measureman2 = new Measureman(view, drawman, throwService);
            Cam1ImageBox = new BitmapImage();
            Cam2ImageBox = new BitmapImage();
            Cam1RoiImageBox = new BitmapImage();
            Cam2RoiImageBox = new BitmapImage();
            Cam1RoiLastThrowImageBox = new BitmapImage();
            Cam2RoiLastThrowImageBox = new BitmapImage();

            var dartboardProjectionImage = throwService.PrepareDartboardProjectionImage();
            view.Dispatcher.Invoke(new Action(() => DartboardProjectionImageBox = drawman.ConvertToBitmap(dartboardProjectionImage)));

            var cam1Task = new Task(() => CaptureImage(1, measureman1));
            var cam2Task = new Task(() => CaptureImage(2, measureman2));
            var tsTask = new Task(() => throwService.AwaitForThrow(cancelToken));

            cam1Task.Start();
            cam2Task.Start();
            tsTask.Start();
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

        private void CaptureImage(int camNumber, Measureman measureman)
        {
            Cam cam;
            cam = camNumber == 1
                      ? (Cam) new Cam1(view)
                      : new Cam2(view);
            cam.originFrame = cam.videoCapture.QueryFrame().ToImage<Bgr, byte>();
            cam.RefreshLines(view);
            measureman.SetupWorkingCam(cam);
            measureman.CalculateSetupLines();
            measureman.CalculateRoiRegion();
            drawman.TresholdRoiRegion(cam);

            while (!cancelToken.IsCancellationRequested)
            {
                using (cam.originFrame)
                {
                    if (cam.originFrame == null)
                    {
                        return;
                    }

                    var throwDetected = measureman.DetectThrow();
                    if (throwDetected)
                    {
                        var dartContourFound = measureman.FindDartContour();
                        if (dartContourFound)
                        {
                            measureman.ProcessDartContour();
                            RefreshImageBoxes(cam);
                        }
                    }

                    // cam.originFrame = cam.videoCapture.QueryFrame().ToImage<Bgr, byte>();
                    // cam.RefreshLines(view);
                    // measureman.CalculateSetupLines();
                    // measureman.CalculateRoiRegion();
                    // drawman.TresholdRoiRegion(cam);
                    //
                    // RefreshImageBoxes(cam);
                }
            }

            ClearImageBoxes(cam);
        }

        private void ClearImageBoxes(Cam cam)
        {
            cam.videoCapture.Dispose();
            if (cam is Cam1)
            {
                view.Dispatcher.Invoke(new Action(() => Cam1ImageBox = new BitmapImage()));
                view.Dispatcher.Invoke(new Action(() => Cam1RoiImageBox = new BitmapImage()));
                view.Dispatcher.Invoke(new Action(() => Cam1RoiLastThrowImageBox = new BitmapImage()));
            }
            else
            {
                view.Dispatcher.Invoke(new Action(() => Cam2ImageBox = new BitmapImage()));
                view.Dispatcher.Invoke(new Action(() => Cam2RoiImageBox = new BitmapImage()));
                view.Dispatcher.Invoke(new Action(() => Cam2RoiLastThrowImageBox = new BitmapImage()));
            }

            view.Dispatcher.Invoke(new Action(() => DartboardProjectionImageBox = new BitmapImage()));
        }

        private void RefreshImageBoxes(Cam cam)
        {
            if (cam is Cam1)
            {
                view.Dispatcher.Invoke(new Action(() => Cam1ImageBox = drawman.ConvertToBitmap(cam.linedFrame)));
                view.Dispatcher.Invoke(new Action(() => Cam1RoiImageBox = drawman.ConvertToBitmap(cam.roiTrasholdFrame)));
                view.Dispatcher.Invoke(new Action(() => Cam1RoiLastThrowImageBox = cam.roiTrasholdFrameLastThrow != null
                                                                                            ? drawman.ConvertToBitmap(cam.roiTrasholdFrameLastThrow)
                                                                                            : new BitmapImage()));
            }
            else
            {
                view.Dispatcher.Invoke(new Action(() => Cam2ImageBox = drawman.ConvertToBitmap(cam.linedFrame)));
                view.Dispatcher.Invoke(new Action(() => Cam2RoiImageBox = drawman.ConvertToBitmap(cam.roiTrasholdFrame)));
                view.Dispatcher.Invoke(new Action(() => Cam2RoiLastThrowImageBox = cam.roiTrasholdFrameLastThrow != null
                                                                                            ? drawman.ConvertToBitmap(cam.roiTrasholdFrameLastThrow)
                                                                                            : new BitmapImage()));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}