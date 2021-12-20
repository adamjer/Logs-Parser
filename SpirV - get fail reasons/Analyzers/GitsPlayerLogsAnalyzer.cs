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
using System.Threading;
using System.Threading.Tasks;

namespace SpirV___get_fail_reasons
{
    class GitsPlayerLogsAnalyzer : Analyzer
    {
        public GitsPlayerLogsAnalyzer() : base()
        {
        }

        public GitsPlayerLogsAnalyzer(DataAnalyzer dataAnalyzer) : base(dataAnalyzer)
        {           
        }

        private String LoadTestLog()
        {
            String result = "";

            result = File.ReadAllText(@"C:\Users\ajerecze\OneDrive - Intel Corporation\Documents\Visual Studio 2019\Projects\spirv---get-fail-reasons\SpirV - get fail reasons\Test\test.txt");

            return result;
        }

        override public void Analyze()
        {
            String hyperlink = "";
            String jsonResult = "";
            int counter = 0;
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

                                    var logNames = result.Artifacts.TaskLogs.Where(l => l.ToLower().Contains("gits_player_output"));

                                    foreach (string logName in logNames)
                                    {
                                        Interlocked.Increment(ref counter);
                                        String[] keywords = { "", "" };
                                        try
                                        {
                                            downloadHyperLink = GetTestResultsHyperlink(result.JobID, testNumber, logName);
                                            log = this.DataAnalyzer.DownloadAsync(downloadHyperLink);
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.Out.WriteLine("Download log: " + ex.Message);
                                            Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                        }

                                        //log = LoadTestLog();

                                        try
                                        {
                                            keywords = new String[] { @"Warn: The", @"GITS internal format." };
                                            parsedLogs = Parse(result, log, keywords, "gits_player_output.log");
                                            task.ParsedResults.AddRange(parsedLogs);
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.Out.WriteLine("Download log: " + ex.Message);
                                            Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                        }

                                        try
                                        {
                                            keywords = new String[] { @"Err:", "\n" };
                                            parsedLogs = Parse(result, log, keywords, "gits_player_output.log");
                                            task.ParsedResults.AddRange(parsedLogs);
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.Out.WriteLine("Download log: " + ex.Message);
                                            Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                        }

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
                Console.Out.WriteLine($"Parsed {counter} stream tests!");
            }
        }
    }
}
