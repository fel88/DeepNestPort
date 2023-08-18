namespace DeepNestLib
{
    public class RectangleSheet : Sheet
    {
        public void Rebuild()
        {
            Points = new SvgPoint[] { };
            AddPoint(new SvgPoint(x, y));
            AddPoint(new SvgPoint(x + Width, y));
            AddPoint(new SvgPoint(x + Width, y + Height));
            AddPoint(new SvgPoint(x, y + Height));
        }
    }
}

