#region Usings

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
        private readonly bool runtimeCapturing;
        private readonly bool withDetection;

        public CamWindowViewModel()
        {
        }

        public CamWindowViewModel(CamWindow camWindowView,
                                  bool runtimeCapturing,
                                  bool withDetection)
        {
            this.camWindowView = camWindowView;
            configService = ServiceBag.All().ConfigService;
            this.runtimeCapturing = runtimeCapturing;
            this.withDetection = withDetection;
            camService = new CamService(camWindowView);
            measureService = new MeasureService(camWindowView, camService);
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

        public void FindThrow()
        {
            camService.FindThrow();
        }
    }
}