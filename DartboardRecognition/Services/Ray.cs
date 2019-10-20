#region Usings

using System.Drawing;

#endregion

namespace DartboardRecognition.Services
{
    public class Ray
    {
        public PointF CamPoint { get; }
        public PointF RayPoint { get; }
        public float ContourArea { get; }

        public Ray(PointF camPoint, PointF rayPoint, float contourArea)
        {
            CamPoint = camPoint;
            RayPoint = rayPoint;
            ContourArea = contourArea;
        }
    }
}