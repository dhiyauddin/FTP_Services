using System;
using FluentFTP;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Net;
using System.IO.Compression;
using Topshelf.Logging;

namespace FTPServices
{
    public class ThreadService
    {
        private static readonly LogWriter Log = HostLogger.Get<ThreadService>();
        public void serviceCheckingFile()
        {
            string server = ConfigurationManager.AppSettings["server"];
            string userName = ConfigurationManager.AppSettings["userName"];
            string password = ConfigurationManager.AppSettings["password"];
            string fileName = ConfigurationManager.AppSettings["fileName"];
            string fullName = ConfigurationManager.AppSettings["fullName"];
            string destinationName = ConfigurationManager.AppSettings["destinationName"];
            string sourceName = ConfigurationManager.AppSettings["sourceName"];


            FtpClient client = new FtpClient(server);
            client.Credentials = new NetworkCredential(userName, password);
            client.Connect();
            //client.UploadFile(@fullName, destinationName + fileName);

            Log.Info("--------------------------------");
            Log.Info("FILE FROM SERVER:");
            // get a list of files and directories in the "/ftp" folder
            foreach (FtpListItem item in client.GetListing(destinationName))
            {

                // if this is a file
                if (item.Type == FtpFileSystemObjectType.File)
                {
                    Log.Info(item);


                    // get the file size
                    long size = client.GetFileSize(item.FullName);

                    // calculate a hash for the file on the server side (default algorithm)
                    FtpHash hash = client.GetChecksum(item.FullName);

                }

                // get modified date/time of the file or folder
                DateTime time = client.GetModifiedTime(item.FullName);
            }

            string[] filePaths = Directory.GetFiles(@sourceName);

            Log.Info("--------------------------------");
            Log.Info("FILE FROM LOCAL: (Matched with Server)");
            int count = 1;
            foreach (String item in filePaths)
            {
                string localFile = item.Replace(sourceName, "");

                foreach (FtpListItem itemA in client.GetListing(destinationName))
                {
                    if (itemA.Type == FtpFileSystemObjectType.File)
                    {
                        if (localFile.Equals(itemA.Name))
                        {
                            Log.Info(count + ")" + itemA.Name);
                            client.UploadFile(sourceName + localFile, destinationName + localFile);
                            count++;
                        }
                    }
                }                
            }
            compressfile(sourceName);
            client.Disconnect();
        }

        void compressfile(string sourceName)
        {
            string today = DateTime.Now.ToString("yyyy_MM_dd");
            string startPath = @sourceName;
            string zipPath = @"C://Users//mdhiy//Downloads//archive_" + today + ".zip";
            try
            {
                ZipFile.CreateFromDirectory(startPath, zipPath);
            }
            catch (Exception e) 
            {
                if(e!=null)
                    Log.Info(e.InnerException);
            }

            decompressfile(today);

        }
        void decompressfile(String today)
        {
            string zipPath = @"C://Users//mdhiy//Downloads//archive_" + today + ".zip";
            string extractPath = @"C://Users//mdhiy//Downloads//extract";
            try
            {
                ZipFile.ExtractToDirectory(zipPath, extractPath);
            }
            catch (Exception e) 
            {
                if (e != null)
                    Log.Info(e.InnerException);
            }
}
    }
}
