using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace DeepNestLib
{
    public class NFP : IStringify
    {
        public int Z;
        public bool fitted { get { return sheet != null; } }
        public NFP sheet;
        public override string ToString()
        {
            var str1 = (Points != null) ? Points.Count() + "" : "null";
            return $"nfp: id: {id}; source: {source}; rotation: {rotation}; points: {str1}";
        }
        public NFP()
        {
            Points = new SvgPoint[] { };
        }

        public string Name { get; set; }
        public void AddPoint(SvgPoint point)
        {
            var list = Points.ToList();
            list.Add(point);
            Points = list.ToArray();
        }

        #region gdi section
        public bool isBin;

        #endregion
        public void reverse()
        {
            Points = Points.Reverse().ToArray();
        }

        public StringBuilder GetXml()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<root>");
            sb.AppendLine("<region>");
            foreach (var item in Points)
            {
                sb.AppendLine($"<point x=\"{item.x}\" y=\"{item.y}\"/>");
            }
            sb.AppendLine("</region>");
            if (children != null)
                foreach (var item in children)
                {
                    sb.AppendLine("<region>");
                    foreach (var citem in item.Points)
                    {
                        sb.AppendLine($"<point x=\"{citem.x}\" y=\"{citem.y}\"/>");
                    }
                    sb.AppendLine("</region>");
                }

            sb.AppendLine("</root>");

            return sb;
        }
        public double x { get; set; }
        public double y { get; set; }

        public double WidthCalculated
        {
            get
            {
                var maxx = Points.Max(z => z.x);
                var minx = Points.Min(z => z.x);

                return maxx - minx;
            }
        }

        public double HeightCalculated
        {
            get
            {
                var maxy = Points.Max(z => z.y);
                var miny = Points.Min(z => z.y);
                return maxy - miny;
            }
        }

        public SvgPoint this[int ind]
        {
            get
            {
                return Points[ind];
            }
        }

        public List<NFP> children;




        public int Length
        {
            get
            {
                return Points.Length;
            }
        }

        //public float? width;
        //public float? height;
        public int length
        {
            get
            {
                return Points.Length;
            }
        }

        public int Id;
        public int id
        {
            get
            {
                return Id;
            }
            set
            {
                Id = value;
            }
        }

        public double? offsetx;
        public double? offsety;
        public int? source = null;
        public float Rotation;

        public SvgPoint Center()
        {
            var m = new Matrix();
            m.Translate((float)x, (float)y);
            m.Rotate(rotation);

            var pnts = Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray();
            m.TransformPoints(pnts);
            var maxx = pnts.Max(z => z.X);
            var minx = pnts.Min(z => z.X);
            var maxy = pnts.Max(z => z.Y);
            var miny = pnts.Min(z => z.Y);
            return new SvgPoint((maxx + minx) / 2, (maxy + miny) / 2);
        }

        public float rotation
        {
            get
            {
                return Rotation;
            }
            set
            {
                Rotation = value;
            }
        }
        public SvgPoint[] Points;
        public float Area
        {
            get
            {
                float ret = 0;
                if (Points.Length < 3) return 0;
                List<SvgPoint> pp = new List<SvgPoint>();
                pp.AddRange(Points);
                pp.Add(Points[0]);
                for (int i = 1; i < pp.Count; i++)
                {
                    var s0 = pp[i - 1];
                    var s1 = pp[i];
                    ret += (float)(s0.x * s1.y - s0.y * s1.x);
                }
                return (float)Math.Abs(ret / 2);
            }
        }

        internal void push(SvgPoint svgPoint)
        {
            List<SvgPoint> points = new List<SvgPoint>();
            if (Points == null)
            {
                Points = new SvgPoint[] { };
            }
            points.AddRange(Points);
            points.Add(svgPoint);
            Points = points.ToArray();

        }

        public NFP slice(int v)
        {
            var ret = new NFP();
            List<SvgPoint> pp = new List<SvgPoint>();
            for (int i = v; i < length; i++)
            {
                pp.Add(new SvgPoint(this[i].x, this[i].y));

            }
            ret.Points = pp.ToArray();
            return ret;
        }

        public string stringify()
        {
            throw new NotImplementedException();
        }
    }
}
