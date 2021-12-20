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
        private List<GtaxResult> Results { get; set; }
        private DataAnalyzer DataAnalyzer { get; set; }

        private Application excel;
        private Workbook workBook;
        private List<Worksheet> workSheets;

        public ExcelDataWriter(List<GtaxResult> r, DataAnalyzer jsa)
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
            if (Program.excelSaveType == ExcelDataSaveType.Basic)
                this.WriteBasic();
            else if (Program.excelSaveType == ExcelDataSaveType.Advanced)
                this.WriteAdvanced();
            else if (Program.excelSaveType == ExcelDataSaveType.Fatal)
                this.WriteFatal();
        }

        private void WriteBasic()
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
                    workSheets[0].Cells[row, 3] = Results[i].ParsedResults.Count;
                    workSheets[0].Hyperlinks.Add(workSheets[0].Cells[row, 5], Results[i].Link, Type.Missing, "GTA-x", "GTA-X");

                    int passed = 0;
                    for (int j = 0; j < Results[i].ParsedResults.Count; j++)
                    {
                        if (Results[i].ParsedResults[j].Contains("subcase passed"))
                            passed++;
                    }
                    workSheets[0].Cells[row, 4] = (double)passed / Results[i].ParsedResults.Count * 100 + " %";

                    if (passed == Results[i].ParsedResults.Count)
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
                workBook.SaveAs(DataAnalyzer.Name + ".xlsx");
                workBook.Close();
                excel.Quit();
            }
        }

        private void WriteAdvanced()
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
                    for (int j = 0; j < Results[i].ParsedResults.Count; j++)
                    {
                        workSheets[0].Cells[row + j, 1] = Results[i].Name;
                        workSheets[0].Hyperlinks.Add(workSheets[0].Cells[row + j, 5], Results[i].Link, Type.Missing, Results[i].Link, "GTA-X");
                        if (Results[i].ParsedResults[j].Contains("passed"))
                        {
                            workSheets[0].Cells[row + j, 3] = "Passed";
                            workSheets[0].Cells[row + j, 3].Interior.Color = Color.Green;
                        }
                        else if (Results[i].ParsedResults[j].Contains("failed"))
                        {
                            workSheets[0].Cells[row + j, 3] = "Failed";
                            isFailed = true;
                            workSheets[0].Cells[row + j, 3].Interior.Color = Color.Red;
                        }

                        workSheets[0].Cells[row + j, 2] = Regex.Match(Results[i].ParsedResults[j], @"(?<=--> )(.*)(?=subcase:)").Value;

                        if (Results[i].ParsedResults[j].Length > 5000)
                            Results[i].ParsedResults[j] = Results[i].ParsedResults[j].Substring(0, 5000);

                        workSheets[0].Cells[row + j, 4] = Results[i].ParsedResults[j];
                    }

                    subtasks += Results[i].ParsedResults.Count;

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
                workBook.SaveAs(DataAnalyzer.Name + "_advanced.xlsx");
                workBook.Close();
                excel.Quit();
            }
        }

        private void WriteFatal()
        {
            try
            {
                excel = new Application();
                workBook = null;
                workSheets = new List<Worksheet>();

                excel.Visible = true;
                workBook = excel.Workbooks.Add();
                workSheets.Add(workBook.ActiveSheet);

                workSheets[0].Name = DataAnalyzer.Name;
                const int testNameColumn = 1, statusColumn = 2, machineColumn = 3, gtaxLinkColumn = 4;
                workSheets[0].Cells[1, testNameColumn] = "Test Name";
                workSheets[0].Cells[1, statusColumn] = "Status";
                workSheets[0].Cells[1, machineColumn] = "Machine";
                workSheets[0].Cells[1, gtaxLinkColumn] = "GTA-X Link";

                int row;
                for (int i = 0; i < Results.Count; i++)
                {
                    row = i + 2;
                    workSheets[0].Cells[row, testNameColumn] = Results[i].Name;
                    workSheets[0].Cells[row, statusColumn] = Results[i].Status;
                    workSheets[0].Cells[row, machineColumn] = Results[i].Machine;
                    workSheets[0].Hyperlinks.Add(workSheets[0].Cells[row, gtaxLinkColumn], Results[i].Link, Type.Missing, Results[i].Link, Results[i].Link);

                    for(int j = 0; j < Results[i].ParsedResults.Count; j++)
                    {
                        workSheets[0].Cells[row, gtaxLinkColumn + j + 1] = Results[i].ParsedResults[j];
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
                excel.Quit();
            }
            finally
            {
                workBook.SaveAs(DataAnalyzer.Name + "_fatal.xlsx");
                workBook.Close();
                excel.Quit();
            }
        }

        private int MaxFails()
        {
            int result = 0;
            foreach (var r in Results)
            {
                if (r.ParsedResults.Count > result)
                    result = r.ParsedResults.Count;
            }
            return result;
        }
    }
}
