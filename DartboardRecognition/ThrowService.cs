using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;

namespace DartboardRecognition
{
    public class ThrowService
    {
        private MainWindow view;
        private Drawman drawman;
        private Image<Bgr, byte> dartboardProjectionFrame;
        private Point projectionCenterPoint;
        private Stack<Point> cam1RayPoint;
        private Stack<Point> cam2RayPoint;
        private Queue<Throw> throws;

        public ThrowService(MainWindow view, Drawman drawman, CancellationToken cancelToken)
        {
            this.view = view;
            this.drawman = drawman;
            dartboardProjectionFrame = new Image<Bgr, byte>(view.ProjectionFrameWidth, view.ProjectionFrameHeight);
            projectionCenterPoint = new Point(dartboardProjectionFrame.Width / 2, dartboardProjectionFrame.Height / 2);
            cam1RayPoint = new Stack<Point>();
            cam2RayPoint = new Stack<Point>();
            throws = new Queue<Throw>();
        }

        public void AwaitForThrows(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                var anotherThrowDetected = cam1RayPoint.Count == 1 && cam2RayPoint.Count == 1;
                if (anotherThrowDetected)
                {
                    FindProjectionPoi();
                }
            }
        }

        private void FindProjectionPoi()
        {
            var poi = Measureman.FindLinesIntersection(view.Cam1SetupPoint,
                                                       cam1RayPoint.Pop(),
                                                       view.Cam2SetupPoint,
                                                       cam2RayPoint.Pop());

            var anotherThrow = PrepareThrowData(poi);
            throws.Enqueue(anotherThrow);

            // drawman.DrawCircle(dartboardWorkingProjectionFrame, poi, view.PoiRadius, view.PoiColor, view.PoiThickness);
            // drawman.SaveToImageBox(dartboardWorkingProjectionFrame, view.DartboardProjectionImageBox);
            // view.PointsBox.Text += $"{anotherThrow.Sector} x {anotherThrow.Multiplier} = {anotherThrow.TotalPoints}\n";
        }

        private Throw PrepareThrowData(Point poi)
        {
            var startRadSector = -1.41372;
            var radSectorStep = 0.314159;
            var angle = Measureman.FindAngle(projectionCenterPoint, poi);
            var distance = Measureman.FindDistance(projectionCenterPoint, poi);
            var sector = 0;
            var multiplier = 1;

            if (distance <= view.ProjectionCoefficent * 7)
            {
                sector = 50;
            }

            if (distance > view.ProjectionCoefficent * 7 &&
                distance <= view.ProjectionCoefficent * 17)
            {
                sector = 25;
            }

            if (distance > view.ProjectionCoefficent * 170)
            {
                sector = 0;
            }

            if (distance >= view.ProjectionCoefficent * 95 &&
                distance <= view.ProjectionCoefficent * 105)
            {
                multiplier = 3;
            }

            if (distance >= view.ProjectionCoefficent * 160 &&
                distance <= view.ProjectionCoefficent * 170)
            {
                multiplier = 2;
            }

            // Find sector
            if (angle >= -1.41372 && angle < -1.099561)
            {
                sector = 1;
            }
            else if (angle >= -1.099561 && angle < -0.785402)
            {
                sector = 18;
            }
            else if (angle >= -0.785402 && angle < -0.471243)
            {
                sector = 4;
            }
            else if (angle >= -0.471243 && angle < -0.157084)
            {
                sector = 13;
            }
            else if (angle >= -0.157084 && angle < 0.157075)
            {
                sector = 6;
            }
            else if (angle >= 0.157075 && angle < 0.471234)
            {
                sector = 10;
            }
            else if (angle >= 0.471234 && angle < 0.785393)
            {
                sector = 15;
            }
            else if (angle >= 0.785393 && angle < 1.099552)
            {
                sector = 2;
            }
            else if (angle >= 1.099552 && angle < 1.413711)
            {
                sector = 17;
            }
            else if (angle >= 1.413711 && angle < 1.72787)
            {
                sector = 3;
            }
            else if (angle >= 1.72787 && angle < 2.042029)
            {
                sector = 19;
            }
            else if (angle >= 2.042029 && angle < 2.356188)
            {
                sector = 7;
            }
            else if (angle >= 2.356188 && angle < 2.670347)
            {
                sector = 16;
            }
            else if (angle >= 2.670347 && angle < 2.984506)
            {
                sector = 8;
            }
            else if (angle >= 2.984506 && angle < 3.14159 ||
                     angle >= -3.14159 && angle < -3.29868)
            {
                sector = 11;
            }
            else if (angle >= -3.29868 && angle < -2.67036)
            {
                sector = 14;
            }
            else if (angle >= -2.67036 && angle < -2.3562)
            {
                sector = 9;
            }
            else if (angle >= -2.3562 && angle < -2.04204)
            {
                sector = 12;
            }
            else if (angle >= -2.04204 && angle < -1.72788)
            {
                sector = 5;
            }
            else if (angle >= -1.72788 && angle < -1.41372)
            {
                sector = 20;
            }

            return new Throw(poi, sector, multiplier, dartboardProjectionFrame);
        }

        public void SaveRay(Point rayPoint, Cam cam)
        {
            if (cam is Cam1)
            {
                cam1RayPoint.Push(rayPoint);
            }
            else
            {
                cam2RayPoint.Push(rayPoint);
            }
        }
    }
}