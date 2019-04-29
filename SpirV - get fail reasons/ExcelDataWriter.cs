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
        private String TestSession { get; set; }

        private Application excel;
        private Workbook workBook;
        private List<Worksheet> workSheets;

        public ExcelDataWriter(List<SpirVTask> r, String ts)
        {
            Results = r;
            TestSession = ts;
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

                for(int i = 0; i < Results.Count; i++)
                {
                    workSheets[0].Cells[i + 1, 1] = Results[i].Name;
                    if (Results[i].FailedSubTask.Count == 0)
                        workSheets[0].Rows[i + 1].Interior.Color = Color.Green;

                    for (int j = 0; j < Results[i].FailedSubTask.Count; j++)
                    {
                        if (Results[i].FailedSubTask[j].Length > 5000)
                            workSheets[0].Cells[i + 1 , j + 2] = Results[i].FailedSubTask[j].Substring(0, 5000);
                        else
                            workSheets[0].Cells[i + 1, j + 2] = Results[i].FailedSubTask[j];
                    }
                }               
            }
            catch(Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                workBook.SaveAs(TestSession + ".xlsx");
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
