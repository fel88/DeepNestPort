using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeepNestLib
{
    public class _Clipper
    {
        public static ClipperLib.IntPoint[] ScaleUpPaths(SvgPoint[] points, double scale = 1)
        {
            var result = new ClipperLib.IntPoint[points.Length];

            Parallel.For(0, points.Length, i => result[i] = new ClipperLib.IntPoint((long)Math.Round((decimal)points[i].x * (decimal)scale), (long)Math.Round((decimal)points[i].y * (decimal)scale)));

            return result.ToArray();
        } // 2 secs
        public static ClipperLib.IntPoint[] ScaleUpPaths(NFP p, double scale = 1)
        {
            List<ClipperLib.IntPoint> ret = new List<ClipperLib.IntPoint>();

            for (int i = 0; i < p.Points.Count(); i++)
            {
                //p.Points[i] = new SvgNestPort.SvgPoint((float)Math.Round(p.Points[i].x * scale), (float)Math.Round(p.Points[i].y * scale));
                ret.Add(new ClipperLib.IntPoint(
                    (long)Math.Round((decimal)p.Points[i].x * (decimal)scale),
                    (long)Math.Round((decimal)p.Points[i].y * (decimal)scale)
                ));

            }
            return ret.ToArray();
        }
        /*public static IntPoint[] ScaleUpPath(IntPoint[] p, double scale = 1)
        {
            for (int i = 0; i < p.Length; i++)
            {

                //p[i] = new IntPoint(p[i].X * scale, p[i].Y * scale);
                p[i] = new IntPoint(
                    (long)Math.Round((decimal)p[i].X * (decimal)scale),
                    (long)Math.Round((decimal)p[i].Y * (decimal)scale));
            }
            return p.ToArray();
        }
        public static void ScaleUpPaths(List<List<IntPoint>> p, double scale = 1)
        {
            for (int i = 0; i < p.Count; i++)
            {
                for (int j = 0; j < p[i].Count; j++)
                {
                    p[i][j] = new IntPoint(p[i][j].X * scale, p[i][j].Y * scale);

                }
            }


        }*/
    }
}

