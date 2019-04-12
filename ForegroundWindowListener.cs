using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NormalKeyboardSwitcher
{

    // delegate fow event hooks
    delegate void WinEventHookDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    delegate void ForegroundWindowChangedDelegate(IntPtr hwnd);

    delegate bool PostInputLanguageRequestDelegate(IntPtr hwnd, IntPtr handle);

    /// <summary>
    /// Listens for changing of windows systemwide
    /// 
    /// Contains operations with windows
    /// </summary>
    class ForegroundWindowListener
    {

        private IntPtr hwnd;
        private WinEventHookDelegate WindowEventHookInstance;
        private IntPtr windowEventHook;
        


        public ForegroundWindowListener()
        {
            WindowEventHookInstance = new WinEventHookDelegate(WindowEventHook);
            //Install();
            
        }

        ~ForegroundWindowListener()
        {
            //Uninstall();
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

        /// <summary>
        /// Sends WM_INPUTLANGCHANGEREQUEST to given window
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="handle"></param>
        /// <returns></returns>
        static bool InputLanguageRequest(IntPtr hwnd, IntPtr handle) {

            StringBuilder windowTextBuilder = new StringBuilder(100 + 1);
            GetWindowText(hwnd, windowTextBuilder, 100);
            if(windowTextBuilder.ToString().ToLower().Contains("comsol multiphysics")) {
                return false;
            }


            int res;

            //res = PostMessage(hwnd, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, handle);

            //PostMessage(hwnd, WM_INPUTLANGCHANGE, IntPtr.Zero, handle);

            //SendMessage(hwnd, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, handle);

            // SendMessage(hwnd, WM_INPUTLANGCHANGE, IntPtr.Zero, handle);

            //res = PostMessage(HWND_BROADCAST, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, handle);

            //res = PostMessage(hwnd, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, handle);
            //res = PostMessage(hwnd, WM_INPUTLANGCHANGE, IntPtr.Zero, handle);

            //res = PostMessage(hwnd, WM_INPUTLANGCHANGE, IntPtr.Zero, handle);


            // doesn't work in FAR manager?
            int x = unchecked((short)handle);
            handle = (IntPtr)x;
            res = PostMessage(hwnd, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, handle);




            //int x = unchecked((short)handle);
            //x = x & 0x0000;
            //handle = (IntPtr)x;
            //res = PostMessage(hwnd, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, handle);

            //SystemParametersInfo(SPI_SETDEFAULTINPUTLANG, 0, ref handle, 0);
            //int x = unchecked((short)handle);
            //handle = (IntPtr)x;
            //res = PostMessage(hwnd, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, handle);

            //SystemParametersInfo(SPI_SETDEFAULTINPUTLANG, 0, ref handle, 0); // does nothing


            //int x = unchecked((short)handle);
            //handle = (IntPtr)x;
            //res = PostMessage(HWND_BROADCAST, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, handle);

            return true;
        }

        /// <summary>
        /// Sends WM_INPUTLANGCHANGEREQUEST to given window and some of it's childs
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="language"></param>
        public void InputLangChangeRequest(IntPtr hwnd, UsedInputLanguage language)
        {
            IntPtr targetHandle = language.InputLanguage.Handle;
            IntPtr currentHandle = GetUserInputLanguageHandle(hwnd);
            //hwnd = GetRootOwner();

            if (hwnd != null /*&& targetHandle != currentHandle*/)
            {
                InputLanguageRequest(hwnd, targetHandle);

                StringBuilder buf = new StringBuilder(100);
                GetClassName(hwnd, buf, 100);

                //if this is a dialog class then post message to all descendants 
                if (buf.ToString() == "#32770")
                    EnumChildWindows(hwnd, InputLanguageRequest, targetHandle);
            }

            //SetDefaultInputLang(language);


        }

        public void InputLangChangeRequestForChildsIfRequired(IntPtr hwnd, UsedInputLanguage language) {

            IntPtr handle = language.InputLanguage.Handle;

        }


        public void InputLangChangeRequestFocused(UsedInputLanguage language)
        {
            hwnd = GetRootOwner();
            InputLangChangeRequest(hwnd, language);
        }


        public event ForegroundWindowChangedDelegate ForegroundWindowChanged;

        private void FireForegroundWindowChanged(IntPtr hwnd)
        {
            this.hwnd = hwnd;
            ForegroundWindowChanged?.Invoke(hwnd);
        }

        /// <summary>
        /// Returns active window
        /// </summary>
        /// <returns></returns>
        private IntPtr GetRootOwner() {
            IntPtr hwnd = GetForegroundWindow();
            //hwnd = GetAncestor(hwnd, GA_ROOTOWNER); // not needed?
            return hwnd;

            // will take window with GetActiveWindow, not with GetForegroundWindow
            // doesn't work
            //IntPtr hwnd = GetActiveWindow();
            //return hwnd;
        }

        //private UsedInputLanguage GetUserInputLanguage(IntPtr hwnd) {
        //    int idThreas = GetWindowThreadProcessId(hwnd, 0);
        //    IntPtr res = GetKeyboardLayout(idThreas);
        //    return inputController.GetInputLanguage(res);
        //}

        private IntPtr GetUserInputLanguageHandle(IntPtr hwnd)
        {
            int idThreas = GetWindowThreadProcessId(hwnd, 0);
            IntPtr handle = GetKeyboardLayout(idThreas);
            //return inputController.GetInputLanguage(res);
            return handle;
        }

        private bool SetDefaultInputLang(UsedInputLanguage inputLanguage) {

            //return SystemParametersInfo(SPI_SETDEFAULTINPUTLANG, 0, inputLanguage.InputLanguage.Handle.ToInt32() & 0xffff, 0); // doesn't work
            return SystemParametersInfo(SPI_SETDEFAULTINPUTLANG, 0, (int) inputLanguage.InputLanguage.Handle, 0); // doesn't work
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(int uiAction, int uiParam, ref IntPtr pvParam, int fWinIni);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWinEventHook(int eventMin, int eventMax, IntPtr hmodWinEventProc, WinEventHookDelegate lpfnWinEventProc, int idProcess, int idThread, int dwflags);

        [DllImport("user32", EntryPoint = "CallNextHookEx")]
        static extern int CallNextHook(IntPtr hHook, int ncode, IntPtr wParam, IntPtr lParam);


        [DllImport("user32.dll", SetLastError = true)]
        private static extern int UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        private static extern int PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowThreadProcessId(IntPtr hWnd, int gaFlags);

        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(int idThread);

        [DllImport("user32.dll")]
        static extern IntPtr GetAncestor(IntPtr hWnd, int gaFlags);

        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        static extern int GetClassName(IntPtr hWnd, StringBuilder className, int nMaxCount);

        [DllImport("user32.dll")]
        static extern bool EnumChildWindows(IntPtr hWnd, PostInputLanguageRequestDelegate handler, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool SystemParametersInfo(int uiAction, int uiParam, int pvParam, int fWinIni);

        private const int WM_INPUTLANGCHANGEREQUEST = 0x0050;
        private const int WM_INPUTLANGCHANGE = 0x0051;
        private const int SPI_SETDEFAULTINPUTLANG = 0x005A;
        private const int EVENT_SYSTEM_FOREGROUND = 3;

        private const int WINEVENT_INCONTEXT = 4;
        private const int WINEVENT_OUTOFCONTEXT = 0;
        private const int WINEVENT_SKIPOWNPROCESS = 2;
        private const int WINEVENT_SKIPOWNTHREAD = 1;
        private const int GA_ROOTOWNER = 3;
        private static IntPtr HWND_BROADCAST = (IntPtr) 0xffff; 

    }
}
