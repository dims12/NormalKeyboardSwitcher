using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NormalKeyboardSwitcher
{

    delegate IntPtr KeyboardHookDelegate(int nCode, IntPtr wParam, IntPtr lParam);
    delegate int NextTemporaryInputLanguageDelegate();
    delegate void DropTemporaryInputLanguageDelegate();
    delegate void SwitchToTemporaryInputLanguageDelegate();

    [StructLayout(LayoutKind.Sequential)]
    public class KBDLLHOOKSTRUCT
    {
        public Keys keys;
        public int scanCode;
        public int flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    public enum FirstKey
    {
        Control,
        LeftAlt
    }

    class KeyboardListener
    {

        private IntPtr keyboardHookID = IntPtr.Zero;
        private bool firstPressed = false;
        private bool secondPressed = false;

        private bool enabled = false;
        private FirstKey firstKey = FirstKey.Control;
        
        private KeyboardHookDelegate KeyboardHookInstance;

        public KeyboardListener(FirstKey firstKey) {
            FirstKey = firstKey;
            KeyboardHookInstance = new KeyboardHookDelegate(KeyboardHook);
            Install();
        }

        ~KeyboardListener()
        {
            Uninstall();
        }

        void Install()
        {
            keyboardHookID = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookInstance, IntPtr.Zero, 0);
        }

        void Uninstall()
        {
            UnhookWindowsHookEx(keyboardHookID);
        }
        
        FirstKey FirstKey
        {
            get
            {
                return (FirstKey)firstKey;
            }
            set
            {
                firstKey = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }

        private bool IsFirstKey(Keys key)
        {
            if( FirstKey == FirstKey.Control )
            {
                return key == Keys.LControlKey || key == Keys.RControlKey;
            }
            else if( FirstKey == FirstKey.LeftAlt )
            {
                return key == Keys.LMenu;
            }
            else
            {
                return false;
            }

        }

        private bool IsSecondKey(Keys key)
        {
            return key == Keys.LShiftKey || key == Keys.RShiftKey;
        }


        IntPtr KeyboardHook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if( nCode >= 0 )
            {
                KBDLLHOOKSTRUCT KeyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

                bool skip = false;
                int oldKeyCount = (firstPressed ? 1 : 0) + (secondPressed ? 1 : 0);

                if ( wParam.ToInt32() == WM_KEYDOWN && IsFirstKey(KeyInfo.keys) )
                {
                    firstPressed = true;
                }
                else if (wParam.ToInt32() == WM_KEYDOWN && IsSecondKey(KeyInfo.keys))
                {
                    secondPressed = true;
                }
                else if (wParam.ToInt32() == WM_KEYUP && IsFirstKey(KeyInfo.keys))
                {
                    firstPressed = false;
                }
                else if (wParam.ToInt32() == WM_KEYUP && IsSecondKey(KeyInfo.keys))
                {
                    secondPressed = false;
                }
                /*
                else if (KeyInfo.keys == Keys.LControlKey || KeyInfo.keys == Keys.RControlKey || KeyInfo.keys == Keys.RShiftKey || KeyInfo.keys == Keys.LShiftKey)
                {
                    // nothing
                }
                */

                else
                {
                    skip = true;
                    
                }

                if (!skip)
                {
                    int newKeyCount = (firstPressed ? 1 : 0) + (secondPressed ? 1 : 0);

                    if(oldKeyCount==1 && newKeyCount==2)
                    {
                        FireNextTemporaryInputLanguage();
                    }
                    else if( oldKeyCount>0 && newKeyCount == 0 )
                    {
                        FireSwitchToTemporaryInputLanguage();
                    }

                }
                else if( firstPressed || secondPressed )
                {
                    firstPressed = secondPressed = false;
                    FireDropTemporaryInputLanguage();
                }

            }

            return CallNextHookEx(keyboardHookID, nCode, wParam, lParam);
        }

        


        public event NextTemporaryInputLanguageDelegate NextTemporaryInputLanguage;

        private void FireNextTemporaryInputLanguage()
        {
            NextTemporaryInputLanguage?.Invoke();
        }

        public DropTemporaryInputLanguageDelegate DropTemporaryInputLanguage;

        private void FireDropTemporaryInputLanguage()
        {
            DropTemporaryInputLanguage?.Invoke();
        }

        public SwitchToTemporaryInputLanguageDelegate SwitchToTemporaryInputLanguage;

        private void FireSwitchToTemporaryInputLanguage()
        {

            SwitchToTemporaryInputLanguage?.Invoke();
        }

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int VK_SHIFT = 0x10;
        private const int VK_CONTROL = 0x11;
        private const int VK_MENU = 0x12; // ALT key
        private const int VK_LMENU = 0xA4;


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, KeyboardHookDelegate lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
