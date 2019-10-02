#region Usings

using System;
using System.Configuration;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using DartboardRecognition.Services;

#endregion

namespace DartboardRecognition.Windows
{
    public class CamWindowViewModel
    {
        private readonly Dispatcher camWindowDispatcher;
        private readonly CamWindow camWindowView;
        private readonly MeasureService measureService;
        private readonly DrawService drawService;
        private readonly ThrowService throwService;
        private readonly CamService cam;
        private readonly bool runtimeCapturing;
        private readonly bool withDetection;

        public CamWindowViewModel()
        {
        }

        public CamWindowViewModel(CamWindow camWindowView,
                                  DrawService drawService,
                                  ThrowService throwService,
                                  bool runtimeCapturing,
                                  bool withDetection)
        {
            this.camWindowView = camWindowView;
            camWindowDispatcher = camWindowView.Dispatcher;
            this.drawService = drawService;
            this.throwService = throwService;
            this.runtimeCapturing = runtimeCapturing;
            this.withDetection = withDetection;
            measureService = new MeasureService(camWindowView, drawService, throwService);
            cam = new CamService(camWindowView);

            measureService.SetupWorkingCam(cam);
        }

        public void SetWindowTitle()
        {
            camWindowView.Title = $"Cam {camWindowView.camNumber.ToString()}";
        }

        public void LoadSettings()
        {
            var camNumberStr = camWindowView.camNumber.ToString();
            camWindowView.TresholdMinSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}TresholdMinSlider"]);
            camWindowView.TresholdMaxSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}TresholdMaxSlider"]);
            camWindowView.RoiPosXSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}RoiPosXSlider"]);
            camWindowView.RoiPosYSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}RoiPosYSlider"]);
            camWindowView.RoiWidthSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}RoiWidthSlider"]);
            camWindowView.RoiHeightSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}RoiHeightSlider"]);
            camWindowView.SurfaceSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}SurfaceSlider"]);
            camWindowView.SurfaceCenterSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}SurfaceCenterSlider"]);
            camWindowView.SurfaceLeftSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}SurfaceLeftSlider"]);
            camWindowView.SurfaceRightSlider.Value = double.Parse(ConfigurationManager.AppSettings[$"Cam{camNumberStr}SurfaceRightSlider"]);
        }

        public void SaveSettings()
        {
            var doc = new XmlDocument();
            var camNumberStr = camWindowView.camNumber.ToString();

            lock (camWindowView.settingsLock)
            {
                Rewrite($"Cam{camNumberStr}TresholdMinSlider", camWindowView.TresholdMinSlider.Value.ToString());
                Rewrite($"Cam{camNumberStr}TresholdMaxSlider", camWindowView.TresholdMaxSlider.Value.ToString());
                Rewrite($"Cam{camNumberStr}RoiPosXSlider", camWindowView.RoiPosXSlider.Value.ToString());
                Rewrite($"Cam{camNumberStr}RoiPosYSlider", camWindowView.RoiPosYSlider.Value.ToString());
                Rewrite($"Cam{camNumberStr}RoiWidthSlider", camWindowView.RoiWidthSlider.Value.ToString());
                Rewrite($"Cam{camNumberStr}RoiHeightSlider", camWindowView.RoiHeightSlider.Value.ToString());
                Rewrite($"Cam{camNumberStr}SurfaceSlider", camWindowView.SurfaceSlider.Value.ToString());
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

        public void DoCaptures()
        {
            measureService.DoCaptures();
            // using (cam.originFrame = cam.videoCapture.QueryFrame().ToImage<Bgr, byte>())
            // {
            //     cam.RefreshLines(camWindowView);
            //     measureman.CalculateSetupLines();
            //     measureman.CalculateRoiRegion();
            //     drawman.TresholdRoiRegion(cam);
            // }
        }

        public void RefreshImageBoxes()
        {
            camWindowDispatcher.Invoke(new Action(() => camWindowView.ImageBox.Source = drawService.ToBitmap(cam.linedFrame)));
            camWindowDispatcher.Invoke(new Action(() => camWindowView.ImageBoxRoi.Source = drawService.ToBitmap(cam.roiTrasholdFrame)));
            camWindowDispatcher.Invoke(new Action(() => camWindowView.ImageBoxRoiLastThrow.Source = cam.roiTrasholdFrameLastThrow != null
                                                                                                        ? drawService.ToBitmap(cam.roiTrasholdFrameLastThrow)
                                                                                                        : new BitmapImage()));
        }

        public bool DetectThrow()
        {
            DoCaptures();

            var throwDetected = withDetection && measureService.DetectThrow();

            if (runtimeCapturing)
            {
                RefreshImageBoxes();
            }

            return throwDetected;
        }
            
        public void FindDart()
        {
            var dartContourFound = measureService.FindDartContour();
            if (dartContourFound)
            {
                measureService.ProcessDartContour();
                RefreshImageBoxes();
            }
        }

        public void OnClosing()
        {
            cam.videoCapture.Dispose();
        }
    }
}