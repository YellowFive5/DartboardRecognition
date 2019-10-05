﻿#region Usings

using System.ComponentModel;
using System.Drawing;
using Emgu.CV.Structure;

#endregion

namespace DartboardRecognition.Windows
{
    public partial class CamWindow
    {
        public readonly int camNumber;
        private readonly CamWindowViewModel viewModel;

        public Bgr CamRoiRectColor { get; } = new Bgr(Color.LawnGreen);
        public int CamRoiRectThickness { get; } = 5;
        public Bgr CamSurfaceLineColor { get; } = new Bgr(Color.Red);
        public int CamSurfaceLineThickness { get; } = 5;
        public MCvScalar CamContourRectColor { get; } = new Bgr(Color.Blue).MCvScalar;
        public int CamContourRectThickness { get; } = 5;
        public MCvScalar CamSpikeLineColor { get; } = new Bgr(Color.White).MCvScalar;
        public int CamSpikeLineThickness { get; } = 4;
        public int MinContourArcLength { get; } = 190;
        public int ProjectionPoiRadius { get; } = 6;
        public MCvScalar ProjectionPoiColor { get; } = new Bgr(Color.Yellow).MCvScalar;
        public int ProjectionPoiThickness { get; } = 6;

        public CamWindow(int camNumber,
                         bool runtimeCapturing,
                         bool withDetection)
        {
            InitializeComponent();

            this.camNumber = camNumber;
            Title = $"Cam {camNumber.ToString()}";

            viewModel = new CamWindowViewModel(this, runtimeCapturing, withDetection);
            DataContext = viewModel;

            viewModel.LoadSettings();

            Show();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            viewModel.OnClosing();
            viewModel.SaveSettings();
        }

        public bool DetectThrow()
        {
            return viewModel.DetectThrow();
        }

        public void FindDart()
        {
            // viewModel.FindDart();
        }

        public void FindThrow()
        {
            viewModel.FindThrow();
        }
    }
}