using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.Office.Interop.Excel;
using System.Text.RegularExpressions;

namespace SpirV___get_fail_reasons
{
    class ExcelDataWriter
    {
        private List<SpirVTask> Results { get; set; }
        private DataAnalyzer DataAnalyzer { get; set; }

        private Application excel;
        private Workbook workBook;
        private List<Worksheet> workSheets;

        public ExcelDataWriter(List<SpirVTask> r, DataAnalyzer jsa)
        {
            Results = r;
            DataAnalyzer = jsa;
        }

        /*public void Write()
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

                workSheets[0].Name = DataAnalyzer.Name;
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
                    if(Results[i].SubTask.Count > 0)
                    {
                        workSheets[0].Cells[row, 2] = "Failed";
                        workSheets[0].Cells[row, 2].Interior.Color = Color.Red;
                    }
                    else
                    {
                        workSheets[0].Cells[row, 2] = "Passed";
                        workSheets[0].Cells[row, 2].Interior.Color = Color.Green;
                    }

                    for (int j = 0; j < Results[i].SubTask.Count; j++)
                    {
                        column = j + 4;
                        if (Results[i].SubTask[j].Length > 5000)
                            workSheets[0].Cells[row , column] = Results[i].SubTask[j].Substring(0, 5000);
                        else
                            workSheets[0].Cells[row, column] = Results[i].SubTask[j];
                    }
                }               
            }
            catch(Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                workBook.SaveAs(DataAnalyzer.JobsetID + ".xlsx");
                workBook.Close();
                excel.Quit();
            }*/

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

                workSheets[0].Name = DataAnalyzer.Name;
                workSheets[0].Cells[1, 1] = "TestName";
                workSheets[0].Cells[1, 2] = "Status";
                workSheets[0].Cells[1, 3] = "Subtasks Count";
                workSheets[0].Cells[1, 4] = "Pass Ratio";
                workSheets[0].Cells[1, 5] = "GTA-x Link";

                int row;
                for (int i = 0; i < Results.Count; i++)
                {
                    row = i + 2;
                    workSheets[0].Cells[row, 1] = Results[i].Name;
                    workSheets[0].Cells[row, 3] = Results[i].SubTask.Count;
                    workSheets[0].Hyperlinks.Add(workSheets[0].Cells[row, 5], Results[i].GTAXlink, Type.Missing, "GTA-x", "GTA-X");

                    int passed = 0;
                    for (int j = 0; j < Results[i].SubTask.Count; j++)
                    {
                        if (Results[i].SubTask[j].Contains("subcase passed"))
                            passed++;
                    }
                    workSheets[0].Cells[row, 4] = (double)passed / Results[i].SubTask.Count * 100 + " %";

                    if (passed == Results[i].SubTask.Count)
                    {
                        workSheets[0].Cells[row, 2] = "Passed";
                        workSheets[0].Cells[row, 2].Interior.Color = Color.Green;                      
                    }
                    else
                    {
                        workSheets[0].Cells[row, 2] = "Failed";
                        workSheets[0].Cells[row, 2].Interior.Color = Color.Red;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                workBook.SaveAs(DataAnalyzer.JobsetID + ".xlsx");
                workBook.Close();
                excel.Quit();
            }
        }

        public void WriteAdvanced()
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

                workSheets[0].Name = DataAnalyzer.Name;
                workSheets[0].Cells[1, 1] = "Test Name";
                workSheets[0].Cells[1, 2] = "Subcase";
                workSheets[0].Cells[1, 3] = "Status";
                workSheets[0].Cells[1, 5] = "GTA-x Link";
                workSheets[0].Cells[1, 4] = "Summary";
                //for (int i = 1; i <= max; ++i)
                //    workSheets[0].Cells[1, i + 3] = "Subcase" + i;

                int row;
                int subtasks = 0;
                for (int i = 0; i < Results.Count; i++)
                {
                    row = i + 2 + subtasks;
                    workSheets[0].Cells[row, 1] = Results[i].Name;
                    row++;

                    bool isFailed = false;
                    for (int j = 0; j < Results[i].SubTask.Count; j++)
                    {
                        workSheets[0].Cells[row + j, 1] = Results[i].Name;
                        workSheets[0].Hyperlinks.Add(workSheets[0].Cells[row + j, 5], Results[i].GTAXlink, Type.Missing, "GTA-x", "GTA-X");
                        if (Results[i].SubTask[j].Contains("passed"))
                        {
                            workSheets[0].Cells[row + j, 3] = "Passed";
                            workSheets[0].Cells[row + j, 3].Interior.Color = Color.Green;
                        }
                        else if (Results[i].SubTask[j].Contains("failed"))
                        {
                            workSheets[0].Cells[row + j, 3] = "Failed";
                            isFailed = true;
                            workSheets[0].Cells[row + j, 3].Interior.Color = Color.Red;
                        }

                        workSheets[0].Cells[row + j, 2] = Regex.Match(Results[i].SubTask[j], @"(?<=--> )(.*)(?=subcase:)").Value;

                        if (Results[i].SubTask[j].Length > 5000)
                            Results[i].SubTask[j] = Results[i].SubTask[j].Substring(0, 5000);

                        workSheets[0].Cells[row + j, 4] = Results[i].SubTask[j];
                    }

                    subtasks += Results[i].SubTask.Count;

                    if (isFailed)
                        workSheets[0].Cells[row - 1, 1].Interior.Color = Color.IndianRed;
                    else
                        workSheets[0].Cells[row - 1, 1].Interior.Color = Color.LightGreen;
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                workBook.SaveAs(DataAnalyzer.JobsetID + ".xlsx");
                workBook.Close();
                excel.Quit();
            }
        }

        private int MaxFails()
        {
            int result = 0;
            foreach (var r in Results)
            {
                if (r.SubTask.Count > result)
                    result = r.SubTask.Count;
            }
            return result;
        }
    }
}
