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

            var firstBestRay = rays.OrderByDescending(i => i.ContourArea).ElementAt(0);
            var secondBestRay = rays.OrderByDescending(i => i.ContourArea).ElementAt(1);
            rays.Clear();

            var poi = MeasureService.FindLinesIntersection(firstBestRay.CamPoint,
                                                           firstBestRay.RayPoint,
                                                           secondBestRay.CamPoint,
                                                           secondBestRay.RayPoint);
            var anotherThrow = PrepareThrowData(poi);
            throwsCollection.Enqueue(anotherThrow);

            drawService.ProjectionDrawLine(firstBestRay.CamPoint, firstBestRay.RayPoint, true);
            drawService.ProjectionDrawLine(secondBestRay.CamPoint, secondBestRay.RayPoint, false);
            drawService.ProjectionDrawThrow(poi, false);
            drawService.PrintThrow(anotherThrow);
        }

        private Throw PrepareThrowData(PointF poi)
        {
            var sectors = new List<int>()
                          {
                              14, 9, 12, 5, 20,
                              1, 18, 4, 13, 6,
                              10, 15, 2, 17, 3,
                              19, 7, 16, 8, 11
                          };
            var angle = MeasureService.FindAngle(drawService.ProjectionCenterPoint, poi);
            var distance = MeasureService.FindDistance(drawService.ProjectionCenterPoint, poi);
            var sector = 0;
            var multiplier = 1;

            if (distance >= drawService.ProjectionCoefficent * 95 &&
                distance <= drawService.ProjectionCoefficent * 105)
            {
                multiplier = 3;
            }
            else if (distance >= drawService.ProjectionCoefficent * 160 &&
                     distance <= drawService.ProjectionCoefficent * 170)
            {
                multiplier = 2;
            }

            // Find sector
            if (distance <= drawService.ProjectionCoefficent * 7)
            {
                sector = 50;
            }
            else if (distance > drawService.ProjectionCoefficent * 7 &&
                     distance <= drawService.ProjectionCoefficent * 17)
            {
                sector = 25;
            }
            else if (distance > drawService.ProjectionCoefficent * 170)
            {
                sector = 0;
            }
            else
            {
                var startRadSector = -2.9845105;
                var radSectorStep = 0.314159;
                var radSector = startRadSector;
                foreach (var proceedSector in sectors)
                {
                    if (angle >= radSector && angle < radSector + radSectorStep)
                    {
                        sector = proceedSector;
                        break;
                    }

                    sector = 11;

                    radSector += radSectorStep;
                }
            }

            return new Throw(poi, sector, multiplier, drawService.ProjectionFrameSide);
        }

        public void SaveRay(Ray ray)
        {
            rays.Add(ray);
        }
    }
}