#region Usings

using System.ComponentModel;
using System.Windows;
using Autofac;
using DartboardRecognition.Services;
using NLog;
using IContainer = Autofac.IContainer;

#endregion

namespace DartboardRecognition.Windows
{
    public partial class MainWindow
    {
        private readonly MainWindowViewModel viewModel;
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        public static IContainer ServiceContainer { get; private set; }

        public MainWindow()
        {
            logger.Info("\n");
            logger.Info("App start");

            InitializeComponent();
            RegisterContainer();
            viewModel = new MainWindowViewModel(this);
            DataContext = viewModel;
            viewModel.LoadSettings();
        }

        private void RegisterContainer()
        {
            logger.Debug("Services container register start");

            var cb = new ContainerBuilder();

            cb.Register(r => logger).AsSelf().SingleInstance();

            var configService = new ConfigService(logger);
            cb.Register(r => configService).AsSelf().SingleInstance();

            var drawService = new DrawService(this, configService, logger);
            cb.Register(r => drawService).AsSelf().SingleInstance();

            var throwService = new ThrowService(drawService, logger);
            cb.Register(r => throwService).AsSelf().SingleInstance();

            ServiceContainer = cb.Build();

            logger.Debug("Services container register end");
        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            logger.Debug("MainWindow start button clicked");

            viewModel.OnStartButtonClicked();
        }

        private void StopButtonClick(object sender, RoutedEventArgs e)
        {
            logger.Debug("MainWindow stop button clicked");

            viewModel.OnStopButtonClicked();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            logger.Debug("MainWindow on closing");

            viewModel.OnStopButtonClicked();
            viewModel.SaveSettings();
        }

        private void SaveSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            logger.Debug("MainWindow saveSettings button clicked");

            viewModel.SaveSettings();
        }
    }
}