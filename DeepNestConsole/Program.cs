using DeepNestLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepNestConsole
{
    class Program
    {

        static void ShowUsage()
        {
            Console.WriteLine("deepnest console tool. usage:");
            Console.WriteLine("Runing sample:");
            Console.WriteLine("deepNestConsole sample");
            Console.WriteLine("");            
            Console.WriteLine("Runing nesting from xml plan:");
            Console.WriteLine("deepNestConsole xml [xml]");
        }

        static void Main(string[] args)
        {
            if (args.Count() < 1)
            {
                ShowUsage();
                return;
            }
            var type = args[0];
            SampleProgram sample = new SampleProgram();
            switch (type)
            {
                case "xml":
                    {
                        if (args.Count() < 2)
                        {
                            Console.WriteLine("wrong format");
                            return;
                        }
                        if (!File.Exists(args[1]))
                        {
                            Console.WriteLine("xml file not exist.");
                            return;
                        }
                        try
                        {
                            sample.Context.LoadXml(args[1]);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error loading xml file: " + ex.Message);
                            return;
                        }
                    }
                    break;             
                case "sample":
                    {
                        sample.Context.LoadSampleData();
                    }
                    break;
                default:
                    {
                        Console.WriteLine("wrong format");
                        ShowUsage();
                        return;
                    }
            }

            sample.Run();
        }
    }
}
