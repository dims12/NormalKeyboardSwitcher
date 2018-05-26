using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NormalKeyboardSwitcher
{

    delegate void WinEventHookDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    delegate void ForegroundWindowChangedDelegate(IntPtr hwnd);

    class ForegroundWindowListener
    {

        private IntPtr hwnd;
        private WinEventHookDelegate WindowEventHookInstance;
        private IntPtr windowEventHook;


        public ForegroundWindowListener()
        {
            WindowEventHookInstance = new WinEventHookDelegate(WindowEventHook);
            Install();
            
        }

        ~ForegroundWindowListener()
        {
            Uninstall();
        }
        
        
        private void WindowEventHook(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            FireForegroundWindowChanged(hwnd);
        }

        
        public void Install()
        {
            if (windowEventHook == IntPtr.Zero)
            {


                windowEventHook = SetWinEventHook(
                    EVENT_SYSTEM_FOREGROUND, // eventMin
                    EVENT_SYSTEM_FOREGROUND, // eventMax
                    IntPtr.Zero,             // hmodWinEventProc
                    WindowEventHookInstance,     // lpfnWinEventProc
                    0,                       // idProcess
                    0,                       // idThread
                    WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);

                if (windowEventHook == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
        }

        void Uninstall()
        {
            UnhookWinEvent(windowEventHook);
        }


        public void InputLangChangeRequest(IntPtr hwnd, UsedInputLanguage language)
        {
            IntPtr handle = language.InputLanguage.Handle;

            if (hwnd != null)
            {
                PostMessage(hwnd, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, handle);
            }


        }

        public void InputLangChangeRequest(UsedInputLanguage language)
        {
            InputLangChangeRequest(hwnd, language);
        }


        public event ForegroundWindowChangedDelegate ForegroundWindowChanged;

        private void FireForegroundWindowChanged(IntPtr hwnd)
        {
            this.hwnd = hwnd;
            ForegroundWindowChanged?.Invoke(hwnd);
        }

        /*
        public static Process GetCurrentForegroundProcess()
        {
            IntPtr hwnd = GetForegroundWindow();
            uint pid = GetWindowThreadProcessId(hwnd, IntPtr.Zero);
            Process p = Process.GetProcessById((int)pid);
            return p;
        }

        public static UsedInputLanguage GetCurrentForegroundInputLanguage()
        {
            Process process = GetCurrentForegroundProcess();
            return GetInputLanguage(process);
        }
        /// <summary>
        /// Returns InputLanguage of a given process or `null`
        /// </summary>
        /// <param name="thread"></param>
        /// <returns></returns>
        public static UsedInputLanguage GetInputLanguage(Process process)
        {
            uint hkl = GetKeyboardLayout((uint)process.Id);
            foreach(UsedInputLanguage il in UsedInputLanguages)
            {
                if( hkl == (uint)il.InputLanguage.Handle )
                {
                    return il;
                }
            }
            return null;

        }


        */


        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWinEventHook(int eventMin, int eventMax, IntPtr hmodWinEventProc, WinEventHookDelegate lpfnWinEventProc, int idProcess, int idThread, int dwflags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        private static extern int PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private const int WM_INPUTLANGCHANGEREQUEST = 0x0050;
        private const int EVENT_SYSTEM_FOREGROUND = 3;

        private const int WINEVENT_INCONTEXT = 4;
        private const int WINEVENT_OUTOFCONTEXT = 0;
        private const int WINEVENT_SKIPOWNPROCESS = 2;
        private const int WINEVENT_SKIPOWNTHREAD = 1;
    }
}
