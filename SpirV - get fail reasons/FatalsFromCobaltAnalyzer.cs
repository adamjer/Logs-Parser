using Newtonsoft.Json;
using System;
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
        public List<SpirVTask> Results { get; set; }

        public FatalsFromCobaltAnalyzer(DataAnalyzer dataAnalyzer)
        {
            this.DataAnalyzer = dataAnalyzer;
            this.Results = new List<SpirVTask>();
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

        private void ParseFatals(Result result, ref SpirVTask task, String[] logFilesToLookIn)
        {
            String hyperlink, log;
            String testNumber = Regex.Split(result.GtaResultKey, @"\D+").Last();

            foreach (String taskLog in result.Artifacts.TaskLogs)
            {

                bool containsLogFiles = false;
                foreach (String logFile in logFilesToLookIn)
                {
                    if (logFile == taskLog)
                    {
                        containsLogFiles = true;
                        break;
                    }
                }

                if (containsLogFiles)
                {
                    hyperlink = $"http://gtax-presi-igk.intel.com/logs/jobs/jobs/0000/{result.JobID.Substring(0, 4)}/{result.JobID}/logs/tests/{testNumber}/{taskLog}";

                    try
                    {
                        log = this.DataAnalyzer.webClient.DownloadString(hyperlink);
                        Match firstHeader = null, secondHeader = null;
                        while (true)
                        {
                            if (firstHeader == null && secondHeader == null)
                            {
                                firstHeader = Regex.Match(log, @"\* FATAL \*");
                                secondHeader = Regex.Match(log.Substring(firstHeader.Index), @"\* END FATAL \*");
                            }
                            else
                            {
                                firstHeader = firstHeader.NextMatch();
                                if (!firstHeader.Success)
                                    break;
                                secondHeader = Regex.Match(log.Substring(firstHeader.Index), @"\* END FATAL \*");
                                if (!secondHeader.Success)
                                    break;
                            }

                            if (firstHeader.Index > 0 && secondHeader.Index > 0)
                            {
                                String fatal = ReadMatch(log, firstHeader, secondHeader);
                                if (fatal != String.Empty)
                                    task.SubTask.Add(@"***** **** *** ** " + fatal + @"* *** **** *****");
                            }
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        Console.Out.WriteLine(ex.Message + " " + hyperlink);
                    }
                    break;
                }
            }
        }

        private void Parse(Result result, ref SpirVTask task, String[] logFilesToLookIn, String[] keyWords, String[] matchers)
        {

        }

        public void Analyze()
        {
            String hyperlink = "";
            String jsonResult = "";
            foreach (JobSetSessionNS.JobSetSession jobSetSession in DataAnalyzer.JobSetSession.JobSetSessions)
            {
                hyperlink = GetJobsetHyperlink(jobSetSession);
                if (jobSetSession.Status.ToLower() == "completed" && jobSetSession.BusinessAttributes.Planning.Attributes.Environment.ToLower() == "simulation")
                {
                    jsonResult = this.DataAnalyzer.webClient.DownloadString(hyperlink);
                    JobSet jobSet = JsonConvert.DeserializeObject<JobSet>(jsonResult);
                    jsonResult = "";

                    foreach (Job job in jobSet.Jobs)
                    {
                        foreach (Result result in job.Results)
                        {
                            if (result.PhaseName == "tests" && result.Status.ToLower() == "failed")
                            {
                                if (result.Artifacts != null)
                                {
                                    SpirVTask task = new SpirVTask();
                                    task.Name = result.BusinessAttributes.ItemName;
                                    task.SetLink(result.JobID, result.ID);
                                    String[] logFilesToLookin = { "cobalt2.log", "cobalt2-tail.log" };

                                    ParseFatals(result, ref task, logFilesToLookin);

                                    //Parse

                                    if (task.SubTask.Count > 0)
                                        Results.Add(task);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
