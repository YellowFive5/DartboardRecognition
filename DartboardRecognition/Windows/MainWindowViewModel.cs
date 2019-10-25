#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Autofac;
using DartboardRecognition.Services;

#endregion

namespace DartboardRecognition.Windows
{
    public class MainWindowViewModel
    {
        private readonly MainWindow mainWindowView;
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
            drawService.ProjectionPrepare();
        }

        private void StartCapturing()
        {
            drawService.ProjectionClear();
            cts = new CancellationTokenSource();
            cancelToken = cts.Token;
            StartDetection();
        }

        private void StartDetection()
        {
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

            DoCaptures();

            Task.Run(() =>
                     {
                         Thread.CurrentThread.Name = $"Recognition_workerThread";

                         while (!cancelToken.IsCancellationRequested)
                         {
                             foreach (var cam in cams)
                             {
                                 var response = cam.Detect();

                                 if (response == ResponseType.Trow)
                                 {
                                     cam.ProcessContour();
                                     FindThrowFromRemainingCams(cam);
                                     break;
                                 }

                                 if (response == ResponseType.Extraction)
                                 {
                                     var now = DateTime.Now;
                                     while (DateTime.Now - now < TimeSpan.FromSeconds(extractionSleepTime)) // todo something with thread
                                     {
                                     }

                                     drawService.ProjectionClear();
                                     DoCaptures();
                                     break;
                                 }
                             }
                         }

                         foreach (var cam in cams)
                         {
                             cam.Dispatcher.Invoke(() => cam.Close());
                         }
                     });
        }

        private void FindThrowFromRemainingCams(CamWindow succeededCam)
        {
            foreach (var cam in cams.Where(cam => cam != succeededCam))
            {
                cam.FindThrow();
                cam.ProcessContour();
            }

            throwService.CalculateAndSaveThrow();
        }

        private void DoCaptures()
        {
            foreach (var cam in cams)
            {
                cam.DoCapture();
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
            StartCapturing();
        }

        public void OnStopButtonClicked()
        {
            mainWindowView.PointsBox.Text = "";
            ToggleViewControls();
            StopCapturing();
        }

        private void ToggleViewControls()
        {
            var mainContainer = (Panel) mainWindowView.Content;
            var element = mainContainer.Children;
            var listElement = element.Cast<FrameworkElement>().ToList();
            var listControl = listElement.OfType<Control>();
            foreach (var control in listControl)
            {
                control.IsEnabled = !control.IsEnabled;
            }
        }

        public void LoadSettings()
        {
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
        }

        public void SaveSettings()
        {
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
        }
    }
}