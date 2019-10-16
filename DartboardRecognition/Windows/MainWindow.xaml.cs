﻿#region Usings

using System.ComponentModel;
using System.Windows;
using Autofac;
using DartboardRecognition.Services;
using IContainer = Autofac.IContainer;

#endregion

namespace DartboardRecognition.Windows
{
    public partial class MainWindow
    {
        private readonly MainWindowViewModel viewModel;
        public static IContainer ServiceContainer { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            RegisterContainer();
            viewModel = new MainWindowViewModel(this);
            DataContext = viewModel;
        }

        private void RegisterContainer()
        {
            var cb = new ContainerBuilder();

            cb.RegisterType<ConfigService>().AsSelf().SingleInstance();

            var drawService = new DrawService(this);
            cb.Register(r => drawService).AsSelf().SingleInstance();

            var throwService = new ThrowService(drawService);
            cb.Register(r => throwService).AsSelf().SingleInstance();

            ServiceContainer = cb.Build();
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