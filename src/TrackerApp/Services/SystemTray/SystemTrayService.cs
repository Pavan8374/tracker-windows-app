using NSTracker.Services.Navigation;

namespace NSTracker.Services.SystemTray
{
    public class SystemTrayService : ISystemTrayService, IDisposable
    {
        private NotifyIcon? _notifyIcon;
        private readonly INavigationService _navigationService;
        private bool _disposed;
        private Form? _mainForm;
        private FormWindowState _lastWindowState;

        public SystemTrayService(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public void Initialize()
        {
            if (_notifyIcon != null) return;

            _notifyIcon = new NotifyIcon
            {
                Visible = false, // Start hidden until main form is shown
                Text = "NSTracker"
            };

            // Load the icon
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Icons", "logo.ico");
            if (File.Exists(iconPath))
            {
                _notifyIcon.Icon = new Icon(iconPath);
            }

            // Create context menu
            var contextMenu = new ContextMenuStrip();

            var openItem = new ToolStripMenuItem("Open");
            openItem.Click += (s, e) => RestoreFromTray();

            var logoutItem = new ToolStripMenuItem("Logout");
            logoutItem.Click += async (s, e) =>
            {
                HideTrayIcon();
                _mainForm = null;
                _navigationService.ShowLoginForm();
            };

            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) =>
            {
                Cleanup();
                Application.Exit();
            };

            contextMenu.Items.AddRange(new ToolStripItem[]
            {
                openItem,
                new ToolStripSeparator(),
                logoutItem,
                new ToolStripSeparator(),
                exitItem
            });

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += (s, e) => RestoreFromTray();
        }

        public void ShowTrayIcon()
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = true;
            }
        }

        public void HideTrayIcon()
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
            }
        }

        public void MinimizeToTray(Form form)
        {
            if (form == null || form is LoginForm) return; // Don't minimize LoginForm to tray

            _mainForm = form;
            _lastWindowState = form.WindowState;
            form.WindowState = FormWindowState.Minimized;
            form.Hide();
            ShowTrayIcon();
        }

        public void RestoreFromTray()
        {
            if (_mainForm == null || _mainForm.IsDisposed)
            {
                // If main form is not available, create a new one
                _navigationService.ShowTrackerForm();
                return;
            }

            // Restore the window
            _mainForm.Show();
            _mainForm.WindowState = _lastWindowState;
            _mainForm.BringToFront();
            _mainForm.Focus();
        }

        public void ShowBalloonTip(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
        {
            _notifyIcon?.ShowBalloonTip(3000, title, message, icon);
        }

        public void UpdateIcon(Icon icon)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Icon = icon;
            }
        }

        public void Cleanup()
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }
            _mainForm = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                Cleanup();
            }

            _disposed = true;
        }
    }
}
