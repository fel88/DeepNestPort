using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeepNestLib
{
    public class D3
    {

        // Returns the 2D cross product of AB and AC vectors, i.e., the z-component of
        // the 3D cross product in a quadrant I Cartesian coordinate system (+x is
        // right, +y is up). Returns a positive value if ABC is counter-clockwise,
        // negative if clockwise, and zero if the points are collinear.
        public static double cross(double[] a, double[] b, double[] c)
        {
            return (b[0] - a[0]) * (c[1] - a[1]) - (b[1] - a[1]) * (c[0] - a[0]);
        }
        // Computes the upper convex hull per the monotone chain algorithm.
        // Assumes points.length >= 3, is sorted by x, unique in y.
        // Returns an array of indices into points in left-to-right order.
        public static int[] computeUpperHullIndexes(double[][] points)
        {
            Dictionary<int, int> indexes = new Dictionary<int, int>();
            indexes.Add(0, 0);
            indexes.Add(1, 1);
            var n = points.Count();
            var size = 2;

            for (var i = 2; i < n; ++i)
            {
                while (size > 1 && cross(points[indexes[size - 2]], points[indexes[size - 1]], points[i]) <= 0) --size;

                if (!indexes.ContainsKey(size))
                {
                    indexes.Add(size, -1);
                }
                indexes[size++] = i;
            }
            List<int> ret = new List<int>();
            for (int i = 0; i < size; i++)
            {
                ret.Add(indexes[i]);
            }
            return ret.ToArray();
            //return indexes.slice(0, size); // remove popped points
        }

        public class HullInfoPoint
        {
            public double x;
            public double y;
            public int index;
        }
        public static double[][] polygonHull(double[][] points)
        {
            int n;
            n = points.Count();
            if ((n) < 3) return null;



            HullInfoPoint[] sortedPoints = new HullInfoPoint[n];
            double[][] flippedPoints = new double[n][];



            for (int i = 0; i < n; ++i) sortedPoints[i] = new HullInfoPoint { x = points[i][0], y = points[i][1], index = i };
            sortedPoints = sortedPoints.OrderBy(x => x.x).ThenBy(z => z.y).ToArray();

            for (int i = 0; i < n; ++i) flippedPoints[i] = new double[] { sortedPoints[i].x, -sortedPoints[i].y };

            var upperIndexes = computeUpperHullIndexes(sortedPoints.Select(z => new double[] { z.x, z.y, z.index }).ToArray());
            var lowerIndexes = computeUpperHullIndexes(flippedPoints);


            // Construct the hull polygon, removing possible duplicate endpoints.
            var skipLeft = lowerIndexes[0] == upperIndexes[0];
            var skipRight = lowerIndexes[lowerIndexes.Length - 1] == upperIndexes[upperIndexes.Length - 1];
            List<double[]> hull = new List<double[]>();

            // Add upper hull in right-to-l order.
            // Then add lower hull in left-to-right order.
            for (int i = upperIndexes.Length - 1; i >= 0; --i)
                hull.Add(points[sortedPoints[upperIndexes[i]].index]);
            //for (int i = +skipLeft; i < lowerIndexes.Length - skipRight; ++i) hull.push(points[sortedPoints[lowerIndexes[i]][2]]);
            for (int i = skipLeft ? 1 : 0; i < lowerIndexes.Length - (skipRight ? 1 : 0); ++i) hull.Add(points[sortedPoints[lowerIndexes[i]].index]);

            return hull.ToArray();
        }

    }
}
