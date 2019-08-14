#region Usings

using System;
using System.Configuration;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using Emgu.CV.Structure;

#endregion

namespace DartboardRecognition
{
    public class CamWindowViewModel
    {
        private Dispatcher camWindowDispatcher;
        private CamWindow camWindowView;
        private int camNumber;
        private Measureman measureman;
        private Drawman drawman;
        private ThrowService throwService;
        private Cam cam;
        private CancellationToken cancelToken;

        public CamWindowViewModel()
        {
        }

        public CamWindowViewModel(CamWindow camWindowView, int camNumber, Drawman drawman, ThrowService throwService, CancellationToken cancelToken)
        {
            this.camWindowView = camWindowView;
            camWindowDispatcher = camWindowView.Dispatcher;
            this.camNumber = camNumber;
            this.drawman = drawman;
            this.throwService = throwService;
            this.cancelToken = cancelToken;
            measureman = new Measureman(camWindowView, drawman, throwService);
            cam = new Cam(camWindowView);
        }

        public void SetWindowTitle()
        {
            camWindowView.Title = $"Cam {camNumber.ToString()}";
        }

        public void LoadSettings()
        {
            var camNumberStr = camNumber.ToString();
            camWindowView.TresholdMinSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}TresholdMinSlider"]);
            camWindowView.TresholdMaxSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}TresholdMaxSlider"]);
            camWindowView.RoiPosXSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}RoiPosXSlider"]);
            camWindowView.RoiPosYSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}RoiPosYSlider"]);
            camWindowView.RoiWidthSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}RoiWidthSlider"]);
            camWindowView.RoiHeightSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}RoiHeightSlider"]);
            camWindowView.SurfaceSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}SurfaceSlider"]);
            camWindowView.IndexBox.Text = ConfigurationManager.AppSettings[$"Cam{camNumberStr}IndexBox"];
            camWindowView.SurfaceCenterSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}SurfaceCenterSlider"]);
            camWindowView.SurfaceLeftSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}SurfaceLeftSlider"]);
            camWindowView.SurfaceRightSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}SurfaceRightSlider"]);
        }

        public void SaveSettings(object settingsLock)
        {
            var doc = new XmlDocument();
            var camNumberStr = camNumber.ToString();

            lock (settingsLock)
            {
                Rewrite($"Cam{camNumberStr}TresholdMinSlider", camWindowView.TresholdMinSlider.Value.ToString());
                Rewrite($"Cam{camNumberStr}TresholdMaxSlider", camWindowView.TresholdMaxSlider.Value.ToString());
                Rewrite($"Cam{camNumberStr}RoiPosXSlider", camWindowView.RoiPosXSlider.Value.ToString());
                Rewrite($"Cam{camNumberStr}RoiPosYSlider", camWindowView.RoiPosYSlider.Value.ToString());
                Rewrite($"Cam{camNumberStr}RoiWidthSlider", camWindowView.RoiWidthSlider.Value.ToString());
                Rewrite($"Cam{camNumberStr}RoiHeightSlider", camWindowView.RoiHeightSlider.Value.ToString());
                Rewrite($"Cam{camNumberStr}SurfaceSlider", camWindowView.SurfaceSlider.Value.ToString());
                Rewrite($"Cam{camNumberStr}IndexBox", camWindowView.IndexBox.Text);
                Rewrite($"Cam{camNumberStr}SurfaceCenterSlider", camWindowView.SurfaceCenterSlider.Value.ToString());
                Rewrite($"Cam{camNumberStr}SurfaceLeftSlider", camWindowView.SurfaceLeftSlider.Value.ToString());
                Rewrite($"Cam{camNumberStr}SurfaceRightSlider", camWindowView.SurfaceRightSlider.Value.ToString());
            }

            void Rewrite(string key, string value)
            {
                doc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

                if (doc.DocumentElement != null)
                {
                    foreach (XmlElement element in doc.DocumentElement)
                    {
                        if (element.Name.Equals("appSettings"))
                        {
                            foreach (XmlNode node in element.ChildNodes)
                            {
                                if (node.Attributes != null && node.Attributes[0].Value.Equals(key))
                                {
                                    node.Attributes[1].Value = value;
                                }
                            }
                        }
                    }
                }

                doc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        public void RunWork(bool runtimeCapturing)
        {
            cam.originFrame = cam.videoCapture.QueryFrame().ToImage<Bgr, byte>();
            cam.RefreshLines(camWindowView);
            measureman.SetupWorkingCam(cam);
            measureman.CalculateSetupLines();
            measureman.CalculateRoiRegion();
            drawman.TresholdRoiRegion(cam);

            while (!cancelToken.IsCancellationRequested)
            {
                using (cam.originFrame)
                {
                    if (cam.originFrame == null)
                    {
                        return;
                    }

                    // var throwDetected = measureman.DetectThrow();
                    // if (throwDetected)
                    // {
                    //     var dartContourFound = measureman.FindDartContour();
                    //     if (dartContourFound)
                    //     {
                    //         measureman.ProcessDartContour();
                    //         RefreshImageBoxes();
                    //     }
                    // }

                    if (runtimeCapturing)
                    {
                        cam.originFrame = cam.videoCapture.QueryFrame().ToImage<Bgr, byte>();
                        cam.RefreshLines(camWindowView);
                        measureman.CalculateSetupLines();
                        measureman.CalculateRoiRegion();
                        drawman.TresholdRoiRegion(cam);
                        RefreshImageBoxes();
                    }
                }
            }

            camWindowDispatcher.Invoke(() => camWindowView.Close());
            cam.videoCapture.Dispose();
            Dispatcher.CurrentDispatcher.Thread.Abort();
        }

        private void RefreshImageBoxes()
        {
            camWindowDispatcher.Invoke(new Action(() => camWindowView.ImageBox.Source = drawman.ConvertToBitmap(cam.linedFrame)));
            camWindowDispatcher.Invoke(new Action(() => camWindowView.ImageBoxRoi.Source = drawman.ConvertToBitmap(cam.roiTrasholdFrame)));
            camWindowDispatcher.Invoke(new Action(() => camWindowView.ImageBoxRoiLastThrow.Source = cam.roiTrasholdFrameLastThrow != null
                                                                                                        ? drawman.ConvertToBitmap(cam.roiTrasholdFrameLastThrow)
                                                                                                        : new BitmapImage()));
        }
    }
}