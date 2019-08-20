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

                    args = RemoveUsedArguments(args, i);
                    break;
                }
            }
        }

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

        private static void ParseClear(ref string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower() == "-clear")
                {
                    Clear = true;

                    args = RemoveUsedArgument(args, i);
                    break;
                }
            }
        }

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

        public static void Parse(ref string[] args)
        {
            ParseExecutionType(ref args);
            ParseOutputDirectory(ref args);
            ParseConformancePassRatio(ref args);
            ParseClear(ref args);
            ParseEnvironment(ref args);
        }

        static void Main(string[] args)
        {
            if (IsParseable(args))
            {
                Parse(ref args);
            }

            if (executionType == ExecutionType.GetSpirV)
                spirVProgram.Main();
            else if (executionType == ExecutionType.GetCorruptedImages)
                GetCorruptedImagesProgram.Main();

            Console.Out.WriteLine("Enter any key to exit...");
            Console.In.ReadLine();
        }
    }
}
