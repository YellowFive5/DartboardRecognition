#region Usings

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Point = System.Drawing.Point;

#endregion

namespace DartboardRecognition.Services
{
    public class DrawService
    {
        public void DrawLine(Image<Bgr, byte> image,
                             PointF point1,
                             PointF point2,
                             MCvScalar color,
                             int thickness)
        {
            CvInvoke.Line(image,
                          new Point((int) point1.X, (int) point1.Y),
                          new Point((int) point2.X, (int) point2.Y),
                          color,
                          thickness);
        }

        public void DrawRectangle(Image<Bgr, byte> image,
                                  Rectangle rectangle,
                                  MCvScalar color,
                                  int thickness)
        {
            CvInvoke.Rectangle(image, rectangle, color, thickness);
        }

        public void DrawCircle(Image<Bgr, byte> image,
                               PointF centerpoint,
                               int radius,
                               MCvScalar color,
                               int thickness)
        {
            CvInvoke.Circle(image,
                            new Point((int) centerpoint.X, (int) centerpoint.Y),
                            radius,
                            color,
                            thickness);
        }

        public void DrawString(Image<Bgr, byte> image,
                               string text,
                               int pointX,
                               int pointY,
                               double scale,
                               Bgr color,
                               int thickness)
        {
            image.Draw(text,
                       new Point(pointX, pointY),
                       FontFace.HersheySimplex,
                       scale,
                       color,
                       thickness);
        }

        public BitmapImage ToBitmap(IImage image)
        {
            using (var stream = new MemoryStream())
            {
                var imageToSave = new BitmapImage();
                image.Bitmap.Save(stream, ImageFormat.Bmp);
                imageToSave.BeginInit();
                imageToSave.StreamSource = new MemoryStream(stream.ToArray());
                imageToSave.EndInit();
                return imageToSave;
            }
        }

        public void TresholdRoiRegion(CamService cam)
        {
            cam.roiTrasholdFrame = cam.roiFrame.Clone().Convert<Gray, byte>().Not();
            cam.roiTrasholdFrame._SmoothGaussian(5);
            cam.roiTrasholdFrame._ThresholdBinary(new Gray(cam.tresholdMinSlider),
                                                  new Gray(cam.tresholdMaxSlider));
        }
    }
}