using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dull.WinformApp
{
    public partial class Form1 : Form
    {
        const int HCBT_CREATEWND = (int)CbtHookAction.HCBT_CREATEWND;
        const int HCBT_ACTIVATE = (int)CbtHookAction.HCBT_ACTIVATE;
        const int CB_SETCURSEL = 0x014E;
        const int WM_COMMAND = 0x0111;
        const int BM_CLICK = 0x00F5;
        public Form1()
        {
            InitializeComponent();

            tbOutputDirSelected.Text = Environment.CurrentDirectory;

            hookProc = new HookProc(Filter);
        }

        IntPtr hook;
        HookProc hookProc;
        AutoResetEvent downloadEvent = new AutoResetEvent(false);
        WebDocument currentRequest;

        private void button1_Click(object sender, EventArgs e)
        {
            this.webBrowser1.DocumentCompleted += webBrowser1_DocumentCompleted;
            
            this.webBrowser1.Navigate("https://www.fenginfo.com/17.html");            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            InstallHook();
            this.webBrowser1.ShowSaveAsDialog();
            UninstallHook();
        }

        private void InstallHook()
        {
            hook = SetWindowsHookEx(HookType.WH_CBT, hookProc, IntPtr.Zero, AppDomain.GetCurrentThreadId());
            if (hook == IntPtr.Zero)
            {
                var error = new Win32Exception(Marshal.GetLastWin32Error());
                Console.WriteLine($"Fail to install hook: {error.Message}");
            }
            else
            {
                Console.WriteLine("install hook: success");                
            }

            hwndList.Clear();
        }

        List<IntPtr> hwndList = new List<IntPtr>();
        const int iDlg = 1;
        const int iSaveButton = 17;
        const int iFileNameEditParent = 67;
        const int iFileNameEdit = 69;
        const int iSaveTypeCombBoxParent = 86;
        const int iSaveTypeCombBox = 87;
        private IntPtr Filter(int code, IntPtr wParam, IntPtr lParam)
        {            
            switch (code)
            {
                case HCBT_CREATEWND:
                    if (hwndList.Count < 100)
                    {
                        hwndList.Add(wParam);
                    }
                    break;
                case HCBT_ACTIVATE:                    
                    if (wParam == hwndList[iDlg])
                    {
                        Console.WriteLine($"create thread for hwnd: {wParam.ToString("X")}");
                        var thread = new Thread(AutoSave);
                        thread.Start();
                    }
                    break;
            }

            return CallNextHookEx(hook, code, wParam, lParam);
        }


        //https://www.codeproject.com/Articles/2847/Automated-IE-SaveAs-MHTML
        private void AutoSave()
        {            
            bool visible = false;

            int count = 5;
            while (count-- > 0)
            {
                visible = IsWindowVisible(hwndList[iDlg]);
                if (visible)
                {
                    break;
                }
                Console.WriteLine("AutoSave not ready.");
                Thread.Sleep(250);
            }

            if (visible)
            {
                Console.WriteLine("Do auto save begin!");

                if (hwndList.Count <= iSaveTypeCombBox)
                {
                    Console.WriteLine("fail to save: can not get essential hwnd");
                    return;
                }

                var hwndSaveTypeParent = hwndList[iSaveTypeCombBoxParent];
                var hwndSaveType = hwndList[iSaveTypeCombBox];
                var hwndFileNameParent = hwndList[iFileNameEditParent];
                var hwndFileName = hwndList[iFileNameEdit];
                var hwndSave = hwndList[iSaveButton];

                Console.WriteLine($"save '{currentRequest.Id}'");

                //select save type
                SendMessage(hwndSaveType, CB_SETCURSEL, (int)SaveType.SAVETYPE_ARCHIVE, IntPtr.Zero);
                SendMessage(hwndSaveTypeParent, WM_COMMAND, BuildWParam(0, 0x0008), hwndSaveType);
                //set file name
                SetWindowText(hwndFileName, currentRequest.Id);
                SendMessage(hwndFileNameParent, WM_COMMAND, BuildWParam(0x03E9, 0x0300), hwndFileName);
                //click save button
                SendMessage(hwndSave, BM_CLICK, 0, IntPtr.Zero);
            }
            else
            {
                Console.WriteLine("auto save fail!");
            }

            Thread.Sleep(2000);
            downloadEvent.Set();
        }

        private void UninstallHook()
        {
            if (hook != IntPtr.Zero)
            {
                var result = UnhookWindowsHookEx(hook);
                Console.WriteLine($"uninstall hook: {result}");
            }

            hwndList.Clear();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.webBrowser1.DocumentCompleted -= webBrowser1_DocumentCompleted;

            Console.WriteLine("webBrowser1_DocumentCompleted");
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(HookType code, HookProc func, IntPtr hmod, int threadID);

        [DllImport("user32.dll")] 
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hhook);

        [DllImport("user32.dll")] 
        static extern IntPtr CallNextHookEx(IntPtr hhook, int code, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetWindowText(IntPtr hwnd, string lpString);

        public static int BuildWParam(ushort low, ushort high)
        {
            return ((int)high << 16) | (int)low;
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog { Multiselect = false, Filter = "CSV 文件 (*.csv)|*.csv" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;

                tbFileSelected.Text = dlg.FileName;
            }
        }

        private void btnSelectOutputDir_Click(object sender, EventArgs e)
        {            
            using (var dlg = new CommonOpenFileDialog() { IsFolderPicker = true, Multiselect = false })
            {
                if (dlg.ShowDialog() != CommonFileDialogResult.Ok) return;

                tbOutputDirSelected.Text = dlg.FileName;
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

                    currentRequest = doc;
                    this.webBrowser1.DocumentCompleted += DocumentCompleted;

                    Console.WriteLine($"navigate: {doc.Url}");
                    this.webBrowser1.Navigate(doc.Url);

                    await Task.Run(() => downloadEvent.WaitOne());
                }
            }            
        }



        private void DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.webBrowser1.DocumentCompleted -= DocumentCompleted;

            InstallHook();
            this.webBrowser1.ShowSaveAsDialog();
            UninstallHook();
        }

        private bool SetSavePath(RequestLite requestLite)
        {
            var dir = "~" + Path.GetFileNameWithoutExtension(requestLite.CsvFile);
            string path = Path.Combine(requestLite.OutputDir, dir);
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
            }

            return true;
        }

        private bool CheckPrerequisites(out RequestLite requestLite, out string msg)
        {
            requestLite = new RequestLite { CsvFile = tbFileSelected.Text, OutputDir = tbOutputDirSelected.Text };
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
            //check output dir
            if (String.IsNullOrWhiteSpace(requestLite.OutputDir))
            {
                msg = "请选择目录！";
                return false;
            }
            if (!Directory.Exists(requestLite.OutputDir))
            {
                try
                {
                    Directory.CreateDirectory(requestLite.OutputDir);
                }
                catch
                {
                    msg = "路径不合法，请重新选择输出目录！";
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

            public WebDocument Current { get; set; }
        }
    }

    enum SaveType : int
    {
        SAVETYPE_HTMLPAGE = 0,
        SAVETYPE_ARCHIVE,
        SAVETYPE_HTMLONLY,
        SAVETYPE_TXTONLY
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct CREATESTRUCT
    {
        public IntPtr lpCreateParams;
        public IntPtr hInstance;
        public IntPtr hMenu;
        public IntPtr hwndParent;
        public int cy;
        public int cx;
        public int y;
        public int x;
        public int style;
        public IntPtr lpszName;
        public IntPtr lpszClass;
        public int dwExStyle;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct CBT_CREATEWND
    {
        public IntPtr lpcs;
        public IntPtr hwndInsertAfter;
    }

    enum CbtHookAction : int
    {
        HCBT_MOVESIZE = 0,
        HCBT_MINMAX = 1,
        HCBT_QS = 2,
        HCBT_CREATEWND = 3,
        HCBT_DESTROYWND = 4,
        HCBT_ACTIVATE = 5,
        HCBT_CLICKSKIPPED = 6,
        HCBT_KEYSKIPPED = 7,
        HCBT_SYSCOMMAND = 8,
        HCBT_SETFOCUS = 9
    }

    enum HookType : int
    {
        WH_JOURNALRECORD = 0,
        WH_JOURNALPLAYBACK = 1,
        WH_KEYBOARD = 2,
        WH_GETMESSAGE = 3,
        WH_CALLWNDPROC = 4,
        WH_CBT = 5,
        WH_SYSMSGFILTER = 6,
        WH_MOUSE = 7,
        WH_HARDWARE = 8,
        WH_DEBUG = 9,
        WH_SHELL = 10,
        WH_FOREGROUNDIDLE = 11,
        WH_CALLWNDPROCRET = 12,
        WH_KEYBOARD_LL = 13,
        WH_MOUSE_LL = 14
    }
    
    delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

    class WebDocument
    {
        public string Id { get; set; }
        public string Catagory { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
    }
    class WebDocumentMap : ClassMap<WebDocument>
    {
        public WebDocumentMap()
        {
            Map(m => m.Id).Name("ID");
            Map(m => m.Catagory).Name("分类");
            Map(m => m.Url).Name("文档URL");
            Map(m => m.Title).Name("文档标题");
        }
    }
}
