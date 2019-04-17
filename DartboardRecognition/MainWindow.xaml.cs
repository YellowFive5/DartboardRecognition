using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Emgu.CV;
using Emgu.CV.Structure;

namespace DartboardRecognition
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        VideoCapture capture = new VideoCapture(0);
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CaptureButtonClick(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Hooks.DispatcherInactive += new EventHandler(CaptureImage);
        }

        private void CaptureImage(object sender, EventArgs e)
        {
            using (Image<Bgr, byte> frame = capture.QueryFrame().ToImage<Bgr, byte>())
            {
                if (frame != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        frame.Draw(new LineSegment2D(
                            new System.Drawing.Point(5,400),
                            new System.Drawing.Point(650, 400)),
                            new Bgr(0, 0, 255),
                            5);
                        frame.Bitmap.Save(stream, ImageFormat.Bmp);

                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = new MemoryStream(stream.ToArray());
                        bitmap.EndInit();

                        ImageBox.Source = bitmap;

                        //TransformedBitmap bitmap2 = new TransformedBitmap();
                        //bitmap2.BeginInit();
                        //bitmap2.Source = bitmap.Clone();
                        //bitmap2.Transform = new ScaleTransform(-1, 1, 0, 0);
                        //bitmap2.EndInit();
                        //ImageBox2.Source = bitmap2;
                    };
                }
            }
        }

        private void StopButtonClick(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Hooks.DispatcherInactive -= new EventHandler(CaptureImage);
            ImageBox.Source = null;
            ImageBox2.Source = null;
        }
    }
}
