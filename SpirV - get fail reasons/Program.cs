using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpirV___get_fail_reasons
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Out.Write("Enter jobset ID: ");
            JobSetAnalyzer.Instance.JobsetID = Console.In.ReadLine();

            try
            {
                using (JobSetAnalyzer.Instance.webClient = new WebClient())
                {
                    JobSetAnalyzer.Instance.InitWebClient();
                    JobSetAnalyzer.Instance.GetJobSetData();

                    LogsFromJobSetAnalyzer analyzer = new LogsFromJobSetAnalyzer(JobSetAnalyzer.Instance.Results, JobSetAnalyzer.Instance.webClient);
                    analyzer.Analyze();
                    ExcelDataWriter excelDataWriter = new ExcelDataWriter(analyzer.Results, JobSetAnalyzer.Instance.JobsetID);
                    excelDataWriter.Write();
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
        }
    }
}
