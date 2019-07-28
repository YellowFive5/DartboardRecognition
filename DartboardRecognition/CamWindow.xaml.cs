﻿#region Usings

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
        public Point SetupPoint { get; }

        public CamWindow(int camNumber, Drawman drawman, ThrowService throwService, CancellationToken cancelToken)
        {
            InitializeComponent();
            this.camNumber = camNumber;
            viewModel = new CamWindowViewModel(this, camNumber, drawman, throwService, cancelToken);
            DataContext = viewModel;

            SetupPoint = camNumber == 1
                             ? new Point(13, 4)
                             : new Point(1200 - 13, 4);
            Show();
            viewModel.SetWindowTitle();
            viewModel.LoadSettings();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            viewModel.SaveSettings();
        }

        public void Run(bool runtimeCapturing)
        {
            var thread = new Thread(() =>
                                    {
                                        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));

                                        viewModel.RunWork(runtimeCapturing);

                                        Dispatcher.Run();
                                    });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
    }
}