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
            FileMonitor fm = new FileMonitor();
            fm.Run();

            //Console.WriteLine(string.Format(@"{0:yyyyMMdd}", DateTime.Now));
            //Console.ReadKey();
        }
    }

    class FileMonitor
    {
        private DirectoryInfo root;
        private StringBuilder output;

        public FileMonitor()
        {
            output = new StringBuilder();

            string folderName = ConfigurationManager.AppSettings["folderPath"];
            root = new DirectoryInfo(folderName);

        }

        //Main method of class
        //others is private
        public void Run()
        {
            output.AppendLine(string.Format(@"{0:0.0} MB", root.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length) / 1000.0 / 1000.0));
            Scan();
            WriteToFile();
        }

        private void Scan()
        {
            string delimiter = ConfigurationManager.AppSettings["delimiter"];

            var dirs = from dir in root.EnumerateDirectories()
                       orderby dir.Name ascending
                       select new
                       {
                           DirInfo = dir,
                       };

            foreach (var dir in dirs)
            {
                output.AppendLine(delimiter);

                output.AppendLine(dir.DirInfo.Name);
                ScanAllFilesInFolder(dir.DirInfo);

                output.AppendLine(delimiter);
                output.AppendLine();
            }
        }

        //scan all files and sub-files in a folder
        private void ScanAllFilesInFolder(DirectoryInfo dir)
        {
            string tab = ConfigurationManager.AppSettings["tab"];
            var excludes = ConfigurationManager.AppSettings["excludes"].Split(';');

            var files = from file in dir.EnumerateFiles("*", SearchOption.AllDirectories)
                        orderby file.LastWriteTime descending
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
                string txt = string.Format(@"{0}\{1}\{2}", fi.Directory.Parent.Name, folderName, fi.Name);
                txt = string.Format(@"{0}{1}{2}{3}{4:00.00}", txt, tab, fi.LastWriteTime, tab, fi.Length/1000.0/1000.0);
                this.output.AppendLine(txt);
            }
        }

        private void WriteToFile()
        {
            string outputPath = ConfigurationManager.AppSettings["outputPath"];
            string outputName = ConfigurationManager.AppSettings["outputName"];
            string outputExtension = ConfigurationManager.AppSettings["outputExtension"];

            string outFile = string.Format(@"{0}{1}.{2:yyyyMMdd}.{3}", outputPath, outputName, DateTime.Now, outputExtension);

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
