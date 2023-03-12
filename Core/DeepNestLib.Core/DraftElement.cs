using System.Drawing;

namespace DeepNestLib
{
    public abstract class DraftElement
    {
        public object Tag;
        public PointF Start;
        public PointF End;

        public abstract double Length { get; }

        public abstract void Reverse();
        public abstract PointF[] GetPoints();
    }
}
