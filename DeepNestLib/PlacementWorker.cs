using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeepNestLib
{
    public class PlacementWorker
    {

        Polygon binPolygon;
        Polygon[] paths;
        int[] ids;
        float[] rotations;
        SvgNestConfig config;

        public PlacementWorker(Polygon _binPolygon, Polygon[] _paths, int[] _ids,
            float[] _rotations,
            SvgNestConfig _config,
            Dictionary<string, List<NFP>> _nfpCache)
        {
            config = _config;
            binPolygon = _binPolygon;
            ids = _ids;
            paths = _paths;
            rotations = _rotations;
            //nfpCache = _nfpCache;
        }
        public Dictionary<string, List<NFP>> nfpCache = new Dictionary<string, List<NFP>>();

        /*
         this.binPolygon = binPolygon;
	this.paths = paths;
	this.ids = ids;
	this.rotations = rotations;
	this.config = config;
	this.nfpCache = nfpCache || {};*/

        public static Polygon rotatePolygon(Polygon polygon, float degrees)
        {
            Polygon rotated = new Polygon();

            var angle = degrees * Math.PI / 180;
            List<SvgPoint> pp = new List<SvgPoint>();
            for (var i = 0; i < polygon.length; i++)
            {
                var x = polygon[i].x;
                var y = polygon[i].y;
                var x1 = (float)(x * Math.Cos(angle) - y * Math.Sin(angle));
                var y1 = (float)(x * Math.Sin(angle) + y * Math.Cos(angle));

                pp.Add(new SvgPoint(x1, y1));
            }
            rotated.Points = pp.ToArray();

            if (polygon.children != null && polygon.children.Count > 0)
            {
                rotated.children = new List<NFP>(); ;
                for (var j = 0; j < polygon.children.Count; j++)
                {
                    rotated.children.Add((NFP)rotatePolygon(polygon.children[j], degrees));
                }
            }

            return rotated;
        }

        public Placement placePaths(Polygon[] paths)
        {

            if (binPolygon == null)
            {
                return null;
            }

            int i, j, k, m, n;

            Polygon path;
            // rotate paths by given rotation
            var rotated = new List<Polygon>();
            for (i = 0; i < paths.Length; i++)
            {
                var r = rotatePolygon(paths[i], paths[i].rotation);
                r.Rotation = paths[i].rotation;
                r.source = paths[i].source;
                r.Id = paths[i].Id;
                rotated.Add(r);
            }

            paths = rotated.ToArray();

            List<PlacementItem> allplacements = new List<PlacementItem>();

            double fitness = 0;
            var binarea = Math.Abs(GeometryUtil.polygonArea(binPolygon));
            string key;
            List<NFP> nfp;
            while (paths.Length > 0)
            {

                List<Polygon> placed = new List<Polygon>();

                List<PlacementItem> placements = new List<PlacementItem>();

                fitness += 1; // add 1 for each new bin opened (lower fitness is better)
                double? minwidth = null;
                for (i = 0; i < paths.Length; i++)
                {
                    path = paths[i];

                    // inner NFP
                    key =
                        new NfpKey()
                        {
                            BIndex = path.Id,
                            AIndex = -1,
                            Inside = true,
                            ARotation = 0,
                            BRotation = path.Rotation
                        }.stringify();
                    //{ A: -1,B: path.id,inside: true,Arotation: 0,Brotation: path.rotation}

                    if (!nfpCache.ContainsKey(key)) continue;
                    var binNfp = nfpCache[key];

                    // part unplaceable, skip
                    if (binNfp == null || binNfp.Count == 0)
                    {
                        continue;
                    }

                    // ensure all necessary NFPs exist
                    var error = false;
                    for (j = 0; j < placed.Count; j++)
                    {
                        key =
                            new NfpKey()
                            {
                                AIndex = placed[j].id,
                                BIndex = path.Id,
                                Inside = false,
                                ARotation = placed[j].Rotation,
                                BRotation = path.Rotation

                            }.stringify();
                        //    { A: placed[j].id,B: path.id,inside: false,Arotation: placed[j].rotation,Brotation: path.rotation}

                        nfp = nfpCache[key];

                        if (nfp == null)
                        {
                            error = true;
                            break;
                        }
                    }

                    // part unplaceable, skip
                    if (error)
                    {
                        continue;
                    }

                    PlacementItem position = null;
                    if (placed.Count == 0)
                    {
                        // first placement, put it on the left
                        for (j = 0; j < binNfp.Count; j++)
                        {
                            for (k = 0; k < binNfp[j].length; k++)
                            {
                                if (position == null || binNfp[j][k].x - path[0].x < position.x)
                                {


                                    position = new PlacementItem()
                                    {
                                        x = binNfp[j][k].x - path[0].x,
                                        y = binNfp[j][k].y - path[0].y,
                                        id = path.id,
                                        rotation = path.rotation

                                    };
                                }
                            }
                        }

                        placements.Add(position);
                        placed.Add(path);

                        continue;
                    }

                    List<List<ClipperLib.IntPoint>> clipperBinNfp = new List<List<ClipperLib.IntPoint>>();
                    for (j = 0; j < binNfp.Count; j++)
                    {
                        clipperBinNfp.Add(SvgNest.toClipperCoordinates(binNfp[j]).ToList());
                    }

                    _Clipper.ScaleUpPaths(clipperBinNfp, SvgNest.Config.clipperScale);

                    var clipper = new ClipperLib.Clipper();
                    var combinedNfp = new List<List<ClipperLib.IntPoint>>();


                    for (j = 0; j < placed.Count; j++)
                    {
                        key =
                            new NfpKey()
                            {
                                AIndex = placed[j].id,
                                BIndex = path.id,
                                Inside = false,
                                ARotation = placed[j].rotation,
                                BRotation = path.rotation
                            }.stringify();
                        //{ A: placed[j].id,B: path.id,inside: false,Arotation: placed[j].rotation,Brotation: path.rotation}

                        nfp = nfpCache[key];

                        if (nfp == null)
                        {
                            continue;
                        }

                        for (k = 0; k < nfp.Count; k++)
                        {
                            var clone = SvgNest.toClipperCoordinates(nfp[k]);
                            for (m = 0; m < clone.Count(); m++)
                            {
                                clone[m].X += (long)placements[j].x;
                                clone[m].Y += (long)placements[j].y;
                            }

                            _Clipper.ScaleUpPath(clone, SvgNest.Config.clipperScale);
                            clone = ClipperLib.Clipper.CleanPolygon(clone.ToList(), 0.0001
                                * SvgNest.Config.clipperScale).ToArray();
                            var area2 = Math.Abs(ClipperLib.Clipper.Area(clone.ToList()));
                            if (clone.Length > 2
                                && area2 > 0.1 * SvgNest.Config.clipperScale *
                                SvgNest.Config.clipperScale)
                            {
                                clipper.AddPath(clone.ToList(), ClipperLib.PolyType.ptSubject, true);
                            }
                        }
                    }



                    if (!clipper.Execute(ClipperLib.ClipType.ctUnion, combinedNfp, ClipperLib.PolyFillType.pftNonZero, ClipperLib.PolyFillType.pftNonZero))
                    {
                        continue;
                    }

                    // difference with bin polygon
                    var finalNfp = new List<List<ClipperLib.IntPoint>>();
                    clipper = new ClipperLib.Clipper();

                    clipper.AddPaths(combinedNfp, ClipperLib.PolyType.ptClip, true);
                    clipper.AddPaths(clipperBinNfp, ClipperLib.PolyType.ptSubject, true);
                    if (!clipper.Execute(ClipperLib.ClipType.ctDifference, finalNfp, ClipperLib.PolyFillType.pftNonZero, ClipperLib.PolyFillType.pftNonZero))
                    {
                        continue;
                    }

                    finalNfp = ClipperLib.Clipper.CleanPolygons(finalNfp, 0.0001 *
                        SvgNest.Config.clipperScale);

                    for (j = 0; j < finalNfp.Count; j++)
                    {
                        var area2 = Math.Abs(ClipperLib.Clipper.Area(finalNfp[j]));
                        if (finalNfp[j].Count < 3 || area2 < 0.1 * SvgNest.Config.clipperScale *
                        SvgNest.Config.clipperScale)
                        {
                            finalNfp = finalNfp.splice(j, 1);
                            j--;
                        }
                    }

                    if (finalNfp == null || finalNfp.Count == 0)
                    {
                        continue;
                    }

                    List<Polygon> f = new List<Polygon>();


                    for (j = 0; j < finalNfp.Count; j++)
                    {
                        // back to normal scale
                        f.Add(Background.toNestCoordinates(finalNfp[j].ToArray(), SvgNest.Config.clipperScale));
                    }

                    List<Polygon> finalNfp2 = new List<Polygon>();
                    finalNfp2 = f;

                    // choose placement that results in the smallest bounding box
                    // could use convex hull instead, but it can create oddly shaped nests (triangles or long slivers) which are not optimal for real-world use
                    // todo: generalize gravity direction
                    //float? minwidth = null;
                    double? minarea = null;
                    double? minx = null;
                    PlacementItem shiftvector;
                    Polygon nf;
                    double area;

                    for (j = 0; j < finalNfp2.Count; j++)
                    {
                        nf = finalNfp2[j];
                        if (Math.Abs(GeometryUtil.polygonArea(nf)) < 2)
                        {
                            continue;
                        }

                        for (k = 0; k < nf.length; k++)
                        {

                            List<SvgPoint> allpoints = new List<SvgPoint>();
                            for (m = 0; m < placed.Count; m++)
                            {
                                for (n = 0; n < placed[m].length; n++)
                                {
                                    allpoints.Add(
                                        new SvgPoint(
                                        placed[m][n].x + placements[m].x, placed[m][n].y + placements[m].y));
                                }
                            }

                            shiftvector = new PlacementItem()
                            {
                                x = nf[k].x - path[0].x,
                                y = nf[k].y - path[0].y,
                                id = path.id,
                                rotation = path.rotation,
                                nfp = combinedNfp

                            }
/*{
    x: nf[k].x - path[0].x,
y: nf[k].y - path[0].y,
id: path.id,
rotation: path.rotation,
nfp: combinedNfp


}*/;

                            for (m = 0; m < path.length; m++)
                            {
                                allpoints.Add(new SvgPoint(
                                     path[m].x + shiftvector.x, path[m].y + shiftvector.y));
                            }

                            var rectbounds = GeometryUtil.getPolygonBounds(allpoints);

                            // weigh width more, to help compress in direction of gravity
                            area = rectbounds.width * 2 + rectbounds.height;

                            if (minarea == null || area < minarea || (GeometryUtil._almostEqual(minarea, area)
                                                && (minx == null || shiftvector.x < minx)))
                            {
                                minarea = area;
                                minwidth = rectbounds.width;
                                position = shiftvector;
                                minx = shiftvector.x;
                            }
                        }
                    }
                    if (position != null)
                    {
                        placed.Add(path);
                        placements.Add(position);
                    }
                }

                if (minwidth != null)
                {
                    fitness += minwidth.Value / binarea;
                }

                for (i = 0; i < placed.Count; i++)
                {

                    var index = Array.IndexOf(paths, placed[i]);
                    if (index >= 0)
                    {

                        paths = paths.splice(index, 1);
                    }
                }

                if (placements != null && placements.Count > 0)
                {
                    //allplacements.Add(placements);
                    allplacements.AddRange(placements);
                }
                else
                {
                    break; // something went wrong
                }
            }

            // there were parts that couldn't be placed
            fitness += 2 * paths.Length;

            return new Placement()
            {
                placements = new[] { allplacements.ToList() },
                fitness = fitness,
                paths = paths,
                area = binarea
            };
        }


    }

}
