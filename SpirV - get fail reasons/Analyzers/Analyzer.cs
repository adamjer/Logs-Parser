using SpirV___get_fail_reasons.GTAX;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpirV___get_fail_reasons
{
    abstract class Analyzer
    {
        protected DataAnalyzer DataAnalyzer { get; set; }
        public ConcurrentBag<GtaxResult> Results { get; set; }

        protected Analyzer()
        {
            this.DataAnalyzer = null;
            this.Results = new ConcurrentBag<GtaxResult>();
        }

        protected Analyzer(DataAnalyzer dataAnalyzer)
        {
            this.DataAnalyzer = dataAnalyzer;
            this.Results = new ConcurrentBag<GtaxResult>();
        }

        protected String GetJobsetSessionHyperlink(JobSetSessionNS.JobSetSession jobSetSession)
        {
            return $"http://{Program.instance}/api/v1/jobsets/{jobSetSession.ID}/results?group_by=job&include_subtasks_passcounts=false&order_by=id&order_type=desc";
        }

        protected String GetTestResultsHyperlink(String jobID, String testNumber, String logName)
        {
            return $"http://{Program.instance}/logs/jobs/jobs/0000/{jobID.Substring(0, 4)}/{jobID}/logs/tests/{testNumber}/{logName}";
        }

        protected String ReadMatch(String log, Match firstHeader, Match secondHeader)
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

        protected List<String> Parse(Result result, String log, String[] keyWords, String taskLog)
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

        public void Init()
        {
            try
            {
                Console.Out.Write("Enter jobset session ID: ");
                DataAnalyzer.Instance.JobSetSessionID = Console.In.ReadLine();

                using (DataAnalyzer.Instance.webClient = new WebClient())
                {
                    DataAnalyzer.Instance.InitWebClient();
                    DataAnalyzer.Instance.GetJobSetSessionData();

                    this.DataAnalyzer = DataAnalyzer.Instance;
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }


        }

        virtual public void Analyze()
        {
            throw new NotImplementedException();
        }
    }
}
