#region Usings

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
        private readonly ConfigService configService;
        private readonly CamService camService;
        private readonly bool runtimeCapturing;
        private readonly bool withDetection;

        public CamWindowViewModel()
        {
        }

        public CamWindowViewModel(CamWindow camWindowView,
                                  DrawService drawService,
                                  ThrowService throwService,
                                  ConfigService configService,
                                  bool runtimeCapturing,
                                  bool withDetection)
        {
            this.camWindowView = camWindowView;
            this.drawService = drawService;
            this.throwService = throwService;
            this.configService = configService;
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

            camWindowView.TresholdMinSlider.Value = configService.Get<double>($"Cam{camNumberStr}TresholdMinSlider");
            camWindowView.TresholdMaxSlider.Value = configService.Get<double>($"Cam{camNumberStr}TresholdMaxSlider");
            camWindowView.RoiPosXSlider.Value = configService.Get<double>($"Cam{camNumberStr}RoiPosXSlider");
            camWindowView.RoiPosYSlider.Value = configService.Get<double>($"Cam{camNumberStr}RoiPosYSlider");
            camWindowView.RoiWidthSlider.Value = configService.Get<double>($"Cam{camNumberStr}RoiWidthSlider");
            camWindowView.RoiHeightSlider.Value = configService.Get<double>($"Cam{camNumberStr}RoiHeightSlider");
            camWindowView.SurfaceSlider.Value = configService.Get<double>($"Cam{camNumberStr}SurfaceSlider");
            camWindowView.SurfaceCenterSlider.Value = configService.Get<double>($"Cam{camNumberStr}SurfaceCenterSlider");
            camWindowView.SurfaceLeftSlider.Value = configService.Get<double>($"Cam{camNumberStr}SurfaceLeftSlider");
            camWindowView.SurfaceRightSlider.Value = configService.Get<double>($"Cam{camNumberStr}SurfaceRightSlider");
        }

        public void SaveSettings()
        {
            var camNumberStr = camWindowView.camNumber.ToString();

            configService.Set($"Cam{camNumberStr}TresholdMinSlider", camWindowView.TresholdMinSlider.Value);
            configService.Set($"Cam{camNumberStr}TresholdMaxSlider", camWindowView.TresholdMaxSlider.Value);
            configService.Set($"Cam{camNumberStr}RoiPosXSlider", camWindowView.RoiPosXSlider.Value);
            configService.Set($"Cam{camNumberStr}RoiPosYSlider", camWindowView.RoiPosYSlider.Value);
            configService.Set($"Cam{camNumberStr}RoiWidthSlider", camWindowView.RoiWidthSlider.Value);
            configService.Set($"Cam{camNumberStr}RoiHeightSlider", camWindowView.RoiHeightSlider.Value);
            configService.Set($"Cam{camNumberStr}SurfaceSlider", camWindowView.SurfaceSlider.Value);
            configService.Set($"Cam{camNumberStr}SurfaceCenterSlider", camWindowView.SurfaceCenterSlider.Value);
            configService.Set($"Cam{camNumberStr}SurfaceLeftSlider", camWindowView.SurfaceLeftSlider.Value);
            configService.Set($"Cam{camNumberStr}SurfaceRightSlider", camWindowView.SurfaceRightSlider.Value);
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