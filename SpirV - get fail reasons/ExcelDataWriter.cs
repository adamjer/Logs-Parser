using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.Office.Interop.Excel;

namespace SpirV___get_fail_reasons
{
    class ExcelDataWriter
    {
        private List<SpirVTask> Results { get; set; }
        private JobSetAnalyzer JobSetAnalyzer { get; set; }

        private Application excel;
        private Workbook workBook;
        private List<Worksheet> workSheets;

        public ExcelDataWriter(List<SpirVTask> r, JobSetAnalyzer jsa)
        {
            Results = r;
            JobSetAnalyzer = jsa;
        }

        public void Write()
        {
            int max = MaxFails();
            try
            {
                excel = new Application();
                workBook = null;
                workSheets = new List<Worksheet>();

                excel.Visible = true;
                workBook = excel.Workbooks.Add();
                workSheets.Add(workBook.ActiveSheet);

                workSheets[0].Name = JobSetAnalyzer.Name;
                workSheets[0].Cells[1, 1] = "TestName";
                workSheets[0].Cells[1, 2] = "Status";
                workSheets[0].Cells[1, 3] = "Summary";
                for (int i = 1; i <= max; ++i)
                    workSheets[0].Cells[1, i + 3] = "Subcase" + i;

                int column, row;
                for(int i = 0; i < Results.Count; i++)
                {
                    row = i + 2;
                    workSheets[0].Cells[row, 1] = Results[i].Name;
                    if(Results[i].FailedSubTask.Count > 0)
                    {
                        workSheets[0].Cells[row, 2] = "Failed";
                        workSheets[0].Cells[row, 2].Interior.Color = Color.Red;
                    }
                    else
                    {
                        workSheets[0].Cells[row, 2] = "Passed";
                        workSheets[0].Cells[row, 2].Interior.Color = Color.Green;
                    }

                    for (int j = 0; j < Results[i].FailedSubTask.Count; j++)
                    {
                        column = j + 4;
                        if (Results[i].FailedSubTask[j].Length > 5000)
                            workSheets[0].Cells[row , column] = Results[i].FailedSubTask[j].Substring(0, 5000);
                        else
                            workSheets[0].Cells[row, column] = Results[i].FailedSubTask[j];
                    }
                }               
            }
            catch(Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                workBook.SaveAs(JobSetAnalyzer.JobsetID + ".xlsx");
                workBook.Close();
                excel.Quit();
            }
        }

        private int MaxFails()
        {
            int result = 0;
            foreach (var r in Results)
            {
                if (r.FailedSubTask.Count > result)
                    result = r.FailedSubTask.Count;
            }
            return result;
        }
    }
}
