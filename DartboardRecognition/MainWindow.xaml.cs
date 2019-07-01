#region Usings

using System.ComponentModel;
using System.Windows;
using Emgu.CV.Structure;
using Point = System.Drawing.Point;

#endregion

namespace DartboardRecognition
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MainWindowViewModel viewModel;
        public Bgr CamSurfaceLineColor { get; } = new Bgr(System.Drawing.Color.Red);
        public int CamSurfaceLineThickness { get; } = 5;
        public Bgr CamRoiRectColor { get; } = new Bgr(System.Drawing.Color.LawnGreen);
        public int CamRoiRectThickness { get; } = 5;
        public MCvScalar CamContourColor { get; } = new Bgr(System.Drawing.Color.Violet).MCvScalar;
        public int CamContourThickness { get; } = 2;
        public MCvScalar CamContourRectColor { get; } = new Bgr(System.Drawing.Color.Blue).MCvScalar;
        public int CamContourRectThickness { get; } = 5;
        public MCvScalar CamSpikeLineColor { get; } = new Bgr(System.Drawing.Color.White).MCvScalar;
        public int CamSpikeLineThickness { get; } = 4;
        public MCvScalar ProjectionPoiColor { get; } = new Bgr(System.Drawing.Color.Yellow).MCvScalar;
        public int ProjectionPoiRadius { get; } = 6;
        public int ProjectionPoiThickness { get; } = 6;
        public MCvScalar ProjectionGridColor { get; } = new Bgr(System.Drawing.Color.DarkGray).MCvScalar;
        public int ProjectionFrameHeight { get; } = 1200;
        public int ProjectionFrameWidth { get; } = 1200;
        public Point Cam1SetupPoint { get; } = new Point(13, 4);
        public Point Cam2SetupPoint { get; } = new Point(1200 - 13, 4);
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
        public int minContourArcLength { get; } = 190;
        public int projectionLineCam1Bias { get; } = 0;
        public int projectionLineCam2Bias { get; } = 0;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new MainWindowViewModel(this);
            DataContext = viewModel;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            viewModel.SaveSettings();
        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            ToggleControls();
            viewModel.StartCapture();
        }

        private void StopButtonClick(object sender, RoutedEventArgs e)
        {
            ToggleControls();
            viewModel.StopCapture();
            ClearImageBoxes();
        }

        private void ClearImageBoxes()
        {
            // var emptyImage = new BitmapImage();
            // Cam1ImageBox.Source = emptyImage;
            // Cam1ImageBoxRoi.Source = emptyImage;
            // Cam2ImageBox.Source = emptyImage;
            // Cam2ImageBoxRoi.Source = emptyImage;
            // DartboardProjectionImageBox.Source = emptyImage;
            PointsBox.Text = "";
        }

        private void ToggleControls()
        {
            StartButton.IsEnabled = !StartButton.IsEnabled;
            StopButton.IsEnabled = !StopButton.IsEnabled;
            Cam1IndexBox.IsEnabled = !Cam1IndexBox.IsEnabled;
            Cam2IndexBox.IsEnabled = !Cam2IndexBox.IsEnabled;
        }
    }
}