using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DeepNestLib
{
    public class NestingContext
    {
        public List<NFP> Polygons { get; private set; } = new List<NFP>();
        public List<NFP> Sheets { get; private set; } = new List<NFP>();


        public double MaterialUtilization { get; private set; } = 0;
        public int PlacedPartsCount { get; private set; } = 0;


        SheetPlacement current = null;
        public SheetPlacement Current { get { return current; } }
        public SvgNest Nest { get; private set; }

        public int Iterations { get; private set; } = 0;

        public void StartNest()
        {
            current = null;
            Nest = new SvgNest();
            Background.cacheProcess = new Dictionary<string, NFP[]>();
            Background.window = new windowUnk();
            Background.callCounter = 0;
            Iterations = 0;
        }

        bool offsetTreePhase = true;
        public void NestIterate()
        {
            List<NFP> lsheets = new List<NFP>();
            List<NFP> lpoly = new List<NFP>();

            for (int i = 0; i < Polygons.Count; i++)
            {
                Polygons[i].id = i;
            }
            for (int i = 0; i < Sheets.Count; i++)
            {
                Sheets[i].id = i;
            }
            foreach (var item in Polygons)
            {
                NFP clone = new NFP();
                clone.id = item.id;
                clone.source = item.source;
                clone.Points = item.Points.Select(z => new SvgPoint(z.x, z.y) { exact = z.exact }).ToArray();
                if (item.children != null)
                {
                    clone.children = new List<NFP>();
                    foreach (var citem in item.children)
                    {
                        clone.children.Add(new NFP());
                        var l = clone.children.Last();
                        l.id = citem.id;
                        l.source = citem.source;
                        l.Points = citem.Points.Select(z => new SvgPoint(z.x, z.y) { exact = z.exact }).ToArray();
                    }
                }
                lpoly.Add(clone);
            }


            foreach (var item in Sheets)
            {
                NFP clone = new NFP();
                clone.id = item.id;
                clone.source = item.source;
                clone.Points = item.Points.Select(z => new SvgPoint(z.x, z.y) { exact = z.exact }).ToArray();
                if (item.children != null)
                {
                    clone.children = new List<NFP>();
                    foreach (var citem in item.children)
                    {
                        clone.children.Add(new NFP());
                        var l = clone.children.Last();
                        l.id = citem.id;
                        l.source = citem.source;
                        l.Points = citem.Points.Select(z => new SvgPoint(z.x, z.y) { exact = z.exact }).ToArray();
                    }
                }
                lsheets.Add(clone);
            }

            if (offsetTreePhase)
            {
                var grps = lpoly.GroupBy(z => z.source).ToArray();
                if (Background.UseParallel)
                {
                    Parallel.ForEach(grps, (item) =>
                    {
                        SvgNest.offsetTree(item.First(), 0.5 * SvgNest.Config.spacing, SvgNest.Config);
                        foreach (var zitem in item)
                        {
                            zitem.Points = item.First().Points.ToArray();
                        }

                    });

                }
                else
                {

                    foreach (var item in grps)
                    {
                        SvgNest.offsetTree(item.First(), 0.5 * SvgNest.Config.spacing, SvgNest.Config);
                        foreach (var zitem in item)
                        {
                            zitem.Points = item.First().Points.ToArray();
                        }
                    }
                }

                foreach (var item in lsheets)
                {
                    SvgNest.offsetTree(item, -0.5 * SvgNest.Config.spacing, SvgNest.Config, true);
                }
            }



            List<NestItem> partsLocal = new List<NestItem>();
            var p1 = lpoly.GroupBy(z => z.source).Select(z => new NestItem()
            {
                Polygon = z.First(),
                IsSheet = false,
                Quanity = z.Count()
            });

            var p2 = lsheets.GroupBy(z => z.source).Select(z => new NestItem()
            {
                Polygon = z.First(),
                IsSheet = true,
                Quanity = z.Count()
            });


            partsLocal.AddRange(p1);
            partsLocal.AddRange(p2);
            int srcc = 0;
            foreach (var item in partsLocal)
            {
                item.Polygon.source = srcc++;
            }


            Nest.launchWorkers(partsLocal.ToArray());
            var plcpr = Nest.nests.First();

            if (current == null || plcpr.fitness < current.fitness)
            {
                AssignPlacement(plcpr);
            }
            Iterations++;
        }

        public void ExportSvg(string v)
        {
            SvgParser.Export(v, Polygons, Sheets);
        }


        public void AssignPlacement(SheetPlacement plcpr)
        {
            current = plcpr;
            double totalSheetsArea = 0;
            double totalPartsArea = 0;

            PlacedPartsCount = 0;
            List<NFP> placed = new List<NFP>();
            foreach (var item in Polygons)
            {                
                item.sheet = null;
            }
            List<int> sheetsIds = new List<int>();

            foreach (var item in plcpr.placements)
            {
                foreach (var zitem in item)
                {
                    var sheetid = zitem.sheetId;
                    if (!sheetsIds.Contains(sheetid))
                    {
                        sheetsIds.Add(sheetid);
                    }

                    var sheet = Sheets.First(z => z.id == sheetid);
                    totalSheetsArea += GeometryUtil.polygonArea(sheet);

                    foreach (var ssitem in zitem.sheetplacements)
                    {
                        PlacedPartsCount++;
                        var poly = Polygons.First(z => z.id == ssitem.id);
                        totalPartsArea += GeometryUtil.polygonArea(poly);
                        placed.Add(poly);                        
                        poly.sheet = sheet;
                        poly.x = ssitem.x + sheet.x;
                        poly.y = ssitem.y + sheet.y;
                        poly.rotation = ssitem.rotation;
                    }
                }
            }

            var emptySheets = Sheets.Where(z => !sheetsIds.Contains(z.id)).ToArray();

            MaterialUtilization = Math.Abs(totalPartsArea / totalSheetsArea);

            var ppps = Polygons.Where(z => !placed.Contains(z));
            foreach (var item in ppps)
            {
                item.x = -1000;
                item.y = 0;
            }
        }

        public void ReorderSheets()
        {
            double x = 0;
            double y = 0;
            int gap = 10;
            for (int i = 0; i < Sheets.Count; i++)
            {
                Sheets[i].x = x;
                Sheets[i].y = y;
                if (Sheets[i] is Sheet)
                {
                    var r = Sheets[i] as Sheet;
                    x += r.Width + gap;
                }
                else
                {
                    var maxx = Sheets[i].Points.Max(z => z.x);
                    var minx = Sheets[i].Points.Min(z => z.x);
                    var w = maxx - minx;
                    x += w + gap;
                }
            }
        }

        public void AddSheet(int w, int h, int src)
        {
            var tt = new RectangleSheet();
            tt.Name = "sheet" + (Sheets.Count + 1);
            Sheets.Add(tt);

            tt.source = src;
            tt.Height = h;
            tt.Width = w;
            tt.Rebuild();
            ReorderSheets();
        }

        Random r = new Random();


        public void LoadSampleData()
        {
            Console.WriteLine("Adding sheets..");
            //add sheets
            for (int i = 0; i < 5; i++)
            {
                AddSheet(3000, 1500, 0);
            }

            Console.WriteLine("Adding parts..");
            //add parts
            int src1 = GetNextSource();
            for (int i = 0; i < 200; i++)
            {
                AddRectanglePart(src1, 250, 220);
            }

        }
        public void LoadInputData(string path, int count)
        {
            var dir = new DirectoryInfo(path);
            foreach (var item in dir.GetFiles("*.svg"))
            {
                try
                {
                    var src = GetNextSource();
                    for (int i = 0; i < count; i++)
                    {
                        ImportFromRawDetail(SvgParser.LoadSvg(item.FullName), src);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading " + item.FullName + ". skip");
                }
            }
        }
        public NFP ImportFromRawDetail(RawDetail raw, int src)
        {
            NFP po = null;
            List<NFP> nfps = new List<NFP>();
            foreach (var item in raw.Outers)
            {
                var nn = new NFP();
                nfps.Add(nn);
                foreach (var pitem in item.Points)
                {
                    nn.AddPoint(new SvgPoint(pitem.X, pitem.Y));
                }
            }

            if (nfps.Any())
            {
                var tt = nfps.OrderByDescending(z => z.Area).First();
                po = tt;
                po.Name = raw.Name;

                foreach (var r in nfps)
                {
                    if (r == tt) continue;
                    if (po.children == null)
                    {
                        po.children = new List<NFP>();
                    }
                    po.children.Add(r);
                }

                po.source = src;
                Polygons.Add(po);
            }
            return po;
        }
        public int GetNextSource()
        {
            if (Polygons.Any())
            {
                return Polygons.Max(z => z.source.Value) + 1;
            }
            return 0;
        }
        public int GetNextSheetSource()
        {
            if (Sheets.Any())
            {
                return Sheets.Max(z => z.source.Value) + 1;
            }
            return 0;
        }
        public void AddRectanglePart(int src, int ww = 50, int hh = 80)
        {
            int xx = 0;
            int yy = 0;
            NFP pl = new NFP();

            Polygons.Add(pl);
            pl.source = src;
            pl.Points = new SvgPoint[] { };
            pl.AddPoint(new SvgPoint(xx, yy));
            pl.AddPoint(new SvgPoint(xx + ww, yy));
            pl.AddPoint(new SvgPoint(xx + ww, yy + hh));
            pl.AddPoint(new SvgPoint(xx, yy + hh));
        }
        public void LoadXml(string v)
        {
            var d = XDocument.Load(v);
            var f = d.Descendants().First();
            var gap = int.Parse(f.Attribute("gap").Value);
            SvgNest.Config.spacing = gap;

            foreach (var item in d.Descendants("sheet"))
            {
                int src = GetNextSheetSource();
                var cnt = int.Parse(item.Attribute("count").Value);
                var ww = int.Parse(item.Attribute("width").Value);
                var hh = int.Parse(item.Attribute("height").Value);

                for (int i = 0; i < cnt; i++)
                {
                    AddSheet(ww, hh, src);
                }
            }
            foreach (var item in d.Descendants("part"))
            {
                var cnt = int.Parse(item.Attribute("count").Value);
                var path = item.Attribute("path").Value;
                var r = SvgParser.LoadSvg(path);
                var src = GetNextSource();

                for (int i = 0; i < cnt; i++)
                {
                    ImportFromRawDetail(r, src);
                }
            }
        }
    }
}
