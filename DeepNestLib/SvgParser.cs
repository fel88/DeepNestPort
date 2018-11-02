using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DeepNestLib
{
    public class SvgParser
    {

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
}
