#region Usings

using System.ComponentModel;
using System.Drawing;
using DartboardRecognition.Services;
using Emgu.CV.Structure;

#endregion

namespace DartboardRecognition.Windows
{
    public partial class CamWindow
    {
        public readonly int camNumber;
        private readonly CamWindowViewModel viewModel;

        public Bgr CamRoiRectColor { get; } = new Bgr(Color.LawnGreen);
        public int CamRoiRectThickness { get; } = 5;
        public Bgr CamSurfaceLineColor { get; } = new Bgr(Color.Red);
        public int CamSurfaceLineThickness { get; } = 5;
        public MCvScalar CamContourRectColor { get; } = new Bgr(Color.Blue).MCvScalar;
        public int CamContourRectThickness { get; } = 5;
        public MCvScalar CamSpikeLineColor { get; } = new Bgr(Color.White).MCvScalar;
        public int CamSpikeLineThickness { get; } = 4;
        public MCvScalar ProjectionPoiColor { get; } = new Bgr(Color.Yellow).MCvScalar;
        public int ProjectionPoiRadius { get; } = 6;
        public int ProjectionPoiThickness { get; } = 6;
        public int MinContourArcLength { get; } = 190;

        public CamWindow(int camNumber,
                         bool runtimeCapturing,
                         bool withDetection)
        {
            InitializeComponent();

            this.camNumber = camNumber;
            Title = $"Cam {camNumber.ToString()}";

            viewModel = new CamWindowViewModel(this, runtimeCapturing, withDetection);
            DataContext = viewModel;

            viewModel.LoadSettings();

            Show();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            viewModel.OnClosing();
            viewModel.SaveSettings();
        }

        public ResponseType Detect()
        {
            return viewModel.Detect();
        }

        public void ProcessContour()
        {
            viewModel.ProcessContour();
        }

        public void FindThrow()
        {
            viewModel.FindThrow();
        }
    }
}