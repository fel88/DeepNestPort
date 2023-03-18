using DeepNestLib;

namespace DeepNestPort.Core
{
    public class SheetLoadInfo
    {
        public NFP Nfp;
        public string Path;
        public string Info { get; set; }
        float _width;
        public float Width
        {
            get => _width; set
            {
                _width = value;                
            }
        }
        public float Height { get; set; }
        public int Quantity { get; set; }
    }
}