using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DeepNestLib
{ 
    public class GeometryUtil
    {
        // returns true if points are within the given distance
        public static bool _withinDistance(SvgPoint p1, SvgPoint p2, double distance)
        {
            var dx = p1.x - p2.x;
            var dy = p1.y - p2.y;
            return ((dx * dx + dy * dy) < distance * distance);
        }

        // returns an interior NFP for the special case where A is a rectangle
        public static NFP[] noFitPolygonRectangle(NFP A, NFP B)
        {
            var minAx = A[0].x;
            var minAy = A[0].y;
            var maxAx = A[0].x;
            var maxAy = A[0].y;

            for (var i = 1; i < A.Length; i++)
            {
                if (A[i].x < minAx)
                {
                    minAx = A[i].x;
                }
                if (A[i].y < minAy)
                {
                    minAy = A[i].y;
                }
                if (A[i].x > maxAx)
                {
                    maxAx = A[i].x;
                }
                if (A[i].y > maxAy)
                {
                    maxAy = A[i].y;
                }
            }

            var minBx = B[0].x;
            var minBy = B[0].y;
            var maxBx = B[0].x;
            var maxBy = B[0].y;
            for (int i = 1; i < B.Length; i++)
            {
                if (B[i].x < minBx)
                {
                    minBx = B[i].x;
                }
                if (B[i].y < minBy)
                {
                    minBy = B[i].y;
                }
                if (B[i].x > maxBx)
                {
                    maxBx = B[i].x;
                }
                if (B[i].y > maxBy)
                {
                    maxBy = B[i].y;
                }
            }

            if (maxBx - minBx > maxAx - minAx)
            {
                return null;
            }
            if (maxBy - minBy > maxAy - minAy)
            {
                return null;
            }


            var pnts = new NFP[] { new NFP() { Points=new SvgPoint[]{

                new SvgPoint(minAx - minBx + B[0].x, minAy - minBy + B[0].y),
            new SvgPoint(maxAx - maxBx + B[0].x, minAy - minBy + B[0].y),
            new SvgPoint( maxAx - maxBx + B[0].x, maxAy - maxBy + B[0].y),
            new SvgPoint( minAx - minBx + B[0].x, maxAy - maxBy + B[0].y)
            } } };
            return pnts;
        }

  
        // returns the rectangular bounding box of the given polygon
        public static PolygonBounds getPolygonBounds(NFP _polygon)
        {
            return getPolygonBounds(_polygon.Points);
        }
        public static PolygonBounds getPolygonBounds(List<SvgPoint
            > polygon)
        {
            return getPolygonBounds(polygon.ToArray());
        }
        public static PolygonBounds getPolygonBounds(SvgPoint[] polygon)
        {

            if (polygon == null || polygon.Count() < 3)
            {
                throw new ArgumentException("null");
            }

            var xmin = polygon[0].x;
            var xmax = polygon[0].x;
            var ymin = polygon[0].y;
            var ymax = polygon[0].y;

            for (var i = 1; i < polygon.Length; i++)
            {
                if (polygon[i].x > xmax)
                {
                    xmax = polygon[i].x;
                }
                else if (polygon[i].x < xmin)
                {
                    xmin = polygon[i].x;
                }

                if (polygon[i].y > ymax)
                {
                    ymax = polygon[i].y;
                }
                else if (polygon[i].y < ymin)
                {
                    ymin = polygon[i].y;
                }
            }

            var w = xmax - xmin;
            var h = ymax - ymin;
            //return new rectanglef(xmin, ymin, xmax - xmin, ymax - ymin);
            return new PolygonBounds(xmin, ymin, w, h);


        }

        public static bool isRectangle(NFP poly, double? tolerance = null)
        {
            var bb = getPolygonBounds(poly);
            if (tolerance == null)
            {
                tolerance = TOL;
            }


            for (var i = 0; i < poly.Points.Length; i++)
            {
                if (!_almostEqual(poly.Points[i].x, bb.x) && !_almostEqual(poly.Points[i].x, bb.x + bb.width))
                {
                    return false;
                }
                if (!_almostEqual(poly.Points[i].y, bb.y) && !_almostEqual(poly.Points[i].y, bb.y + bb.height))
                {
                    return false;
                }
            }

            return true;
        }

        public static PolygonWithBounds rotatePolygon(NFP polygon, float angle)
        {

            List<SvgPoint> rotated = new List<SvgPoint>();
            angle = (float)(angle * Math.PI / 180.0f);
            for (var i = 0; i < polygon.Points.Length; i++)
            {
                var x = polygon.Points[i].x;
                var y = polygon.Points[i].y;
                var x1 = (float)(x * Math.Cos(angle) - y * Math.Sin(angle));
                var y1 = (float)(x * Math.Sin(angle) + y * Math.Cos(angle));

                rotated.Add(new SvgPoint(x1, y1));
            }
            // reset bounding box
            RectangleF rr = new RectangleF();

            var ret = new PolygonWithBounds()
            {
                Points = rotated.ToArray()
            };
            var bounds = GeometryUtil.getPolygonBounds(ret);
            ret.x = bounds.x;
            ret.y = bounds.y;
            ret.width = bounds.width;
            ret.height = bounds.height;
            return ret;

        }

        public class PolygonWithBounds : NFP
        {
            public double x;
            public double y;
            public double width;
            public double height;
        }
        public static bool _almostEqual(double a, double b, double? tolerance = null)
        {
            if (tolerance == null)
            {
                tolerance = TOL;
            }
            return Math.Abs(a - b) < tolerance;
        }
        public static bool _almostEqual(double? a, double? b, double? tolerance = null)
        {
            return _almostEqual(a.Value, b.Value, tolerance);
        }
        // returns true if point already exists in the given nfp
        public static bool inNfp(SvgPoint p, NFP[] nfp)
        {
            if (nfp == null || nfp.Length == 0)
            {
                return false;
            }

            for (var i = 0; i < nfp.Length; i++)
            {
                for (var j = 0; j < nfp[i].length; j++)
                {
                    if (_almostEqual(p.x, nfp[i][j].x) && _almostEqual(p.y, nfp[i][j].y))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        // normalize vector into a unit vector
        public static SvgPoint _normalizeVector(SvgPoint v)
        {
            if (_almostEqual(v.x * v.x + v.y * v.y, 1))
            {
                return v; // given vector was already a unit vector
            }
            var len = Math.Sqrt(v.x * v.x + v.y * v.y);
            var inverse = (float)(1 / len);

            return new SvgPoint(v.x * inverse, v.y * inverse
        );
        }
        public static double? pointDistance(SvgPoint p, SvgPoint s1, SvgPoint s2, SvgPoint normal, bool infinite = false)
        {
            normal = _normalizeVector(normal);

            var dir = new SvgPoint(normal.y, -normal.x);

            var pdot = p.x * dir.x + p.y * dir.y;
            var s1dot = s1.x * dir.x + s1.y * dir.y;
            var s2dot = s2.x * dir.x + s2.y * dir.y;

            var pdotnorm = p.x * normal.x + p.y * normal.y;
            var s1dotnorm = s1.x * normal.x + s1.y * normal.y;
            var s2dotnorm = s2.x * normal.x + s2.y * normal.y;

            if (!infinite)
            {
                if (((pdot < s1dot || _almostEqual(pdot, s1dot)) && (pdot < s2dot || _almostEqual(pdot, s2dot))) || ((pdot > s1dot || _almostEqual(pdot, s1dot)) && (pdot > s2dot || _almostEqual(pdot, s2dot))))
                {
                    return null; // dot doesn't collide with segment, or lies directly on the vertex
                }
                if ((_almostEqual(pdot, s1dot) && _almostEqual(pdot, s2dot)) && (pdotnorm > s1dotnorm && pdotnorm > s2dotnorm))
                {
                    return Math.Min(pdotnorm - s1dotnorm, pdotnorm - s2dotnorm);
                }
                if ((_almostEqual(pdot, s1dot) && _almostEqual(pdot, s2dot)) && (pdotnorm < s1dotnorm && pdotnorm < s2dotnorm))
                {
                    return -Math.Min(s1dotnorm - pdotnorm, s2dotnorm - pdotnorm);
                }
            }

            return -(pdotnorm - s1dotnorm + (s1dotnorm - s2dotnorm) * (s1dot - pdot) / (s1dot - s2dot));
        }
        static double TOL = (float)Math.Pow(10, -9); // Floating point error is likely to be above 1 epsilon
                                                     // returns true if p lies on the line segment defined by AB, but not at any endpoints
                                                     // may need work!
        public static bool _onSegment(SvgPoint A, SvgPoint B, SvgPoint p)
        {

            // vertical line
            if (_almostEqual(A.x, B.x) && _almostEqual(p.x, A.x))
            {
                if (!_almostEqual(p.y, B.y) && !_almostEqual(p.y, A.y) && p.y < Math.Max(B.y, A.y) && p.y > Math.Min(B.y, A.y))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            // horizontal line
            if (_almostEqual(A.y, B.y) && _almostEqual(p.y, A.y))
            {
                if (!_almostEqual(p.x, B.x) && !_almostEqual(p.x, A.x) && p.x < Math.Max(B.x, A.x) && p.x > Math.Min(B.x, A.x))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            //range check
            if ((p.x < A.x && p.x < B.x) || (p.x > A.x && p.x > B.x) || (p.y < A.y && p.y < B.y) || (p.y > A.y && p.y > B.y))
            {
                return false;
            }


            // exclude end points
            if ((_almostEqual(p.x, A.x) && _almostEqual(p.y, A.y)) || (_almostEqual(p.x, B.x) && _almostEqual(p.y, B.y)))
            {
                return false;
            }

            var cross = (p.y - A.y) * (B.x - A.x) - (p.x - A.x) * (B.y - A.y);

            if (Math.Abs(cross) > TOL)
            {
                return false;
            }

            var dot = (p.x - A.x) * (B.x - A.x) + (p.y - A.y) * (B.y - A.y);



            if (dot < 0 || _almostEqual(dot, 0))
            {
                return false;
            }

            var len2 = (B.x - A.x) * (B.x - A.x) + (B.y - A.y) * (B.y - A.y);



            if (dot > len2 || _almostEqual(dot, len2))
            {
                return false;
            }

            return true;
        }


        // project each point of B onto A in the given direction, and return the 
        public static double? polygonProjectionDistance(NFP A, NFP B, SvgPoint direction)
        {
            var Boffsetx = B.offsetx ?? 0;
            var Boffsety = B.offsety ?? 0;

            var Aoffsetx = A.offsetx ?? 0;
            var Aoffsety = A.offsety ?? 0;

            A = A.slice(0);
            B = B.slice(0);

            // close the loop for polygons
            if (A[0] != A[A.length - 1])
            {
                A.push(A[0]);
            }

            if (B[0] != B[B.length - 1])
            {
                B.push(B[0]);
            }

            var edgeA = A;
            var edgeB = B;

            double? distance = null;
            SvgPoint p, s1, s2;
            double? d;


            for (var i = 0; i < edgeB.length; i++)
            {
                // the shortest/most negative projection of B onto A
                double? minprojection = null;
                SvgPoint minp = null;
                for (var j = 0; j < edgeA.length - 1; j++)
                {
                    p = new SvgPoint(edgeB[i].x + Boffsetx, edgeB[i].y + Boffsety);
                    s1 = new SvgPoint(edgeA[j].x + Aoffsetx, edgeA[j].y + Aoffsety);
                    s2 = new SvgPoint(edgeA[j + 1].x + Aoffsetx, edgeA[j + 1].y + Aoffsety);

                    if (Math.Abs((s2.y - s1.y) * direction.x - (s2.x - s1.x) * direction.y) < TOL)
                    {
                        continue;
                    }

                    // project point, ignore edge boundaries
                    d = pointDistance(p, s1, s2, direction);

                    if (d != null && (minprojection == null || d < minprojection))
                    {
                        minprojection = d;
                        minp = p;
                    }
                }
                if (minprojection != null && (distance == null || minprojection > distance))
                {
                    distance = minprojection;
                }
            }

            return distance;
        }

        public static double polygonArea(NFP polygon)
        {
            double area = 0;
            int i, j;
            for (i = 0, j = polygon.Points.Length - 1; i < polygon.Points.Length; j = i++)
            {
                area += (polygon.Points[j].x + polygon.Points[i].x) * (polygon.Points[j].y
                    - polygon.Points[i].y);
            }
            return 0.5f * area;
        }

        // return true if point is in the polygon, false if outside, and null if exactly on a point or edge
        public static bool? pointInPolygon(SvgPoint point, NFP polygon)
        {
            if (polygon == null || polygon.Points.Length < 3)
            {
                throw new ArgumentException();
            }

            var inside = false;
            //var offsetx = polygon.offsetx || 0;
            //var offsety = polygon.offsety || 0;
            double offsetx = polygon.offsetx == null ? 0 : polygon.offsetx.Value;
            double offsety = polygon.offsety == null ? 0 : polygon.offsety.Value;

            int i, j;
            for (i = 0, j = polygon.Points.Count() - 1; i < polygon.Points.Length; j = i++)
            {
                var xi = polygon.Points[i].x + offsetx;
                var yi = polygon.Points[i].y + offsety;
                var xj = polygon.Points[j].x + offsetx;
                var yj = polygon.Points[j].y + offsety;

                if (_almostEqual(xi, point.x) && _almostEqual(yi, point.y))
                {

                    return null; // no result
                }

                if (_onSegment(new SvgPoint(xi, yi), new SvgPoint(xj, yj), point))
                {
                    return null; // exactly on the segment
                }

                if (_almostEqual(xi, xj) && _almostEqual(yi, yj))
                { // ignore very small lines
                    continue;
                }

                var intersect = ((yi > point.y) != (yj > point.y)) && (point.x < (xj - xi) * (point.y - yi) / (yj - yi) + xi);
                if (intersect) inside = !inside;
            }

            return inside;
        }
        // todo: swap this for a more efficient sweep-line implementation
        // returnEdges: if set, return all edges on A that have intersections

        public static bool intersect(NFP A, NFP B)
        {
            var Aoffsetx = A.offsetx ?? 0;
            var Aoffsety = A.offsety ?? 0;

            var Boffsetx = B.offsetx ?? 0;
            var Boffsety = B.offsety ?? 0;

            A = A.slice(0);
            B = B.slice(0);

            for (var i = 0; i < A.length - 1; i++)
            {
                for (var j = 0; j < B.length - 1; j++)
                {
                    var a1 = new SvgPoint(A[i].x + Aoffsetx, A[i].y + Aoffsety);
                    var a2 = new SvgPoint(A[i + 1].x + Aoffsetx, A[i + 1].y + Aoffsety);
                    var b1 = new SvgPoint(B[j].x + Boffsetx, B[j].y + Boffsety);
                    var b2 = new SvgPoint(B[j + 1].x + Boffsetx, B[j + 1].y + Boffsety);

                    var prevbindex = (j == 0) ? B.length - 1 : j - 1;
                    var prevaindex = (i == 0) ? A.length - 1 : i - 1;
                    var nextbindex = (j + 1 == B.length - 1) ? 0 : j + 2;
                    var nextaindex = (i + 1 == A.length - 1) ? 0 : i + 2;

                    // go even further back if we happen to hit on a loop end point
                    if (B[prevbindex] == B[j] || (_almostEqual(B[prevbindex].x, B[j].x) && _almostEqual(B[prevbindex].y, B[j].y)))
                    {
                        prevbindex = (prevbindex == 0) ? B.length - 1 : prevbindex - 1;
                    }

                    if (A[prevaindex] == A[i] || (_almostEqual(A[prevaindex].x, A[i].x) && _almostEqual(A[prevaindex].y, A[i].y)))
                    {
                        prevaindex = (prevaindex == 0) ? A.length - 1 : prevaindex - 1;
                    }

                    // go even further forward if we happen to hit on a loop end point
                    if (B[nextbindex] == B[j + 1] || (_almostEqual(B[nextbindex].x, B[j + 1].x) && _almostEqual(B[nextbindex].y, B[j + 1].y)))
                    {
                        nextbindex = (nextbindex == B.length - 1) ? 0 : nextbindex + 1;
                    }

                    if (A[nextaindex] == A[i + 1] || (_almostEqual(A[nextaindex].x, A[i + 1].x) && _almostEqual(A[nextaindex].y, A[i + 1].y)))
                    {
                        nextaindex = (nextaindex == A.length - 1) ? 0 : nextaindex + 1;
                    }

                    var a0 = new SvgPoint(A[prevaindex].x + Aoffsetx, A[prevaindex].y + Aoffsety);
                    var b0 = new SvgPoint(B[prevbindex].x + Boffsetx, B[prevbindex].y + Boffsety);

                    var a3 = new SvgPoint(A[nextaindex].x + Aoffsetx, A[nextaindex].y + Aoffsety);
                    var b3 = new SvgPoint(B[nextbindex].x + Boffsetx, B[nextbindex].y + Boffsety);

                    if (_onSegment(a1, a2, b1) || (_almostEqual(a1.x, b1.x) && _almostEqual(a1.y, b1.y)))
                    {
                        // if a point is on a segment, it could intersect or it could not. Check via the neighboring points
                        var b0in = pointInPolygon(b0, A);
                        var b2in = pointInPolygon(b2, A);
                        if ((b0in == true && b2in == false) || (b0in == false && b2in == true))
                        {
                            return true;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (_onSegment(a1, a2, b2) || (_almostEqual(a2.x, b2.x) && _almostEqual(a2.y, b2.y)))
                    {
                        // if a point is on a segment, it could intersect or it could not. Check via the neighboring points
                        var b1in = pointInPolygon(b1, A);
                        var b3in = pointInPolygon(b3, A);

                        if ((b1in == true && b3in == false) || (b1in == false && b3in == true))
                        {
                            return true;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (_onSegment(b1, b2, a1) || (_almostEqual(a1.x, b2.x) && _almostEqual(a1.y, b2.y)))
                    {
                        // if a point is on a segment, it could intersect or it could not. Check via the neighboring points
                        var a0in = pointInPolygon(a0, B);
                        var a2in = pointInPolygon(a2, B);

                        if ((a0in == true && a2in == false) || (a0in == false && a2in == true))
                        {
                            return true;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (_onSegment(b1, b2, a2) || (_almostEqual(a2.x, b1.x) && _almostEqual(a2.y, b1.y)))
                    {
                        // if a point is on a segment, it could intersect or it could not. Check via the neighboring points
                        var a1in = pointInPolygon(a1, B);
                        var a3in = pointInPolygon(a3, B);

                        if ((a1in == true && a3in == false) || (a1in == false && a3in == true))
                        {
                            return true;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    var p = _lineIntersect(b1, b2, a1, a2);

                    if (p != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool isFinite(object obj)
        {
            return true;
        }
        // returns the intersection of AB and EF
        // or null if there are no intersections or other numerical error
        // if the infinite flag is set, AE and EF describe infinite lines without endpoints, they are finite line segments otherwise
        public static SvgPoint _lineIntersect(SvgPoint A, SvgPoint B, SvgPoint E, SvgPoint F, bool infinite = false)
        {
            double a1, a2, b1, b2, c1, c2, x, y;

            a1 = B.y - A.y;
            b1 = A.x - B.x;
            c1 = B.x * A.y - A.x * B.y;
            a2 = F.y - E.y;
            b2 = E.x - F.x;
            c2 = F.x * E.y - E.x * F.y;

            var denom = a1 * b2 - a2 * b1;

            x = (b1 * c2 - b2 * c1) / denom;
            y = (a2 * c1 - a1 * c2) / denom;


            if (!isFinite(x) || !isFinite(y))
            {
                return null;
            }

            // lines are colinear
            /*var crossABE = (E.y - A.y) * (B.x - A.x) - (E.x - A.x) * (B.y - A.y);
            var crossABF = (F.y - A.y) * (B.x - A.x) - (F.x - A.x) * (B.y - A.y);
            if(_almostEqual(crossABE,0) && _almostEqual(crossABF,0)){
                return null;
            }*/

            if (!infinite)
            {
                // coincident points do not count as intersecting
                if (Math.Abs(A.x - B.x) > TOL && ((A.x < B.x) ? x < A.x || x > B.x : x > A.x || x < B.x)) return null;
                if (Math.Abs(A.y - B.y) > TOL && ((A.y < B.y) ? y < A.y || y > B.y : y > A.y || y < B.y)) return null;

                if (Math.Abs(E.x - F.x) > TOL && ((E.x < F.x) ? x < E.x || x > F.x : x > E.x || x < F.x)) return null;
                if (Math.Abs(E.y - F.y) > TOL && ((E.y < F.y) ? y < E.y || y > F.y : y > E.y || y < F.y)) return null;
            }

            return new SvgPoint(x, y);
        }

        // searches for an arrangement of A and B such that they do not overlap
        // if an NFP is given, only search for startpoints that have not already been traversed in the given NFP

        public static SvgPoint searchStartPoint(NFP A, NFP B, bool inside, NFP[] NFP = null)
        {
            // clone arrays
            A = A.slice(0);
            B = B.slice(0);

            // close the loop for polygons
            if (A[0] != A[A.length - 1])
            {
                A.push(A[0]);
            }

            if (B[0] != B[B.length - 1])
            {
                B.push(B[0]);
            }

            for (var i = 0; i < A.length - 1; i++)
            {
                if (!A[i].marked)
                {
                    A[i].marked = true;
                    for (var j = 0; j < B.length; j++)
                    {
                        B.offsetx = A[i].x - B[j].x;
                        B.offsety = A[i].y - B[j].y;

                        bool? Binside = null;
                        for (var k = 0; k < B.length; k++)
                        {
                            var inpoly = pointInPolygon(new SvgPoint(B[k].x + B.offsetx.Value,
                                B[k].y + B.offsety.Value), A);
                            if (inpoly != null)
                            {
                                Binside = inpoly;
                                break;
                            }
                        }

                        if (Binside == null)
                        { // A and B are the same
                            return null;
                        }

                        var startPoint = new SvgPoint(B.offsetx.Value, B.offsety.Value);
                        if (((Binside.Value && inside) || (!Binside.Value && !inside)) &&
                            !intersect(A, B) && !inNfp(startPoint, NFP))
                        {
                            return startPoint;
                        }

                        // slide B along vector
                        var vx = A[i + 1].x - A[i].x;
                        var vy = A[i + 1].y - A[i].y;

                        var d1 = polygonProjectionDistance(A, B, new SvgPoint(vx, vy));
                        var d2 = polygonProjectionDistance(B, A, new SvgPoint(-vx, -vy));

                        double? d = null;

                        // todo: clean this up
                        if (d1 == null && d2 == null)
                        {
                            // nothin
                        }
                        else if (d1 == null)
                        {
                            d = d2;
                        }
                        else if (d2 == null)
                        {
                            d = d1;
                        }
                        else
                        {
                            d = Math.Min(d1.Value, d2.Value);
                        }

                        // only slide until no longer negative
                        // todo: clean this up
                        if (d != null && !_almostEqual(d, 0) && d > 0)
                        {

                        }
                        else
                        {
                            continue;
                        }

                        var vd2 = vx * vx + vy * vy;

                        if (d * d < vd2 && !_almostEqual(d * d, vd2))
                        {
                            var vd = (float)Math.Sqrt(vx * vx + vy * vy);
                            vx *= d.Value / vd;
                            vy *= d.Value / vd;
                        }

                        B.offsetx += vx;
                        B.offsety += vy;

                        for (var k = 0; k < B.length; k++)
                        {
                            var inpoly = pointInPolygon(
                                new SvgPoint(
                                 B[k].x + B.offsetx.Value, B[k].y + B.offsety.Value), A);
                            if (inpoly != null)
                            {
                                Binside = inpoly;
                                break;
                            }
                        }
                        startPoint =
                                            new SvgPoint(B.offsetx.Value, B.offsety.Value);
                        if (((Binside.Value && inside) || (!Binside.Value && !inside)) &&
                            !intersect(A, B) && !inNfp(startPoint, NFP))
                        {
                            return startPoint;
                        }
                    }
                }
            }



            return null;
        }

        public class TouchingItem
        {
            public TouchingItem(int _type, int _a, int _b)
            {
                A = _a;
                B = _b;
                type = _type;
            }
            public int A;
            public int B;
            public int type;

        }

        public static double? segmentDistance(SvgPoint A, SvgPoint B, SvgPoint E, SvgPoint F, SvgPoint direction)
        {
            var normal = new SvgPoint(
                direction.y,
                -direction.x

            );

            var reverse = new SvgPoint(
                    -direction.x,
                     -direction.y
                );

            var dotA = A.x * normal.x + A.y * normal.y;
            var dotB = B.x * normal.x + B.y * normal.y;
            var dotE = E.x * normal.x + E.y * normal.y;
            var dotF = F.x * normal.x + F.y * normal.y;

            var crossA = A.x * direction.x + A.y * direction.y;
            var crossB = B.x * direction.x + B.y * direction.y;
            var crossE = E.x * direction.x + E.y * direction.y;
            var crossF = F.x * direction.x + F.y * direction.y;

            var crossABmin = Math.Min(crossA, crossB);
            var crossABmax = Math.Max(crossA, crossB);

            var crossEFmax = Math.Max(crossE, crossF);
            var crossEFmin = Math.Min(crossE, crossF);

            var ABmin = Math.Min(dotA, dotB);
            var ABmax = Math.Max(dotA, dotB);

            var EFmax = Math.Max(dotE, dotF);
            var EFmin = Math.Min(dotE, dotF);

            // segments that will merely touch at one point
            if (_almostEqual(ABmax, EFmin, TOL) || _almostEqual(ABmin, EFmax, TOL))
            {
                return null;
            }
            // segments miss eachother completely
            if (ABmax < EFmin || ABmin > EFmax)
            {
                return null;
            }

            double overlap;

            if ((ABmax > EFmax && ABmin < EFmin) || (EFmax > ABmax && EFmin < ABmin))
            {
                overlap = 1;
            }
            else
            {
                var minMax = Math.Min(ABmax, EFmax);
                var maxMin = Math.Max(ABmin, EFmin);

                var maxMax = Math.Max(ABmax, EFmax);
                var minMin = Math.Min(ABmin, EFmin);

                overlap = (minMax - maxMin) / (maxMax - minMin);
            }

            var crossABE = (E.y - A.y) * (B.x - A.x) - (E.x - A.x) * (B.y - A.y);
            var crossABF = (F.y - A.y) * (B.x - A.x) - (F.x - A.x) * (B.y - A.y);

            // lines are colinear
            if (_almostEqual(crossABE, 0) && _almostEqual(crossABF, 0))
            {

                var ABnorm = new SvgPoint(B.y - A.y, A.x - B.x);
                var EFnorm = new SvgPoint(F.y - E.y, E.x - F.x);

                var ABnormlength = (float)Math.Sqrt(ABnorm.x * ABnorm.x + ABnorm.y * ABnorm.y);
                ABnorm.x /= ABnormlength;
                ABnorm.y /= ABnormlength;

                var EFnormlength = (float)Math.Sqrt(EFnorm.x * EFnorm.x + EFnorm.y * EFnorm.y);
                EFnorm.x /= EFnormlength;
                EFnorm.y /= EFnormlength;

                // segment normals must point in opposite directions
                if (Math.Abs(ABnorm.y * EFnorm.x - ABnorm.x * EFnorm.y) < TOL && ABnorm.y * EFnorm.y + ABnorm.x * EFnorm.x < 0)
                {
                    // normal of AB segment must point in same direction as given direction vector
                    var normdot = ABnorm.y * direction.y + ABnorm.x * direction.x;
                    // the segments merely slide along eachother
                    if (_almostEqual(normdot, 0, TOL))
                    {
                        return null;
                    }
                    if (normdot < 0)
                    {
                        return 0;
                    }
                }
                return null;
            }

            var distances = new List<double>();

            // coincident points
            if (_almostEqual(dotA, dotE))
            {
                distances.Add(crossA - crossE);
            }
            else if (_almostEqual(dotA, dotF))
            {
                distances.Add(crossA - crossF);
            }
            else if (dotA > EFmin && dotA < EFmax)
            {
                var d = pointDistance(A, E, F, reverse);
                if (d != null && _almostEqual(d, 0))
                { //  A currently touches EF, but AB is moving away from EF
                    var dB = pointDistance(B, E, F, reverse, true);
                    if (dB < 0 || _almostEqual(dB * overlap, 0))
                    {
                        d = null;
                    }
                }
                if (d != null)
                {
                    distances.Add(d.Value);
                }
            }

            if (_almostEqual(dotB, dotE))
            {
                distances.Add(crossB - crossE);
            }
            else if (_almostEqual(dotB, dotF))
            {
                distances.Add(crossB - crossF);
            }
            else if (dotB > EFmin && dotB < EFmax)
            {
                var d = pointDistance(B, E, F, reverse);

                if (d != null && _almostEqual(d, 0))
                { // crossA>crossB A currently touches EF, but AB is moving away from EF
                    var dA = pointDistance(A, E, F, reverse, true);
                    if (dA < 0 || _almostEqual(dA * overlap, 0))
                    {
                        d = null;
                    }
                }
                if (d != null)
                {
                    distances.Add(d.Value);
                }
            }

            if (dotE > ABmin && dotE < ABmax)
            {
                var d = pointDistance(E, A, B, direction);
                if (d != null && _almostEqual(d, 0))
                { // crossF<crossE A currently touches EF, but AB is moving away from EF
                    var dF = pointDistance(F, A, B, direction, true);
                    if (dF < 0 || _almostEqual(dF * overlap, 0))
                    {
                        d = null;
                    }
                }
                if (d != null)
                {
                    distances.Add(d.Value);
                }
            }

            if (dotF > ABmin && dotF < ABmax)
            {
                var d = pointDistance(F, A, B, direction);
                if (d != null && _almostEqual(d, 0))
                { // && crossE<crossF A currently touches EF, but AB is moving away from EF
                    var dE = pointDistance(E, A, B, direction, true);
                    if (dE < 0 || _almostEqual(dE * overlap, 0))
                    {
                        d = null;
                    }
                }
                if (d != null)
                {
                    distances.Add(d.Value);
                }
            }

            if (distances.Count == 0)
            {
                return null;
            }

            //return Math.min.apply(Math, distances);
            return distances.Min();
        }
        public static double? polygonSlideDistance(NFP A, NFP B, nVector direction, bool ignoreNegative)
        {

            SvgPoint A1, A2, B1, B2;
            double Aoffsetx, Aoffsety, Boffsetx, Boffsety;

            Aoffsetx = A.offsetx ?? 0;
            Aoffsety = A.offsety ?? 0;

            Boffsetx = B.offsetx ?? 0;
            Boffsety = B.offsety ?? 0;

            A = A.slice(0);
            B = B.slice(0);

            // close the loop for polygons
            if (A[0] != A[A.length - 1])
            {
                A.push(A[0]);
            }

            if (B[0] != B[B.length - 1])
            {
                B.push(B[0]);
            }

            var edgeA = A;
            var edgeB = B;

            double? distance = null;
            //var p, s1, s2;
            double? d;


            var dir = _normalizeVector(new SvgPoint(direction.x, direction.y));

            var normal = new SvgPoint(
                dir.y,
                 -dir.x
            );

            var reverse = new SvgPoint(-dir.x, -dir.y);

            for (var i = 0; i < edgeB.length - 1; i++)
            {
                //var mind = null;
                for (var j = 0; j < edgeA.length - 1; j++)
                {
                    A1 = new SvgPoint(
                         edgeA[j].x + Aoffsetx, edgeA[j].y + Aoffsety
        );
                    A2 = new SvgPoint(
                        edgeA[j + 1].x + Aoffsetx, edgeA[j + 1].y + Aoffsety
                );
                    B1 = new SvgPoint(edgeB[i].x + Boffsetx, edgeB[i].y + Boffsety);
                    B2 = new SvgPoint(edgeB[i + 1].x + Boffsetx, edgeB[i + 1].y + Boffsety);

                    if ((_almostEqual(A1.x, A2.x) && _almostEqual(A1.y, A2.y)) || (_almostEqual(B1.x, B2.x) && _almostEqual(B1.y, B2.y)))
                    {
                        continue; // ignore extremely small lines
                    }

                    d = segmentDistance(A1, A2, B1, B2, dir);

                    if (d != null && (distance == null || d < distance))
                    {
                        if (!ignoreNegative || d > 0 || _almostEqual(d, 0))
                        {
                            distance = d;
                        }
                    }
                }
            }
            return distance;
        }
        public class nVector
        {
            public SvgPoint start;
            public SvgPoint end;
            public double x;
            public double y;


            public nVector(double v1, double v2, SvgPoint _start, SvgPoint _end)
            {
                this.x = v1;
                this.y = v2;
                this.start = _start;
                this.end = _end;
            }
        }

        // given a static polygon A and a movable polygon B, compute a no fit polygon by orbiting B about A
        // if the inside flag is set, B is orbited inside of A rather than outside
        // if the searchEdges flag is set, all edges of A are explored for NFPs - multiple 
        public static NFP[] noFitPolygon(NFP A, NFP B, bool inside, bool searchEdges)
        {
            if (A == null || A.length < 3 || B == null || B.length < 3)
            {
                return null;
            }

            A.offsetx = 0;
            A.offsety = 0;

            int i = 0, j = 0;

            var minA = A[0].y;
            var minAindex = 0;

            var maxB = B[0].y;
            var maxBindex = 0;

            for (i = 1; i < A.length; i++)
            {
                A[i].marked = false;
                if (A[i].y < minA)
                {
                    minA = A[i].y;
                    minAindex = i;
                }
            }

            for (i = 1; i < B.length; i++)
            {
                B[i].marked = false;
                if (B[i].y > maxB)
                {
                    maxB = B[i].y;
                    maxBindex = i;
                }
            }
            SvgPoint startpoint;
            if (!inside)
            {
                // shift B such that the bottom-most point of B is at the top-most point of A. This guarantees an initial placement with no intersections
                startpoint = new SvgPoint(
                     A[minAindex].x - B[maxBindex].x,
                     A[minAindex].y - B[maxBindex].y);
            }
            else
            {
                // no reliable heuristic for inside

                startpoint = searchStartPoint(A, B, true);
            }

            List<NFP> NFPlist = new List<NFP>();



            while (startpoint != null)
            {

                B.offsetx = startpoint.x;
                B.offsety = startpoint.y;

                // maintain a list of touching points/edges
                List<TouchingItem> touching = null;

                nVector prevvector = null; // keep track of previous vector
                NFP NFP = new NFP();
                /*var NFP = [{
                    x: B[0].x + B.offsetx,
        y: B[0].y + B.offsety





                }];*/
                NFP.push(new SvgPoint(B[0].x + B.offsetx.Value, B[0].y + B.offsety.Value));

                double referencex = B[0].x + B.offsetx.Value;
                double referencey = B[0].y + B.offsety.Value;
                var startx = referencex;
                var starty = referencey;
                var counter = 0;

                while (counter < 10 * (A.length + B.length))
                { // sanity check, prevent infinite loop
                    touching = new List<GeometryUtil.TouchingItem>();
                    // find touching vertices/edges
                    for (i = 0; i < A.length; i++)
                    {
                        var nexti = (i == A.length - 1) ? 0 : i + 1;
                        for (j = 0; j < B.length; j++)
                        {
                            var nextj = (j == B.length - 1) ? 0 : j + 1;
                            if (_almostEqual(A[i].x, B[j].x + B.offsetx) && _almostEqual(A[i].y, B[j].y + B.offsety))
                            {
                                touching.Add(new TouchingItem(0, i, j));
                            }
                            else if (_onSegment(A[i], A[nexti],
                                new SvgPoint(B[j].x + B.offsetx.Value, B[j].y + B.offsety.Value)))
                            {
                                touching.Add(new TouchingItem(1, nexti, j));
                            }
                            else if (_onSegment(
                                new SvgPoint(
                                 B[j].x + B.offsetx.Value, B[j].y + B.offsety.Value),
                                new SvgPoint(
                                 B[nextj].x + B.offsetx.Value, B[nextj].y + B.offsety.Value), A[i]))
                            {
                                touching.Add(new TouchingItem(2, i, nextj));
                            }
                        }
                    }

                    // generate translation vectors from touching vertices/edges
                    var vectors = new List<nVector>();
                    for (i = 0; i < touching.Count; i++)
                    {
                        var vertexA = A[touching[i].A];
                        vertexA.marked = true;

                        // adjacent A vertices
                        var prevAindex = touching[i].A - 1;
                        var nextAindex = touching[i].A + 1;

                        prevAindex = (prevAindex < 0) ? A.length - 1 : prevAindex; // loop
                        nextAindex = (nextAindex >= A.length) ? 0 : nextAindex; // loop

                        var prevA = A[prevAindex];
                        var nextA = A[nextAindex];

                        // adjacent B vertices
                        var vertexB = B[touching[i].B];

                        var prevBindex = touching[i].B - 1;
                        var nextBindex = touching[i].B + 1;

                        prevBindex = (prevBindex < 0) ? B.length - 1 : prevBindex; // loop
                        nextBindex = (nextBindex >= B.length) ? 0 : nextBindex; // loop

                        var prevB = B[prevBindex];
                        var nextB = B[nextBindex];

                        if (touching[i].type == 0)
                        {

                            var vA1 = new nVector(
                                 prevA.x - vertexA.x,
                                 prevA.y - vertexA.y,
                                 vertexA,
                                 prevA
                                    );

                            var vA2 = new nVector(
                                     nextA.x - vertexA.x,
                                     nextA.y - vertexA.y,
                                     vertexA,
                                     nextA
                                        );

                            // B vectors need to be inverted
                            var vB1 = new nVector(
                                         vertexB.x - prevB.x,
                                         vertexB.y - prevB.y,
                                         prevB,
                                         vertexB
                                    );

                            var vB2 = new nVector(
                                             vertexB.x - nextB.x,
                                            vertexB.y - nextB.y,
                                             nextB,
                                             vertexB
                                        );

                            vectors.Add(vA1);
                            vectors.Add(vA2);
                            vectors.Add(vB1);
                            vectors.Add(vB2);
                        }
                        else if (touching[i].type == 1)
                        {
                            vectors.Add(new nVector(
                                 vertexA.x - (vertexB.x + B.offsetx.Value),
                                 vertexA.y - (vertexB.y + B.offsety.Value),
                                 prevA,
                                 vertexA
        ));

                            vectors.Add(new nVector(
                                 prevA.x - (vertexB.x + B.offsetx.Value),
                                prevA.y - (vertexB.y + B.offsety.Value),
                                 vertexA,
                                 prevA
        ));
                        }
                        else if (touching[i].type == 2)
                        {
                            vectors.Add(new nVector(
                                 vertexA.x - (vertexB.x + B.offsetx.Value),
                                vertexA.y - (vertexB.y + B.offsety.Value),
                                 prevB,
                                 vertexB
                            ));

                            vectors.Add(new nVector(
                                 vertexA.x - (prevB.x + B.offsetx.Value),
                                vertexA.y - (prevB.y + B.offsety.Value),
                                 vertexB,
                                 prevB

                            ));
                        }
                    }

                    // todo: there should be a faster way to reject vectors that will cause immediate intersection. For now just check them all

                    nVector translate = null;
                    double maxd = 0;

                    for (i = 0; i < vectors.Count; i++)
                    {
                        if (vectors[i].x == 0 && vectors[i].y == 0)
                        {
                            continue;
                        }

                        // if this vector points us back to where we came from, ignore it.
                        // ie cross product = 0, dot product < 0
                        if (prevvector != null &&
                            vectors[i].y * prevvector.y + vectors[i].x * prevvector.x < 0)
                        {

                            // compare magnitude with unit vectors
                            var vectorlength = (float)Math.Sqrt(vectors[i].x * vectors[i].x + vectors[i].y * vectors[i].y);
                            var unitv = new SvgPoint(vectors[i].x / vectorlength, vectors[i].y / vectorlength);

                            var prevlength = (float)Math.Sqrt(prevvector.x * prevvector.x + prevvector.y * prevvector.y);
                            var prevunit = new SvgPoint(prevvector.x / prevlength, prevvector.y / prevlength);

                            // we need to scale down to unit vectors to normalize vector length. Could also just do a tan here
                            if (Math.Abs(unitv.y * prevunit.x - unitv.x * prevunit.y) < 0.0001)
                            {
                                continue;
                            }
                        }

                        var d = polygonSlideDistance(A, B, vectors[i], true);
                        var vecd2 = vectors[i].x * vectors[i].x + vectors[i].y * vectors[i].y;

                        if (d == null || d * d > vecd2)
                        {
                            var vecd = (float)Math.Sqrt(vectors[i].x * vectors[i].x + vectors[i].y * vectors[i].y);
                            d = vecd;
                        }

                        if (d != null && d > maxd)
                        {
                            maxd = d.Value;
                            translate = vectors[i];
                        }
                    }


                    if (translate == null || _almostEqual(maxd, 0))
                    {
                        // didn't close the loop, something went wrong here
                        NFP = null;
                        break;
                    }

                    translate.start.marked = true;
                    translate.end.marked = true;

                    prevvector = translate;

                    // trim
                    var vlength2 = translate.x * translate.x + translate.y * translate.y;
                    if (maxd * maxd < vlength2 && !_almostEqual(maxd * maxd, vlength2))
                    {
                        var scale = (float)Math.Sqrt((maxd * maxd) / vlength2);
                        translate.x *= scale;
                        translate.y *= scale;
                    }

                    referencex += translate.x;
                    referencey += translate.y;

                    if (_almostEqual(referencex, startx) && _almostEqual(referencey, starty))
                    {
                        // we've made a full loop
                        break;
                    }

                    // if A and B start on a touching horizontal line, the end point may not be the start point
                    var looped = false;
                    if (NFP.length > 0)
                    {
                        for (i = 0; i < NFP.length - 1; i++)
                        {
                            if (_almostEqual(referencex, NFP[i].x) && _almostEqual(referencey, NFP[i].y))
                            {
                                looped = true;
                            }
                        }
                    }

                    if (looped)
                    {
                        // we've made a full loop
                        break;
                    }

                    NFP.push(new SvgPoint(
                         referencex, referencey
                    ));

                    B.offsetx += translate.x;
                    B.offsety += translate.y;

                    counter++;
                }

                if (NFP != null && NFP.length > 0)
                {
                    NFPlist.Add(NFP);

                }

                if (!searchEdges)
                {
                    // only get outer NFP or first inner NFP
                    break;
                }
                startpoint = searchStartPoint(A, B, inside, NFPlist.ToArray());
            }

            return NFPlist.ToArray();
        }



    }
    public class PolygonBounds
    {
        public double x;
        public double y;
        public double width;
        public double height;
        public PolygonBounds(double _x, double _y, double _w, double _h)
        {
            x = _x;
            y = _y;
            width = _w;
            height = _h;
        }
    }
}
