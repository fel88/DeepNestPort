using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeepNestLib
{
    public class Simplify
    {

        // to suit your point format, run search/replace for '.x' and '.y';
        // for 3D version, see 3d branch (configurability would draw significant performance overhead)

        // square distance between 2 points
        public static double getSqDist(SvgPoint p1, SvgPoint p2)
        {

            var dx = p1.x - p2.x;
            var dy = p1.y - p2.y;

            return dx * dx + dy * dy;
        }

        // square distance from a point to a segment
        public static double getSqSegDist(SvgPoint p, SvgPoint p1, SvgPoint p2)
        {

            var x = p1.x;
            var y = p1.y;
            var dx = p2.x - x;
            var dy = p2.y - y;

            if (dx != 0 || dy != 0)
            {

                var t = ((p.x - x) * dx + (p.y - y) * dy) / (dx * dx + dy * dy);

                if (t > 1)
                {
                    x = p2.x;
                    y = p2.y;

                }
                else if (t > 0)
                {
                    x += dx * t;
                    y += dy * t;
                }
            }

            dx = p.x - x;
            dy = p.y - y;

            return dx * dx + dy * dy;
        }
        // rest of the code doesn't care about point format

        // basic distance-based simplification
        public static NFP simplifyRadialDist(NFP points, double? sqTolerance)
        {

            var prevPoint = points[0];
            var newPoints = new NFP();
            newPoints.AddPoint(prevPoint);

            SvgPoint point = null;
            int i = 1;
            for (var len = points.length; i < len; i++)
            {
                point = points[i];

                if (point.marked || getSqDist(point, prevPoint) > sqTolerance)
                {
                    newPoints.AddPoint(point);
                    prevPoint = point;
                }
            }

            if (prevPoint != point) newPoints.AddPoint(point);
            return newPoints;
        }


        public static void simplifyDPStep(NFP points, int first, int last, double? sqTolerance, NFP simplified)
        {
            var maxSqDist = sqTolerance;
            var index = -1;
            var marked = false;
            for (var i = first + 1; i < last; i++)
            {
                var sqDist = getSqSegDist(points[i], points[first], points[last]);

                if (sqDist > maxSqDist)
                {
                    index = i;
                    maxSqDist = sqDist;
                }
                /*if(points[i].marked && maxSqDist <= sqTolerance){
                    index = i;
                    marked = true;
                }*/
            }

            /*if(!points[index] && maxSqDist > sqTolerance){
                console.log('shit shit shit');
            }*/

            if (maxSqDist > sqTolerance || marked)
            {
                if (index - first > 1) simplifyDPStep(points, first, index, sqTolerance, simplified);
                simplified.push(points[index]);
                if (last - index > 1) simplifyDPStep(points, index, last, sqTolerance, simplified);
            }
        }

        // simplification using Ramer-Douglas-Peucker algorithm
        public static NFP simplifyDouglasPeucker(NFP points, double? sqTolerance)
        {
            var last = points.length - 1;

            var simplified = new NFP();
            simplified.AddPoint(points[0]);
            simplifyDPStep(points, 0, last, sqTolerance, simplified);
            simplified.push(points[last]);

            return simplified;
        }

        // both algorithms combined for awesome performance
        public static NFP simplify(NFP points, double? tolerance, bool highestQuality)
        {

            if (points.length <= 2) return points;

            var sqTolerance = (tolerance != null) ? (tolerance * tolerance) : 1;

            points = highestQuality ? points : simplifyRadialDist(points, sqTolerance);
            points = simplifyDouglasPeucker(points, sqTolerance);

            return points;
        }
    }
}
