#region Usings

using System.Threading;
using System.Threading.Tasks;
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
            var withDetection = mainWindowView.WithDetectionCapturingCheckBox.IsChecked.Value;

            StartCam(1, runtimeCapturing, withDetection, settingsLock);
            StartCam(2, runtimeCapturing, withDetection, settingsLock);
            StartCam(3, runtimeCapturing, withDetection, settingsLock);
            StartCam(4, runtimeCapturing, withDetection, settingsLock);
            StartThrowService();
        }

        private void StopCapturing()
        {
            cts?.Cancel();
            mainWindowView.DartboardProjectionImageBox.Source = new BitmapImage();
        }

        private void StartThrowService()
        {
            Task.Run(() =>
                     {
                         Thread.CurrentThread.Name = $"ThrowService_workerThread";
                         throwService.AwaitForThrow(cancelToken);
                     });
        }

        private void StartCam(int camNumber, bool runtimeCapturing, bool withDetection, object settingsLock)
        {
            var camWindow = new CamWindow(camNumber, drawman, throwService, cancelToken, settingsLock);
            camWindow.Run(runtimeCapturing, withDetection);
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