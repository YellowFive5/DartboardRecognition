#region Usings

using System.ComponentModel;
using DartboardRecognition.Services;

#endregion

namespace DartboardRecognition.Windows
{
    public partial class CamWindow
    {
        public readonly int camNumber;
        private readonly CamWindowViewModel viewModel;
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