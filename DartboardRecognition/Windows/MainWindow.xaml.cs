#region Usings

using System.ComponentModel;
using System.Windows;

#endregion

namespace DartboardRecognition.Windows
{
    public partial class MainWindow
    {
        private readonly MainWindowViewModel viewModel;
        public int ProjectionCoefficent { get; } = 3;
        public int ProjectionLineCam1Bias { get; } = 0;
        public int ProjectionLineCam2Bias { get; } = 0;
        public int ProjectionFrameHeight { get; } = 1200;
        public int ProjectionFrameWidth { get; } = 1200;

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