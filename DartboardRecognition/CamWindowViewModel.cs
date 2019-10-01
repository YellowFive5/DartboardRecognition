#region Usings

using System;
using System.Configuration;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using Emgu.CV.Structure;

#endregion

namespace DartboardRecognition
{
    public class CamWindowViewModel
    {
        private readonly Dispatcher camWindowDispatcher;
        private readonly CamWindow camWindowView;
        private readonly Measureman measureman;
        private readonly Drawman drawman;
        private readonly ThrowService throwService;
        private readonly Cam cam;
        private readonly bool runtimeCapturing;
        private readonly bool withDetection;

        public CamWindowViewModel()
        {
        }

        public CamWindowViewModel(CamWindow camWindowView,
                                  Drawman drawman,
                                  ThrowService throwService,
                                  bool runtimeCapturing,
                                  bool withDetection)
        {
            this.camWindowView = camWindowView;
            camWindowDispatcher = camWindowView.Dispatcher;
            this.drawman = drawman;
            this.throwService = throwService;
            this.runtimeCapturing = runtimeCapturing;
            this.withDetection = withDetection;
            measureman = new Measureman(camWindowView, drawman, throwService);
            cam = new Cam(camWindowView);

            measureman.SetupWorkingCam(cam);
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

        private void DoCaptures()
        {
            using (cam.originFrame = cam.videoCapture.QueryFrame().ToImage<Bgr, byte>())
            {
                cam.RefreshLines(camWindowView);
                measureman.CalculateSetupLines();
                measureman.CalculateRoiRegion();
                drawman.TresholdRoiRegion(cam);
            }
        }

        private void RefreshImageBoxes()
        {
            camWindowDispatcher.Invoke(new Action(() => camWindowView.ImageBox.Source = drawman.ToBitmap(cam.linedFrame)));
            camWindowDispatcher.Invoke(new Action(() => camWindowView.ImageBoxRoi.Source = drawman.ToBitmap(cam.roiTrasholdFrame)));
            camWindowDispatcher.Invoke(new Action(() => camWindowView.ImageBoxRoiLastThrow.Source = cam.roiTrasholdFrameLastThrow != null
                                                                                                        ? drawman.ToBitmap(cam.roiTrasholdFrameLastThrow)
                                                                                                        : new BitmapImage()));
        }

        public bool DetectThrow()
        {
            DoCaptures();

            var throwDetected = withDetection && measureman.DetectThrow();

            if (throwDetected)
            {
                var dartContourFound = measureman.FindDartContour();
                if (dartContourFound)
                {
                    measureman.ProcessDartContour();
                    RefreshImageBoxes();
                }
            }

            if (runtimeCapturing)
            {
                RefreshImageBoxes();
            }

            return throwDetected;
        }

        public void FindContour()
        {
            throw new NotImplementedException();
        }

        public void OnClosing()
        {
            cam.videoCapture.Dispose();
        }
    }
}