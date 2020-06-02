using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using static Dull.WinformApp.WinAPI;
using static Dull.WinformApp.Helper;

namespace Dull.WinformApp
{
    internal class AutoSaveHookManager
    {
        private const int iDlg = 1;
        private const int iSaveButton = 17;
        private const int iFileNameEditParent = 67;
        private const int iFileNameEdit = 69;
        private const int iSaveTypeCombBoxParent = 86;
        private const int iSaveTypeCombBox = 87;

        private IntPtr hook;
        private HookProc hookProc;
        private List<IntPtr> hwndList = new List<IntPtr>();
        private WebDocument currentWebDoc;
        private AutoResetEvent downloadedEvent;        

        public AutoSaveHookManager(AutoResetEvent downloadedEvent)
        {
            this.downloadedEvent = downloadedEvent;
            
            hookProc = new HookProc(Filter);
        }

        private IntPtr Filter(int code, IntPtr wParam, IntPtr lParam)
        {
            switch (code)
            {
                case (int)CbtHookAction.HCBT_CREATEWND:
                    if (hwndList.Count < 100)
                    {
                        hwndList.Add(wParam);
                    }
                    break;
                case (int)CbtHookAction.HCBT_ACTIVATE:
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

                Console.WriteLine($"save '{currentWebDoc.Id}'");

                //select save type
                SendMessage(hwndSaveType, CB_SETCURSEL, (int)SaveType.SAVETYPE_ARCHIVE, IntPtr.Zero);
                SendMessage(hwndSaveTypeParent, WM_COMMAND, BuildWParam(0, 0x0008), hwndSaveType);
                //set file name
                SetWindowText(hwndFileName, currentWebDoc.Id);
                SendMessage(hwndFileNameParent, WM_COMMAND, BuildWParam(0x03E9, 0x0300), hwndFileName);
                //click save button
                SendMessage(hwndSave, BM_CLICK, 0, IntPtr.Zero);
            }
            else
            {
                Console.WriteLine("auto save fail!");
            }

            Thread.Sleep(2000);
            downloadedEvent.Set();
        }

        public void SetCurrentWebDocument(WebDocument doc)
        {
            currentWebDoc = doc;
        }

        public void Install()
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

        public void Uninstall()
        {
            if (hook != IntPtr.Zero)
            {
                var result = UnhookWindowsHookEx(hook);
                Console.WriteLine($"uninstall hook: {result}");
            }

            hwndList.Clear();
        }

        public enum SaveType : int
        {
            SAVETYPE_HTMLPAGE = 0,
            SAVETYPE_ARCHIVE,
            SAVETYPE_HTMLONLY,
            SAVETYPE_TXTONLY
        }
    }
}
