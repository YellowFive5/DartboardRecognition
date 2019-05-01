#region Usings

using System.ComponentModel;
using System.Windows;
using Emgu.CV.Structure;

#endregion

namespace DartboardRecognition
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MainWindowViewModel viewModel;
        public Bgr SurfaceLineColor { get; } = new Bgr(System.Drawing.Color.Red);
        public int SurfaceLineThickness { get; } = 5;
        public Bgr RoiRectColor { get; } = new Bgr(System.Drawing.Color.LawnGreen);
        public int RoiRectThickness { get; } = 5;
        public MCvScalar ContourColor { get; } = new Bgr(System.Drawing.Color.Violet).MCvScalar;
        public int CountourThickness { get; } = 2;
        public MCvScalar ContourRectColor { get; } = new Bgr(System.Drawing.Color.Blue).MCvScalar;
        public int ContourRectThickness { get; } = 5;
        public MCvScalar SpikeLineColor { get; } = new Bgr(System.Drawing.Color.White).MCvScalar;
        public int SpikeLineThickness { get; } = 4;
        public MCvScalar PointOfImpactColor { get; } = new Bgr(System.Drawing.Color.Yellow).MCvScalar;
        public int PointOfImpactRadius { get; } = 6;
        public int PointOfImpactThickness { get; } = 6;
        public MCvScalar DartboardProjectionColor { get; } = new Bgr(System.Drawing.Color.White).MCvScalar;
        public int DartboardProjectionFrameHeight { get; } = 1200;
        public int DartboardProjectionFrameWidth { get; } = 1200;
        public int DartboardProjectionCoefficent { get; } = 3;
        public int DartboardProjectionThickness { get; } = 2;
        public MCvScalar SurfaceProjectionLineColor { get; } = new Bgr(System.Drawing.Color.Red).MCvScalar;
        public int SurfaceProjectionLineThickness { get; } = 2;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new MainWindowViewModel(this);
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
            ImageBox1.Source = null;
            ImageBox1Roi.Source = null;
            ImageBox2.Source = null;
            ImageBox2Roi.Source = null;
            ImageBox3.Source = null;
        }

        private void ToggleControls()
        {
            StartButton.IsEnabled = !StartButton.IsEnabled;
            StopButton.IsEnabled = !StopButton.IsEnabled;
            Cam1IndexBox.IsEnabled = !Cam1IndexBox.IsEnabled;
            Cam2IndexBox.IsEnabled = !Cam2IndexBox.IsEnabled;
            Throw1RadioButton.IsEnabled = !Throw1RadioButton.IsEnabled;
            Throw2RadioButton.IsEnabled = !Throw2RadioButton.IsEnabled;
            Throw3RadioButton.IsEnabled = !Throw3RadioButton.IsEnabled;
            UseCamsRadioButton.IsEnabled = !UseCamsRadioButton.IsEnabled;
        }
    }
}