using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons;
using Giselle.DCConDownloader.Utils;

namespace Giselle.DCConDownloader
{
    public class DCConDownloader : IDisposable
    {
        public static DCConDownloader Instance { get; private set; }

        [STAThread]
        public static void Main()
        {
            var instance = Instance = new DCConDownloader();
            instance.Run();
        }

        public DCConCrawler DCConCrawler { get; }
        public MainForm MainForm { get; private set; }

        private Timer UiTimer { get; }

        public event EventHandler UiTick;

        private DCConDownloader()
        {
            this.DCConCrawler = new DCConCrawler();

            var timer = this.UiTimer = new Timer();
            timer.Interval = 20;
            timer.Tick += this.OnUiTimerTick;
        }

        private void OnUiTimerTick(object sender, EventArgs e)
        {
            this.UiTick?.Invoke(this, e);
        }

        private void Run()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                this.UiTimer.Start();

                var form = this.MainForm = new MainForm();
                Application.Run(form);
            }
            catch (Exception e)
            {
                this.ShowCrashMessageBox(e);
            }
            finally
            {
                this.DisposeQuietly();
            }

        }

        public void ShowCrashMessageBox(Exception exception)
        {
            var file = this.DumpCrashMessage(exception);

            using (var form = new CrashReportForm(file, exception))
            {
                form.ShowDialog();
            }

        }

        public string GetCrashReportsDirectory()
        {
            var directory = PathUtils.GetPath("CrashReports");
            Directory.CreateDirectory(directory);

            return directory;
        }

        private static string GetFileName(string will)
        {
            if (Exist(will) == false)
            {
                return will;
            }

            var extension = Path.GetExtension(will);
            var prefix = will.Substring(0, will.Length - extension.Length);

            for (int i = 1; ; i++)
            {
                will = prefix + "_(" + i + ")" + extension;

                if (Exist(will) == false)
                {
                    return will;
                }

            }

        }

        private static bool Exist(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }

        private string DumpCrashMessage(Exception exception)
        {
            try
            {
                var dateTime = DateTime.Now;
                var directory = this.GetCrashReportsDirectory();
                var file = GetFileName(PathUtils.GetPath(directory, $"{dateTime:yyyy_MM_dd HH_mm_ss_fff}.log"));
                File.WriteAllText(file, string.Concat(exception));

                return file;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }

        }

        ~DCConDownloader()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.MainForm.DisposeQuietly();
            this.UiTimer.DisposeQuietly();
        }

    }

}
