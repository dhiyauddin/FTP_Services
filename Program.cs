using System.Threading;
using System.Threading.Tasks;
using Topshelf;
using Topshelf.Logging;
using Serilog;
using System.IO;

namespace FTPServices
{
    /*
    class Program
    {        
        static void Main(string[] args)
        {
            ThreadService _obj = new ThreadService();
            Thread _instance = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        _obj.serviceCheckingFile();

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    Thread.Sleep(2880000);

                }
            });

            _instance.Start();
        }

        
    }
    */

    //------------------
    public class Program
    {
        static int Main(string[] args)
        {
            ILogger configuration = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .WriteTo.RollingFile(@"D:\log\log.txt", retainedFileCountLimit: 7)
                .CreateLogger();

            StreamWriter writer =
                new StreamWriter(File.Open(@"D:\seriself\seriself.txt", FileMode.Append, FileAccess.Write,
                    FileShare.ReadWrite));

            Serilog.Debugging.SelfLog.Out = writer;

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    ThreadService _obj = new ThreadService();
                    _obj.serviceCheckingFile();
                    writer.Flush();
                    Thread.Sleep(60000);
                }
            });

            return (int)HostFactory.Run(x =>
            {
                x.UseSerilog(configuration);
                x.SetDescription("Service running for monitor,checking,upload and archieve file required.");
                x.SetDisplayName("FTPUploadService");
                x.SetServiceName("FTPUploadService");
                x.Service<FTPUploadService>(sc =>
                {
                    sc.ConstructUsing(() => new FTPUploadService());
                    sc.WhenStarted((s, hostControl) => s.Start(hostControl));
                    sc.WhenStopped((s, hostControl) => s.Stop(hostControl));
                });
                x.StartAutomatically();
            });
        }
    }

    public class FTPUploadService : ServiceControl
    {
        private static readonly LogWriter Log = HostLogger.Get<FTPUploadService>();

        public bool Start(HostControl hostControl)
        {
            Log.Info("FTPService is Starting...");
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            Log.Info("FTPService is Stopping...");
            return true;
        }
    }
}
