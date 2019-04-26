using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpirV___get_fail_reasons
{
    sealed class JobSetAnalyzer
    {
        public String JobsetID { get; set; }
        public WebClient webClient { get; set; }
        public SortedSet<Result> Results { get; set; }

        private static JobSetAnalyzer m_oInstance = null;
        private static readonly object m_oPadLock = new object();
        private int m_nCounter = 0;
        private String jsonResult;
        private JobSet jobSet;
        

        public static JobSetAnalyzer Instance
        {
            get
            {
                lock (m_oPadLock)
                {
                    if (m_oInstance == null)
                    {
                        m_oInstance = new JobSetAnalyzer();
                    }
                    return m_oInstance;
                }
            }
        }

        public void InitWebClient()
        {
            Console.Write("Enter user name: ");
            String user = Console.ReadLine();
            Console.Write("and password: ");
            String password = this.ReadPassword();

            this.webClient.Credentials = new NetworkCredential(user, password);
        }

        public void GetJobSetData()
        {
            String resultsLink = $"http://gtax-igk.intel.com/api/v1/jobsets/{JobsetID}/results?group_by=job&include_subtasks_passcounts=false&order_by=id&order_type=desc";
            this.jsonResult = this.webClient.DownloadString(resultsLink);

            this.jobSet = JsonConvert.DeserializeObject<JobSet>(this.jsonResult);
            this.OrderJobSet();
        }

        private void OrderJobSet()
        {
            this.Results = new SortedSet<Result>();
            foreach (Job job in this.jobSet.Jobs)
            {
                foreach (Result result in job.Results)
                {
                    if (result.PhaseName == "tests" && (result.Status == "passed" || result.Status == "failed"))
                    {
                        this.Results.Add(result);
                    }
                }
            }
            this.jobSet = null;
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

        private JobSetAnalyzer()
        {
            m_nCounter = 1;
        }
    }
}
