using IxMilia.Dxf.Entities;
using netDxf;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;

namespace DeepNestLib
{
    public class DxfParser
    {
        public static RawDetail[] LoadDxf(string path, bool split = false)
        {
            FileInfo fi = new FileInfo(path);

            RawDetail s = new RawDetail();

            s.Name = fi.FullName;


            List<DraftElement> elems = new List<DraftElement>();


            netDxf.DxfDocument doc = netDxf.DxfDocument.Load(path);
            double mult = 1;
            if (doc.DrawingVariables.InsUnits == netDxf.Units.DrawingUnits.Inches)
            {
                mult = 25.4;
            }
            foreach (var cr in doc.Entities.Circles)
            {

                LocalContour cc = new LocalContour();

                for (int i = 0; i <= 360; i += 15)
                {
                    var ang = i * Math.PI / 180f;
                    var xx = cr.Center.X + cr.Radius * Math.Cos(ang);
                    var yy = cr.Center.Y + cr.Radius * Math.Sin(ang);
                    cc.Points.Add(new PointF((float)xx, (float)yy));
                }
                PolylineElement p = new PolylineElement() { Tag = cr };
                elems.Add(p);
                p.Start = cc.Points[0];
                p.End = cc.Points[cc.Points.Count - 1];
                p.Points = cc.Points.ToArray();
            }
            foreach (var cr in doc.Entities.Arcs)
            {
                var sang = cr.StartAngle;
                var eang = cr.EndAngle;
                var center = new PointF((float)cr.Center.X, (float)cr.Center.Y);
                List<PointF> pp = new List<PointF>();

                if (sang > eang)
                {
                    sang -= 360;
                }

                for (double i = sang; i < eang; i += 15)
                {
                    var tt = GetPointFromAngle(center, (float)cr.Radius, i);
                    pp.Add(new PointF((float)tt.X, (float)tt.Y));
                }
                var t = GetPointFromAngle(center, (float)cr.Radius, eang);
                pp.Add(new PointF((float)t.X, (float)t.Y));
                PolylineElement p = new PolylineElement() { Tag = cr };
                elems.Add(p);

                p.Start = pp[0];
                p.End = pp[pp.Count - 1];
                p.Points = pp.ToArray();
            }
            foreach (var cr in doc.Entities.Splines)
            {
                LocalContour cc = new LocalContour();
                var list = cr.PolygonalVertexes(100);

                cc.Points.AddRange(list.Select(z => new PointF((float)(float)z.X, (float)z.Y)));
                PolylineElement p = new PolylineElement()
                {
                    Tag = new PolylineExportInfo()
                    {
                        IsClosed = cr.IsClosed,
                        Points = list.Select(z => new Vector3(z.X * mult, z.Y * mult, 0)).ToArray()
                    }
                };
                elems.Add(p);
                p.Start = cc.Points[0];
                p.End = cc.Points[cc.Points.Count - 1];
                p.Points = cc.Points.ToArray();
            }
            foreach (var item in doc.Entities.Lines)
            {
                elems.Add(new LineElement()
                {
                    Tag = item,
                    Start = new PointF((float)item.StartPoint.X, (float)item.StartPoint.Y),
                    End = new PointF((float)item.EndPoint.X, (float)item.EndPoint.Y)
                });
            }

            List<RawDetail> ret = new List<RawDetail>();

            foreach (var item in elems)
            {
                item.Mult(mult);
            }
            elems = elems.Where(z => z.Length > RemoveThreshold).ToList();
            var cntrs2 = ConnectElements(elems.ToArray());
            if (split)
            {
                var nfps = cntrs2;
                for (int i = 0; i < nfps.Length; i++)
                {
                    for (int j = 0; j < nfps.Length; j++)
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
                    RawDetail det = new RawDetail()
                    {
                        Name = fi.FullName + "_" + i
                    };
                    if (tops[i].Points.Count < 3)
                        continue;

                    det.Outers.Add(tops[i]);
                    ret.Add(det);
                }
            }
            else
            {
                s.Outers.AddRange(cntrs2);
                if (s.Outers.Any(z => z.Points.Count < 3))
                {
                    throw new Exception("few points");
                }

                ret.Add(s);
            }
            return ret.ToArray();
        }

        public static double RemoveThreshold = 10e-5;
        public static double ClosingThreshold = 10e-2;

        public static LocalContour[] ConnectElements(DraftElement[] elems)
        {
            List<LocalContour> ret = new List<LocalContour>();

            List<PointF> pp = new List<PointF>();
            List<DraftElement> last = new List<DraftElement>();
            last.AddRange(elems);
            List<object> accum = new List<object>();
            while (last.Any())
            {
                if (pp.Count == 0)
                {
                    pp.AddRange(last.First().GetPoints());
                    accum.Add(last.First().Tag);
                    last.RemoveAt(0);
                }
                else
                {
                    var ll = pp.Last();
                    var f1 = last.OrderBy(z => Math.Min(z.Start.DistTo(ll), z.End.DistTo(ll))).First();

                    var dist = Math.Min(f1.Start.DistTo(ll), f1.End.DistTo(ll));
                    if (dist > ClosingThreshold)
                    {
                        ret.Add(new LocalContour() { Points = pp.ToList(), Tag = accum.ToArray() });
                        pp.Clear();
                        accum.Clear();
                        continue;
                    }
                    accum.Add(f1.Tag);
                    last.Remove(f1);
                    if (f1.Start.DistTo(ll) < f1.End.DistTo(ll))
                    {
                        pp.AddRange(f1.GetPoints().Skip(1));
                        //pp.Add(f1.End);
                    }
                    else
                    {
                        f1.Reverse();
                        pp.AddRange(f1.GetPoints().Skip(1));
                        //pp.Add(f1.Start);
                    }
                }
            }
            if (pp.Any())
            {
                ret.Add(new LocalContour() { Points = pp.ToList(), Tag = accum.ToArray() });
            }
            return ret.ToArray();
        }
        public static PointF GetPointFromAngle(PointF center, float radius, double angle)
        {
            double y = Math.Sin(angle * Math.PI / 180.0);
            var p1 = new PointF((float)Math.Cos(angle * Math.PI / 180.0), (float)y);
            p1 = new PointF(p1.X * radius, p1.Y * radius);
            p1 = new PointF(p1.X + center.X, p1.Y + center.Y);
            return p1;
        }

    }
}