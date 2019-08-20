using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpirV___get_fail_reasons
{
    class GetCorruptedImagesProgram
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

                    ImagesFromDataAnalyzer analyzer = new ImagesFromDataAnalyzer(DataAnalyzer.Instance);
                    analyzer.Analyze();
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
        }
    }
}
