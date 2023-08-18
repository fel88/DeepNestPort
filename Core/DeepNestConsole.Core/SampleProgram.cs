using DeepNestLib;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace DeepNestConsole.Core
{
    public class SampleProgram
    {
        public NestingContext Context = new NestingContext();
        const int NestTimeLimit = 10000;
        Stopwatch timer = new Stopwatch();
        public bool IsFinished()
        {
            //insert you code here            
            return timer.ElapsedMilliseconds > NestTimeLimit;
        }

        public void LoadXml(string v)
        {
            var d = XDocument.Load(v);
            var f = d.Descendants().First();
            var gap = int.Parse(f.Attribute("gap").Value);
            SvgNest.Config.spacing = gap;

            foreach (var item in d.Descendants("sheet"))
            {
                int src = Context.GetNextSheetSource();
                var cnt = int.Parse(item.Attribute("count").Value);
                if (item.Attribute("width") != null)
                {
                    var ww = int.Parse(item.Attribute("width").Value);
                    var hh = int.Parse(item.Attribute("height").Value);

                    for (int i = 0; i < cnt; i++)
                    {
                        Context.AddSheet(ww, hh, src);
                    }
                }
                else
                {
                    var r = CsvParser.Parse(item.Value);
                    var nfp = r[0].ToNfp();
                    for (int i = 0; i < cnt; i++)
                    {
                        Context.AddSheet(nfp, src);
                    }

                }
            }
            foreach (var item in d.Descendants("part"))
            {
                var cnt = int.Parse(item.Attribute("count").Value);
                RawDetail[] r = null;
                if (item.Attribute("path") != null)
                {
                    var path = item.Attribute("path").Value;

                    if (path.ToLower().EndsWith("svg"))
                    {
                        r = SvgParser.LoadSvg(path);
                    }
                    else if (path.ToLower().EndsWith("dxf"))
                    {
                        r = DxfParser.LoadDxf(path);
                    }
                    else if (path.ToLower().EndsWith("csv"))
                    {
                        r = CsvParser.Load(path);
                    }
                    else
                    {
                        continue;
                    }
                }
                else //inline mode
                {
                    r = CsvParser.Parse(item.Value);
                    r[0].Name = item.Attribute("name").Value;
                }


                var src = Context.GetNextSource();

                foreach (var itemr in r)
                {
                    for (int i = 0; i < cnt; i++)
                    {
                        Context.ImportFromRawDetail(itemr, src);
                    }
                }
            }
        }

        public void Run()
        {
            Background.UseParallel = true;
            SvgNest.Config.placementType = PlacementTypeEnum.gravity;
            Console.WriteLine("Settings updated..");

            Console.WriteLine("Start nesting..");
            Console.WriteLine("Parts: " + Context.Polygons.Count());
            Console.WriteLine("Sheets: " + Context.Sheets.Count());

            Context.StartNest();

            timer = Stopwatch.StartNew();
            double bestFitness = double.MaxValue;
            do
            {
                var sw = Stopwatch.StartNew();
                Context.NestIterate();
                sw.Stop();
                if (Context.Current.fitness.Value < bestFitness)
                {
                    bestFitness = Context.Current.fitness.Value;
                    Console.WriteLine($"Iteration: {Context.Iterations}; fitness: {Context.Current.fitness}; nesting time: {sw.ElapsedMilliseconds}ms");
                }
            } while (!IsFinished());

            #region convert results
            string path = "output.svg";
            Export(path);
            Console.WriteLine($"Results exported in: {path}");
            #endregion            
        }

        public void Export(string v)
        {
            if (v.ToLower().EndsWith("svg"))
                SvgParser.Export(v, Context.Polygons, Context.Sheets);
            else if (v.ToLower().EndsWith("dxf"))
                DxfExporter.Export(v, Context.Polygons, Context.Sheets);
            else if (v.ToLower().EndsWith("jpg"))
                throw new NotImplementedException();
            //JpgExporter.Export(v, Context.Polygons, Context.Sheets);
            else
                throw new NotImplementedException($"unknown format: {v}");
        }

    }
}
