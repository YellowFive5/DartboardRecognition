#region Usings

using System;
using DartboardRecognition.Windows;

#endregion

namespace DartboardRecognition.Services
{
    public class ServiceBag
    {
        public ConfigService ConfigService { get; }
        public DrawService DrawService { get; }
        public ThrowService ThrowService { get; }
        public MainWindow MainWindow { get; }

        private static ServiceBag services;

        private ServiceBag(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
            ConfigService = new ConfigService();
            DrawService = new DrawService();
            ThrowService = new ThrowService();
        }

        public static void Initialize(MainWindow mainWindow)
        {
            if (services == null)
            {
                services = new ServiceBag(mainWindow);
            }
        }

        public static ServiceBag All()
        {
            if (services == null)
            {
                throw new Exception($"{nameof(ServiceBag)} is not initialized yet");
            }

            return services;
        }
    }
}