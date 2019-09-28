#region Usings

using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

#endregion

namespace DartboardRecognition
{
    public class MainWindowViewModel
    {
        private MainWindow mainWindowView;
        private Dispatcher mainWindowDispatcher;
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
            mainWindowDispatcher = mainWindowView.Dispatcher;
        }

        private void StartCapturing()
        {
            drawman = new Drawman();
            cts = new CancellationTokenSource();
            cancelToken = cts.Token;
            throwService = new ThrowService(mainWindowView, drawman);
            var settingsLock = new object();

            var dartboardProjectionImage = throwService.PrepareDartboardProjectionImage();
            mainWindowView.DartboardProjectionImageBox.Source = drawman.ConvertToBitmap(dartboardProjectionImage);

            var runtimeCapturing = mainWindowView.RuntimeCapturingCheckBox.IsChecked.Value;

            StartCam(1, runtimeCapturing, settingsLock);
            StartCam(2, runtimeCapturing, settingsLock);
            StartCam(3, runtimeCapturing, settingsLock);
            StartCam(4, runtimeCapturing, settingsLock);
            // StartThrowService();
        }

        private void StopCapturing()
        {
            cts?.Cancel();
            mainWindowView.DartboardProjectionImageBox.Source = new BitmapImage();
        }

        private void StartThrowService()
        {
            var thread = new Thread(() =>
                                    {
                                        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
                                        mainWindowView.Closed += (s, args) =>
                                                                     Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);

                                        throwService.AwaitForThrow(cancelToken);

                                        Dispatcher.Run();
                                    });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void StartCam(int camNumber, bool runtimeCapturing, object settingsLock)
        {
            var thread = new Thread(() =>
                                    {
                                        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));

                                        var camWindow = new CamWindow(camNumber, drawman, throwService, cancelToken, settingsLock);
                                        camWindow.Closed += (s, args) =>
                                                                Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                                        camWindow.Run(runtimeCapturing);

                                        Dispatcher.Run();
                                    });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
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
            mainWindowView.StartButton.IsEnabled = !mainWindowView.StartButton.IsEnabled;
            mainWindowView.StopButton.IsEnabled = !mainWindowView.StopButton.IsEnabled;
        }
    }
}