#region Usings

using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Emgu.CV.Structure;

#endregion

namespace DartboardRecognition
{
    public class MainWindowViewModel
    {
        private Cam cam1;
        private Cam cam2;
        private MainWindow view;
        private Dispatcher dispatcher;
        private Measureman measureman1;
        private Measureman measureman2;
        private Drawman drawman1;
        private Drawman drawman2;
        private CancellationToken cancelToken;
        private CancellationTokenSource cts;

        public MainWindowViewModel(MainWindow view)
        {
            this.view = view;
            dispatcher = Dispatcher.CurrentDispatcher;
            LoadSettings();
            cts = new CancellationTokenSource();
            cancelToken = cts.Token;
        }

        public void StartCapture()
        {
            cam1 = new Cam1(view);
            cam2 = new Cam2(view);
            drawman1 = new Drawman(view);
            drawman2 = new Drawman(view);
            measureman1 = new Measureman(view, drawman1);
            measureman2 = new Measureman(view, drawman2);

            measureman1.CalculateDartboardProjection();

            if (view.Throw1RadioButton.IsChecked.Value)
            {
                cam1.SetProcessingCapture(1);
                cam2.SetProcessingCapture(1);
            }

            if (view.Throw2RadioButton.IsChecked.Value)
            {
                cam1.SetProcessingCapture(2);
                cam2.SetProcessingCapture(2);
            }

            if (view.Throw3RadioButton.IsChecked.Value)
            {
                cam1.SetProcessingCapture(3);
                cam2.SetProcessingCapture(3);
            }

            var t = new Task(CaptureImageCam1);
            var t2 = new Task(CaptureImageCam2);
            t.Start();
            t2.Start();
        }

        public void StopCapture()
        {
            cts.Cancel();
            cam1.videoCapture.Dispose();
            cam2.videoCapture.Dispose();
        }

        private void LoadSettings()
        {
            view.Cam1TresholdMinSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1TresholdMinSlider"]);
            view.Cam1TresholdMaxSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1TresholdMaxSlider"]);
            view.Cam1RoiPosXSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1RoiPosXSlider"]);
            view.Cam1RoiPosYSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1RoiPosYSlider"]);
            view.Cam1RoiWidthSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1RoiWidthSlider"]);
            view.Cam1RoiHeightSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1RoiHeightSlider"]);
            view.Cam1SurfaceSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1SurfaceSlider"]);
            view.Cam1IndexBox.Text = ConfigurationManager.AppSettings["Cam1IndexBox"];
            view.Cam1SurfaceCenterSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1SurfaceCenterSlider"]);
            view.Cam1SurfaceLeftSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1SurfaceLeftSlider"]);
            view.Cam1SurfaceRightSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam1SurfaceRightSlider"]);

            view.Cam2TresholdMinSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2TresholdMinSlider"]);
            view.Cam2TresholdMaxSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2TresholdMaxSlider"]);
            view.Cam2RoiPosXSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2RoiPosXSlider"]);
            view.Cam2RoiPosYSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2RoiPosYSlider"]);
            view.Cam2RoiWidthSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2RoiWidthSlider"]);
            view.Cam2RoiHeightSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2RoiHeightSlider"]);
            view.Cam2SurfaceSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2SurfaceSlider"]);
            view.Cam2IndexBox.Text = ConfigurationManager.AppSettings["Cam2IndexBox"];
            view.Cam2SurfaceCenterSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2SurfaceCenterSlider"]);
            view.Cam2SurfaceLeftSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2SurfaceLeftSlider"]);
            view.Cam2SurfaceRightSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2SurfaceRightSlider"]);
        }

        public void SaveSettings()
        {
            var configManager = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetExecutingAssembly().Location);
            configManager.AppSettings.Settings.Clear();
            configManager.AppSettings.Settings.Add("Cam1TresholdMinSlider", view.Cam1TresholdMinSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1TresholdMaxSlider", view.Cam1TresholdMaxSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1RoiPosXSlider", view.Cam1RoiPosXSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1RoiPosYSlider", view.Cam1RoiPosYSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1RoiWidthSlider", view.Cam1RoiWidthSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1RoiHeightSlider", view.Cam1RoiHeightSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1SurfaceSlider", view.Cam1SurfaceSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1IndexBox", view.Cam1IndexBox.Text);
            configManager.AppSettings.Settings.Add("Cam1SurfaceCenterSlider", view.Cam1SurfaceCenterSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1SurfaceLeftSlider", view.Cam1SurfaceLeftSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam1SurfaceRightSlider", view.Cam1SurfaceRightSlider.Value.ToString());

            configManager.AppSettings.Settings.Add("Cam2TresholdMinSlider", view.Cam2TresholdMinSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2TresholdMaxSlider", view.Cam2TresholdMaxSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2RoiPosXSlider", view.Cam2RoiPosXSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2RoiPosYSlider", view.Cam2RoiPosYSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2RoiWidthSlider", view.Cam2RoiWidthSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2RoiHeightSlider", view.Cam2RoiHeightSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2SurfaceSlider", view.Cam2SurfaceSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2IndexBox", view.Cam2IndexBox.Text);
            configManager.AppSettings.Settings.Add("Cam2SurfaceCenterSlider", view.Cam2SurfaceCenterSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2SurfaceLeftSlider", view.Cam2SurfaceLeftSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2SurfaceRightSlider", view.Cam2SurfaceRightSlider.Value.ToString());

            configManager.Save(ConfigurationSaveMode.Modified);
        }

        private void CaptureImageCam1()
        {
            while (!cancelToken.IsCancellationRequested)
            {
                cam1.originFrame = cam1.videoCapture.QueryFrame().ToImage<Bgr, byte>();
                using (cam1.originFrame)
                {
                    if (cam1.originFrame == null)
                    {
                        return;
                    }

                    measureman1.SetupWorkingCam(cam1);
                    measureman1.CalculateSetupLines();
                    measureman1.CalculateRoiRegion();
                    drawman1.TresholdRoiRegion(cam1);

                    if (measureman1.ThrowDetected())
                    {
                        measureman1.CalculateDartContour();
                    }

                    drawman1.SaveToImageBox1(cam1.linedFrame, view.ImageBox1);
                    // drawman1.SaveToImageBox(cam1.roiTrasholdFrame, view.ImageBox1Roi);
                }
            }
        }

        private void CaptureImageCam2()
        {
            while (!cancelToken.IsCancellationRequested)
            {
                cam2.originFrame = cam2.videoCapture.QueryFrame().ToImage<Bgr, byte>();
                using (cam2.originFrame)
                {
                    if (cam2.originFrame == null)
                    {
                        return;
                    }

                    measureman2.SetupWorkingCam(cam2);
                    measureman2.CalculateSetupLines();
                    measureman2.CalculateRoiRegion();
                    drawman2.TresholdRoiRegion(cam2);

                    if (measureman2.ThrowDetected())
                    {
                        measureman2.CalculateDartContour();
                    }

                    drawman2.SaveToImageBox2(cam2.linedFrame, view.ImageBox2);
                    // drawman2.SaveToImageBox(cam2.roiTrasholdFrame, view.ImageBox2Roi);
                }
            }
        }
    }
}