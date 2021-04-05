using Newtonsoft.Json;
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
    class FatalsFromCobaltAnalyzer
    {
        private DataAnalyzer DataAnalyzer { get; set; }
        public ConcurrentBag<SpirVTask> Results { get; set; }

        public FatalsFromCobaltAnalyzer(DataAnalyzer dataAnalyzer)
        {
            this.DataAnalyzer = dataAnalyzer;
            this.Results = new ConcurrentBag<SpirVTask>();
        }

        private String GetJobsetHyperlink(JobSetSessionNS.JobSetSession jobSetSession)
        {
            if (Program.environment == EnvironmentType.Silicon)
                return $"http://gtax-igk.intel.com/api/v1/jobsets/{jobSetSession.ID}/results?group_by=job&include_subtasks_passcounts=false&order_by=id&order_type=desc";
            else if (Program.environment == EnvironmentType.Simulation)
                return $"http://gtax-presi-igk.intel.com/api/v1/jobsets/{jobSetSession.ID}/results?group_by=job&include_subtasks_passcounts=false&order_by=id&order_type=desc";
            else if (Program.environment == EnvironmentType.Emulation)
                throw new Exception("Emulation not supported yet!");
            else
                return "";
        }

        private String ReadMatch(String log, Match firstHeader, Match secondHeader)
        {
            String line;
            StringReader reader = new StringReader(log.Substring(firstHeader.Index, secondHeader.Index + secondHeader.Length));
            StringBuilder builder = new StringBuilder();

            if (secondHeader.Value == "\n")
                return log.Substring(firstHeader.Index, secondHeader.Index + secondHeader.Length);

            while (!(line = reader.ReadLine()).Contains(secondHeader.Value))
            {
                if (line == null)
                    break;
                if (!builder.ToString().Contains(line))
                    builder.AppendLine(line);
            }
            builder.Append(line);

            String result = builder.ToString();
            if (result == String.Empty)
                return String.Empty;

            return result.Remove(result.LastIndexOf("\r\n"), 1);
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
                //log = LoadTestLog();
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

        private List<String> Parse(Result result, String log, String[] keyWords, String taskLog)
        {
            List<String> parsedLogs = new List<String>();
            try
            {
                //log = LoadTestLog();
                Match firstHeader = null, secondHeader = null;
                while (true)
                {
                    if (firstHeader == null && secondHeader == null)
                    {
                        firstHeader = Regex.Match(log, Regex.Escape(keyWords[0]));
                        if (!firstHeader.Success)
                            break;
                        secondHeader = Regex.Match(log.Substring(firstHeader.Index), Regex.Escape(keyWords[1]));
                        if (!secondHeader.Success)
                            break;
                    }
                    else
                    {
                        firstHeader = firstHeader.NextMatch();
                        if (!firstHeader.Success)
                            break;
                        secondHeader = Regex.Match(log.Substring(firstHeader.Index), Regex.Escape(keyWords[1]));
                        if (!secondHeader.Success)
                            break;
                    }

                    if (firstHeader.Index > 0 && secondHeader.Index > 0)
                    {
                        String parsedLog = "File: " + taskLog + Environment.NewLine + ReadMatch(log, firstHeader, secondHeader);
                        if (parsedLog != String.Empty)
                        {
                            if (!parsedLogs.Contains(parsedLog))
                                parsedLogs.Add(parsedLog);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("Parse(): " + ex.Message);
            }
            return parsedLogs;
        }

        private List<String> ParseExceptionLastLines(String log, String sentence, int length, String taskLog)
        {
            List<String> parsedLogs = new List<String>();
            if (log.Contains(sentence))
            {
                parsedLogs.Add("File: " + taskLog + Environment.NewLine + log.Substring(log.Length - length));
            }

            return parsedLogs;
        }



        public void Analyze()
        {
            String hyperlink = "";
            String jsonResult = "";
            foreach (JobSetSessionNS.JobSetSession jobSetSession in DataAnalyzer.JobSetSession.JobSetSessions)
            {
                hyperlink = GetJobsetHyperlink(jobSetSession);
                if (true)
                {
                    jsonResult = this.DataAnalyzer.webClient.DownloadString(hyperlink);
                    JobSet jobSet = JsonConvert.DeserializeObject<JobSet>(jsonResult);
                    jsonResult = "";

                    foreach (Job job in jobSet.Jobs)
                    {
                        Parallel.ForEach(job.Results, result =>
                            {
                                if (result.PhaseName == "tests")
                                {
                                    if (result.Artifacts != null)
                                    {
                                        SpirVTask task = new SpirVTask();
                                        List<String> parsedLogs = new List<string>();
                                        task.Name = result.BusinessAttributes.ItemName;
                                        task.Status = result.GTAStatus;
                                        task.SetLink(result.JobID, result.ID);

                                        String log = "", downloadHyperLink = "";
                                        String testNumber = Regex.Split(result.GtaResultKey, @"\D+").Last();

                                        var logNames = result.Artifacts.TaskLogs.Where(l => l.ToLower().Contains("cobalt2")).Where(l => !l.Contains(".gz"));
                                        if (logNames.Contains("Cobalt2-tail.log"))
                                        {
                                            logNames = logNames.Where(l => !l.Contains("Cobalt2.log") && !l.Contains("Cobalt2-head.log"));
                                        }

                                        foreach (string logName in logNames)
                                        {
                                            String[] keywords;
                                            try
                                            {
                                                downloadHyperLink = $"http://gtax-presi-igk.intel.com/logs/jobs/jobs/0000/{result.JobID.Substring(0, 4)}/{result.JobID}/logs/tests/{testNumber}/{logName}";
                                                log = this.DataAnalyzer.DownloadAsync(downloadHyperLink);
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.Out.WriteLine("Download log: " + ex.Message);
                                                Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                                log = this.DataAnalyzer.DownloadAsync(downloadHyperLink);
                                            }

                                            try
                                            {
                                                parsedLogs = ParseFatals(result, log, "Cobalt2.log");
                                                task.SubTask.AddRange(parsedLogs);
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.Out.WriteLine(ex.Message);
                                                Console.Out.WriteLine("Download log: " + ex.Message);
                                                Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                            }

                                            try
                                            {
                                                keywords = new String[] { @"||  || ||\\   ////",
                                        @"For more details refer to HDC Blue Screen dump above." };
                                                parsedLogs = Parse(result, log, keywords, "Cobalt2.log");
                                                task.SubTask.AddRange(parsedLogs);
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.Out.WriteLine(ex.Message);
                                                Console.Out.WriteLine("Download log: " + ex.Message);
                                                Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                            }

                                            try
                                            {
                                                keywords = new String[] { @"*********************** WARNING *************************",
                                        @"*********************************************************" };
                                                parsedLogs = Parse(result, log, keywords, "Cobalt2.log");
                                                task.SubTask.AddRange(parsedLogs);
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.Out.WriteLine(ex.Message);
                                                Console.Out.WriteLine("Download log: " + ex.Message);
                                                Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                            }

                                            try
                                            {
                                                keywords = new String[] { @"FATAL: ASSERT", "Fail" };
                                                parsedLogs = Parse(result, log, keywords, "Cobalt2.log");
                                                task.SubTask.AddRange(parsedLogs);
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
                                                task.SubTask.AddRange(parsedLogs);
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
                                                task.SubTask.AddRange(parsedLogs);
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
                                                task.SubTask.AddRange(parsedLogs);
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.Out.WriteLine(ex.Message);
                                                Console.Out.WriteLine("Download log: " + ex.Message);
                                                Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                            }

                                            try
                                            {
                                                if (task.SubTask.Count == 0)
                                                    task.SubTask.AddRange(ParseExceptionLastLines(log, "failed with exception, what():", 2000, "Cobalt2.log"));
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.Out.WriteLine(ex.Message);
                                                Console.Out.WriteLine("Download log: " + ex.Message);
                                                Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                            }

                                            try
                                            {
                                                if (task.SubTask.Count > 0)
                                                {
                                                    task.SubTask = task.SubTask.Distinct().ToList();
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

                        //foreach (Result result in job.Results)
                        //{
                        //    if (result.PhaseName == "tests")
                        //    {
                        //        if (result.Artifacts != null)
                        //        {
                        //            SpirVTask task = new SpirVTask();
                        //            task.Name = result.BusinessAttributes.ItemName;
                        //            task.Status = result.GTAStatus;
                        //            task.SetLink(result.JobID, result.ID);
                        //            String[] logFilesToLookIn = { "cobalt2-tail.log", "cobalt2.log" };

                        //            Int32 counter = task.SubTask.Count;
                        //            ParseFatals(result, ref task, logFilesToLookIn[0]);
                        //            if (task.SubTask.Count == counter)
                        //            {
                        //                ParseFatals(result, ref task, logFilesToLookIn[1]);
                        //            }

                        //            counter = task.SubTask.Count;
                        //            String[] keywords = new String[] { @"||  || ||\\   ////",
                        //                @"For more details refer to HDC Blue Screen dump above." };
                        //            Parse(result, ref task, logFilesToLookIn[0], keywords);
                        //            if (task.SubTask.Count == counter)
                        //                Parse(result, ref task, logFilesToLookIn[1], keywords);

                        //            counter = task.SubTask.Count;
                        //            keywords = new String[] { "*********************** WARNING *************************",
                        //                "*********************************************************" };
                        //            Parse(result, ref task, logFilesToLookIn[0], keywords);
                        //            if (task.SubTask.Count == counter)
                        //                Parse(result, ref task, logFilesToLookIn[1], keywords);

                        //            if (task.SubTask.Count > 0)
                        //                Results.Add(task);
                        //        }
                        //    }
                        //}
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
