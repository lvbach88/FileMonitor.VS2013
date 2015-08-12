using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                FileMonitor fm = new FileMonitor();
                fm.Run();
                System.Diagnostics.Process.Start(fm.OutFile);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Press any key to continue....");
                Console.ReadKey();
            }

        }
    }

    class FileMonitor
    {
        private DirectoryInfo root;
        private StringBuilder output;
        private string outFile;

        public string OutFile
        {
            get
            {
                return outFile;
            }

            private set
            {
                outFile = value;
            }
        }

        public FileMonitor()
        {
            output = new StringBuilder();

            string folderName = ConfigurationManager.AppSettings["folderPath"];
            root = new DirectoryInfo(folderName);

            string outputPath = ConfigurationManager.AppSettings["outputPath"];
            string outputName = ConfigurationManager.AppSettings["outputName"];
            string outputExtension = ConfigurationManager.AppSettings["outputExtension"];

            this.outFile = string.Format(@"{0}{1}.{2:yyyyMMdd}.{3}", outputPath, outputName, DateTime.Now, outputExtension);
        }

        //Main method of class
        //others is private
        public void Run()
        {
            output.AppendLine(string.Format(@"Total size: {0:0.0} MB", root.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length) / 1000.0 / 1000.0));
            Scan();
            WriteToFile();
        }

        private void Scan()
        {
            string delimiter = ConfigurationManager.AppSettings["delimiter"];
            string teachers = ConfigurationManager.AppSettings["teachers"];
            var dirs = from dir in root.EnumerateDirectories()
                       orderby dir.Name ascending
                       select new
                       {
                           DirInfo = dir,
                       };

            foreach (var dir in dirs)
            {
                if (teachers.ToLower().Equals("all")
                    || teachers.Split(';').Contains(dir.DirInfo.Name))
                {
                    //output.AppendLine(delimiter);
                    output.AppendLine(dir.DirInfo.Name);
                    ScanAllFilesInFolder(dir.DirInfo);

                    output.AppendLine(delimiter);
                    output.AppendLine();
                }
                
            }
        }

        //scan all files and sub-files in a folder
        private void ScanAllFilesInFolder(DirectoryInfo dir)
        {
            string tab = ConfigurationManager.AppSettings["tab"];
            var excludes = ConfigurationManager.AppSettings["excludes"].Split(';');

            var files = from file in dir.EnumerateFiles("*", SearchOption.AllDirectories)
                        orderby file.Directory.Name descending, file.LastWriteTime descending
                        select new 
                        {
                            FileInformation = file,
                        };

            foreach (var file in files)
            {
                FileInfo fi = file.FileInformation;

                if (excludes.Contains(fi.Name))
                {
                    continue;
                }

                string folderName = fi.Directory.Name;
                //string txt = string.Format(@"{0}\{1}\{2}", fi.Directory.Parent.Name, folderName, fi.Name);
                string txt = string.Format(@"{0}", fi.FullName);
                txt = string.Format(@"{0}{1}{2:yyyyMMdd}{3}{4:000.000}KB", txt, tab, fi.LastWriteTime, tab, fi.Length / 1000.0);
                this.output.AppendLine(txt);
            }
        }

        private void WriteToFile()
        {
            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }

            using (StreamWriter outfile = new StreamWriter(outFile))
            {
                outfile.Write(output.ToString());
            }
        }

    }
}
