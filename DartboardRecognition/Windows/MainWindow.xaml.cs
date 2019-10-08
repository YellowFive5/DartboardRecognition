#region Usings

using System.ComponentModel;
using System.Windows;

#endregion

namespace DartboardRecognition.Windows
{
    public partial class MainWindow
    {
        private readonly MainWindowViewModel viewModel;

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