#region Usings

using System;
using System.Configuration;
using System.Drawing;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using Point = System.Drawing.Point;

#endregion

namespace DartboardRecognition
{
    public class MainWindowViewModel
    {
        private Cam cam1;
        private Cam cam2;
        private MainWindow view;
        private Dispatcher dispatcher;
        private Geometr geometr;
        private Drawer drawer;

        public MainWindowViewModel(MainWindow view)
        {
            this.view = view;
            drawer = new Drawer(view);
            geometr = new Geometr();
            dispatcher = Dispatcher.CurrentDispatcher;
            LoadSettings();
        }

        public void StartCapture()
        {
            cam1 = new Cam1(view);
            cam2 = new Cam2(view);

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

            cam1.camHandler = (s, e2) => CaptureImage(cam1);
            cam2.camHandler = (s, e2) => CaptureImage(cam2);
            dispatcher.Hooks.DispatcherInactive += cam1.camHandler;
            dispatcher.Hooks.DispatcherInactive += cam2.camHandler;
        }

        public void StopCapture()
        {
            dispatcher.Hooks.DispatcherInactive -= cam1.camHandler;
            dispatcher.Hooks.DispatcherInactive -= cam2.camHandler;
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

            view.Cam2TresholdMinSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2TresholdMinSlider"]);
            view.Cam2TresholdMaxSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2TresholdMaxSlider"]);
            view.Cam2RoiPosXSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2RoiPosXSlider"]);
            view.Cam2RoiPosYSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2RoiPosYSlider"]);
            view.Cam2RoiWidthSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2RoiWidthSlider"]);
            view.Cam2RoiHeightSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2RoiHeightSlider"]);
            view.Cam2SurfaceSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2SurfaceSlider"]);
            view.Cam2IndexBox.Text = ConfigurationManager.AppSettings["Cam2IndexBox"];
            view.Cam2SurfaceCenterSlider.Value = double.Parse(ConfigurationManager.AppSettings["Cam2SurfaceCenterSlider"]);
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

            configManager.AppSettings.Settings.Add("Cam2TresholdMinSlider", view.Cam2TresholdMinSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2TresholdMaxSlider", view.Cam2TresholdMaxSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2RoiPosXSlider", view.Cam2RoiPosXSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2RoiPosYSlider", view.Cam2RoiPosYSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2RoiWidthSlider", view.Cam2RoiWidthSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2RoiHeightSlider", view.Cam2RoiHeightSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2SurfaceSlider", view.Cam2SurfaceSlider.Value.ToString());
            configManager.AppSettings.Settings.Add("Cam2IndexBox", view.Cam2IndexBox.Text);
            configManager.AppSettings.Settings.Add("Cam2SurfaceCenterSlider", view.Cam2SurfaceCenterSlider.Value.ToString());

            configManager.Save(ConfigurationSaveMode.Modified);
        }

        private void CaptureImage(Cam cam)
        {
            drawer.DrawDartboardProjection();
            cam.originFrame = view.UseCamsRadioButton.IsChecked.Value
                                  ? cam.videoCapture.QueryFrame().ToImage<Bgr, byte>()
                                  : cam.processingCapture.Clone();

            using (cam.originFrame)
            {
                if (cam.originFrame == null)
                {
                    return;
                }

                //DrawLines
                cam.linedFrame = cam.originFrame.Clone();

                var roiRectangle = new Rectangle((int) cam.roiPosXSlider.Value,
                                                 (int) cam.roiPosYSlider.Value,
                                                 (int) cam.roiWidthSlider.Value,
                                                 (int) cam.roiHeightSlider.Value);
                drawer.DrawRectangle(cam.linedFrame, roiRectangle, view.RoiRectColor.MCvScalar, view.RoiRectThickness);

                cam.surfacePoint1 = new Point(0, (int) cam.surfaceSlider.Value);
                cam.surfacePoint2 = new Point(cam.originFrame.Cols, (int) cam.surfaceSlider.Value);
                drawer.DrawLine(cam.linedFrame, cam.surfacePoint1, cam.surfacePoint2, view.SurfaceLineColor.MCvScalar, view.SurfaceLineThickness);

                cam.surfaceCenterPoint1 = new Point
                                          {
                                              X = (int) cam.surfaceCenterSlider.Value,
                                              Y = (int) cam.surfaceSlider.Value
                                          };
                cam.surfaceCenterPoint2 = new Point
                                          {
                                              X = cam.surfaceCenterPoint1.X,
                                              Y = cam.surfaceCenterPoint1.Y - 50
                                          };
                drawer.DrawLine(cam.linedFrame, cam.surfaceCenterPoint1, cam.surfaceCenterPoint2, view.SurfaceLineColor.MCvScalar, view.SurfaceLineThickness);

                //FindROIRegion
                cam.roiFrame = cam.originFrame.Clone();
                cam.roiFrame.ROI = roiRectangle;

                //TresholdROIRegion
                cam.roiTrasholdFrame = cam.roiFrame.Clone().Convert<Gray, byte>().Not();
                cam.roiTrasholdFrame._ThresholdBinary(new Gray(cam.tresholdMinSlider.Value),
                                                      new Gray(cam.tresholdMaxSlider.Value));

                //FindDartContours
                cam.roiContourFrame = cam.roiFrame.Clone();
                CvInvoke.FindContours(cam.roiTrasholdFrame, cam.contours, cam.matHierarсhy, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
                //CvInvoke.DrawContours(linedFrame, contours, -1, contourColor, contourThickness, offset: new System.Drawing.Point(0, (int)RoiPosYSlider.Value));

                if (cam.contours.Size > 0)
                {
                    for (var i = 0; i < cam.contours.Size; i++)
                    {
                        // Filter contour
                        var arclength = CvInvoke.ArcLength(cam.contours[i], true);
                        if (arclength < cam.minContourArcLength)
                        {
                            continue;
                        }

                        // Find moments and centerpoint
                        var moments = CvInvoke.Moments(cam.contours[i]);
                        var centerPoint = new Point((int) (moments.M10 / moments.M00), (int) cam.roiPosYSlider.Value + (int) (moments.M01 / moments.M00));
                        drawer.DrawCircle(cam.linedFrame, centerPoint, 4, new Bgr(Color.Blue).MCvScalar, 3);

                        // Find contour rectangle
                        var rect = CvInvoke.MinAreaRect(cam.contours[i]);
                        var box = CvInvoke.BoxPoints(rect);
                        var point1 = new Point((int) box[0].X, (int) cam.roiPosYSlider.Value + (int) box[0].Y);
                        var point2 = new Point((int) box[1].X, (int) cam.roiPosYSlider.Value + (int) box[1].Y);
                        var point3 = new Point((int) box[2].X, (int) cam.roiPosYSlider.Value + (int) box[2].Y);
                        var point4 = new Point((int) box[3].X, (int) cam.roiPosYSlider.Value + (int) box[3].Y);
                        drawer.DrawLine(cam.linedFrame, point1, point2, view.ContourRectColor, view.ContourRectThickness);
                        drawer.DrawLine(cam.linedFrame, point2, point3, view.ContourRectColor, view.ContourRectThickness);
                        drawer.DrawLine(cam.linedFrame, point3, point4, view.ContourRectColor, view.ContourRectThickness);
                        drawer.DrawLine(cam.linedFrame, point4, point1, view.ContourRectColor, view.ContourRectThickness);

                        // Setup vertical contour middlepoints 
                        Point middlePoint1;
                        Point middlePoint2;
                        if (geometr.FindDistance(point1, point2) < geometr.FindDistance(point4, point1))
                        {
                            middlePoint1 = geometr.FindMiddlePoint(point1, point2);
                            middlePoint2 = geometr.FindMiddlePoint(point4, point3);
                        }
                        else
                        {
                            middlePoint1 = geometr.FindMiddlePoint(point4, point1);
                            middlePoint2 = geometr.FindMiddlePoint(point3, point2);
                        }

                        // Find spikeLine to surface
                        var spikePoint1 = middlePoint1;
                        var spikePoint2 = middlePoint2;
                        cam.spikeLineLength = cam.surfacePoint2.Y - middlePoint2.Y;
                        var angle = Math.Atan2(middlePoint1.Y - middlePoint2.Y, middlePoint1.X - middlePoint2.X);
                        spikePoint1.X = (int) (middlePoint2.X + Math.Cos(angle) * cam.spikeLineLength);
                        spikePoint1.Y = (int) (middlePoint2.Y + Math.Sin(angle) * cam.spikeLineLength);
                        drawer.DrawLine(cam.linedFrame, spikePoint1, spikePoint2, view.SpikeLineColor, view.SpikeLineThickness);

                        // Find point of impact with surface
                        var pointOfImpact = geometr.FindIntersectionPoint(spikePoint1, spikePoint2, cam.surfacePoint1, cam.surfacePoint2);
                        if (pointOfImpact != null)
                        {
                            drawer.DrawCircle(cam.linedFrame, pointOfImpact.Value, view.PointOfImpactRadius, view.PointOfImpactColor, view.PointOfImpactThickness);
                        }
                    }
                }

                drawer.SaveBitmapToImageBox(cam.linedFrame, cam.imageBox);
                drawer.SaveBitmapToImageBox(cam.roiTrasholdFrame, cam.imageBoxRoi);
            }
        }
    }
}