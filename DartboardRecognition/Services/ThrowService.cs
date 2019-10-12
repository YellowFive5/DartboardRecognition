#region Usings

using System.Collections.Generic;
using System.Drawing;
using System.Linq;

#endregion

namespace DartboardRecognition.Services
{
    public class ThrowService
    {
        private readonly DrawService drawService;
        private readonly List<Ray> rays;
        private readonly Queue<Throw> throwsCollection;

        public ThrowService(DrawService drawService)
        {
            this.drawService = drawService; 
            rays = new List<Ray>();
            throwsCollection = new Queue<Throw>();
        }

        public void CalculateAndSaveThrow()
        {
            if (rays.Count < 2)
            {
                rays.Clear();
                return;
            }

            var firstBestRay = rays.OrderByDescending(i => i.ContourWidth).ElementAt(0);
            var secondBestRay = rays.OrderByDescending(i => i.ContourWidth).ElementAt(1);
            rays.Clear();

            var poi = MeasureService.FindLinesIntersection(firstBestRay.CamPoint,
                                                           firstBestRay.RayPoint,
                                                           secondBestRay.CamPoint,
                                                           secondBestRay.RayPoint);
            var anotherThrow = PrepareThrowData(poi);
            throwsCollection.Enqueue(anotherThrow);

            drawService.DrawThrow(poi);

            // mainWindow.Dispatcher.Invoke(new Action(() => mainWindow.PointsBox.Text = ""));
            // mainWindow.Dispatcher.Invoke(new Action(() => mainWindow.PointsBox.Text += $"{anotherThrow.Sector} x {anotherThrow.Multiplier} = {anotherThrow.TotalPoints}\n"));
        }

        private Throw PrepareThrowData(PointF poi)
        {
            var startRadSector = -1.41372;
            var radSectorStep = 0.314159;
            var angle = MeasureService.FindAngle(drawService.ProjectionCenterPoint, poi);
            var distance = MeasureService.FindDistance(drawService.ProjectionCenterPoint, poi);
            var sector = 0;
            var multiplier = 1;

            if (distance >= drawService.ProjectionCoefficent * 95 &&
                distance <= drawService.ProjectionCoefficent * 105)
            {
                multiplier = 3;
            }

            if (distance >= drawService.ProjectionCoefficent * 160 &&
                distance <= drawService.ProjectionCoefficent * 170)
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

            if (distance <= drawService.ProjectionCoefficent * 7)
            {
                sector = 50;
            }

            if (distance > drawService.ProjectionCoefficent * 7 &&
                distance <= drawService.ProjectionCoefficent * 17)
            {
                sector = 25;
            }

            if (distance > drawService.ProjectionCoefficent * 170)
            {
                sector = 0;
            }

            return new Throw(poi, sector, multiplier, drawService.ProjectionFrameSide);
        }

        public void SaveRay(Ray ray)
        {
            rays.Add(ray);
        }
    }
}