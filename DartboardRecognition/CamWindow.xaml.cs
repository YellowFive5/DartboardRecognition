#region Usings

using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Threading;
using Emgu.CV.Structure;

#endregion

namespace DartboardRecognition
{
    public partial class CamWindow
    {
        public int camNumber;
        private CamWindowViewModel viewModel;
        private object settingsLock;

        public Bgr CamRoiRectColor { get; } = new Bgr(Color.LawnGreen);
        public int CamRoiRectThickness { get; } = 5;
        public Bgr CamSurfaceLineColor { get; } = new Bgr(Color.Red);
        public int CamSurfaceLineThickness { get; } = 5;
        public MCvScalar CamContourRectColor { get; } = new Bgr(Color.Blue).MCvScalar;
        public int CamContourRectThickness { get; } = 5;
        public MCvScalar CamSpikeLineColor { get; } = new Bgr(Color.White).MCvScalar;
        public int CamSpikeLineThickness { get; } = 4;
        public int minContourArcLength { get; } = 190;
        public int ProjectionPoiRadius { get; } = 6;
        public MCvScalar ProjectionPoiColor { get; } = new Bgr(Color.Yellow).MCvScalar;
        public int ProjectionPoiThickness { get; } = 6;

        public CamWindow(int camNumber, Drawman drawman, ThrowService throwService, CancellationToken cancelToken, object settingsLock)
        {
            InitializeComponent();
            this.camNumber = camNumber;
            this.settingsLock = settingsLock;
            viewModel = new CamWindowViewModel(this, camNumber, drawman, throwService, cancelToken);
            DataContext = viewModel;

            Show();
            viewModel.SetWindowTitle();
            viewModel.LoadSettings();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            viewModel.SaveSettings(settingsLock);
        }

        public void Run(bool runtimeCapturing)
        {
            var thread = new Thread(() =>
                                    {
                                        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
                                        Closed += (s, args) =>
                                                      Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);

                                        viewModel.RunWork(runtimeCapturing);

                                        Dispatcher.Run();
                                    });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
    }
}