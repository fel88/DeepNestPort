using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeepNestLib
{
    public class NFP : Polygon
    {
        public bool fitted;
        public override string ToString()
        {
            var str1 = (Points != null) ? Points.Count() + "" : "null";
            return $"nfp: id: {id}; source: {source}; rotation: {rotation}; points: {str1}";
        }
    }
}
