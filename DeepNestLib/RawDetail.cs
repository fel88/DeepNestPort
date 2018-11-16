using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeepNestLib
{
    public class RawDetail
    {
        public List<LocalContour> Outers = new List<LocalContour>();
        public List<LocalContour> Holes = new List<LocalContour>();

        public string Name { get; set; }
    }
}
