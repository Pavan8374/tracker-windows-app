using ReaLTaiizor.Controls;
using ReaLTaiizor.Forms;
using System.Runtime.InteropServices;
using static ReaLTaiizor.Forms.MaterialFlexibleForm;

namespace NSTracker.Utility
{
    public class AutoClosingMessageBox
    {
        private static System.Threading.Timer _timeoutTimer;
        private static string _caption;

        private AutoClosingMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, ButtonsPosition buttonsPosition, int timeout)
        {
            _caption = caption;
            _timeoutTimer = new System.Threading.Timer(OnTimerElapsed, null, timeout, Timeout.Infinite);
            MaterialMessageBox.Show(
                    text: text,
                    caption: caption,
                    buttons: buttons,
                    icon: icon,
                    buttonsPosition: MaterialFlexibleForm.ButtonsPosition.Center
                );
        }

        public static void Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,  ButtonsPosition buttonsPosition, int timeout)
        {
            new AutoClosingMessageBox(text, caption, buttons, icon, buttonsPosition, timeout);
        }

        private static void OnTimerElapsed(object state)
        {
            IntPtr mbWnd = FindWindow(null, _caption);
            if (mbWnd != IntPtr.Zero)
            {
                SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
            _timeoutTimer.Dispose();
        }

        private const int WM_CLOSE = 0x0010;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    }
}
