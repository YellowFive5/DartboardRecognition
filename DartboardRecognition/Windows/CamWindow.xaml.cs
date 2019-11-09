#region Usings

using System.ComponentModel;
using System.Windows;
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
        private readonly ConfigService configService;

        public CamWindow(int camNumber)
        {
            InitializeComponent();
            logger = MainWindow.ServiceContainer.Resolve<Logger>();
            configService = MainWindow.ServiceContainer.Resolve<ConfigService>();

            var withSetupSliders = configService.Read<bool>("SetupSlidersCheckBox");
            if (!withSetupSliders)
            {
                Height = 464;
            }

            this.camNumber = camNumber;
            Title = $"Cam {camNumber.ToString()}";

            viewModel = new CamWindowViewModel(this);
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

        private void CalibrateCamSetupPoint(object sender, RoutedEventArgs e)
        {
            viewModel.CalibrateCamSetupPoint();
        }
    }
}