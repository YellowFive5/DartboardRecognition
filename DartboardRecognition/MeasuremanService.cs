#region Usings

using System;
using System.Drawing;

#endregion

namespace DartboardRecognition
{
    public partial class Measureman
    {
        public static Point FindLinesIntersection(Point line1Point1, Point line1Point2, Point line2Point1,
                                                  Point line2Point2)
        {
            var tolerance = 0.001;
            double x1 = line1Point1.X;
            double y1 = line1Point1.Y;
            double x2 = line1Point2.X;
            double y2 = line1Point2.Y;
            double x3 = line2Point1.X;
            double y3 = line2Point1.Y;
            double x4 = line2Point2.X;
            double y4 = line2Point2.Y;
            double x;
            double y;

            if (Math.Abs(x1 - x2) < tolerance)
            {
                var m2 = (y4 - y3) / (x4 - x3);
                var c2 = -m2 * x3 + y3;
                x = x1;
                y = c2 + m2 * x1;
            }
            else if (Math.Abs(x3 - x4) < tolerance)
            {
                var m1 = (y2 - y1) / (x2 - x1);
                var c1 = -m1 * x1 + y1;
                x = x3;
                y = c1 + m1 * x3;
            }
            else
            {
                var m1 = (y2 - y1) / (x2 - x1);
                var c1 = -m1 * x1 + y1;
                var m2 = (y4 - y3) / (x4 - x3);
                var c2 = -m2 * x3 + y3;
                x = (c1 - c2) / (m2 - m1);
                y = c2 + m2 * x;
            }

            return new Point {X = (int) x, Y = (int) y};
        }

        private static Point FindMiddle(Point point1, Point point2)
        {
            var mpX = (point1.X + point2.X) / 2;
            var mpY = (point1.Y + point2.Y) / 2;
            return new Point(mpX, mpY);
        }

        public static int FindDistance(Point point1, Point point2)
        {
            return (int) Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
        }

        public static double FindAngle(Point point1, Point point2)
        {
            return Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
        }
    }
}