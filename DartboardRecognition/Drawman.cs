#region Usings

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Point = System.Drawing.Point;
using Image = System.Windows.Controls.Image;

#endregion

namespace DartboardRecognition
{
    public class Drawman
    {
        private MainWindow view;

        public Drawman(MainWindow view)
        {
            this.view = view;
        }

        public void DrawLine(Image<Bgr, byte> image, Point point1, Point point2, MCvScalar color, int thickness)
        {
            CvInvoke.Line(image, point1, point2, color, thickness);
        }

        public void DrawRectangle(Image<Bgr, byte> image, Rectangle rectangle, MCvScalar color, int thickness)
        {
            CvInvoke.Rectangle(image, rectangle, color, thickness);
        }

        public void DrawCircle(Image<Bgr, byte> image, Point centerpoint, int radius, MCvScalar color, int thickness)
        {
            CvInvoke.Circle(image, centerpoint, radius, color, thickness);
        }

        public void DrawString(Image<Bgr, byte> image, string text, int pointX, int pointY, double scale, Bgr color, int thickness)
        {
            image.Draw(text, new Point(pointX, pointY), FontFace.HersheySimplex, scale, color, thickness);
        }

        public void SaveToImageBox(IImage image, Image imageBox)
        {
            var bitmap = ConvertToBitmap(image);
            imageBox.Dispatcher.Invoke(() => { imageBox.Source = bitmap; });
        }

        public void SaveToImageBox1(IImage image, Image imageBox)
        {
            var bitmap = ConvertToBitmap(image);
            view.ImageBox1.Dispatcher.BeginInvoke(new Action(() => view.ImageBox1.Source = bitmap), DispatcherPriority.ContextIdle);
        }

        public void SaveToImageBox2(IImage image, Image imageBox)
        {
            var bitmap = ConvertToBitmap(image);
            view.ImageBox2.Dispatcher.BeginInvoke(new Action(() => view.ImageBox2.Source = bitmap), DispatcherPriority.ContextIdle);
        }

        private BitmapImage ConvertToBitmap(IImage image)
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

        public void TresholdRoiRegion(Cam cam)
        {
            cam.roiTrasholdFrame = cam.roiFrame.Clone().Convert<Gray, byte>().Not();
            cam.roiTrasholdFrame._ThresholdBinary(new Gray(cam.tresholdMinSlider),
                                                  new Gray(cam.tresholdMaxSlider));
        }
    }
}