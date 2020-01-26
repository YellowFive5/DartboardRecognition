#region Usings

using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Autofac;
using DartboardRecognition.Services;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Extensions.Logging;
using IContainer = Autofac.IContainer;

#endregion

namespace DartboardRecognition.Windows
{
    public partial class MainWindow
    {
        private readonly MainWindowViewModel viewModel;
        private Logger logger;
        public static IContainer ServiceContainer { get; private set; }

        public MainWindow()
        {
            ConfigureNlog();

            logger.Info("\n");
            logger.Info("App start");

            InitializeComponent();
            RegisterContainer();
            viewModel = new MainWindowViewModel(this);
            DataContext = viewModel;
            viewModel.LoadSettings();
        }

        private void ConfigureNlog()
        {
            var config = new ConfigurationBuilder()
                         .SetBasePath(Directory.GetCurrentDirectory())
                         .AddJsonFile("appsettings.json", true, true)
                         .Build();
            LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog"));
            logger = LogManager.GetCurrentClassLogger();
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

        private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.SaveSettings();

            if (SettingsTabItem.IsSelected)
            {
                viewModel.LoadSettings();
            }
        }
    }
}