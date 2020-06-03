using CsvHelper;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dull.WinformApp
{
    public partial class Form1 : Form
    {       
        private AutoSaveHookManager autoSaveHookManager;
        private AutoResetEvent downloadedEvent = new AutoResetEvent(false);

        public Form1()
        {
            InitializeComponent();

            autoSaveHookManager = new AutoSaveHookManager(downloadedEvent);

            Disposed += Form1_Disposed;
        }

        private void Form1_Disposed(object sender, EventArgs e)
        {
            autoSaveHookManager.Dispose();
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog { Multiselect = false, Filter = "CSV Files (*.csv)|*.csv" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;

                tbFileSelected.Text = dlg.FileName;
            }
        }

        private async void btnDownloadFiles_Click(object sender, EventArgs e)
        {
            RequestLite requestLite;

            if (CheckPrerequisites(out requestLite, out string msg) == false)
            {
                MessageBox.Show(msg);
                return;
            }

            if (SetSavePath(requestLite) == false)
            {
                return;
            }

            
            using (var reader = new StreamReader(requestLite.CsvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Configuration.RegisterClassMap<WebDocumentMap>();
                var docs = csv.GetRecords<WebDocument>();

                foreach (var doc in docs)
                {
                    if (String.IsNullOrWhiteSpace(doc.Id)) continue;

                    autoSaveHookManager.SetCurrentWebDocument(doc);
                    this.webBrowser1.DocumentCompleted += DocumentCompleted;

                    Console.WriteLine($"navigate: {doc.Url}");
                    this.webBrowser1.Navigate(doc.Url);

                    await Task.Run(() => downloadedEvent.WaitOne());
                }

                MessageBox.Show("All done！");
            }            
        }

        private void DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.webBrowser1.DocumentCompleted -= DocumentCompleted;

            autoSaveHookManager.Install();
            this.webBrowser1.ShowSaveAsDialog();
            autoSaveHookManager.Uninstall();
        }

        private bool SetSavePath(RequestLite requestLite)
        {
            var dir = "~" + Path.GetFileNameWithoutExtension(requestLite.CsvFile);
            string path = Path.Combine(Environment.CurrentDirectory, dir);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            using (var dlg = new CommonOpenFileDialog())
            {
                dlg.InitialDirectory = path;
                dlg.IsFolderPicker = true;
                dlg.Multiselect = false;
                dlg.Title = "Download to";

                if (dlg.ShowDialog() != CommonFileDialogResult.Ok)
                {
                    return false;
                }

                requestLite.OutputDir = dlg.FileName;
            }            
            return true;
        }

        private bool CheckPrerequisites(out RequestLite requestLite, out string msg)
        {
            requestLite = new RequestLite { CsvFile = tbFileSelected.Text };
            msg = null;

            //check csv file
            if (!File.Exists(requestLite.CsvFile))
            {
                msg = "csv file not found！";
                return false;
            }
            if (Path.GetExtension(requestLite.CsvFile).ToLower() != ".csv")
            {
                msg = "only csv file allowed!";
                return false;
            }
            try
            {
                using (var fs = File.OpenRead(requestLite.CsvFile))
                {

                }
            }
            catch (IOException e)
            {
                msg = e.Message;
                return false;
            }

            //all passed
            return true;
        }

        class RequestLite
        {
            public string CsvFile { get; set; }
            public string OutputDir { get; set; }
        }
    }              
}
