using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using Point = System.Drawing.Point;
using Image = System.Windows.Controls.Image;

namespace DartboardRecognition
{
    public class Drawer
    {
        private MainWindow view;
        private Bgr surfaceLineColor;
        private int surfaceLineThickness;
        private Bgr roiRectColor;
        private int roiRectThickness;
        private MCvScalar contourColor;
        private int countourThickness;
        private MCvScalar contourRectColor;
        private int contourRectThickness;
        private MCvScalar spikeLineColor;
        private int spikeLineThickness;
        private MCvScalar pointOfImpactColor;
        private int pointOfImpactRadius;
        private int pointOfImpactThickness;
        private Image<Bgr, byte> dartboardProjectionFrame;
        private MCvScalar dartboardProjectionColor;
        private int dartboardProjectionFrameHeight;
        private int dartboardProjectionFrameWidth;
        private int dartboardProjectionCoefficent;
        private int dartboardProjectionThickness;
        private MCvScalar surfaceProjectionLineColor;
        private int surfaceProjectionLineThickness;

        public Drawer(MainWindow view)
        {
            this.view = view;
            surfaceLineColor = view.SurfaceLineColor;
            surfaceLineThickness = view.SurfaceLineThickness;
            roiRectColor = view.RoiRectColor;
            roiRectThickness = view.RoiRectThickness;
            contourColor = view.ContourColor;
            countourThickness = view.CountourThickness;
            contourRectColor = view.ContourRectColor;
            contourRectThickness = view.ContourRectThickness;
            spikeLineColor = view.SpikeLineColor;
            spikeLineThickness = view.SpikeLineThickness;
            pointOfImpactColor = view.PointOfImpactColor;
            pointOfImpactRadius = view.PointOfImpactRadius;
            pointOfImpactThickness = view.PointOfImpactThickness;
            dartboardProjectionColor = view.DartboardProjectionColor;
            dartboardProjectionFrameHeight = view.DartboardProjectionFrameHeight;
            dartboardProjectionFrameWidth = view.DartboardProjectionFrameWidth;
            dartboardProjectionCoefficent = view.DartboardProjectionCoefficent;
            dartboardProjectionThickness = view.DartboardProjectionThickness;
            surfaceProjectionLineColor = view.SurfaceProjectionLineColor;
            surfaceProjectionLineThickness = view.SurfaceProjectionLineThickness;
        }

        public void DrawDartboardProjection()
        {
            // Draw dartboard projection
            dartboardProjectionFrame = new Image<Bgr, byte>(dartboardProjectionFrameWidth, dartboardProjectionFrameHeight);
            var dartboardCenterPoint = new Point(dartboardProjectionFrame.Width / 2, dartboardProjectionFrame.Height / 2);
            DrawCircle(dartboardProjectionFrame, dartboardCenterPoint, dartboardProjectionCoefficent * 7, dartboardProjectionColor, dartboardProjectionThickness);
            DrawCircle(dartboardProjectionFrame, dartboardCenterPoint, dartboardProjectionCoefficent * 17, dartboardProjectionColor, dartboardProjectionThickness);
            DrawCircle(dartboardProjectionFrame, dartboardCenterPoint, dartboardProjectionCoefficent * 95, dartboardProjectionColor, dartboardProjectionThickness);
            DrawCircle(dartboardProjectionFrame, dartboardCenterPoint, dartboardProjectionCoefficent * 105, dartboardProjectionColor, dartboardProjectionThickness);
            DrawCircle(dartboardProjectionFrame, dartboardCenterPoint, dartboardProjectionCoefficent * 160, dartboardProjectionColor, dartboardProjectionThickness);
            DrawCircle(dartboardProjectionFrame, dartboardCenterPoint, dartboardProjectionCoefficent * 170, dartboardProjectionColor, dartboardProjectionThickness);
            for (var i = 0; i <= 360; i += 9)
            {
                var segmentPoint1 = new Point
                                    {
                                        X = (int) (dartboardCenterPoint.X + Math.Cos(0.314159 * i - 0.15708) * dartboardProjectionCoefficent * 170),
                                        Y = (int) (dartboardCenterPoint.Y + Math.Sin(0.314159 * i - 0.15708) * dartboardProjectionCoefficent * 170)
                                    };
                var segmentPoint2 = new Point
                                    {
                                        X = (int) (dartboardCenterPoint.X + Math.Cos(0.314159 * i - 0.15708) * dartboardProjectionCoefficent * 17),
                                        Y = (int) (dartboardCenterPoint.Y + Math.Sin(0.314159 * i - 0.15708) * dartboardProjectionCoefficent * 17)
                                    };
                DrawLine(dartboardProjectionFrame, segmentPoint1, segmentPoint2, dartboardProjectionColor, dartboardProjectionThickness);
            }

            // Draw surface projection lines
            var surfaceProjectionLineCam1Point1 = new Point
                                                  {
                                                      X = (int) (dartboardCenterPoint.X + Math.Cos(-0.785398) * dartboardProjectionCoefficent * 170),
                                                      Y = (int) (dartboardCenterPoint.Y + Math.Sin(-0.785398) * dartboardProjectionCoefficent * 170)
                                                  };
            var surfaceProjectionLineCam1Point2 = new Point
                                                  {
                                                      X = (int) (dartboardCenterPoint.X + Math.Cos(-3.92699) * dartboardProjectionCoefficent * 170),
                                                      Y = (int) (dartboardCenterPoint.Y + Math.Sin(-3.92699) * dartboardProjectionCoefficent * 170)
                                                  };
            DrawLine(dartboardProjectionFrame, surfaceProjectionLineCam1Point1, surfaceProjectionLineCam1Point2, surfaceProjectionLineColor, surfaceProjectionLineThickness);

            var surfaceProjectionLineCam2Point1 = new Point
                                                  {
                                                      X = (int) (dartboardCenterPoint.X + Math.Cos(0.785398) * dartboardProjectionCoefficent * 170),
                                                      Y = (int) (dartboardCenterPoint.Y + Math.Sin(0.785398) * dartboardProjectionCoefficent * 170)
                                                  };
            var surfaceProjectionLineCam2Point2 = new Point
                                                  {
                                                      X = (int) (dartboardCenterPoint.X + Math.Cos(3.92699) * dartboardProjectionCoefficent * 170),
                                                      Y = (int) (dartboardCenterPoint.Y + Math.Sin(3.92699) * dartboardProjectionCoefficent * 170)
                                                  };
            DrawLine(dartboardProjectionFrame, surfaceProjectionLineCam2Point1, surfaceProjectionLineCam2Point2, surfaceProjectionLineColor, surfaceProjectionLineThickness);

            SaveBitmapToImageBox(dartboardProjectionFrame, view.ImageBox3);
        }

        public void DrawLine(Image<Bgr, byte> img, Point point1, Point point2, MCvScalar color, int thickness)
        {
            CvInvoke.Line(img, point1, point2, color, thickness);
        }

        public void DrawRectangle(Image<Bgr, byte> img, Rectangle rect, MCvScalar color, int thickness)
        {
            CvInvoke.Rectangle(img, rect, color, thickness);
        }

        public void DrawCircle(Image<Bgr, byte> img, Point centerpoint, int radius, MCvScalar color, int thickness)
        {
            CvInvoke.Circle(img, centerpoint, radius, color, thickness);
        }

        public void SaveBitmapToImageBox(IImage frame, Image imageBox)
        {
            using (var stream = new MemoryStream())
            {
                var imageToSave = new BitmapImage();
                frame.Bitmap.Save(stream, ImageFormat.Bmp);
                imageToSave.BeginInit();
                imageToSave.StreamSource = new MemoryStream(stream.ToArray());
                imageToSave.EndInit();
                imageBox.Source = imageToSave;
            }
        }
    }
}