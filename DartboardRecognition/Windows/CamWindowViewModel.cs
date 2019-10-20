#region Usings

using Autofac;
using DartboardRecognition.Services;

#endregion

namespace DartboardRecognition.Windows
{
    public class CamWindowViewModel
    {
        private readonly CamWindow camWindowView;
        private readonly MeasureService measureService;
        private readonly ConfigService configService;
        private readonly CamService camService;

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
        }

        public void LoadSettings()
        {
            var camNumberStr = camWindowView.camNumber.ToString();

            camWindowView.TresholdMinSlider.Value = configService.Read<double>($"Cam{camNumberStr}TresholdMinSlider");
            camWindowView.TresholdMaxSlider.Value = configService.Read<double>($"Cam{camNumberStr}TresholdMaxSlider");
            camWindowView.RoiPosXSlider.Value = configService.Read<double>($"Cam{camNumberStr}RoiPosXSlider");
            camWindowView.RoiPosYSlider.Value = configService.Read<double>($"Cam{camNumberStr}RoiPosYSlider");
            camWindowView.RoiWidthSlider.Value = configService.Read<double>($"Cam{camNumberStr}RoiWidthSlider");
            camWindowView.RoiHeightSlider.Value = configService.Read<double>($"Cam{camNumberStr}RoiHeightSlider");
            camWindowView.SurfaceSlider.Value = configService.Read<double>($"Cam{camNumberStr}SurfaceSlider");
            camWindowView.SurfaceCenterSlider.Value = configService.Read<double>($"Cam{camNumberStr}SurfaceCenterSlider");
            camWindowView.SurfaceLeftSlider.Value = configService.Read<double>($"Cam{camNumberStr}SurfaceLeftSlider");
            camWindowView.SurfaceRightSlider.Value = configService.Read<double>($"Cam{camNumberStr}SurfaceRightSlider");
        }

        public void SaveSettings()
        {
            var camNumberStr = camWindowView.camNumber.ToString();

            configService.Write($"Cam{camNumberStr}TresholdMinSlider", camWindowView.TresholdMinSlider.Value);
            configService.Write($"Cam{camNumberStr}TresholdMaxSlider", camWindowView.TresholdMaxSlider.Value);
            configService.Write($"Cam{camNumberStr}RoiPosXSlider", camWindowView.RoiPosXSlider.Value);
            configService.Write($"Cam{camNumberStr}RoiPosYSlider", camWindowView.RoiPosYSlider.Value);
            configService.Write($"Cam{camNumberStr}RoiWidthSlider", camWindowView.RoiWidthSlider.Value);
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