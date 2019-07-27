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
        private MainWindow view;
        private Dispatcher dispatcher;
        private Drawman drawman;
        private ThrowService throwService;
        private CancellationToken cancelToken;
        private CancellationTokenSource cts;

        public MainWindowViewModel()
        {
        }

        public MainWindowViewModel(MainWindow view)
        {
            this.view = view;
            dispatcher = view.Dispatcher;
            // LoadSettings();
        }

        private void StartCapture()
        {
            drawman = new Drawman();
            cts = new CancellationTokenSource();
            cancelToken = cts.Token;
            throwService = new ThrowService(view, drawman);

            var dartboardProjectionImage = throwService.PrepareDartboardProjectionImage();
            view.DartboardProjectionImageBox.Source = drawman.ConvertToBitmap(dartboardProjectionImage);

            Task.Factory.StartNew(() => BeginCapturing(1));
            Task.Factory.StartNew(() => BeginCapturing(2));
            Task.Factory.StartNew(() => throwService.AwaitForThrow(cancelToken));
        }

        private void StopCapture()
        {
            cts.Cancel();
            view.DartboardProjectionImageBox.Source = new BitmapImage();
        }

        private void BeginCapturing(int camNumber)
        {
            var measureman = new Measureman(view, drawman, throwService);
            CamWindow camWindow = null;
            Cam cam;

            dispatcher.Invoke(() =>
                              {
                                  camWindow = new CamWindow(camNumber);
                                  camWindow.Show();
                              });

            cam = camNumber == 1
                      ? (Cam) new Cam1(camWindow)
                      : new Cam2(camWindow);
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
            StartCapture();
        }

        public void OnStopButtonClicked()
        {
            ToggleViewControls();
            view.PointsBox.Text = "";
            StopCapture();
        }

        private void ToggleViewControls()
        {
            view.StartButton.IsEnabled = !view.StartButton.IsEnabled;
            view.StopButton.IsEnabled = !view.StopButton.IsEnabled;
        }
    }
}