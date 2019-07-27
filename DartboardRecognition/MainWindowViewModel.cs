#region Usings

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Emgu.CV.Structure;

#endregion

namespace DartboardRecognition
{
    public class MainWindowViewModel
    {
        private MainWindow mainWindowView;
        private Dispatcher dispatcher;
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
            dispatcher = mainWindowView.Dispatcher;
        }

        private void StartCapturing()
        {
            drawman = new Drawman();
            cts = new CancellationTokenSource();
            cancelToken = cts.Token;
            throwService = new ThrowService(mainWindowView, drawman);

            var dartboardProjectionImage = throwService.PrepareDartboardProjectionImage();
            mainWindowView.DartboardProjectionImageBox.Source = drawman.ConvertToBitmap(dartboardProjectionImage);

            Task.Factory.StartNew(() => BeginCamWork(1));
            Task.Factory.StartNew(() => BeginCamWork(2));
            Task.Factory.StartNew(() => throwService.AwaitForThrow(cancelToken));
        }

        private void StopCapturing()
        {
            cts.Cancel();
            mainWindowView.DartboardProjectionImageBox.Source = new BitmapImage();
        }

        private void BeginCamWork(int camNumber)
        {
            CamWindow camWindow = null;
            Cam cam;

            dispatcher.Invoke(() =>
                              {
                                  camWindow = new CamWindow(camNumber);
                                  camWindow.Show();
                              });

            var measureman = new Measureman(camWindow, drawman, throwService);
            cam = new Cam(camWindow);

            cam.originFrame = cam.videoCapture.QueryFrame().ToImage<Bgr, byte>();
            cam.RefreshLines(camWindow);
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
                            RefreshImageBoxes(cam, camWindow);
                        }
                    }

                    // Runtime image capturing
                    cam.originFrame = cam.videoCapture.QueryFrame().ToImage<Bgr, byte>();
                    cam.RefreshLines(camWindow);
                    measureman.CalculateSetupLines();
                    measureman.CalculateRoiRegion();
                    drawman.TresholdRoiRegion(cam);
                    RefreshImageBoxes(cam, camWindow);
                }
            }

            dispatcher.Invoke(() => camWindow.Close());
            cam.videoCapture.Dispose();
        }

        private void RefreshImageBoxes(Cam cam, CamWindow camWindow)
        {
            dispatcher.Invoke(new Action(() => camWindow.ImageBox.Source = drawman.ConvertToBitmap(cam.linedFrame)));
            dispatcher.Invoke(new Action(() => camWindow.ImageBoxRoi.Source = drawman.ConvertToBitmap(cam.roiTrasholdFrame)));
            dispatcher.Invoke(new Action(() => camWindow.ImageBoxRoiLastThrow.Source = cam.roiTrasholdFrameLastThrow != null
                                                                                           ? drawman.ConvertToBitmap(cam.roiTrasholdFrameLastThrow)
                                                                                           : new BitmapImage()));
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
            mainWindowView.StartButton.IsEnabled = !mainWindowView.StartButton.IsEnabled;
            mainWindowView.StopButton.IsEnabled = !mainWindowView.StopButton.IsEnabled;
        }
    }
}