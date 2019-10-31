#region Usings

using System.ComponentModel;
using Autofac;
using DartboardRecognition.Services;
using NLog;

#endregion

namespace DartboardRecognition.Windows
{
    public partial class CamWindow
    {
        public readonly int camNumber;
        private readonly CamWindowViewModel viewModel;
        private readonly Logger logger;

        public CamWindow(int camNumber,
                         bool runtimeCapturing,
                         bool withDetection,
                         bool withSetupSliders)
        {
            InitializeComponent();
            logger = MainWindow.ServiceContainer.Resolve<Logger>();

            if (withSetupSliders)
            {
                Height = 640;
            }

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

        public ResponseType DetectMove()
        {
            return viewModel.DetectMove();
        }

        public ResponseType DetectThrow()
        {
            return viewModel.DetectThrow();
        }

        public void FindThrow()
        {
            viewModel.FindThrow();
        }

        public void FindAndProcessDartContour()
        {
            viewModel.FindAndProcessDartContour();
        }

        public void ClearImageBoxes()
        {
            viewModel.ClearImageBoxes();
        }
    }
}