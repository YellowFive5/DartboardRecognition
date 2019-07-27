#region Usings

using System.ComponentModel;
using System.Drawing;
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

        public CamWindow(int camNumber)
        {
            InitializeComponent();
            viewModel = new CamWindowViewModel(this, camNumber);
            DataContext = viewModel;
            this.camNumber = camNumber;

            SetupPoint = camNumber == 1
                             ? new Point(13, 4)
                             : new Point(1200 - 13, 4);
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            viewModel.SaveSettings();
        }
    }
}