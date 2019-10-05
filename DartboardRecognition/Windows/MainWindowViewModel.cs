#region Usings

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using DartboardRecognition.Services;

#endregion

namespace DartboardRecognition.Windows
{
    public class MainWindowViewModel
    {
        private readonly MainWindow mainWindowView;
        private DrawService drawService;
        private ThrowService throwService;
        private ConfigService configService;
        private CancellationToken cancelToken;
        private CancellationTokenSource cts;

        public MainWindowViewModel()
        {
        }

        public MainWindowViewModel(MainWindow mainWindowView)
        {
            this.mainWindowView = mainWindowView;
            drawService = new DrawService();
            throwService = new ThrowService(mainWindowView, drawService);
            configService = new ConfigService();
            cts = new CancellationTokenSource();
        }

        private void StartCapturing()
        {
            cancelToken = cts.Token;
            mainWindowView.DartboardProjectionImageBox.Source = throwService.PrepareDartboardProjectionImage();

            StartThrowService();

            StartRecognition();
        }

        private void StartThrowService()
        {
            Task.Run(() =>
                     {
                         Thread.CurrentThread.Name = $"ThrowService_workerThread";
                         throwService.AwaitForThrow(cancelToken);
                     });
        }

        private void StartRecognition()
        {
            var settingsLock = new object();
            var runtimeCapturing = mainWindowView.RuntimeCapturingCheckBox.IsChecked.Value;
            var withDetection = mainWindowView.WithDetectionCapturingCheckBox.IsChecked.Value;

            var cams = new List<CamWindow>
                       {
                           new CamWindow(1, drawService, throwService, configService, runtimeCapturing, false),
                           new CamWindow(2, drawService, throwService, configService, runtimeCapturing, withDetection),
                           new CamWindow(3, drawService, throwService, configService, runtimeCapturing, withDetection),
                           new CamWindow(4, drawService, throwService, configService, runtimeCapturing, withDetection)
                       };

            Task.Run(() =>
                     {
                         Thread.CurrentThread.Name = $"Recognition_workerThread";

                         while (!cancelToken.IsCancellationRequested)
                         {
                             foreach (var cam in cams)
                             {
                                 var throwDetected = cam.DetectThrow();
                                 if (throwDetected)
                                 {
                                     RedetectAllCams(cams);
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

        private void RedetectAllCams(IEnumerable<CamWindow> cams)
        {
            foreach (var cam in cams)
            {
                // cam.DetectThrow();
                // cam.FindDart();
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
            mainWindowView.WithDetectionCapturingCheckBox.IsEnabled = !mainWindowView.WithDetectionCapturingCheckBox.IsEnabled;
            mainWindowView.StartButton.IsEnabled = !mainWindowView.StartButton.IsEnabled;
            mainWindowView.StopButton.IsEnabled = !mainWindowView.StopButton.IsEnabled;
        }
    }
}