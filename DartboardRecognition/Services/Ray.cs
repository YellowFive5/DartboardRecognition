#region Usings

using System.Drawing;

#endregion

namespace DartboardRecognition.Services
{
    public class Ray
    {
        public PointF CamPoint { get; }
        public PointF RayPoint { get; }
        public double ContourArc { get; }

        public Ray(PointF camPoint, PointF rayPoint, double contourArc)
        {
            CamPoint = camPoint;
            RayPoint = rayPoint;
            ContourArc = contourArc;
        }
    }
}