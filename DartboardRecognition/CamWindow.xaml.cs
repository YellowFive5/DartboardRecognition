#region Usings

using System.ComponentModel;
using System.Drawing;

#endregion

namespace DartboardRecognition
{
    /// <summary>
    /// Interaction logic for CamWindow.xaml
    /// </summary>
    public partial class CamWindow
    {
        private CamWindowViewModel viewModel;
        public Point SetupPoint { get; }

        public CamWindow(int camNumber)
        {
            InitializeComponent();
            viewModel = new CamWindowViewModel(this, camNumber);
            DataContext = viewModel;

            SetupPoint = camNumber == 1
                             ? new Point(13, 4)
                             : new Point(1200 - 13, 4);
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            viewModel.SaveSettings();
        }
    }
}