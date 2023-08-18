namespace DeepNestLib
{
    public class SheetPlacement
    {
        public double? fitness;

        public float[] Rotation;
        public List<SheetPlacementItem>[] placements;

        public NFP[] paths;
        public double area;
        public double mergedLength;
        internal int index;
    }
}

