#region Using

using System.Collections.Generic;
using System.Drawing;

#endregion

namespace DartboardRecognition
{
    public class Storage
    {
        public Queue<Point> Cam1RaysCollection { get; } = new Queue<Point>();
        public Queue<Point> Cam2RaysCollection { get; } = new Queue<Point>();
        public Queue<Point> PoiCollection { get; } = new Queue<Point>();
        public Queue<Throw> ThrowsCollection { get; } = new Queue<Throw>();

        public void SaveCam1Ray(Point rayPoint)
        {
            if (!IsContains(Cam1RaysCollection, rayPoint))
            {
                Cam1RaysCollection.Enqueue(rayPoint);
            }
        }

        public void SaveCam2Ray(Point rayPoint)
        {
            if (!IsContains(Cam2RaysCollection, rayPoint))
            {
                Cam2RaysCollection.Enqueue(rayPoint);
            }
        }

        public void SavePoi(Point point)
        {
            PoiCollection.Enqueue(point);
        }

        public void SaveThrow(Throw thrw)
        {
            ThrowsCollection.Enqueue(thrw);
        }

        private bool IsContains(Queue<Point> collection, Point point)
        {
            return collection.Contains(point);
        }

        public void ClearPoiCollection()
        {
            PoiCollection.Clear();
        }
    }
}