using System.Drawing;

namespace DeepNestLib
{
    public class PolylineElement : DraftElement
    {
        public PointF[] Points;

        public override double Length
        {
            get
            {
                double len = 0;
                for (int i = 1; i < Points.Length; i++)
                {
                    len += Points[i - 1].DistTo(Points[i]);
                }
                return len;
            }
        }

        public override PointF[] GetPoints()
        {
            return Points;
        }

        public override void Reverse()
        {
            Points = Points.Reverse().ToArray();
            Start = Points[0];
            End = Points[Points.Length - 1];
        }
    }
}
