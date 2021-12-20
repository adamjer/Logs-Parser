using Newtonsoft.Json;
using SpirV___get_fail_reasons.GTAX;
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
    class ImagesFromDataAnalyzer : Analyzer
    {
        public Dictionary<Image, IList<String>> Images { get; set; }
        public int ImagesCounter { get; private set; } = 0;

        public ImagesFromDataAnalyzer() : base()
        {
            this.Images = new Dictionary<Image, IList<String>>();
        }

        public ImagesFromDataAnalyzer(DataAnalyzer dataAnalyzer) : base(dataAnalyzer)
        {
            this.Images = new Dictionary<Image, IList<String>>();
        }

        private String ReferencePath
        {
            get { return Program.outputPath + @"\testSession_" + DataAnalyzer.JobSetSessionID + @"\references"; }
        }
        private String TestPath
        {
            get { return Program.outputPath + @"\testSession_" + DataAnalyzer.JobSetSessionID + @"\current"; }
        }
        private bool FlushImages
        {
            get { return this.Images.Count > 10; }
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
                var match = Regex.Match(image.Value[4], @".*f(?<frameNumber>\d+)\._(?<type>.+)_(?<id>\d+)\.png");
                var id = match.Groups["id"].Value;
                var type = match.Groups["type"].Value;
                var frameNumber = match.Groups["frameNumber"].Value;
                var frameName = $"frame{new String('0', 8 - frameNumber.Length)}{frameNumber}";
                var pathName = $@"\{image.Value[0]}\{image.Value[1]}\{image.Value[2]}\{image.Value[3]}";
                if (type == "result")
                {
                    pathName = this.TestPath + pathName;
                    if (!Directory.Exists(pathName))
                        Directory.CreateDirectory(pathName);
                    image.Key.Save($@"{pathName}\{frameName}.png");
                }
                //else if (type == "ref")
                //    pathName = this.ReferencePath + pathName;
                //image.Key.Save(pathName);
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
            else if (forcedFlush)
            {
                this.DumpImages();

                Console.Out.WriteLine($"Forced images flush! Image counter: {ImagesCounter}");
                this.Images.Clear();
            }
        }

        override public void Analyze()
        {
            String hyperlink = "";
            String jsonResult = "";
            this.CreateInitCatalogs();
            foreach (JobSetSessionNS.JobSetSession jobSetSession in DataAnalyzer.JobSetSession.JobSetSessions)
            {
                hyperlink = this.GetJobsetSessionHyperlink(jobSetSession);
                jsonResult = this.DataAnalyzer.webClient.DownloadString(hyperlink);
                GTAX_Jobs jobs = JsonConvert.DeserializeObject<GTAX_Jobs>(jsonResult);
                jsonResult = "";

                foreach (Job job in jobs.Jobs)
                {
                    foreach (Result result in job.Results)
                    {
                        if (result.PhaseName == "tests" && result.Command.ToLower().Contains("test_trace")) //result.Status.ToLower() == "failed"
                        {
                            if (result.Artifacts != null)
                            {
                                String testNumber = Regex.Split(result.GtaResultKey, @"\D+").Last();
                                foreach (String taskLog in result.Artifacts.TaskLogs)
                                {
                                    string framesType = @"(result)"; //@"(reference)|(result)"
                                    if (Regex.IsMatch(taskLog, $@".*f\d+\._{framesType}_\d+\.png"))
                                    {
                                        hyperlink = $"http://gtax-igk.intel.com/logs/jobs/jobs/0000/{result.JobID.Substring(0, 4)}/{result.JobID}/logs/tests/{testNumber}/{taskLog}";

                                        try
                                        {
                                            using (Stream stream = DataAnalyzer.webClient.OpenRead(hyperlink))
                                            {
                                                this.Images.Add(Image.FromStream(stream), new List<string>()
                                                { 
                                                    job.ID,
                                                    result.ID,
                                                    testNumber,
                                                    result.BusinessAttributes.ItemName,
                                                    taskLog 
                                                });
                                                ImagesCounter++;
                                                Console.Out.WriteLine($"Found corrupted image: {taskLog}. Image no. {ImagesCounter}");
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
            this.SaveImages(true);
            Console.Out.WriteLine($"Found {ImagesCounter / 2} unique images.");
        }
    }
}
