﻿#region Usings

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
        private readonly double extractionSleepTime;

        public MainWindowViewModel()
        {
        }

        public MainWindowViewModel(MainWindow mainWindowView)
        {
            this.mainWindowView = mainWindowView;
            drawService = MainWindow.ServiceContainer.Resolve<DrawService>();
            throwService = MainWindow.ServiceContainer.Resolve<ThrowService>();
            configService = MainWindow.ServiceContainer.Resolve<ConfigService>();
            extractionSleepTime = configService.Read<double>("ExtractionSleepTime");
            logger = MainWindow.ServiceContainer.Resolve<Logger>();
            drawService.ProjectionPrepare();
        }

        private void StartDetection()
        {
            drawService.ProjectionClear();
            cts = new CancellationTokenSource();
            cancelToken = cts.Token;

            var runtimeCapturing = mainWindowView.RuntimeCapturingCheckBox.IsChecked.Value;
            var withDetection = mainWindowView.WithDetectionCheckBox.IsChecked.Value;
            var withSetupSliders = mainWindowView.SetupSlidersCheckBox.IsChecked.Value;

            cams = new List<CamWindow>();
            if (mainWindowView.Cam1CheckBox.IsChecked.Value)
            {
                cams.Add(new CamWindow(1, runtimeCapturing, withDetection, withSetupSliders));
            }

            if (mainWindowView.Cam2CheckBox.IsChecked.Value)
            {
                cams.Add(new CamWindow(2, runtimeCapturing, withDetection, withSetupSliders));
            }

            if (mainWindowView.Cam3CheckBox.IsChecked.Value)
            {
                cams.Add(new CamWindow(3, runtimeCapturing, withDetection, withSetupSliders));
            }

            if (mainWindowView.Cam4CheckBox.IsChecked.Value)
            {
                cams.Add(new CamWindow(4, runtimeCapturing, withDetection, withSetupSliders));
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
                                     response = cam.DetectThrow();

                                     if (response == ResponseType.Trow)
                                     {
                                         cam.FindAndProcessDartContour();

                                         FindThrowOnRemainingCams(cam);

                                         logger.Debug($"Cam_{cam.camNumber} detection end with response type '{ResponseType.Trow}'");
                                         break;
                                     }

                                     if (response == ResponseType.Extraction)
                                     {
                                         Thread.Sleep(TimeSpan.FromSeconds(extractionSleepTime));

                                         drawService.ProjectionClear();
                                         ClearAllCamsImageBoxes();

                                         logger.Debug($"Cam_{cam.camNumber} detection end with response type '{ResponseType.Extraction}'");
                                         break;
                                     }
                                 }

                                 Thread.Sleep(TimeSpan.FromSeconds(0.7));

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
            foreach (var cam in cams)
            {
                cam.ClearImageBoxes();
            }
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
            mainWindowView.ProjectionFrameSideTextBox.Text = configService.Read<string>("ProjectionFrameSide");
            mainWindowView.MoveDetectedSleepTimeTextBox.Text = configService.Read<string>("MoveDetectedSleepTime");
            mainWindowView.ExtractionSleepTimeTimeTextBox.Text = configService.Read<string>("ExtractionSleepTime");
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

            logger.Debug("Load settings end");
        }

        public void SaveSettings()
        {
            logger.Debug("Save settings start");

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
            configService.Write("ProjectionFrameSide", mainWindowView.ProjectionFrameSideTextBox.Text);
            configService.Write("MoveDetectedSleepTime", mainWindowView.MoveDetectedSleepTimeTextBox.Text);
            configService.Write("ExtractionSleepTime", mainWindowView.ExtractionSleepTimeTimeTextBox.Text);
            configService.Write("Cam1Id", mainWindowView.Cam1IdTextBox.Text);
            configService.Write("Cam1X", mainWindowView.Cam1XTextBox.Text);
            configService.Write("Cam1Y", mainWindowView.Cam1YTextBox.Text);
            configService.Write("Cam2Id", mainWindowView.Cam2IdTextBox.Text);
            configService.Write("Cam2X", mainWindowView.Cam2XTextBox.Text);
            configService.Write("Cam2Y", mainWindowView.Cam2YTextBox.Text);
            configService.Write("Cam3Id", mainWindowView.Cam3IdTextBox.Text);
            configService.Write("Cam3X", mainWindowView.Cam3XTextBox.Text);
            configService.Write("Cam3Y", mainWindowView.Cam3YTextBox.Text);
            configService.Write("Cam4Id", mainWindowView.Cam4IdTextBox.Text);
            configService.Write("Cam4X", mainWindowView.Cam4XTextBox.Text);
            configService.Write("Cam4Y", mainWindowView.Cam4YTextBox.Text);

            logger.Debug("Save settings end");
        }
    }
}