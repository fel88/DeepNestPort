namespace DeepNestLib
{
    public class SvgPoint
    {
        public bool exact = true;
        public override string ToString()
        {
            return "x: " + x + "; y: " + y;
        }
        public int id;
        public SvgPoint(double _x, double _y)
        {
            x = _x;
            y = _y;
        }
        internal SvgPoint(SvgPoint point)
        {
            this.exact = point.exact;
            this.id = point.id;
            this.marked = point.marked;
            this.x = point.x;
            this.y = point.y;
        }
        public bool marked;
        public double x;
        public double y;
        public SvgPoint Clone()
        {
            return new SvgPoint(this);
        }
    }
}

