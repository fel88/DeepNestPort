using DeepNestLib;
using SvgPathProperties;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace DeepNestConsole.Core
{
    public class CsvParser
    {
        public static RawDetail[] Load(string path, bool split = false)
        {

            var fi = new FileInfo(path);
            RawDetail s = new RawDetail();
            s.Name = fi.Name;
            List<GraphicsPath> paths = new List<GraphicsPath>();

            double scale = 1;
            double rightMax = 0;
            var lines = File.ReadAllLines(path);
            List<RawDetail> ret = new List<RawDetail>();

            List<PointF> points = new List<PointF>();
            foreach (var str in lines)
            {
                var spl2 = str.Split(new string[] { " ", ";", "=" }, StringSplitOptions.RemoveEmptyEntries);
                var ar = spl2.Select(z => float.Parse(z, CultureInfo.InvariantCulture)).ToArray();
                points.Add(new PointF(ar[0], ar[1]));
                rightMax = Math.Max(rightMax, ar[0]);

                s.Outers.Add(new LocalContour() { Points = points.ToList() });
            }

            if (split)
            {
                //split
                var nfps = s.Outers;
                for (int i = 0; i < nfps.Count; i++)
                {
                    for (int j = 0; j < nfps.Count; j++)
                    {
                        if (i != j)
                        {
                            var d2 = nfps[i];
                            var d3 = nfps[j];
                            var f0 = d3.Points[0];

                            if (GeometryUtil.pnpoly(d2.Points.ToArray(), f0.X, f0.Y))
                            {
                                d3.Parent = d2;
                                if (!d2.Childrens.Contains(d3))
                                {
                                    d2.Childrens.Add(d3);
                                }
                            }
                        }
                    }
                }
                var tops = nfps.Where(z => z.Parent == null).ToArray();
                for (int i = 0; i < tops.Length; i++)
                {
                    LocalContour? item = tops[i];
                    RawDetail rr = new RawDetail();
                    rr.Name = fi.FullName + "_" + i;
                    rr.Outers.Add(item);
                    rr.Holes.AddRange(item.Childrens);
                    ret.Add(rr);
                }
            }
            else
            {
                ret.Add(s);
            }

            foreach (var item in ret)
            {
                item.Scale(scale / rightMax);
            }

            return ret.ToArray();
        }

        public static void Export(string path, IEnumerable<NFP> polygons, IEnumerable<NFP> sheets)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("	<svg version=\"1.1\" id=\"svg2\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\"   xml:space=\"preserve\">");

            foreach (var item in polygons.Union(sheets))
            {
                if (!sheets.Contains(item))
                {
                    if (!item.fitted) continue;
                }
                var m = new Matrix();
                m.Translate((float)item.x, (float)item.y);
                m.Rotate(item.rotation);

                PointF[] pp = item.Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray();
                m.TransformPoints(pp);
                var points = pp.Select(z => new SvgPoint(z.X, z.Y)).ToArray();

                string fill = "lightblue";
                if (sheets.Contains(item))
                {
                    fill = "none";
                }

                sb.AppendLine($"<path fill=\"{fill}\"  stroke=\"black\" d=\"");
                for (int i = 0; i < points.Count(); i++)
                {
                    var p = points[i];
                    string coord = p.x.ToString().Replace(",", ".") + " " + p.y.ToString().Replace(",", ".");
                    if (i == 0)
                    {
                        sb.Append("M" + coord + " ");
                        continue;
                    }

                    sb.Append("L" + coord + " ");
                }
                sb.Append("z ");
                if (item.children != null)
                {
                    foreach (var citem in item.children)
                    {
                        pp = citem.Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray();
                        m.TransformPoints(pp);
                        points = pp.Select(z => new SvgPoint(z.X, z.Y)).Reverse().ToArray();

                        for (int i = 0; i < points.Count(); i++)
                        {
                            var p = points[i];
                            string coord = p.x.ToString().Replace(",", ".") + " " + p.y.ToString().Replace(",", ".");
                            if (i == 0)
                            {
                                sb.Append("M" + coord + " ");
                                continue;
                            }

                            sb.Append("L" + coord + " ");
                        }
                        sb.Append("z ");
                    }
                }
                sb.Append("\"/>");

            }
            sb.AppendLine("</svg>");
            File.WriteAllText(path, sb.ToString());
        }

        internal static RawDetail[] Parse(string value, bool split = false)
        {
            RawDetail s = new RawDetail();
            List<GraphicsPath> paths = new List<GraphicsPath>();

            double scale = 1;
            double rightMax = 0;
            var lines = value.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            List<RawDetail> ret = new List<RawDetail>();

            List<PointF> points = new List<PointF>();
            foreach (var str in lines)
            {
                var spl2 = str.ToLower().Split(new string[] { "\t", "z", "y", "x", ", ", " ", "; ", "=" }, StringSplitOptions.RemoveEmptyEntries);
                var ar = spl2.Select(z => float.Parse(z, CultureInfo.InvariantCulture)).ToArray();
                if (ar.Length == 0)
                    continue;

                points.Add(new PointF(ar[0], ar[1]));
                rightMax = Math.Max(rightMax, ar[0]);
            }

            s.Outers.Add(new LocalContour() { Points = points.ToList() });

            if (split)
            {
                //split
                var nfps = s.Outers;
                for (int i = 0; i < nfps.Count; i++)
                {
                    for (int j = 0; j < nfps.Count; j++)
                    {
                        if (i != j)
                        {
                            var d2 = nfps[i];
                            var d3 = nfps[j];
                            var f0 = d3.Points[0];

                            if (GeometryUtil.pnpoly(d2.Points.ToArray(), f0.X, f0.Y))
                            {
                                d3.Parent = d2;
                                if (!d2.Childrens.Contains(d3))
                                {
                                    d2.Childrens.Add(d3);
                                }
                            }
                        }
                    }
                }
                var tops = nfps.Where(z => z.Parent == null).ToArray();
                for (int i = 0; i < tops.Length; i++)
                {
                    LocalContour? item = tops[i];
                    RawDetail rr = new RawDetail();
                    rr.Outers.Add(item);
                    rr.Holes.AddRange(item.Childrens);
                    ret.Add(rr);
                }
            }
            else
            {
                ret.Add(s);
            }


            return ret.ToArray();
        }
    }
}
