using DeepNestLib;

namespace DeepNestPort
{
    public class SheetLoadInfo
    {
        public NFP Nfp;
        public string Path;
        public string Info { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public int Quantity { get; set; }
    }
}
