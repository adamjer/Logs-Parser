using Newtonsoft.Json;
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
    class GitsPlayerLogsAnalyzer
    {
        private DataAnalyzer DataAnalyzer { get; set; }
        public ConcurrentBag<SpirVTask> Results { get; set; }

        public GitsPlayerLogsAnalyzer(DataAnalyzer dataAnalyzer)
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

        private List<String> Parse(Result result, String log, String[] keyWords, String taskLog)
        {
            List<String> parsedLogs = new List<String>();
            try
            {
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

                    if (firstHeader.Index > 0 && (secondHeader.Index + secondHeader.Length) > 0)
                    {
                        String parsedLog = "File: " + taskLog + Environment.NewLine + log.Substring(firstHeader.Index, secondHeader.Index + secondHeader.Length);
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

        public void Analyze()
        {
            String hyperlink = "";
            String jsonResult = "";
            int counter = 0;
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

                                    var logNames = result.Artifacts.TaskLogs.Where(l => l.ToLower().Contains("gits_player_output"));

                                    foreach (string logName in logNames)
                                    {
                                        Interlocked.Increment(ref counter);
                                        String[] keywords = {"", ""};
                                        try
                                        {
                                            downloadHyperLink = $"http://gtax-presi-igk.intel.com/logs/jobs/jobs/0000/{result.JobID.Substring(0, 4)}/{result.JobID}/logs/tests/{testNumber}/{logName}";
                                            log = this.DataAnalyzer.DownloadAsync(downloadHyperLink);
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.Out.WriteLine("Download log: " + ex.Message);
                                            Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                        }

                                        try
                                        {
                                            keywords = new String[] { @"Warn: The", @"GITS internal format." };
                                            parsedLogs = Parse(result, log, keywords, "gits_player_output.log");
                                            task.SubTask.AddRange(parsedLogs);
                                        }
                                        catch(Exception ex)
                                        {
                                            Console.Out.WriteLine("Download log: " + ex.Message);
                                            Console.Out.WriteLine("\tHyperlink: " + downloadHyperLink);
                                        }

                                        try
                                        {
                                            keywords = new String[] { @"Err:", "\n" };
                                            parsedLogs = Parse(result, log, keywords, "gits_player_output.log");
                                            task.SubTask.AddRange(parsedLogs);
                                        }
                                        catch (Exception ex)
                                        {
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
