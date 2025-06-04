using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Tracker.Services.Utility
{
    public class GlobalHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;

        private IntPtr keyboardHookID = IntPtr.Zero;
        private IntPtr mouseHookID = IntPtr.Zero;

        private readonly LowLevelKeyboardProc keyboardProc;
        private readonly LowLevelMouseProc mouseProc;

        public event EventHandler KeyPressed;
        public event EventHandler MouseClicked;

        public GlobalHook()
        {
            keyboardProc = HookKeyboardCallback;
            mouseProc = HookMouseCallback;
            keyboardHookID = SetHook(WH_KEYBOARD_LL, keyboardProc);
            mouseHookID = SetHook(WH_MOUSE_LL, mouseProc);
        }

        private IntPtr SetHook(int hookType, Delegate proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(hookType, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookKeyboardCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                KeyPressed?.Invoke(this, EventArgs.Empty);
            }
            return CallNextHookEx(keyboardHookID, nCode, wParam, lParam);
        }

        private IntPtr HookMouseCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)WM_LBUTTONDOWN || wParam == (IntPtr)WM_RBUTTONDOWN)
                {
                    MouseClicked?.Invoke(this, EventArgs.Empty);
                }
            }
            return CallNextHookEx(mouseHookID, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(keyboardHookID);
            UnhookWindowsHookEx(mouseHookID);
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, Delegate lpfn,
            IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }

}
