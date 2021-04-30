using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpirV___get_fail_reasons
{
    class Program
    {
        public static ExecutionType executionType = ExecutionType.GetSpirV;
        public static EnvironmentType environment = EnvironmentType.Silicon;
        public static ExcelDataSaveType excelSaveType = ExcelDataSaveType.Basic;
        public static string outputPath = @"\\samba-users.igk.intel.com\samba\Users\ajerecze\NNImages";
        public static double passRatio = 0.0;
        public static bool Clear = false;

        private static bool IsParseable(string[] args)
        {
            if (args.Length > 0)
                return true;
            return false;
        }

        private static string[] RemoveUsedArguments(string[] args, int index)
        {
            return args.Except(new string[] { args[index], args[index + 1] }).ToArray();
        }

        private static string[] RemoveUsedArgument(string[] args, int index)
        {
            return args.Except(new string[] { args[index] }).ToArray();
        }

        //choose the way application works
        private static void ParseExecutionType(ref string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower() == "-executiontype")
                {
                    if (args[i + 1].ToLower() == "getspirv")
                        executionType = ExecutionType.GetSpirV;
                    else if (args[i + 1].ToLower() == "getcorruptedimages")
                        executionType = ExecutionType.GetCorruptedImages;
                    else if (args[i + 1].ToLower() == "getfatalsfromcobalt")
                        executionType = ExecutionType.GetFatalsFromCobalt;
                    else if (args[i + 1].ToLower() == "getgitsplayerlogs")
                        executionType = ExecutionType.GetGitsPlayerLogs;
                    args = RemoveUsedArguments(args, i);
                    break;
                }
            }
        }

        //unused TODO -> choose percentage of conformance similarity below which images won't be downloaded
        private static void ParseConformancePassRatio(ref string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower() == "-passratio")
                {
                    passRatio = Double.Parse(args[i + 1]);

                    args = RemoveUsedArguments(args, i);
                    break;
                }
            }
        }

        //choose where to save images
        private static void ParseOutputDirectory(ref string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower() == "-outputdirectory")
                {
                    outputPath = args[i + 1];

                    args = RemoveUsedArguments(args, i);
                    break;
                }
            }
        }

        //choose to delete all images that are in output directory
        private static void ParseClear(ref string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower() == "-clearImages")
                {
                    Clear = true;

                    args = RemoveUsedArgument(args, i);
                    break;
                }
            }
        }

        //choose environment
        private static void ParseEnvironment(ref string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower() == "-environment")
                {
                    if (args[i + 1].ToLower() == "silicon")
                        environment = EnvironmentType.Silicon;
                    else if (args[i + 1].ToLower() == "simulation")
                        environment = EnvironmentType.Simulation;
                    else if (args[i + 1].ToLower() == "emulation")
                        environment = EnvironmentType.Emulation;

                    args = RemoveUsedArguments(args, i);
                    break;
                }
            }
        }

        //choose excel formating
        private static void ParseWriteExcelType(ref string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower() == "-excel")
                {
                    if (args[i + 1].ToLower() == "basic")
                        excelSaveType = ExcelDataSaveType.Basic;
                    else if (args[i + 1].ToLower() == "advanced")
                        excelSaveType = ExcelDataSaveType.Advanced;
                    else if (args[i + 1].ToLower() == "fatal")
                        excelSaveType = ExcelDataSaveType.Fatal;

                    args = RemoveUsedArguments(args, i);
                    break;
                }
            }
        }

        public static void Parse(ref string[] args)
        {
            ParseExecutionType(ref args);
            ParseOutputDirectory(ref args);
            ParseConformancePassRatio(ref args);
            ParseWriteExcelType(ref args);
            ParseClear(ref args);
            ParseEnvironment(ref args);
        }

        static void Main(string[] args)
        {
            if (IsParseable(args))
            {
                Parse(ref args);
            }

            Analyzer analyzer = null;
            if (executionType == ExecutionType.GetSpirV)
                analyzer = new LogsFromDataAnalyzer();
            if (executionType == ExecutionType.GetCorruptedImages)
                analyzer = new ImagesFromDataAnalyzer();
            if (executionType == ExecutionType.GetFatalsFromCobalt)
                analyzer = new FatalsFromCobaltAnalyzer();
            if (executionType == ExecutionType.GetGitsPlayerLogs)
                analyzer = new GitsPlayerLogsAnalyzer();

            try
            {
                analyzer.Init();
                analyzer.Analyze();

                ExcelDataWriter excelDataWriter = new ExcelDataWriter(analyzer.Results.ToList(), DataAnalyzer.Instance);
                excelDataWriter.Write();
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }

            Console.Out.WriteLine("Enter any key to exit...");
            Console.In.ReadLine();
        }
    }
}
