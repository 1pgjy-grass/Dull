using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using static Dull.WinformApp.WinAPI;
using static Dull.WinformApp.Helper;
using System.Windows.Forms;

namespace Dull.WinformApp
{
    internal class AutoSaveHookManager : IDisposable
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
        private IntPtr hwndConfirm;
        private WebDocument currentWebDoc;
        private AutoResetEvent downloadedEvent;
        private ManualResetEvent confirmEvent = new ManualResetEvent(true);
        private bool opened;

        public void Dispose()
        {
            downloadedEvent.Dispose();
            confirmEvent.Dispose();
        }

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
                    if (hwndList.Count <= iSaveTypeCombBox)
                    {
                        hwndList.Add(wParam);
                        if (!opened) opened = true;
                    }
                    else if(hwndConfirm == IntPtr.Zero)
                    {
                        unsafe
                        {
                            var param = (CbtCreateWnd*)lParam.ToPointer();
                            if ((int)param->lpcs->lpszClass == DLG_CLASS)
                            {
                                hwndConfirm = wParam;
                                confirmEvent.Reset();
                            }
                        }
                    }
                    break;
                case (int)CbtHookAction.HCBT_ACTIVATE:
                    if (wParam == hwndList[iDlg] && opened)
                    {
                        opened = false;
                        Console.WriteLine($"create thread for hwnd(saveAs): {wParam:X}");
                        var autoSave = BuildAutoAction(() => WaitVisble(wParam), Save, NotifySaved);
                        var thread = new Thread(autoSave);
                        thread.Start();
                    }
                    else if (wParam == hwndConfirm)
                    {
                        Console.WriteLine($"create thread for hwnd(confirm): {wParam:X}");
                        var autoConfirm = BuildAutoAction(() => WaitVisble(wParam), Confirm, null);
                        var thread = new Thread(autoConfirm);
                        thread.Start();
                    }
                    break;
            }

            return CallNextHookEx(hook, code, wParam, lParam);
        }

        private ThreadStart BuildAutoAction(Action preAction, Action coreAction, Action postAction) => () =>
        {
            preAction();
            coreAction();
            postAction?.Invoke();
        };
        private void Confirm()
        {
            Console.WriteLine("confirm!");
            SendKeys.SendWait("%y");
            confirmEvent.Set();
        }
        private void NotifySaved()
        {
            downloadedEvent.Set();
        }
        private void Save()
        {
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

            Thread.Sleep(2000);
            confirmEvent.WaitOne();
        }
        private void WaitVisble(IntPtr hwnd)
        {
            int count = 5;
            while (count-- > 0)
            {
                if (IsWindowVisible(hwnd)) return;

                Thread.Sleep(250);
            }
        }    

        public void SetCurrentWebDocument(WebDocument doc)
        {
            currentWebDoc = doc;
        }

        public void Install()
        {
            hook = SetWindowsHookEx(HookType.WH_CBT, hookProc, IntPtr.Zero, GetCurrentThreadId());
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
            hwndConfirm = IntPtr.Zero;
        }

        public void Uninstall()
        {
            if (hook != IntPtr.Zero)
            {
                var result = UnhookWindowsHookEx(hook);
                Console.WriteLine($"uninstall hook: {result}");
            }

            hwndList.Clear();
            hwndConfirm = IntPtr.Zero;
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
