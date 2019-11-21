#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Autofac;
using DartboardRecognition.Services;
using NLog;

#endregion

namespace DartboardRecognition.Windows
{
    public class MainWindowViewModel
    {
        private readonly MainWindow mainWindowView;
        private readonly Logger logger;
        private CancellationToken cancelToken;
        private CancellationTokenSource cts;
        private List<CamWindow> cams;
        private readonly DrawService drawService;
        private readonly ThrowService throwService;
        private readonly ConfigService configService;
        private double extractionSleepTime;
        private double thresholdSleepTime;
        private double moveDetectedSleepTime;

        public MainWindowViewModel()
        {
        }

        public MainWindowViewModel(MainWindow mainWindowView)
        {
            this.mainWindowView = mainWindowView;
            drawService = MainWindow.ServiceContainer.Resolve<DrawService>();
            throwService = MainWindow.ServiceContainer.Resolve<ThrowService>();
            configService = MainWindow.ServiceContainer.Resolve<ConfigService>();
            logger = MainWindow.ServiceContainer.Resolve<Logger>();
            drawService.ProjectionPrepare();
        }

        private void StartDetection()
        {
            extractionSleepTime = configService.Read<double>("ExtractionSleepTime");
            thresholdSleepTime = configService.Read<double>("ThresholdSleepTime");
            moveDetectedSleepTime = configService.Read<double>("MoveDetectedSleepTime");
            var runtimeCapturing = mainWindowView.RuntimeCapturingCheckBox.IsChecked.Value;
            drawService.ProjectionClear();
            cts = new CancellationTokenSource();
            cancelToken = cts.Token;

            cams = new List<CamWindow>();
            if (mainWindowView.Cam1CheckBox.IsChecked.Value)
            {
                cams.Add(new CamWindow(1));
            }

            if (mainWindowView.Cam2CheckBox.IsChecked.Value)
            {
                cams.Add(new CamWindow(2));
            }

            if (mainWindowView.Cam3CheckBox.IsChecked.Value)
            {
                cams.Add(new CamWindow(3));
            }

            if (mainWindowView.Cam4CheckBox.IsChecked.Value)
            {
                cams.Add(new CamWindow(4));
            }

            logger.Info($"Detection for {cams.Count} cams start");

            Task.Run(() =>
                     {
                         Thread.CurrentThread.Name = $"Recognition_workerThread";

                         ClearAllCamsImageBoxes();

                         while (!cancelToken.IsCancellationRequested)
                         {
                             foreach (var cam in cams)
                             {
                                 logger.Debug($"Cam_{cam.camNumber} detection start");

                                 var response = cam.DetectMove();

                                 if (response == ResponseType.Move)
                                 {
                                     Thread.Sleep(TimeSpan.FromSeconds(moveDetectedSleepTime));
                                     response = cam.DetectThrow();

                                     if (response == ResponseType.Trow)
                                     {
                                         cam.FindAndProcessDartContour();

                                         FindThrowOnRemainingCams(cam);

                                         logger.Debug($"Cam_{cam.camNumber} detection end with response type '{ResponseType.Trow}'. Cycle break");
                                         break;
                                     }

                                     if (response == ResponseType.Extraction)
                                     {
                                         Thread.Sleep(TimeSpan.FromSeconds(extractionSleepTime));

                                         drawService.ProjectionClear();
                                         ClearAllCamsImageBoxes();

                                         logger.Debug($"Cam_{cam.camNumber} detection end with response type '{ResponseType.Extraction}'. Cycle break");
                                         break;
                                     }
                                 }

                                 if (!runtimeCapturing)
                                 {
                                     Thread.Sleep(TimeSpan.FromSeconds(thresholdSleepTime));
                                 }

                                 logger.Debug($"Cam_{cam.camNumber} detection end with response type '{ResponseType.Nothing}'");
                             }
                         }

                         logger.Info($"Detection for {cams.Count} cams end. Cancellation requested");

                         foreach (var cam in cams)
                         {
                             cam.Dispatcher.Invoke(() => cam.Close());
                         }
                     });
        }

        private void FindThrowOnRemainingCams(CamWindow succeededCam)
        {
            logger.Info($"Finding throws from remaining cams start. Succeeded cam: {succeededCam.camNumber}");

            foreach (var cam in cams.Where(cam => cam != succeededCam))
            {
                cam.FindThrow();
                cam.FindAndProcessDartContour();
            }

            throwService.CalculateAndSaveThrow();

            logger.Info($"Finding throws from remaining cams end");
        }

        private void ClearAllCamsImageBoxes()
        {
            logger.Debug($"Clear all cams imageboxes start");

            foreach (var cam in cams)
            {
                cam.ClearImageBoxes();
            }

            logger.Debug($"Clear all cams imageboxes end");
        }

        private void StopCapturing()
        {
            cts?.Cancel();
            mainWindowView.DartboardProjectionImageBox.Source = new BitmapImage();
        }

        public void OnStartButtonClicked()
        {
            ToggleViewControls();
            StartDetection();
        }

        public void OnStopButtonClicked()
        {
            mainWindowView.PointsBox.Text = "";
            ToggleViewControls();
            StopCapturing();
        }

        private void ToggleViewControls()
        {
            foreach (TabItem tabItem in mainWindowView.TabControl.Items)
            {
                tabItem.IsEnabled = !tabItem.IsEnabled;
            }

            mainWindowView.StartButton.IsEnabled = !mainWindowView.StartButton.IsEnabled;
            mainWindowView.StopButton.IsEnabled = !mainWindowView.StopButton.IsEnabled;
        }

        public void LoadSettings()
        {
            logger.Debug("Load settings start");

            mainWindowView.Left = configService.Read<double>("MainWindowPositionLeft");
            mainWindowView.Top = configService.Read<double>("MainWindowPositionTop");
            mainWindowView.RuntimeCapturingCheckBox.IsChecked = configService.Read<bool>("RuntimeCapturingCheckBox");
            mainWindowView.WithDetectionCheckBox.IsChecked = configService.Read<bool>("WithDetectionCheckBox");
            mainWindowView.SetupSlidersCheckBox.IsChecked = configService.Read<bool>("SetupSlidersCheckBox");
            mainWindowView.Cam1CheckBox.IsChecked = configService.Read<bool>("Cam1CheckBox");
            mainWindowView.Cam2CheckBox.IsChecked = configService.Read<bool>("Cam2CheckBox");
            mainWindowView.Cam3CheckBox.IsChecked = configService.Read<bool>("Cam3CheckBox");
            mainWindowView.Cam4CheckBox.IsChecked = configService.Read<bool>("Cam4CheckBox");
            mainWindowView.CamFovTextBox.Text = configService.Read<string>("CamFovAngle");
            mainWindowView.CamResolutionWidthTextBox.Text = configService.Read<string>("ResolutionWidth");
            mainWindowView.CamResolutionHeightTextBox.Text = configService.Read<string>("ResolutionHeight");
            mainWindowView.MinContourArcTextBox.Text = configService.Read<string>("MinContourArc");
            mainWindowView.MovesExtractionTextBox.Text = configService.Read<string>("MovesExtraction");
            mainWindowView.MovesDartTextBox.Text = configService.Read<string>("MovesDart");
            mainWindowView.MovesNoiseTextBox.Text = configService.Read<string>("MovesNoise");
            mainWindowView.SmoothGaussTextBox.Text = configService.Read<string>("SmoothGauss");
            mainWindowView.MoveDetectedSleepTimeTextBox.Text = configService.Read<string>("MoveDetectedSleepTime");
            mainWindowView.ExtractionSleepTimeTimeTextBox.Text = configService.Read<string>("ExtractionSleepTime");
            mainWindowView.ThresholdSleepTimeTimeTextBox.Text = configService.Read<string>("ThresholdSleepTime");
            mainWindowView.Cam1IdTextBox.Text = configService.Read<string>("Cam1Id");
            mainWindowView.Cam1XTextBox.Text = configService.Read<string>("Cam1X");
            mainWindowView.Cam1YTextBox.Text = configService.Read<string>("Cam1Y");
            mainWindowView.Cam2IdTextBox.Text = configService.Read<string>("Cam2Id");
            mainWindowView.Cam2XTextBox.Text = configService.Read<string>("Cam2X");
            mainWindowView.Cam2YTextBox.Text = configService.Read<string>("Cam2Y");
            mainWindowView.Cam3IdTextBox.Text = configService.Read<string>("Cam3Id");
            mainWindowView.Cam3XTextBox.Text = configService.Read<string>("Cam3X");
            mainWindowView.Cam3YTextBox.Text = configService.Read<string>("Cam3Y");
            mainWindowView.Cam4IdTextBox.Text = configService.Read<string>("Cam4Id");
            mainWindowView.Cam4XTextBox.Text = configService.Read<string>("Cam4X");
            mainWindowView.Cam4YTextBox.Text = configService.Read<string>("Cam4Y");
            mainWindowView.ToCam1Distance.Text = configService.Read<string>("ToCam1Distance");
            mainWindowView.ToCam2Distance.Text = configService.Read<string>("ToCam2Distance");
            mainWindowView.ToCam3Distance.Text = configService.Read<string>("ToCam3Distance");
            mainWindowView.ToCam4Distance.Text = configService.Read<string>("ToCam4Distance");

            logger.Debug("Load settings end");
        }

        public void SaveSettings()
        {
            logger.Debug("Save settings start");

            configService.Write("MainWindowPositionLeft", mainWindowView.Left);
            configService.Write("MainWindowPositionTop", mainWindowView.Top);
            configService.Write("RuntimeCapturingCheckBox", mainWindowView.RuntimeCapturingCheckBox.IsChecked.Value);
            configService.Write("WithDetectionCheckBox", mainWindowView.WithDetectionCheckBox.IsChecked.Value);
            configService.Write("SetupSlidersCheckBox", mainWindowView.SetupSlidersCheckBox.IsChecked.Value);
            configService.Write("Cam1CheckBox", mainWindowView.Cam1CheckBox.IsChecked.Value);
            configService.Write("Cam2CheckBox", mainWindowView.Cam2CheckBox.IsChecked.Value);
            configService.Write("Cam3CheckBox", mainWindowView.Cam3CheckBox.IsChecked.Value);
            configService.Write("Cam4CheckBox", mainWindowView.Cam4CheckBox.IsChecked.Value);
            configService.Write("CamFovAngle", mainWindowView.CamFovTextBox.Text);
            configService.Write("ResolutionWidth", mainWindowView.CamResolutionWidthTextBox.Text);
            configService.Write("ResolutionHeight", mainWindowView.CamResolutionHeightTextBox.Text);
            configService.Write("MinContourArc", mainWindowView.MinContourArcTextBox.Text);
            configService.Write("MovesExtraction", mainWindowView.MovesExtractionTextBox.Text);
            configService.Write("MovesDart", mainWindowView.MovesDartTextBox.Text);
            configService.Write("MovesNoise", mainWindowView.MovesNoiseTextBox.Text);
            configService.Write("SmoothGauss", mainWindowView.SmoothGaussTextBox.Text);
            configService.Write("MoveDetectedSleepTime", mainWindowView.MoveDetectedSleepTimeTextBox.Text);
            configService.Write("ExtractionSleepTime", mainWindowView.ExtractionSleepTimeTimeTextBox.Text);
            configService.Write("ThresholdSleepTime", mainWindowView.ThresholdSleepTimeTimeTextBox.Text);
            configService.Write("Cam1Id", mainWindowView.Cam1IdTextBox.Text);
            configService.Write("Cam2Id", mainWindowView.Cam2IdTextBox.Text);
            configService.Write("Cam3Id", mainWindowView.Cam3IdTextBox.Text);
            configService.Write("Cam4Id", mainWindowView.Cam4IdTextBox.Text);
            configService.Write("ToCam1Distance", mainWindowView.ToCam1Distance.Text);
            configService.Write("ToCam2Distance", mainWindowView.ToCam2Distance.Text);
            configService.Write("ToCam3Distance", mainWindowView.ToCam3Distance.Text);
            configService.Write("ToCam4Distance", mainWindowView.ToCam4Distance.Text);

            logger.Debug("Save settings end");
        }
    }
}