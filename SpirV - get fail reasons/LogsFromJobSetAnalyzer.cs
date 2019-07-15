using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpirV___get_fail_reasons
{
    class LogsFromJobSetAnalyzer
    {
        private JobSetAnalyzer JobSetAnalyzer { get; set; }
        public List<SpirVTask> Results { get; set; }

        public LogsFromJobSetAnalyzer(JobSetAnalyzer jobSetAnalyzer)
        {
            this.JobSetAnalyzer = jobSetAnalyzer;
            this.Results = new List<SpirVTask>();
        }

        public void Analyze()
        {
            String testNumber, hyperlink, log;
            foreach (Result result in this.JobSetAnalyzer.Results)
            {
                if (result.Artifacts != null)
                {
                    testNumber = Regex.Split(result.GtaResultKey, @"\D+").Last();
                    SpirVTask task = new SpirVTask();
                    foreach (String taskLog in result.Artifacts.TaskLogs)
                    {
                        if (Regex.IsMatch(taskLog, $"{result.BusinessAttributes.ItemName}.*.log"))
                        {
                            hyperlink = $"http://gtax-igk.intel.com/logs/jobs/jobs/0000/{result.JobID.Substring(0, 4)}/{result.JobID}/logs/tests/{testNumber}/{taskLog}";
                            task.Name = result.BusinessAttributes.ItemName;
                            task.SetLink(result.JobID, result.ID);

                            try
                            {
                                log = this.JobSetAnalyzer.webClient.DownloadString(hyperlink);
                                Match firstHeader = null, secondHeader = null;
                                while (true)
                                {
                                    if (firstHeader == null && secondHeader == null)
                                    {
                                        firstHeader = Regex.Match(log, @"--> [0-9.]+ -.*subcase:");
                                        secondHeader = Regex.Match(log, @"--< [0-9.]+ -.*subcase (passed|failed|not run)");
                                    }
                                    else
                                    {
                                        firstHeader = firstHeader.NextMatch();
                                        secondHeader = secondHeader.NextMatch();

                                        if (!firstHeader.Success || !secondHeader.Success)
                                            break;
                                    }

                                    //if (secondHeader.Value.Contains("failed"))
                                    task.SubTask.Add(ReadSubTask(log, firstHeader, secondHeader));
                                }
                                Results.Add(task);
                            }
                            catch (Exception ex)
                            {
                                Console.Out.WriteLine(ex.Message + " " + hyperlink);
                            }
                            break;
                        }
                    }
                }
            }
        }

        private String ReadSubTask(String log, Match firstHeader, Match secondHeader)
        {
            String line;
            StringReader reader = new StringReader(log.Substring(firstHeader.Index, secondHeader.Index + secondHeader.Length - firstHeader.Index));
            StringBuilder builder = new StringBuilder();

            while ((line = reader.ReadLine()) != secondHeader.Value)
            {
                if (line == null)
                    break;
                if (!builder.ToString().Contains(line))
                    builder.AppendLine(line);
            }
            builder.AppendLine(line);

            return builder.ToString();
        }
    }
}
