using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpirV___get_fail_reasons
{
    class ImagesFromDataAnalyzer
    {
        private DataAnalyzer DataAnalyzer { get; set; }
        public Dictionary<Image, String> Images { get; set; }
        public int ImagesCounter { get; private set; } = 0;
        private String ReferencePath
        {
            get { return Program.outputPath + @"\testSession_" + DataAnalyzer.JobSetSessionID + @"\references"; }
        }
        private String TestPath
        {
            get { return Program.outputPath + @"\testSession_" + DataAnalyzer.JobSetSessionID + @"\tests"; }
        }
        private bool FlushImages
        {
            get { return this.Images.Count > 50; }
        }

        public ImagesFromDataAnalyzer(DataAnalyzer dataAnalyzer)
        {
            this.DataAnalyzer = dataAnalyzer;
            this.Images = new Dictionary<Image, String>();
        }

        private void CreateInitCatalogs()
        {
            if (Program.Clear)
            {
                if (Directory.Exists(this.ReferencePath))
                    Directory.Delete(this.ReferencePath, true);
                if (Directory.Exists(this.TestPath))
                    Directory.Delete(this.TestPath, true);
            }

            System.IO.Directory.CreateDirectory(ReferencePath);
            System.IO.Directory.CreateDirectory(TestPath);
        }

        private void DumpImages()
        {
            Console.Out.WriteLine("Save in progess...");
            foreach (var image in this.Images)
            {
                if (image.Value.Contains("test}.png"))
                {
                    image.Key.Save(this.TestPath + @"\" + image.Value);
                }
                else if (image.Value.Contains("ref}.png"))
                {
                    image.Key.Save(this.ReferencePath + @"\" + image.Value);
                }
            }
        }

        private void SaveImages(bool forcedFlush = false)
        {
            if (!forcedFlush && this.FlushImages)
            {
                this.DumpImages();

                Console.Out.WriteLine($"Images flushed! Image counter: {ImagesCounter}");
                this.Images.Clear();
            }
            else if(forcedFlush)
            {
                this.DumpImages();

                Console.Out.WriteLine($"Forced images flush! Image counter: {ImagesCounter}");
                this.Images.Clear();
            }
        }

        public void Analyze()
        {
            String hyperlink = "";
            String jsonResult = "";
            this.CreateInitCatalogs();
            foreach (JobSetSessionNS.JobSetSession jobSetSession in DataAnalyzer.JobSetSession.JobSetSessions)
            {
                hyperlink = $"http://gtax-igk.intel.com/api/v1/jobsets/{jobSetSession.ID}/results?group_by=job&include_subtasks_passcounts=false&order_by=id&order_type=desc";
                if (jobSetSession.Status.ToLower() == "completed" && jobSetSession.BusinessAttributes.Planning.Attributes.Environment.ToLower() == "silicon")
                {
                    jsonResult = this.DataAnalyzer.webClient.DownloadString(hyperlink);
                    JobSet jobSet = JsonConvert.DeserializeObject<JobSet>(jsonResult);
                    jsonResult = "";

                    foreach (Job job in jobSet.Jobs)
                    {
                        foreach (Result result in job.Results)
                        {
                            if (result.PhaseName == "tests" && result.Status.ToLower() == "failed")
                            {
                                if (result.Artifacts != null)
                                {
                                    String testNumber = Regex.Split(result.GtaResultKey, @"\D+").Last();
                                    String testName = "";
                                    foreach (String taskLog in result.Artifacts.TaskLogs)
                                    {
                                        if (Regex.IsMatch(taskLog, @".*_ref.png") || Regex.IsMatch(taskLog, @".*_test.png"))
                                        {
                                            hyperlink = $"http://gtax-igk.intel.com/logs/jobs/jobs/0000/{result.JobID.Substring(0, 4)}/{result.JobID}/logs/tests/{testNumber}/{taskLog}";
                                            testName = result.BusinessAttributes.ItemName + "{" + hyperlink.Split('/').Last();
                                            testName = testName.Replace(".png", "}.png");

                                            try
                                            {
                                                using (Stream stream = DataAnalyzer.webClient.OpenRead(hyperlink))
                                                {
                                                    this.Images.Add(Image.FromStream(stream), testName);
                                                    ImagesCounter++;
                                                    Console.Out.WriteLine($"Found corrupted image: {testName}. Image no. {ImagesCounter}");
                                                    this.SaveImages();
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.Out.WriteLine(ex.Message + " " + hyperlink);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }                  
                }
            }
            this.SaveImages(true);
            Console.Out.WriteLine($"Found {ImagesCounter / 2} unique images.");
        }
    }
}
