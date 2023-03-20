using IxMilia.Dxf.Entities;
using IxMilia.Dxf;
using System.Drawing;
using System.Reflection.Emit;

namespace DeepNestLib
{
    public static class DxfExporter
    {

        public static int Export(string path, IEnumerable<NFP> polygons, IEnumerable<NFP> sheets)
        {
            Dictionary<DxfFile, int> dxfexports = new Dictionary<DxfFile, int>();
            for (int i = 0; i < sheets.Count(); i++)
            {
                DxfFile sheetdxf;
                double sheetwidth;
                GenerateSheetOutline(sheets, i, out sheetdxf, out sheetwidth);

                foreach (NFP nFP in polygons)
                {
                    var pivot = nFP.Points[0];

                    DxfFile fl;
                    if (nFP.fitted == false || !nFP.Name.ToLower().Contains(".dxf") || nFP.sheet.Id != sheets.ElementAt(i).Id)
                    {
                        continue;
                    }
                    else
                    {
                        //fl = DxfFile.Load(nFP.Name);
                    }

                    var fle = nFP.Tag as object[];
                    double sheetXoffset = -sheetwidth * i;

                    DxfPoint offsetdistance = new DxfPoint(nFP.x + sheetXoffset, nFP.y, 0D);
                    List<DxfEntity> newlist = OffsetToNest(fle, new DxfPoint(pivot.x, pivot.y, 0), offsetdistance, nFP.Rotation);

                    foreach (DxfEntity ent in newlist)
                    {
                        sheetdxf.Entities.Add(ent);
                    }
                }

                dxfexports.Add(sheetdxf, sheets.ElementAt(i).Id);
            }

            int sheetcount = 0;
            for (int i = 0; i < dxfexports.Count(); i++)
            {
                var dxf = dxfexports.ElementAt(i).Key;
                var id = dxfexports.ElementAt(i).Value;

                if (dxf.Entities.Count != 1)
                {
                    sheetcount += 1;
                    if (sheets.Count() == 1)
                    {
                        dxf.Save(path, true);
                    }
                    else
                    {
                        FileInfo fi = new FileInfo(path);
                        dxf.Save($"{fi.FullName.Substring(0, fi.FullName.Length - 4)}{id}.dxf", true);
                    }
                }
            }

            return sheetcount;
        }
        private static void GenerateSheetOutline(IEnumerable<NFP> sheets, int i, out DxfFile sheetdxf, out double sheetwidth)
        {
            // Generate Sheet Outline in Dxf
            sheetdxf = new DxfFile();
            sheetdxf.Views.Clear();

            List<DxfVertex> sheetverts = new List<DxfVertex>();
            double sheetheight = sheets.ElementAt(i).HeightCalculated;
            sheetwidth = sheets.ElementAt(i).WidthCalculated;

            // Bl Point
            sheetverts.Add(new DxfVertex(new DxfPoint(0, 0, 0)));

            // BR Point
            sheetverts.Add(new DxfVertex(new DxfPoint(sheetwidth, 0, 0)));

            // TR Point
            sheetverts.Add(new DxfVertex(new DxfPoint(sheetwidth, sheetheight, 0)));

            // TL Point
            sheetverts.Add(new DxfVertex(new DxfPoint(0, sheetheight, 0)));

            DxfPolyline sheetentity = new DxfPolyline(sheetverts)
            {
                IsClosed = true,
                Layer = $"Plate H{sheetheight} W{sheetwidth}",
            };

            sheetdxf.Entities.Add(sheetentity);
        }
        private static List<DxfEntity> OffsetToNest(IList<object> entities, DxfPoint pivot, DxfPoint offset, double rotationAngle)
        {
            List<DxfEntity> dxfreturn = new List<DxfEntity>();
            List<DxfPoint> tmpPts;
            List<DxfEntity> dxfEntities = new List<DxfEntity>();
            foreach (var item in entities)
            {
                if (item is netDxf.Entities.Circle c)
                {
                    DxfCircle dc = new DxfCircle(new DxfPoint(c.Center.X, c.Center.Y, 0), c.Radius);
                    dxfEntities.Add(dc);
                }
                else if (item is netDxf.Entities.Line l)
                {
                    DxfLine ln = new DxfLine(new DxfPoint(l.StartPoint.X, l.StartPoint.Y, 0),
                        new DxfPoint(l.EndPoint.X, l.EndPoint.Y, 0));
                    dxfEntities.Add(ln);
                }
                else if (item is netDxf.Entities.Arc a)
                {
                    DxfArc arc = new DxfArc(new DxfPoint(a.Center.X, a.Center.Y, 0),
                        a.Radius, a.StartAngle, a.EndAngle);
                    dxfEntities.Add(arc);
                }
                else if (item is netDxf.Entities.Spline s)
                {
                    var verts = s.PolygonalVertexes(100);
                    double mult = 25.4;

                    var vv = verts.Select(z => new DxfVertex(new DxfPoint(z.X * mult, z.Y * mult, 0))).ToList();

                    DxfPolyline poly = new DxfPolyline(vv);
                    poly.IsClosed = s.IsClosed;
                    dxfEntities.Add(poly);
                }
                else if (item is PolylineExportInfo pei)
                {
                    var vv = pei.Points.Select(z => new DxfVertex(new DxfPoint(z.X, z.Y, 0))).ToList();
                    DxfPolyline poly = new DxfPolyline(vv);
                    poly.IsClosed = pei.IsClosed;
                    dxfEntities.Add(poly);
                }
            }
            foreach (DxfEntity entity in dxfEntities)
            {
                switch (entity.EntityType)
                {
                    case DxfEntityType.Arc:
                        DxfArc _dxfArc = (DxfArc)entity;
                        var dxfArc = new DxfArc(_dxfArc.Center, _dxfArc.Radius, _dxfArc.StartAngle, _dxfArc.EndAngle);
                        dxfArc.Center = RotateLocation(rotationAngle, dxfArc.Center);
                        dxfArc.Center += offset;
                        dxfArc.StartAngle += rotationAngle;
                        dxfArc.EndAngle += rotationAngle;
                        dxfreturn.Add(dxfArc);
                        break;

                    case DxfEntityType.ArcAlignedText:
                        DxfArcAlignedText dxfArcAligned = (DxfArcAlignedText)entity;
                        dxfArcAligned.CenterPoint = RotateLocation(rotationAngle, dxfArcAligned.CenterPoint);
                        dxfArcAligned.CenterPoint += offset;
                        dxfArcAligned.StartAngle += rotationAngle;
                        dxfArcAligned.EndAngle += rotationAngle;
                        dxfreturn.Add(dxfArcAligned);
                        break;

                    case DxfEntityType.Attribute:
                        DxfAttribute dxfAttribute = (DxfAttribute)entity;
                        dxfAttribute.Location = RotateLocation(rotationAngle, dxfAttribute.Location);
                        dxfAttribute.Location += offset;
                        dxfreturn.Add(dxfAttribute);
                        break;

                    case DxfEntityType.AttributeDefinition:
                        DxfAttributeDefinition dxfAttributecommon = (DxfAttributeDefinition)entity;
                        dxfAttributecommon.Location = RotateLocation(rotationAngle, dxfAttributecommon.Location);
                        dxfAttributecommon.Location += offset;
                        dxfreturn.Add(dxfAttributecommon);
                        break;

                    case DxfEntityType.Circle:
                        {
                            DxfCircle _dxfCircle = (DxfCircle)entity;
                            DxfCircle dxfCircle = new DxfCircle(_dxfCircle.Center, _dxfCircle.Radius);
                            dxfCircle.Center = RotateLocation(rotationAngle, dxfCircle.Center);
                            dxfCircle.Center += offset;
                            dxfreturn.Add(dxfCircle);
                        }
                        break;

                    case DxfEntityType.Ellipse:
                        {
                            DxfEllipse _dxfEllipse = (DxfEllipse)entity;
                            DxfEllipse dxfEllipse = new DxfEllipse(_dxfEllipse.Center, _dxfEllipse.MajorAxis, _dxfEllipse.MinorAxisRatio);
                            dxfEllipse.Center = RotateLocation(rotationAngle, dxfEllipse.Center);
                            dxfEllipse.Center += offset;
                            dxfreturn.Add(dxfEllipse);
                        }
                        break;

                    case DxfEntityType.Image:
                        DxfImage dxfImage = (DxfImage)entity;
                        dxfImage.Location = RotateLocation(rotationAngle, dxfImage.Location);
                        dxfImage.Location += offset;

                        dxfreturn.Add(dxfImage);
                        break;

                    case DxfEntityType.Leader:
                        DxfLeader dxfLeader = (DxfLeader)entity;
                        tmpPts = new List<DxfPoint>();

                        foreach (DxfPoint vrt in dxfLeader.Vertices)
                        {
                            var tmppnt = RotateLocation(rotationAngle, vrt);
                            tmppnt += offset;
                            tmpPts.Add(tmppnt);
                        }

                        dxfLeader.Vertices.Clear();
                        dxfLeader.Vertices.Concat(tmpPts);
                        dxfreturn.Add(dxfLeader);
                        break;

                    case DxfEntityType.Line:
                        DxfLine _dxfLine = (DxfLine)entity;
                        DxfLine dxfLine = new DxfLine(_dxfLine.P1, _dxfLine.P2);
                        dxfLine.P1 = RotateLocation(rotationAngle, dxfLine.P1);
                        dxfLine.P2 = RotateLocation(rotationAngle, dxfLine.P2);
                        dxfLine.P1 += offset;
                        dxfLine.P2 += offset;
                        dxfreturn.Add(dxfLine);
                        break;

                    case DxfEntityType.LwPolyline:
                        {
                            DxfLwPolyline _dxfPoly = (DxfLwPolyline)entity;

                            List<DxfLwPolylineVertex> verts = new List<DxfLwPolylineVertex>();
                            foreach (var _vrt in _dxfPoly.Vertices)
                            {
                                var vrt = new DxfVertex();
                                vrt.Location = new DxfPoint(_vrt.X, _vrt.Y, 0);

                                vrt.Location = RotateLocation(rotationAngle, vrt.Location);
                                vrt.Location += offset;
                                verts.Add(new DxfLwPolylineVertex() { X = vrt.Location.X, Y = vrt.Location.Y });
                            }
                            DxfLwPolyline dxfPoly = new DxfLwPolyline(verts.ToArray());

                            dxfreturn.Add(dxfPoly);
                        }
                        break;

                    case DxfEntityType.MLine:
                        DxfMLine mLine = (DxfMLine)entity;
                        tmpPts = new List<DxfPoint>();
                        mLine.StartPoint += offset;

                        mLine.StartPoint = RotateLocation(rotationAngle, mLine.StartPoint);

                        foreach (DxfPoint vrt in mLine.Vertices)
                        {
                            var tmppnt = RotateLocation(rotationAngle, vrt);
                            tmppnt += offset;
                            tmpPts.Add(tmppnt);
                        }

                        mLine.Vertices.Clear();
                        mLine.Vertices.Concat(tmpPts);
                        dxfreturn.Add(mLine);
                        break;

                    case DxfEntityType.Polyline:
                        {
                            DxfPolyline polyline = (DxfPolyline)entity;

                            List<DxfVertex> verts = new List<DxfVertex>();
                            foreach (DxfVertex vrt in polyline.Vertices)
                            {
                                var tmppnt = vrt;
                                tmppnt.Location = RotateLocation(rotationAngle, tmppnt.Location);
                                tmppnt.Location += offset;
                                verts.Add(tmppnt);
                            }

                            DxfPolyline polyout = new DxfPolyline(verts);
                            polyout.Location = polyline.Location + offset;
                            polyout.IsClosed = polyline.IsClosed;
                            polyout.Layer = polyline.Layer;
                            dxfreturn.Add(polyout);
                        }
                        break;

                    case DxfEntityType.Body:
                    case DxfEntityType.DgnUnderlay:
                    case DxfEntityType.Dimension:
                    case DxfEntityType.DwfUnderlay:
                    case DxfEntityType.Face:
                    case DxfEntityType.Helix:
                    case DxfEntityType.Insert:
                    case DxfEntityType.Light:
                    case DxfEntityType.ModelerGeometry:
                    case DxfEntityType.MText:
                    case DxfEntityType.OleFrame:
                    case DxfEntityType.Ole2Frame:
                    case DxfEntityType.PdfUnderlay:
                    case DxfEntityType.Point:
                    case DxfEntityType.ProxyEntity:
                    case DxfEntityType.Ray:
                    case DxfEntityType.Region:
                    case DxfEntityType.RText:
                    case DxfEntityType.Section:
                    case DxfEntityType.Seqend:
                    case DxfEntityType.Shape:
                    case DxfEntityType.Solid:
                    case DxfEntityType.Spline:
                    case DxfEntityType.Text:
                    case DxfEntityType.Tolerance:
                    case DxfEntityType.Trace:
                    case DxfEntityType.Underlay:
                    case DxfEntityType.Vertex:
                    case DxfEntityType.WipeOut:
                    case DxfEntityType.XLine:
                        throw new ArgumentException("unsupported entity type: " + entity.EntityType);
                }
            }

            return dxfreturn;
        }

        public static DxfPoint RotateLocation(double rotationAngle, DxfPoint pt)
        {
            var angle = (float)(rotationAngle * Math.PI / 180.0f);
            var x = pt.X;
            var y = pt.Y;
            var x1 = (float)((x * Math.Cos(angle)) - (y * Math.Sin(angle)));
            var y1 = (float)((x * Math.Sin(angle)) + (y * Math.Cos(angle)));
            return new DxfPoint(x1, y1, pt.Z);
        }
    }
}