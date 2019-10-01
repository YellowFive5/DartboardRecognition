#region Usings

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

#endregion

namespace DartboardRecognition
{
    public class MainWindowViewModel
    {
        private readonly MainWindow mainWindowView;
        private Drawman drawman;
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
            drawman = new Drawman();
            cts = new CancellationTokenSource();
            cancelToken = cts.Token;
            throwService = new ThrowService(mainWindowView, drawman);

            var dartboardProjectionImage = throwService.PrepareDartboardProjectionImage();
            mainWindowView.DartboardProjectionImageBox.Source = drawman.ToBitmap(dartboardProjectionImage);

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
                           new CamWindow(1, drawman, throwService, settingsLock, runtimeCapturing, withDetection),
                           new CamWindow(2, drawman, throwService, settingsLock, runtimeCapturing, withDetection),
                           new CamWindow(3, drawman, throwService, settingsLock, runtimeCapturing, withDetection),
                           new CamWindow(4, drawman, throwService, settingsLock, runtimeCapturing, withDetection)
                       };

            Task.Run(() =>
                     {
                         Thread.CurrentThread.Name = $"Recognition_workerThread";

                         while (!cancelToken.IsCancellationRequested)
                         {
                             foreach (var cam in cams)
                             {
                                 if (cam.DetectThrow())
                                 {
                                     RedetectAll(cams);
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

        private void RedetectAll(List<CamWindow> cams)
        {
            foreach (var cam in cams)
            {
                cam.FindContour();
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