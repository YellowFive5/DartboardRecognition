#region Usings

using System.Drawing;

#endregion

namespace DartboardRecognition.Services
{
    public class Throw
    {
        public PointF Poi { get; }
        public int ProjectionResolution { get; }
        public int Sector { get; }
        public int Multiplier { get; }
        public int TotalPoints { get; }
        public bool IsBull { get; }
        public bool Is25 { get; }
        public bool IsZero { get; }
        public bool IsSingle { get; }
        public bool IsDouble { get; }
        public bool IsTremble { get; }

        public Throw(PointF poi, int sector, int multiplier, int projectionSide)
        {
            Poi = poi;
            Sector = sector;
            Multiplier = multiplier;
            ProjectionResolution = projectionSide;
            TotalPoints = sector * multiplier;
            switch (multiplier)
            {
                case 1:
                    IsSingle = true;
                    break;
                case 2:
                    IsDouble = true;
                    break;
                case 3:
                    IsTremble = true;
                    break;
            }

            switch (sector)
            {
                case 25:
                    Is25 = true;
                    break;
                case 50:
                    IsBull = true;
                    break;
                case 0:
                    IsZero = true;
                    break;
            }
        }

        public override string ToString()
        {
            return $"{Multiplier} x {Sector} = {TotalPoints}";
        }
    }
}