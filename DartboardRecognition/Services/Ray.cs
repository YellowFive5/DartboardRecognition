#region Usings

using System.Drawing;

#endregion

namespace DartboardRecognition.Services
{
    public class Ray
    {
        public PointF CamPoint { get; }
        public PointF RayPoint { get; }
        public float ContourWidth { get; }

        public Ray(PointF camPoint, PointF rayPoint, float contourWidth)
        {
            CamPoint = camPoint;
            RayPoint = rayPoint;
            ContourWidth = contourWidth;
        }
    }
}