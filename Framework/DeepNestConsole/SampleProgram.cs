﻿using DeepNestLib;
using System;
using System.Diagnostics;
using System.Linq;

namespace DeepNestConsole
{
    public class SampleProgram
    {
        public NestingContext Context = new NestingContext();
        public bool IsFinished()
        {
            //insert you code here            
            return Context.Iterations >= 3;
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
            do
            {
                var sw = Stopwatch.StartNew();
                Context.NestIterate();
                sw.Stop();
                Console.WriteLine("Iteration: " + Context.Iterations + "; fitness: " + Context.Current.fitness + "; nesting time: " + sw.ElapsedMilliseconds + "ms");
            } while (!IsFinished());

            #region convert results
            string path = "output.dxf";
            Context.Export(path);
            Console.WriteLine($"Results exported in: {path}");
            #endregion            
        }
    }
}
