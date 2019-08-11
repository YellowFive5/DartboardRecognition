#region Usings

using System.ComponentModel;
using System.Windows;

#endregion

namespace CamCalibrator
{
    public partial class MainWindow
    {
        private MainWindowViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new MainWindowViewModel(this);
            DataContext = viewModel;
        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            viewModel.StartCapture();
        }

        private void StopButtonClick(object sender, RoutedEventArgs e)
        {
            viewModel.StopCapture();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            viewModel.StopCapture();
        }
    }
}