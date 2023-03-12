using System.Drawing;

namespace DeepNestLib
{
    public class LineElement : DraftElement
    {
        public override PointF[] GetPoints()
        {
            return new[] { Start, End };
        }
        public override double Length => Start.DistTo(End);
        public override void Reverse()
        {
            var temp = End;
            End = Start;
            Start = temp;
        }
    }

}
