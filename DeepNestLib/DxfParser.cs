using IxMilia.Dxf;
using IxMilia.Dxf.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace DeepNestLib
{
    public class DxfParser
    {
        public static RawDetail loadDxf(string path)
        {
            FileInfo fi = new FileInfo(path);
            DxfFile dxffile = DxfFile.Load(fi.FullName);
            RawDetail s = new RawDetail();


            s.Name = fi.FullName;
            IEnumerable<DxfEntity> entities = dxffile.Entities.ToArray();


            LocalContour points = new LocalContour();
            List<LineElement> elems = new List<LineElement>();

            foreach (DxfEntity ent in entities)
            {
                switch (ent.EntityType)
                {
                    case DxfEntityType.LwPolyline:
                        {

                            DxfLwPolyline poly = (DxfLwPolyline)ent;
                            if (poly.Vertices.Count() < 2)
                            {
                                continue;
                            }
                            foreach (DxfLwPolylineVertex vert in poly.Vertices)
                            {
                                points.Points.Add(new PointF((float)vert.X, (float)vert.Y));
                            }
                        }
                        break;
                    case DxfEntityType.Arc:
                        {
                            DxfArc arc = (DxfArc)ent;
                            List<PointF> pp = new List<PointF>();

                            if (arc.StartAngle > arc.EndAngle)
                            {
                                arc.StartAngle -= 360;
                            }

                            for (double i = arc.StartAngle; i < arc.EndAngle; i += 15)
                            {
                                var tt = arc.GetPointFromAngle(i);
                                pp.Add(new PointF((float)tt.X, (float)tt.Y));
                            }
                            var t = arc.GetPointFromAngle(arc.EndAngle);
                            pp.Add(new PointF((float)t.X, (float)t.Y));
                            for (int j = 1; j < pp.Count; j++)
                            {
                                var p1 = pp[j - 1];
                                var p2 = pp[j];
                                elems.Add(new LineElement() { Start = new PointF((float)p1.X, (float)p1.Y), End = new PointF((float)p2.X, (float)p2.Y) });
                            }
                        }
                        break;
                    case DxfEntityType.Circle:
                        {
                            DxfCircle cr = (DxfCircle)ent;
                            LocalContour cc = new LocalContour();

                            for (int i = 0; i <= 360; i += 15)
                            {
                                var ang = i * Math.PI / 180f;
                                var xx = cr.Center.X + cr.Radius * Math.Cos(ang);
                                var yy = cr.Center.Y + cr.Radius * Math.Sin(ang);
                                cc.Points.Add(new PointF((float)xx, (float)yy));
                            }
                            for (int i = 1; i < cc.Points.Count; i++)
                            {
                                var p1 = cc.Points[i - 1];
                                var p2 = cc.Points[i];
                                elems.Add(new LineElement() { Start = p1, End = p2 });
                            }
                        }
                        break;
                    case DxfEntityType.Line:
                        {
                            DxfLine poly = (DxfLine)ent;
                            elems.Add(new LineElement() { Start = new PointF((float)poly.P1.X, (float)poly.P1.Y), End = new PointF((float)poly.P2.X, (float)poly.P2.Y) });
                            break;
                        }

                    case DxfEntityType.Polyline:
                        {

                            DxfPolyline poly = (DxfPolyline)ent;
                            if (poly.Vertices.Count() < 2)
                            {
                                continue;
                            }

                            foreach (DxfVertex vert in poly.Vertices)
                            {
                                points.Points.Add(new PointF((float)vert.Location.X, (float)vert.Location.Y));

                            }
                            break;
                        }
                    default:
                        throw new ArgumentException("unsupported entity type: " + ent);

                };
            }

            
            elems = elems.Where(z => z.Start.DistTo(z.End) > RemoveThreshold).ToList();
            var cntrs2 = ConnectElements(elems.ToArray());
            s.Outers.AddRange(cntrs2);
            if (s.Outers.Any(z => z.Points.Count < 3))
            {
                throw new Exception("few points");
            }

            return s;
        }
        public static double RemoveThreshold = 10e-5;
        public static double ClosingThreshold = 10e-4;

        public static LocalContour[] ConnectElements(LineElement[] elems)
        {
            List<LocalContour> ret = new List<LocalContour>();

            List<PointF> pp = new List<PointF>();
            List<LineElement> last = new List<LineElement>();
            last.AddRange(elems);

            while (last.Any())
            {
                if (pp.Count == 0)
                {
                    pp.Add(last.First().Start);
                    pp.Add(last.First().End);
                    last.RemoveAt(0);
                }
                else
                {
                    var ll = pp.Last();
                    var f1 = last.OrderBy(z => Math.Min(z.Start.DistTo(ll), z.End.DistTo(ll))).First();

                    var dist = Math.Min(f1.Start.DistTo(ll), f1.End.DistTo(ll));
                    if (dist > ClosingThreshold)
                    {
                        ret.Add(new LocalContour() { Points = pp.ToList() });
                        pp.Clear();
                        continue;
                    }
                    last.Remove(f1);
                    if (f1.Start.DistTo(ll) < f1.End.DistTo(ll))
                    {
                        pp.Add(f1.End);
                    }
                    else
                    {
                        pp.Add(f1.Start);
                    }
                }
            }
            if (pp.Any())
            {
                ret.Add(new LocalContour() { Points = pp.ToList() });
            }
            return ret.ToArray();
        }
    }
}
