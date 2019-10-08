#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
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
        private CancellationToken cancelToken;
        private CancellationTokenSource cts;
        private List<CamWindow> cams;

        public MainWindowViewModel()
        {
        }

        public MainWindowViewModel(MainWindow mainWindowView)
        {
            this.mainWindowView = mainWindowView;

            ServiceBag.Initialize(mainWindowView);
        }

        private void StartCapturing()
        {
            cts = new CancellationTokenSource();
            cancelToken = cts.Token;

            ServiceBag.All()
                      .DrawService
                      .DrawProjectionImage();

            StartDetection();
        }

        private void StartDetection()
        {
            var runtimeCapturing = mainWindowView.RuntimeCapturingCheckBox.IsChecked.Value;
            var withDetection = mainWindowView.WithDetectionCapturingCheckBox.IsChecked.Value;

            cams = new List<CamWindow>
                   {
                       new CamWindow(1, runtimeCapturing, false),
                       new CamWindow(2, runtimeCapturing, withDetection),
                       new CamWindow(3, runtimeCapturing, withDetection),
                       new CamWindow(4, runtimeCapturing, withDetection)
                   };

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
                                     Thread.Sleep(TimeSpan.FromSeconds(5));
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

            ServiceBag.All().ThrowService.CalculateAndSaveThrow();
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