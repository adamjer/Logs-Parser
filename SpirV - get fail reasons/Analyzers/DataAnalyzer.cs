using Newtonsoft.Json;
using SpirV___get_fail_reasons.GTAX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpirV___get_fail_reasons
{
    sealed class DataAnalyzer
    {
        public String JobsetID { get; set; }
        public String JobSetSessionID { get; set; }
        public WebClient webClient { get; set; }
        public SortedSet<Result> Results { get; set; }
        public String Name { get; set; }
        public JobSetSessionData JobSetSession { get; set; }
        private String Login { get; set; }
        private String Password { get; set; }

        private String ResultsLink
        {
            get
            {
                return $"http://{Program.instance}/api/v1/jobsets?full_info=true&jobset_session_ids={this.JobSetSessionID}&order_by=id&order_type=desc";
            }
        }

        private String JobSetLink
        {
            get
            {
                return $"http://{Program.instance}/api/v1/jobsets/{JobsetID}/results?group_by=job&include_subtasks_passcounts=false&order_by=id&order_type=desc";
            }
        }


        private static DataAnalyzer m_oInstance = null;
        private static readonly object m_oPadLock = new object();
        private String jsonResult;
        private GTAX_Jobs jobs;
        

        public static DataAnalyzer Instance
        {
            get
            {
                lock (m_oPadLock)
                {
                    if (m_oInstance == null)
                    {
                        m_oInstance = new DataAnalyzer();
                    }
                    return m_oInstance;
                }
            }
        }

        public void InitWebClient()
        {
            Console.Write("Enter user name: ");
            Login = Console.ReadLine();
            Console.Write("and password: ");
            Password = this.ReadPassword();

            this.webClient.Credentials = new NetworkCredential(Login, Password);
        }

        public String Download(String uri)
        {
            return this.webClient.DownloadString(uri);
        }

        public String DownloadAsync(String uri)
        {
            WebClient temp = new WebClient();
            temp.Credentials = new NetworkCredential(Login, Password);
            using (temp)
            {
                var result = temp.DownloadString(uri);
                return result;
            }
        }

        public void GetJobSetData()
        {
            //if Program.executionType == 
            String resultsLink = JobSetLink;
            this.jsonResult = this.webClient.DownloadString(resultsLink);

            this.jobs = JsonConvert.DeserializeObject<GTAX_Jobs>(this.jsonResult);
            this.Name = Regex.Match(this.jobs.Jobs[0].Name, @"gfx-driver-ci-master-\d+").Value;
            this.OrderJobSet();
            this.jsonResult = "";
        }

        public void GetJobSetSessionData()
        {
            this.jsonResult = this.webClient.DownloadString(ResultsLink);

            this.JobSetSession = JsonConvert.DeserializeObject<JobSetSessionData>(this.jsonResult);
            this.Name = JobSetSessionID;
            this.jsonResult = "";
        }

        private void OrderJobSet()
        {
            this.Results = new SortedSet<Result>();
            foreach (Job job in this.jobs.Jobs)
            {
                foreach (Result result in job.Results)
                {
                    if (result.PhaseName == "tests" && (result.Status == "passed" || result.Status == "failed"))
                    {
                        this.Results.Add(result);
                    }
                }
            }
            this.jobs = null;
        }

        public string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key != ConsoleKey.Backspace)
                {
                    Console.Write("*");
                    password += info.KeyChar;
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        // remove one character from the list of password characters
                        password = password.Substring(0, password.Length - 1);
                        // get the location of the cursor
                        int pos = Console.CursorLeft;
                        // move the cursor to the left by one character
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                        // replace it with space
                        Console.Write(" ");
                        // move the cursor to the left by one character again
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    }
                }
                info = Console.ReadKey(true);
            }

            // add a new line because user pressed enter at the end of their password
            Console.WriteLine();
            return password;
        }
    }
}
