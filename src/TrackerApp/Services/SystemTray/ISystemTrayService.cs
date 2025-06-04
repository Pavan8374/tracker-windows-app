namespace NSTracker.Services.SystemTray
{
    public interface ISystemTrayService
    {
        void Initialize();
        void ShowBalloonTip(string title, string message, ToolTipIcon icon = ToolTipIcon.Info);
        void UpdateIcon(Icon icon);
        void Cleanup();
        void MinimizeToTray(Form form);
        void RestoreFromTray();
        void HideTrayIcon();
        void ShowTrayIcon();
    }
}
