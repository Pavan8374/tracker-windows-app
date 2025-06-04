using System.Runtime.InteropServices;

namespace NSTracker.Services.AutoSuggestions
{
    public static class NotificationHelper
    {
        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int SetCurrentProcessExplicitAppUserModelID(string AppID);

        public static void SetAppUserModelID(string appID)
        {
            int result = SetCurrentProcessExplicitAppUserModelID(appID);
            if (result != 0)
            {
                throw new InvalidOperationException("Failed to set AppUserModelID.");
            }
        }
    }
}
