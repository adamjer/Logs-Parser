using Newtonsoft.Json;
using SpirV___get_fail_reasons.GTAX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpirV___get_fail_reasons
{
    class JobOutputAnalyzer : Analyzer
    {
        public JobOutputAnalyzer() : base()
        {
        }

        public JobOutputAnalyzer(DataAnalyzer dataAnalyzer) : base(dataAnalyzer)
        {
        }

        string CombineMessage(IList<Information> message)
        {
            StringBuilder builder = new System.Text.StringBuilder();

            foreach (var information in message)
            {
                builder.AppendLine(information.Message);
            }

            return builder.ToString();
        }

        override public void Analyze()
        {
            String hyperlink = "";
            String jsonResult = "";
            int jobCounter = 0;
            foreach (JobSetSessionNS.JobSetSession jobSetSession in DataAnalyzer.JobSetSession.JobSetSessions)
            {
                hyperlink = GetJobsetSessionHyperlink(jobSetSession);

                jsonResult = this.DataAnalyzer.webClient.DownloadString(hyperlink);
                GTAX_Jobs jobs = JsonConvert.DeserializeObject<GTAX_Jobs>(jsonResult);
                jsonResult = "";


                try
                {


                    foreach (Job job in jobs.Jobs)
                    {
                        jobCounter++;
                        hyperlink = $"http://{Program.instance}/api/v1/jobs/{job.ID}/output";
                        var jobOutputResult = this.DataAnalyzer.webClient.DownloadString(hyperlink);
                        JobOutput jobOutput = JsonConvert.DeserializeObject<JobOutput>(jobOutputResult);

                        foreach (var run in jobOutput.Data.Runs)
                        {
                            for (int i = 0; i < run.Messages.Count; i++)
                            {
                                GtaxResult gtaxResult = new GtaxResult();
                                foreach (var info in run.Messages[i])
                                {
                                    if (Regex.IsMatch(info.Message, @"Maximum number of 3 GET connection errors to") ||
                                        Regex.IsMatch(info.Message, @"Machine is responsive to network ping") ||
                                        Regex.IsMatch(info.Message, @"Client issue: Unable to ping GTA-X Client"))
                                    {
                                        gtaxResult.Name = $"Message number: {i}";
                                        gtaxResult.Link = $"https://{Program.instance}/#/jobs/{job.Results[0].JobID}";
                                        gtaxResult.Status = "Error";
                                        gtaxResult.Machine = job.Results[0].ClientName;
                                        gtaxResult.ParsedResults.Add(CombineMessage(run.Messages[i]));
                                        break;
                                    }
                                }

                                try
                                {
                                    if (gtaxResult.ParsedResults.Count > 0)
                                    {
                                        Results.Add(gtaxResult);
                                        Console.Out.WriteLine($"Results count: {Results.Count} in {jobCounter} searched jobs!");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.Out.WriteLine(ex.Message);
                                }

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                }
            }
        }
    }
}
