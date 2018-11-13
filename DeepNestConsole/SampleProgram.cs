using DeepNestLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DeepNestConsole
{
    public class SampleProgram
    {
        List<NFP> polygons = new List<NFP>();
        List<NFP> sheets = new List<NFP>();

        public void AddSheet(int w, int h)
        {
            var tt = new RectanglePolygonSheet();
            tt.Name = "sheet" + (sheets.Count + 1);
            sheets.Add(tt);
            var p = sheets.Last();
            tt.Height = h;
            tt.Width = w;
            tt.Rebuild();
            ReorderSheets();
        }

        Random r = new Random();

        public int GetNextSource()
        {
            if (polygons.Any())
            {
                return polygons.Max(z => z.source.Value) + 1;
            }
            return 0;
        }

        public void AddRectanglePart(int src, int ww = 50, int hh = 80)
        {
            int xx = 0;
            int yy = 0;
            NFP pl = new NFP();

            polygons.Add(pl);
            pl.source = src;
            pl.Points = new SvgPoint[] { };
            pl.AddPoint(new SvgPoint(xx, yy));
            pl.AddPoint(new SvgPoint(xx + ww, yy));
            pl.AddPoint(new SvgPoint(xx + ww, yy + hh));
            pl.AddPoint(new SvgPoint(xx, yy + hh));
        }


        public bool IsFinished()
        {
            //insert you code here
            //return current.fitness < 12e6;
            return iterations >= 3;
        }

        public void LoadDataFromXml(string v)
        {
            var d = XDocument.Load(v);
            var f = d.Descendants().First();
            var gap = int.Parse(f.Attribute("gap").Value);
            SvgNest.Config.spacing = gap;

            foreach (var item in d.Descendants("sheet"))
            {
                var cnt = int.Parse(item.Attribute("count").Value);
                var ww = int.Parse(item.Attribute("width").Value);
                var hh = int.Parse(item.Attribute("height").Value);

                for (int i = 0; i < cnt; i++)
                {
                    AddSheet(ww, hh);
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

        public void StartNest()
        {
            current = null;
            nest = new SvgNest();
            Background.cacheProcess2 = new Dictionary<string, NFP[]>();

            Background.window = new windowUnk();
            Background.callCounter = 0;
        }

        public void DeepNestIterate()
        {


            List<NFP> lsheets = new List<NFP>();
            List<NFP> lpoly = new List<NFP>();
            for (int i = 0; i < polygons.Count; i++)
            {
                polygons[i].id = i;
            }
            for (int i = 0; i < sheets.Count; i++)
            {
                sheets[i].id = i;
            }
            foreach (var item in polygons)
            {
                NFP clone = new NFP();
                clone.id = item.id;
                clone.source = item.source;
                clone.Points = item.Points.Select(z => new SvgPoint(z.x, z.y) { exact = z.exact }).ToArray();

                lpoly.Add(clone);
            }


            foreach (var item in sheets)
            {
                RectanglePolygonSheet clone = new RectanglePolygonSheet();
                clone.id = item.id;
                clone.source = item.source;
                clone.Points = item.Points.Select(z => new SvgPoint(z.x, z.y) { exact = z.exact }).ToArray();

                lsheets.Add(clone);
            }


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


            nest.launchWorkers(partsLocal.ToArray());
            var plcpr = nest.nests.First();


            if (current == null || plcpr.fitness < current.fitness)
            {
                AssignPlacement(plcpr);
            }
        }

        SheetPlacement current = null;
        SvgNest nest;
        public void AssignPlacement(SheetPlacement plcpr)
        {
            current = plcpr;
            double totalSheetsArea = 0;
            double totalPartsArea = 0;

            List<Polygon> placed = new List<Polygon>();
            foreach (var item in polygons)
            {
                item.fitted = false;
            }
            foreach (var item in plcpr.placements)
            {
                foreach (var zitem in item)
                {
                    var sheetid = zitem.sheetId;
                    var sheet = sheets.First(z => z.id == sheetid);
                    totalSheetsArea += GeometryUtil.polygonArea(sheet);

                    foreach (var ssitem in zitem.sheetplacements)
                    {

                        var poly = polygons.First(z => z.id == ssitem.id);
                        totalPartsArea += GeometryUtil.polygonArea(poly);
                        placed.Add(poly);
                        poly.fitted = true;
                        poly.x = ssitem.x + sheet.x;
                        poly.y = ssitem.y + sheet.y;
                        poly.rotation = ssitem.rotation;
                    }
                }
            }

            var ppps = polygons.Where(z => !placed.Contains(z));
            foreach (var item in ppps)
            {
                item.x = -500;
                item.y = 0;
            }
        }
        public void ReorderSheets()
        {
            double x = 0;
            double y = 0;
            for (int i = 0; i < sheets.Count; i++)
            {
                sheets[i].x = x;
                sheets[i].y = y;
                var r = sheets[i] as RectanglePolygonSheet;
                x += r.Width + 10;
            }
        }


        public void LoadSampleData()
        {
            Console.WriteLine("Adding sheets..");
            //add sheets
            for (int i = 0; i < 5; i++)
            {
                AddSheet(3000, 1500);
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
        public void ImportFromRawDetail(RawDetail raw, int src)
        {
            NFP po = new NFP();
            po.Name = raw.Name;
            po.Points = new SvgPoint[] { };
            //if (raw.Outers.Any())
            {
                var tt = raw.Outers.Union(raw.Holes).OrderByDescending(z => z.Len).First();
                foreach (var item in tt.Points)
                {
                    po.AddPoint(new SvgPoint(item.X, item.Y));
                }

                po.source = src;
                polygons.Add(po);
            }
        }
        int iterations = 0;
        public void Run()
        {


            Background.UseParallel = true;
            SvgNest.Config.placementType = PlacementTypeEnum.gravity;
            Console.WriteLine("Settings updated..");

            Console.WriteLine("Start nesting..");
            Console.WriteLine("Parts: " + polygons.Count());
            Console.WriteLine("Sheets: " + sheets.Count());
            StartNest();
            iterations = 0;
            do
            {
                var sw = Stopwatch.StartNew();
                DeepNestIterate();
                sw.Stop();
                Console.WriteLine("Iteration: " + iterations + "; fitness: " + current.fitness + "; nesting time: " + sw.ElapsedMilliseconds + "ms");
                iterations++;
            } while (!IsFinished());

            #region convert results
            SvgParser.Export("temp.svg", polygons, sheets);
            Console.WriteLine("Results exported in: temp.svg");
            #endregion            
        }
    }
}
