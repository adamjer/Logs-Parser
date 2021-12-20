using Newtonsoft.Json;
using SpirV___get_fail_reasons.GTAX;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpirV___get_fail_reasons
{
    class FatalsFromCobaltAnalyzer : Analyzer
    {

        public FatalsFromCobaltAnalyzer() : base()
        {
        }

        public FatalsFromCobaltAnalyzer(DataAnalyzer dataAnalyzer) : base(dataAnalyzer)
        {
        }


        private String LoadTestLog()
        {
            String result = "";

            result = File.ReadAllText(@"C:\Users\ajerecze\OneDrive - Intel Corporation\Documents\Visual Studio 2019\Projects\spirv---get-fail-reasons\SpirV - get fail reasons\Test\test.txt");

            return result;
        }

        private List<String> ParseFatals(Result result, String log, String taskLog)
        {
            List<String> parsedLogs = new List<String>();

            try
            {
                Match firstHeader = null, secondHeader = null;
                while (true)
                {
                    if (firstHeader == null && secondHeader == null)
                    {
                        firstHeader = Regex.Match(log, @"\*\*\*\*\* \*\*\*\* \*\*\* \*\* \* FATAL \* \*\* \*\*\* \*\*\*\* \*\*\*\*\*");
                        if (!firstHeader.Success)
                            break;
                        secondHeader = Regex.Match(log.Substring(firstHeader.Index), @"\*\*\*\*\* \*\*\*\* \*\*\* \*\* END FATAL \*\* \*\*\* \*\*\*\* \*\*\*\*\*");
                        if (!secondHeader.Success)
                            break;
                    }
                    else
                    {
                        firstHeader = firstHeader.NextMatch();
                        if (!firstHeader.Success)
                            break;
                        secondHeader = Regex.Match(log.Substring(firstHeader.Index), @"\*\*\*\*\* \*\*\*\* \*\*\* \*\* END FATAL \*\* \*\*\* \*\*\*\* \*\*\*\*\*");
                        if (!secondHeader.Success)
                            break;
                    }

                    if (firstHeader.Index > 0 && secondHeader.Index > 0)
                    {
                        String fatal = "File: " + taskLog + Environment.NewLine + ReadMatch(log, firstHeader, secondHeader);
                        if (fatal != String.Empty)
                        {
                            if (!parsedLogs.Contains(fatal))
                                parsedLogs.Add(fatal);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("ParseFatals:" + ex.Message);
            }
            return parsedLogs;
        }

        private List<String> ParseExceptionLastLines(String log, String sentence, int length, String taskLog)
        {
            List<String> parsedLogs = new List<String>();
            if (log.Contains(sentence))
            {
                parsedLogs.Add("File: " + taskLog + Environment.NewLine + log.Substring(log.Length > length ? log.Length - length : 0));
            }

            return parsedLogs;
        }

        override public void Analyze()
        {
            String hyperlink = "";
            String jsonResult = "";
            foreach (JobSetSessionNS.JobSetSession jobSetSession in DataAnalyzer.JobSetSession.JobSetSessions)
            {
                hyperlink = GetJobsetSessionHyperlink(jobSetSession);
                if (true)
                {
                    jsonResult = this.DataAnalyzer.webClient.DownloadString(hyperlink);
                    GTAX_Jobs jobs = JsonConvert.DeserializeObject<GTAX_Jobs>(jsonResult);
                    jsonResult = "";

                    foreach (Job job in jobs.Jobs)
                    {
                        Parallel.ForEach(job.Results, result =>
                            {
                                if (result.PhaseName == "tests")
                                {
                                    if (result.Artifacts != null)
                                    {
                                        GtaxResult task = new GtaxResult();
                                        List<String> parsedLogs = new List<string>();
                                        task.Name = result.BusinessAttributes.ItemName;
                                        task.Status = result.GTAStatus;
                                        task.SetLink(result.JobID, result.ID);

                                        String log = "", downloadHyperLink = "";
                                        String testNumber = Regex.Split(result.GtaResultKey, @"\D+").Last();

                                        var logNames = result.Artifacts.TaskLogs.Where(l => l.ToLower().Contains("cobalt2")).Where(l => !l.Contains(".gz"));
                                        if (logNames.Any(l => l.ToLower().Contains("cobalt2-tail.log")))
                                        {
                                            logNames = logNames.Where(l => !l.ToLower().Contains("cobalt2.log") && !l.ToLower().Contains("cobalt2-head.log"));
                                        }

                                        foreach (string logName in logNames)
                                        {                                           
                                            try
                                            {
                                                downloadHyperLink = GetTestResultsHyperlink(result.JobID, testNumber, logName);
                                                log = this.DataAnalyzer.DownloadAsync(downloadHyperLink);
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.Out.WriteLine("Download log: " + ex.Message);
                                                Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                                log = this.DataAnalyzer.DownloadAsync(downloadHyperLink);
                                            }

                                            //log = LoadTestLog();

                                            try
                                            {
                                                parsedLogs = ParseFatals(result, log, "Cobalt2.log");
                                                task.ParsedResults.AddRange(parsedLogs);
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.Out.WriteLine(ex.Message);
                                                Console.Out.WriteLine("Download log: " + ex.Message);
                                                Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                            }

                                            String[] keywords;
                                            //Bluescreens are contained in FATALS
                                            //try
                                            //{
                                            //    keywords = new String[] { @"||  || ||\\   ////",
                                            //        @"For more details refer to HDC Blue Screen dump above." };
                                            //    parsedLogs = Parse(result, log, keywords, "Cobalt2.log");
                                            //    task.ParsedResults.AddRange(parsedLogs);
                                            //}
                                            //catch (Exception ex)
                                            //{
                                            //    Console.Out.WriteLine(ex.Message);
                                            //    Console.Out.WriteLine("Download log: " + ex.Message);
                                            //    Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                            //}

                                            try
                                            {
                                                keywords = new String[] { @"*********************** WARNING *************************",
                                                    @"*********************************************************" };
                                                parsedLogs = Parse(result, log, keywords, "Cobalt2.log");
                                                task.ParsedResults.AddRange(parsedLogs);
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.Out.WriteLine(ex.Message);
                                                Console.Out.WriteLine("Download log: " + ex.Message);
                                                Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                            }

                                            try
                                            {
                                                keywords = new String[] { @"Fatal: ASSERT", @"Fail" };
                                                parsedLogs = Parse(result, log, keywords, "Cobalt2.log");
                                                task.ParsedResults.AddRange(parsedLogs);
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.Out.WriteLine(ex.Message);
                                                Console.Out.WriteLine("Download log: " + ex.Message);
                                                Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                            }

                                            try
                                            {
                                                keywords = new String[] { @"ERROR:", "\n" };
                                                parsedLogs = Parse(result, log, keywords, "Cobalt2.log");
                                                task.ParsedResults.AddRange(parsedLogs);
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.Out.WriteLine(ex.Message);
                                                Console.Out.WriteLine("Download log: " + ex.Message);
                                                Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                            }

                                            try
                                            {
                                                keywords = new String[] { @"WARNING:", "\n" };
                                                parsedLogs = Parse(result, log, keywords, "Cobalt2.log");
                                                task.ParsedResults.AddRange(parsedLogs);
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.Out.WriteLine(ex.Message);
                                                Console.Out.WriteLine("Download log: " + ex.Message);
                                                Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                            }
                                           
                                            try
                                            {
                                                keywords = new String[] { @"WARNING", "\n" };
                                                parsedLogs = Parse(result, log, keywords, "Cobalt2.log");
                                                task.ParsedResults.AddRange(parsedLogs);
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.Out.WriteLine(ex.Message);
                                                Console.Out.WriteLine("Download log: " + ex.Message);
                                                Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                            }

                                            try
                                            {
                                                keywords = new String[] { @"INFO", "\n" };
                                                parsedLogs = Parse(result, log, keywords, "Cobalt2.log");
                                                task.ParsedResults.AddRange(parsedLogs);
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.Out.WriteLine(ex.Message);
                                                Console.Out.WriteLine("Download log: " + ex.Message);
                                                Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                            }

                                            try
                                            {
                                                keywords = new String[] { @"INFO:", "\n" };
                                                parsedLogs = Parse(result, log, keywords, "Cobalt2.log");
                                                task.ParsedResults.AddRange(parsedLogs);
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.Out.WriteLine(ex.Message);
                                                Console.Out.WriteLine("Download log: " + ex.Message);
                                                Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                            }

                                            //try
                                            //{
                                            //    if (task.ParsedResults.Count == 0)
                                            //        task.ParsedResults.AddRange(ParseExceptionLastLines(log, "failed with exception, what():", 2000, "Cobalt2.log"));
                                            //}
                                            //catch (Exception ex)
                                            //{
                                            //    Console.Out.WriteLine(ex.Message);
                                            //    Console.Out.WriteLine("Download log: " + ex.Message);
                                            //    Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                            //}

                                            try
                                            {
                                                if (task.ParsedResults.Count > 0)
                                                {
                                                    task.ParsedResults = task.ParsedResults.Distinct().ToList();
                                                    Results.Add(task);
                                                    Console.Out.WriteLine("Results count: " + Results.Count);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.Out.WriteLine(ex.Message);
                                            }
                                        }
                                    }
                                }
                            });
                    }
                }
                else
                {
                    Console.Out.WriteLine("TestSession not completed yet!");
                }
            }
        }
    }
}
