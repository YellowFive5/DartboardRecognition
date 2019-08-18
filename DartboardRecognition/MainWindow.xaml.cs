#region Usings

using System.ComponentModel;
using System.Drawing;
using System.Windows;
using Emgu.CV.Structure;

#endregion

namespace DartboardRecognition
{
    public partial class MainWindow
    {
        private MainWindowViewModel viewModel;
        public MCvScalar CamContourColor { get; } = new Bgr(System.Drawing.Color.Violet).MCvScalar;
        public int CamContourThickness { get; } = 2;
        public MCvScalar ProjectionGridColor { get; } = new Bgr(System.Drawing.Color.DarkGray).MCvScalar;
        public int ProjectionFrameHeight { get; } = 1200;
        public int ProjectionFrameWidth { get; } = 1200;
        public PointF Cam1SetupPoint { get; } = new PointF(13, 4);
        public PointF Cam2SetupPoint { get; } = new PointF(1200 - 13, 4);
        public int ProjectionCoefficent { get; } = 3;
        public int ProjectionGridThickness { get; } = 2;
        public MCvScalar ProjectionSurfaceLineColor { get; } = new Bgr(System.Drawing.Color.Red).MCvScalar;
        public int ProjectionSurfaceLineThickness { get; } = 2;
        public MCvScalar ProjectionRayColor { get; } = new Bgr(System.Drawing.Color.White).MCvScalar;
        public int ProjectionRayThickness { get; } = 2;
        public MCvScalar PoiColor { get; } = new Bgr(System.Drawing.Color.Magenta).MCvScalar;
        public int PoiRadius { get; } = 6;
        public int PoiThickness { get; } = 6;
        public Bgr ProjectionDigitsColor { get; } = new Bgr(System.Drawing.Color.White);
        public double ProjectionDigitsScale { get; } = 2;
        public int ProjectionDigitsThickness { get; } = 2;
        public int ProjectionLineCam1Bias { get; } = 0;
        public int ProjectionLineCam2Bias { get; } = 0;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new MainWindowViewModel(this);
            DataContext = viewModel;
        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            viewModel.OnStartButtonClicked();
        }

        private void StopButtonClick(object sender, RoutedEventArgs e)
        {
            viewModel.OnStopButtonClicked();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            viewModel.OnStopButtonClicked();
        }
    }
}