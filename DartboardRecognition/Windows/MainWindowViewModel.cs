#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Autofac;
using DartboardRecognition.Services;

#endregion

namespace DartboardRecognition.Windows
{
    public class MainWindowViewModel
    {
        private readonly MainWindow mainWindowView;
        private CancellationToken cancelToken;
        private CancellationTokenSource cts;
        private List<CamWindow> cams;
        private readonly DrawService drawService;
        private readonly ThrowService throwService;
        private readonly ConfigService configService;

        public MainWindowViewModel()
        {
        }

        public MainWindowViewModel(MainWindow mainWindowView)
        {
            this.mainWindowView = mainWindowView;
            drawService = MainWindow.ServiceContainer.Resolve<DrawService>();
            throwService = MainWindow.ServiceContainer.Resolve<ThrowService>();
            configService = MainWindow.ServiceContainer.Resolve<ConfigService>();
            drawService.DrawProjection();
        }

        private void StartCapturing()
        {
            drawService.DrawProjection();
            cts = new CancellationTokenSource();
            cancelToken = cts.Token;
            StartDetection();
        }

        private void StartDetection()
        {
            var runtimeCapturing = mainWindowView.RuntimeCapturingCheckBox.IsChecked.Value;
            var withDetection = mainWindowView.WithDetectionCheckBox.IsChecked.Value;
            var withSetupSliders = mainWindowView.SetupSlidersCheckBox.IsChecked.Value;

            cams = new List<CamWindow>();
            if (mainWindowView.Cam1CheckBox.IsChecked.Value)
            {
                cams.Add(new CamWindow(1, runtimeCapturing, withDetection, withSetupSliders));
            }

            if (mainWindowView.Cam2CheckBox.IsChecked.Value)
            {
                cams.Add(new CamWindow(2, runtimeCapturing, withDetection, withSetupSliders));
            }

            if (mainWindowView.Cam3CheckBox.IsChecked.Value)
            {
                cams.Add(new CamWindow(3, runtimeCapturing, withDetection, withSetupSliders));
            }

            if (mainWindowView.Cam4CheckBox.IsChecked.Value)
            {
                cams.Add(new CamWindow(4, runtimeCapturing, withDetection, withSetupSliders));
            }

            DoCaptures();

            Task.Run(() =>
                     {
                         Thread.CurrentThread.Name = $"Recognition_workerThread";

                         while (!cancelToken.IsCancellationRequested)
                         {
                             foreach (var cam in cams)
                             {
                                 var response = cam.Detect();

                                 if (response == ResponseType.Trow)
                                 {
                                     cam.ProcessContour();
                                     FindThrowFromRemainingCams(cam);
                                     break;
                                 }

                                 if (response == ResponseType.Extraction)
                                 {
                                     var now = DateTime.Now;
                                     while (DateTime.Now - now < TimeSpan.FromSeconds(5))
                                     {
                                     }

                                     DoCaptures();
                                     break;
                                 }
                             }
                         }

                         foreach (var cam in cams)
                         {
                             cam.Dispatcher.Invoke(() => cam.Close());
                         }
                     });
        }

        private void FindThrowFromRemainingCams(CamWindow succeededCam)
        {
            foreach (var cam in cams.Where(cam => cam != succeededCam))
            {
                cam.FindThrow();
                cam.ProcessContour();
            }

            throwService.CalculateAndSaveThrow();
        }

        private void DoCaptures()
        {
            foreach (var cam in cams)
            {
                cam.DoCapture();
            }
        }

        private void StopCapturing()
        {
            cts?.Cancel();
            mainWindowView.DartboardProjectionImageBox.Source = new BitmapImage();
        }

        public void OnStartButtonClicked()
        {
            ToggleViewControls();
            StartCapturing();
        }

        public void OnStopButtonClicked()
        {
            mainWindowView.PointsBox.Text = "";
            ToggleViewControls();
            StopCapturing();
        }

        private void ToggleViewControls()
        {
            mainWindowView.RuntimeCapturingCheckBox.IsEnabled = !mainWindowView.RuntimeCapturingCheckBox.IsEnabled;
            mainWindowView.WithDetectionCheckBox.IsEnabled = !mainWindowView.WithDetectionCheckBox.IsEnabled;
            mainWindowView.StartButton.IsEnabled = !mainWindowView.StartButton.IsEnabled;
            mainWindowView.StopButton.IsEnabled = !mainWindowView.StopButton.IsEnabled;
            mainWindowView.SetupSlidersCheckBox.IsEnabled = !mainWindowView.SetupSlidersCheckBox.IsEnabled;
            mainWindowView.Cam1CheckBox.IsEnabled = !mainWindowView.Cam1CheckBox.IsEnabled;
            mainWindowView.Cam2CheckBox.IsEnabled = !mainWindowView.Cam2CheckBox.IsEnabled;
            mainWindowView.Cam3CheckBox.IsEnabled = !mainWindowView.Cam3CheckBox.IsEnabled;
            mainWindowView.Cam4CheckBox.IsEnabled = !mainWindowView.Cam4CheckBox.IsEnabled;
        }

        public void LoadSettings()
        {
            mainWindowView.RuntimeCapturingCheckBox.IsChecked = configService.Read<bool>("RuntimeCapturingCheckBox");
            mainWindowView.WithDetectionCheckBox.IsChecked = configService.Read<bool>("WithDetectionCheckBox");
            mainWindowView.SetupSlidersCheckBox.IsChecked = configService.Read<bool>("SetupSlidersCheckBox"); ;
            mainWindowView.Cam1CheckBox.IsChecked = configService.Read<bool>("Cam1CheckBox"); ;
            mainWindowView.Cam2CheckBox.IsChecked = configService.Read<bool>("Cam2CheckBox"); ;
            mainWindowView.Cam3CheckBox.IsChecked = configService.Read<bool>("Cam3CheckBox"); ;
            mainWindowView.Cam4CheckBox.IsChecked = configService.Read<bool>("Cam4CheckBox"); ;
        }

        public void SaveSettings()
        {
            configService.Write("RuntimeCapturingCheckBox", mainWindowView.RuntimeCapturingCheckBox.IsChecked.Value);
            configService.Write("WithDetectionCheckBox", mainWindowView.WithDetectionCheckBox.IsChecked.Value);
            configService.Write("SetupSlidersCheckBox", mainWindowView.SetupSlidersCheckBox.IsChecked.Value);
            configService.Write("Cam1CheckBox", mainWindowView.Cam1CheckBox.IsChecked.Value);
            configService.Write("Cam2CheckBox", mainWindowView.Cam2CheckBox.IsChecked.Value);
            configService.Write("Cam3CheckBox", mainWindowView.Cam3CheckBox.IsChecked.Value);
            configService.Write("Cam4CheckBox", mainWindowView.Cam4CheckBox.IsChecked.Value);
        }
    }
}