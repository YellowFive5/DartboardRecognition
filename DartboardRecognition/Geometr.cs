#region Usings

using System;
using Point = System.Drawing.Point;

#endregion

namespace DartboardRecognition
{
    public class Geometr
    {
        public Point FindMiddlePoint(Point point1, Point point2)
        {
            var mpX = (point1.X + point2.X) / 2;
            var mpY = (point1.Y + point2.Y) / 2;
            return new Point(mpX, mpY);
        }

        public int FindDistance(Point point1, Point point2)
        {
            return (int) Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
        }

        public Point? FindIntersectionPoint(Point line1Point1, Point line1Point2, Point line2Point1,
                                            Point line2Point2)
        {
            float a1 = line1Point2.Y - line1Point1.Y;
            float b1 = line1Point2.X - line1Point1.X;
            var c1 = a1 * line1Point1.X + b1 * line1Point1.Y;

            float a2 = line2Point2.Y - line2Point1.Y;
            float b2 = line2Point2.X - line2Point1.X;
            var c2 = a2 * line2Point1.X + b2 * line2Point1.Y;

            var det = a1 * b2 - a2 * b1;
            if (det == 0)
            {
                return null;
            }
            else
            {
                var x = (int) ((b2 * c1 - b1 * c2) / det);
                var y = (int) ((a1 * c2 - a2 * c1) / det);
                return new Point(x, y);
            }
        }
    }
}