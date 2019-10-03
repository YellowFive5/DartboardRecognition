#region Usings

using System;
using System.Configuration;
using System.Xml;
using DartboardRecognition.Services;

#endregion

namespace DartboardRecognition.Windows
{
    public class CamWindowViewModel
    {
        private readonly CamWindow camWindowView;
        private readonly MeasureService measureService;
        private readonly DrawService drawService;
        private readonly ThrowService throwService;
        private readonly CamService camService;
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
            this.drawService = drawService;
            this.throwService = throwService;
            this.runtimeCapturing = runtimeCapturing;
            this.withDetection = withDetection;
            camService = new CamService(camWindowView, drawService);
            measureService = new MeasureService(camWindowView, camService, drawService, throwService);
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

        public bool DetectThrow()
        {
            camService.DoCaptures();

            var throwDetected = withDetection && camService.DetectThrow();

            if (runtimeCapturing && !throwDetected)
            {
                camService.DoCaptures();
                camService.RefreshImageBoxes();
            }

            return throwDetected;
        }

        public void FindDart()
        {
            var dartContourFound = measureService.FindDartContour();
            if (dartContourFound)
            {
                measureService.ProcessDartContour();
                camService.RefreshImageBoxes();
            }
        }

        public void OnClosing()
        {
            camService.videoCapture.Dispose();
        }
    }
}