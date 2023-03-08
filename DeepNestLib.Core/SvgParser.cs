using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DeepNestLib
{
    public class SvgParser
    {
        public static RawDetail LoadSvg(string path)
        {
            XDocument doc = XDocument.Load(path);
            var fi = new FileInfo(path);
            RawDetail s = new RawDetail();
            s.Name = fi.Name;
            List<GraphicsPath> paths = new List<GraphicsPath>();
            var ns = doc.Descendants().First().Name.Namespace.NamespaceName;


            foreach (var item in doc.Descendants("path"))
            {
                var dd = (item.Attribute("d").Value);

                List<string> cmnds = new List<string>();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < dd.Length; i++)
                {
                    if (char.IsLetter(dd[i]))
                    {
                        if (sb.Length > 0)
                        {
                            cmnds.Add(sb.ToString());
                        }
                        sb = new StringBuilder();
                    }
                    sb.Append(dd[i]);
                }
                if (sb.Length > 0)
                {
                    cmnds.Add(sb.ToString());
                }
                //GraphicsPath p = new GraphicsPath();


                //polygons.Add(new SvgNestPort.Polygon() { orig = item,
                //    /*Points = p.PathPoints.Select(z => new SvgPoint(z.X, z.Y)).ToArray()*/ });

            }
            foreach (var item in doc.Descendants("rect"))
            {
                float xx = 0;
                float yy = 0;
                if (item.Attribute("x") != null)
                {
                    xx = float.Parse(item.Attribute("x").Value);
                }
                if (item.Attribute("y") != null)
                {
                    yy = float.Parse(item.Attribute("y").Value);
                }
                var ww = float.Parse(item.Attribute("width").Value);
                var hh = float.Parse(item.Attribute("height").Value);
                GraphicsPath p = new GraphicsPath();
                p.AddRectangle(new RectangleF(xx, yy, ww, hh));
                s.Outers.Add(new LocalContour() { Points = p.PathPoints.ToList() });

            }

            foreach (var item in doc.Descendants(XName.Get("polygon", ns)))
            {
                var str = item.Attribute("points").Value.ToString();
                var spl = str.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                List<PointF> points = new List<PointF>();
                foreach (var sitem in spl)
                {
                    var spl2 = sitem.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                    var ar = spl2.Select(z => float.Parse(z, CultureInfo.InvariantCulture)).ToArray();
                    points.Add(new PointF(ar[0], ar[1]));
                }
                s.Outers.Add(new LocalContour() { Points = points.ToList() });
            }


            return s;
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

        public static SvgConfig Conf = new SvgConfig();
        // return a polygon from the given SVG element in the form of an array of points
        public static NFP polygonify(XElement element)
        {
            List<SvgPoint> poly = new List<SvgPoint>();
            int i;

            switch (element.Name.LocalName)
            {
                case "polygon":
                case "polyline":
                    {
                        var pp = element.Attribute("points").Value;
                        var spl = pp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var item in spl)
                        {
                            var spl2 = item.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                            var x = float.Parse(spl2[0], CultureInfo.InvariantCulture);
                            var y = float.Parse(spl2[1], CultureInfo.InvariantCulture);
                            poly.Add(new SvgPoint(x, y));
                        }

                    }
                    break;
                case "rect":
                    {
                        float x = 0;
                        float y = 0;
                        if (element.Attribute("x") != null)
                        {
                            x = float.Parse(element.Attribute("x").Value, CultureInfo.InvariantCulture);
                        }
                        if (element.Attribute("y") != null)
                        {
                            y = float.Parse(element.Attribute("y").Value, CultureInfo.InvariantCulture);
                        }
                        var w = float.Parse(element.Attribute("width").Value, CultureInfo.InvariantCulture);
                        var h = float.Parse(element.Attribute("height").Value, CultureInfo.InvariantCulture);
                        poly.Add(new SvgPoint(x, y));
                        poly.Add(new SvgPoint(x + w, y));
                        poly.Add(new SvgPoint(x + w, y + h));
                        poly.Add(new SvgPoint(x, y + h));
                    }


                    break;
                case "circle":
                    throw new NotImplementedException();

                    break;
                case "ellipse":
                    throw new NotImplementedException();

                    break;
                case "path":
                    throw new NotImplementedException();

                    //				// we'll assume that splitpath has already been run on this path, and it only has one M/m command 
                    //				var seglist = element.pathSegList;

                    //            var firstCommand = seglist.getItem(0);
                    //            var lastCommand = seglist.getItem(seglist.numberOfItems - 1);

                    //            var x = 0, y = 0, x0 = 0, y0 = 0, x1 = 0, y1 = 0, x2 = 0, y2 = 0, prevx = 0, prevy = 0, prevx1 = 0, prevy1 = 0, prevx2 = 0, prevy2 = 0;

                    //            for (var i = 0; i < seglist.numberOfItems; i++)
                    //            {
                    //                var s = seglist.getItem(i);
                    //                var command = s.pathSegTypeAsLetter;

                    //                prevx = x;
                    //                prevy = y;

                    //                prevx1 = x1;
                    //                prevy1 = y1;

                    //                prevx2 = x2;
                    //                prevy2 = y2;

                    //                if (/[MLHVCSQTA] /.test(command))
                    //                {
                    //                    if ('x1' in s) x1 = s.x1;
                    //            if ('x2' in s) x2 = s.x2;
                    //            if ('y1' in s) y1 = s.y1;
                    //            if ('y2' in s) y2 = s.y2;
                    //            if ('x' in s) x = s.x;
                    //            if ('y' in s) y = s.y;
                    //        }
                    //					else{
                    //						if ('x1' in s) x1=x+s.x1;
                    //						if ('x2' in s) x2=x+s.x2;
                    //						if ('y1' in s) y1=y+s.y1;
                    //						if ('y2' in s) y2=y+s.y2;							
                    //						if ('x'  in s) x+=s.x;
                    //						if ('y'  in s) y+=s.y;
                    //					}
                    //					switch(command){
                    //						// linear line types
                    //						case 'm':
                    //						case 'M':
                    //						case 'l':
                    //						case 'L':
                    //						case 'h':
                    //						case 'H':
                    //						case 'v':
                    //						case 'V':
                    //							var point = { };
                    //    point.x = x;
                    //							point.y = y;
                    //							poly.push(point);
                    //						break;
                    //						// Quadratic Beziers
                    //						case 't':
                    //						case 'T':
                    //						// implicit control point
                    //						if(i > 0 && /[QqTt]/.test(seglist.getItem(i-1).pathSegTypeAsLetter)){
                    //							x1 = prevx + (prevx-prevx1);
                    //							y1 = prevy + (prevy-prevy1);
                    //						}
                    //						else{
                    //							x1 = prevx;
                    //							y1 = prevy;
                    //						}
                    //						case 'q':
                    //						case 'Q':
                    //							var pointlist = GeometryUtil.QuadraticBezier.linearize({x: prevx, y: prevy}, {x: x, y: y}, {x: x1, y: y1}, this.conf.tolerance);
                    //pointlist.shift(); // firstpoint would already be in the poly
                    //							for(var j=0; j<pointlist.length; j++){
                    //    var point = { };
                    //    point.x = pointlist[j].x;
                    //    point.y = pointlist[j].y;
                    //    poly.push(point);
                    //}
                    //						break;
                    //						case 's':
                    //						case 'S':
                    //							if(i > 0 && /[CcSs]/.test(seglist.getItem(i-1).pathSegTypeAsLetter)){
                    //    x1 = prevx + (prevx - prevx2);
                    //    y1 = prevy + (prevy - prevy2);
                    //}
                    //							else{
                    //    x1 = prevx;
                    //    y1 = prevy;
                    //}
                    //						case 'c':
                    //						case 'C':
                    //							var pointlist = GeometryUtil.CubicBezier.linearize({ x: prevx, y: prevy}, { x: x, y: y}, { x: x1, y: y1}, { x: x2, y: y2}, this.conf.tolerance);
                    //pointlist.shift(); // firstpoint would already be in the poly
                    //							for(var j=0; j<pointlist.length; j++){
                    //    var point = { };
                    //    point.x = pointlist[j].x;
                    //    point.y = pointlist[j].y;
                    //    poly.push(point);
                    //}
                    //						break;
                    //						case 'a':
                    //						case 'A':
                    //							var pointlist = GeometryUtil.Arc.linearize({ x: prevx, y: prevy}, { x: x, y: y}, s.r1, s.r2, s.angle, s.largeArcFlag,s.sweepFlag, this.conf.tolerance);
                    //pointlist.shift();

                    //							for(var j=0; j<pointlist.length; j++){
                    //    var point = { };
                    //    point.x = pointlist[j].x;
                    //    point.y = pointlist[j].y;
                    //    poly.push(point);
                    //}
                    //						break;
                    //						case 'z': case 'Z': x=x0; y=y0; break;
                    //}
                    //					// Record the start of a subpath
                    //					if (command=='M' || command=='m') x0=x, y0=y;
                    //				}

                    break;
            }

            // do not include last point if coincident with starting point
            while (poly.Count > 0 && GeometryUtil._almostEqual(poly[0].x, poly[poly.Count - 1].x, Conf.toleranceSvg)
                && GeometryUtil._almostEqual(poly[0].y, poly[poly.Count - 1].y, Conf.toleranceSvg))
            {
                poly.RemoveAt(0);
            }

            return new NFP() { Points = poly.ToArray() };
        }



    }

    public class SvgConfig
    {
        public float tolerance = 2f; // max bound for bezier->line segment conversion, in native SVG units
        public float toleranceSvg = 0.005f;// fudge factor for browser inaccuracy in SVG unit handling

    }
    public class LocalContour
    {
        public float Len
        {
            get
            {
                float len = 0;
                for (int i = 1; i <= Points.Count; i++)
                {
                    var p1 = Points[i - 1];
                    var p2 = Points[i % Points.Count];
                    len += (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
                }
                return len;
            }
        }
        public List<PointF> Points = new List<PointF>();
        public bool Enable = true;
    }
}
