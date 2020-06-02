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
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog { Multiselect = false, Filter = "CSV 文件 (*.csv)|*.csv" })
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

                MessageBox.Show("下载完成！");
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
                dlg.Title = "下载即将开始，请按下【选择文件夹】";

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
                msg = "源文件不存在或未指定，请选择文件！";
                return false;
            }
            if (Path.GetExtension(requestLite.CsvFile).ToLower() != ".csv")
            {
                msg = "只接受csv格式源文件，请重新选择文件！";
                return false;
            }
            using (var fs = new FileStream(requestLite.CsvFile, FileMode.Open))
            {
                if (!fs.CanRead)
                {
                    msg = "源文件被其他程序锁定，请释放后重试！";
                    return false;
                }
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
