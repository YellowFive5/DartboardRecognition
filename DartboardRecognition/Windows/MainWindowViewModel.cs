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
        private CancellationToken cancelToken;
        private CancellationTokenSource cts;

        public MainWindowViewModel()
        {
        }

        public MainWindowViewModel(MainWindow mainWindowView)
        {
            this.mainWindowView = mainWindowView;
        }

        private void StartCapturing()
        {
            drawService = new DrawService();
            cts = new CancellationTokenSource();
            cancelToken = cts.Token;
            throwService = new ThrowService(mainWindowView, drawService);

            var dartboardProjectionImage = throwService.PrepareDartboardProjectionImage();
            mainWindowView.DartboardProjectionImageBox.Source = drawService.ToBitmap(dartboardProjectionImage);

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
                           new CamWindow(1, drawService, throwService, settingsLock, runtimeCapturing, false),
                           new CamWindow(2, drawService, throwService, settingsLock, runtimeCapturing, withDetection),
                           new CamWindow(3, drawService, throwService, settingsLock, runtimeCapturing, withDetection),
                           new CamWindow(4, drawService, throwService, settingsLock, runtimeCapturing, withDetection)
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
                                     RedetectFromAll(cams);
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

        private void RedetectFromAll(List<CamWindow> cams)
        {
            foreach (var cam in cams)
            {
                cam.DoCaptures();
                cam.RefreshImages();
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