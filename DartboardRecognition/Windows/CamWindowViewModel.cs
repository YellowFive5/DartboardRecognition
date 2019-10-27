#region Usings

using Autofac;
using DartboardRecognition.Services;
using NLog;

#endregion

namespace DartboardRecognition.Windows
{
    public class CamWindowViewModel
    {
        private readonly CamWindow camWindowView;
        private readonly MeasureService measureService;
        private readonly ConfigService configService;
        private readonly CamService camService;
        private readonly Logger logger;

        public CamWindowViewModel()
        {
        }

        public CamWindowViewModel(CamWindow camWindowView,
                                  bool runtimeCapturing,
                                  bool withDetection)
        {
            this.camWindowView = camWindowView;
            camService = new CamService(camWindowView, runtimeCapturing, withDetection);
            measureService = new MeasureService(camService);
            configService = MainWindow.ServiceContainer.Resolve<ConfigService>();
            logger = MainWindow.ServiceContainer.Resolve<Logger>();
        }

        public void LoadSettings()
        {
            var camNumberStr = camWindowView.camNumber.ToString();

            camWindowView.TresholdSlider.Value = configService.Read<double>($"Cam{camNumberStr}TresholdSlider");
            camWindowView.RoiPosYSlider.Value = configService.Read<double>($"Cam{camNumberStr}RoiPosYSlider");
            camWindowView.RoiHeightSlider.Value = configService.Read<double>($"Cam{camNumberStr}RoiHeightSlider");
            camWindowView.SurfaceSlider.Value = configService.Read<double>($"Cam{camNumberStr}SurfaceSlider");
            camWindowView.SurfaceCenterSlider.Value = configService.Read<double>($"Cam{camNumberStr}SurfaceCenterSlider");
            camWindowView.SurfaceLeftSlider.Value = configService.Read<double>($"Cam{camNumberStr}SurfaceLeftSlider");
            camWindowView.SurfaceRightSlider.Value = configService.Read<double>($"Cam{camNumberStr}SurfaceRightSlider");
        }

        public void SaveSettings()
        {
            var camNumberStr = camWindowView.camNumber.ToString();

            configService.Write($"Cam{camNumberStr}TresholdSlider", camWindowView.TresholdSlider.Value);
            configService.Write($"Cam{camNumberStr}RoiPosYSlider", camWindowView.RoiPosYSlider.Value);
            configService.Write($"Cam{camNumberStr}RoiHeightSlider", camWindowView.RoiHeightSlider.Value);
            configService.Write($"Cam{camNumberStr}SurfaceSlider", camWindowView.SurfaceSlider.Value);
            configService.Write($"Cam{camNumberStr}SurfaceCenterSlider", camWindowView.SurfaceCenterSlider.Value);
            configService.Write($"Cam{camNumberStr}SurfaceLeftSlider", camWindowView.SurfaceLeftSlider.Value);
            configService.Write($"Cam{camNumberStr}SurfaceRightSlider", camWindowView.SurfaceRightSlider.Value);
        }

        public void DoCapture()
        {
            camService.DoCapture();
            camService.RefreshImageBoxes();
        }

        public ResponseType Detect()
        {
            return camService.Detect();
        }

        public void FindThrow()
        {
            camService.FindThrow();
        }

        public void ProcessContour()
        {
            var found = measureService.FindDartContour();
            if (found)
            {
                measureService.ProcessDartContour();
            }
        }

        public void OnClosing()
        {
            camService.videoCapture.Dispose();
        }
    }
}