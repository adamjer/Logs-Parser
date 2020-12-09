using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpirV___get_fail_reasons
{
    class GetFatalsFromCobaltProgram
    {
        public static void Main()
        {
            Console.Out.Write("Enter jobset session ID: ");
            DataAnalyzer.Instance.JobSetSessionID = Console.In.ReadLine();

            try
            {
                using (DataAnalyzer.Instance.webClient = new WebClient())
                {
                    DataAnalyzer.Instance.InitWebClient();
                    DataAnalyzer.Instance.GetJobSetSessionData();

                    //LogsFromDataAnalyzer analyzer = new LogsFromDataAnalyzer(DataAnalyzer.Instance);
                    FatalsFromCobaltAnalyzer analyzer = new FatalsFromCobaltAnalyzer(DataAnalyzer.Instance);
                    analyzer.Analyze();

                    ExcelDataWriter excelDataWriter = new ExcelDataWriter(analyzer.Results, DataAnalyzer.Instance);
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
