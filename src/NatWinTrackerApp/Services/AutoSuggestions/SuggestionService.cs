using Microsoft.Toolkit.Uwp.Notifications;
using System.Timers;
using Windows.UI.Notifications;
using Timer = System.Windows.Forms.Timer;

namespace NSTracker.Services.AutoSuggestions
{
    public class SuggestionService : ISuggestionService
    {
        private System.Timers.Timer _intervalTimer;
        private Random _random = new Random();

        private bool morningNotification = false;
        private bool trackingNotification = false;
        private bool lunchNotification = false;
        private bool afterLunchNotification = false;
        private bool endNotification = false;

        private readonly Dictionary<string, (TimeSpan Start, TimeSpan End)> _notificationIntervals;
        private readonly Dictionary<string, bool> _notificationSent;
        private DateTime _lastResetDate;
        private readonly string[] _morningQuotes = new string[]
        {
            "The future belongs to those who believe in the beauty of their dreams. - Eleanor Roosevelt",
            "Success is not final, failure is not fatal: it is the courage to continue that counts. - Winston Churchill",
            "Believe you can and you're halfway there. - Theodore Roosevelt",
            "Every morning we are born again. What we do today matters most. - Buddha",
            "The only way to do great work is to love what you do. - Steve Jobs"
        };

        public SuggestionService()
        {

            var intervals = new Dictionary<string, (TimeSpan Start, TimeSpan End)>
            {
                { "Morning", (new TimeSpan(10, 15, 0), new TimeSpan(10, 45, 0)) },
                { "LunchBreak", (new TimeSpan(13, 00, 0), new TimeSpan(13, 15, 0)) },
                { "TrackingReminder", (new TimeSpan(14, 00, 0), new TimeSpan(14, 15, 0)) },
                { "EveningWrapup", (new TimeSpan(18, 30, 0), new TimeSpan(19, 00, 0)) }
            };

            _notificationIntervals = intervals;
            _notificationSent = _notificationIntervals.ToDictionary(k => k.Key, v => false);
            _lastResetDate = GetIndianStandardTime().Date;

            _intervalTimer = new System.Timers.Timer();
            _intervalTimer.Elapsed += CheckNextNotification;
            //ScheduleNextCheck();
        }

        public void TrackingNotStarted(bool isTracking)
        {
            DateTime now = GetIndianStandardTime();
            TimeSpan startTime = new TimeSpan(10, 15, 0);   // 10:15 AM
            TimeSpan endTime = new TimeSpan(11, 0, 0);     // 11:00 AM

            // Check if current time is between 10:00 AM and 11:00 AM
            if (now.TimeOfDay >= startTime && now.TimeOfDay <= endTime)
            {
                // Only show notification if tracking is not started
                if (!isTracking && !trackingNotification)
                {
                    //ShowNotification(
                    //    "Tracking Reminder",
                    //    "Have you started tracking your tasks? It's time to log your progress!"
                    //);

                    string title = "Tracking Reminder";
                    string message = "Have you started tracking your tasks? Don't forget to log your progress!";
                    string logoPath = LoadLogo();
                    string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", "Notifications", "reminder.png");
                    SendToastNotification(title, message, logoPath, imagePath);
                    trackingNotification = true;
                };
            }
        }

        private void CheckNextNotification(object sender, ElapsedEventArgs e)
        {
            DateTime now = GetIndianStandardTime();

            // Reset notifications if it's a new day
            if (now.Date != _lastResetDate)
            {
                ResetNotifications();
                _lastResetDate = now.Date;
            }

            foreach (var (key, interval) in _notificationIntervals)
            {
                if (!_notificationSent[key] &&
                    now.TimeOfDay >= interval.Start &&
                    now.TimeOfDay <= interval.End)
                {
                    TriggerNotification(key);
                    _notificationSent[key] = true; // Mark the notification as sent
                    break; // Only trigger one notification per check
                }
            }

            if (AllNotificationsSentForToday())
            {
                _intervalTimer.Stop();
            }
            else
            {
                ScheduleNextCheck();
            }
        }

        private void ResetNotifications()
        {
            // Reset all notifications to not sent
            foreach (var key in _notificationSent.Keys.ToList())
            {
                _notificationSent[key] = false;
            }
        }

        private void ScheduleNextCheck()
        {
            DateTime now = GetIndianStandardTime();

            // Find the next notification time from the unsent notifications
            var nextNotification = _notificationIntervals
                .Where(kv => !_notificationSent[kv.Key] && now.TimeOfDay < kv.Value.End)
                .Select(kv => (kv.Key, NextTime: GetRandomTime(kv.Value.Start, kv.Value.End)))
                .OrderBy(kv => kv.NextTime)
                .FirstOrDefault();

            if (nextNotification != default)
            {
                TimeSpan timeUntilNext = nextNotification.NextTime - now.TimeOfDay;
                _intervalTimer.Interval = Math.Max(1, timeUntilNext.TotalMilliseconds);
                _intervalTimer.Start();
            }
            else
            {
                // Stop the timer if all notifications are sent
                _intervalTimer.Stop();
            }
        }

        private bool AllNotificationsSentForToday()
        {
            return _notificationSent.Values.All(sent => sent);
        }

        private DateTime GetIndianStandardTime()
        {
            DateTime utcNow = DateTime.UtcNow;
            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, istZone);
        }

        private void TriggerNotification(string key)
        {
            switch (key)
            {
                case "Morning":
                    ShowMorningNotification();
                    break;
                case "LunchBreak":
                    ShowLunchBreakNotification();
                    break;
                case "TrackingReminder":
                    ShowTrackingReminderNotification();
                    break;
                case "EveningWrapup":
                    ShowEveningWrapupNotification();
                    break;
            }
        }

        private TimeSpan GetRandomTime(TimeSpan start, TimeSpan end)
        {
            int randomMinutes = _random.Next((int)(end - start).TotalMinutes);
            return start.Add(TimeSpan.FromMinutes(randomMinutes));
        }

        private void ShowMorningNotification()
        {
            string quote = _morningQuotes[_random.Next(_morningQuotes.Length)];
            //ShowNotification(
            //    "Good Morning!",
            //    $"Start your day with purpose!\n\n{quote}\n\nLet's begin tracking and make today count!"
            //);
            string title = "Good Morning!";
            string message = $"Start your day with purpose!\n\n{quote}\n\nLet's begin tracking and make today count!";
            string logoPath = LoadLogo();
            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", "Notifications", "DayStart.png");
            if (!morningNotification) 
            {
                SendToastNotification(title, message, logoPath, imagePath);
                morningNotification = true;
            }
        }

        private void ShowLunchBreakNotification()
        {
            //ShowNotification(
            //    "Lunch Break",
            //    "Time to recharge! Take a moment to relax and enjoy your lunch."
            //);

            string title = "Lunch Break";
            string message = "Time to recharge! Take a moment to relax and enjoy your lunch.";
            string logoPath = LoadLogo();
            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", "Notifications", "lunch.png");
            if (!lunchNotification)
            {
                SendToastNotification(title, message, logoPath, imagePath);
                lunchNotification = true;
            }
        }

        private void ShowTrackingReminderNotification()
        {
            //ShowNotification(
            //    "Tracking Reminder",
            //    "Have you started tracking your tasks? Don't forget to log your progress!"
            //);

            string title = "Tracking Reminder";
            string message = "Have you started tracking your tasks? Don't forget to log your progress!";
            string logoPath = LoadLogo();
            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", "Notifications", "reminder.png");
            if (!afterLunchNotification)
            {
                SendToastNotification(title, message, logoPath, imagePath);
                afterLunchNotification = true;
            }
        }

        private void ShowEveningWrapupNotification()
        {
            //ShowNotification(
            //    "End of Day",
            //    "Thank you for your hard work today! Take a moment to reflect on your achievements.\n\nGood Evening and rest well."
            //);

           string title = "End of Day";
           string message = "Thank you for your hard work today! Take a moment to reflect on your achievements.\n\nGood Evening and rest well.";
           string logoPath = LoadLogo();
           string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", "Notifications","entry.png");

            if (!endNotification)
            {
                SendToastNotification(title, message, logoPath, imagePath);
                endNotification = true;
            }
        }

        private string LoadLogo()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", "N-logo.png");
        }


        

        public void Start()
        {
            _intervalTimer.Start();
        }

        public void Stop()
        {
            _intervalTimer.Stop();
        }



        public static void SendToastNotification(string title, string message, string logoPath, string imagePath)
        {
            int expirationTimeInSeconds = 5;

            try
            {
                var toastContent = new ToastContentBuilder()
                    .AddText(title, hintMaxLines: 1)
                    .AddText(message)
                    .AddAppLogoOverride(new Uri(logoPath), ToastGenericAppLogoCrop.Circle)
                    .AddInlineImage(new Uri(imagePath))
                    .GetToastContent();

                var toast = new ToastNotification(toastContent.GetXml())
                {
                    ExpirationTime = DateTime.Now.AddSeconds(expirationTimeInSeconds)
                };

                var notifier = ToastNotificationManagerCompat.CreateToastNotifier();

                // Show the new toast notification
                notifier.Show(toast);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending notification: {ex.Message}", "Notification Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowNotification(string title, string message)
        {
            Icon icon = SystemIcons.Information;
            string timerFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Icons", "NatrixLogo.ico");
            if (File.Exists(timerFilePath))
            {
                icon = new Icon(timerFilePath);
            }

            NotifyIcon notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = icon,
                BalloonTipTitle = title,
                BalloonTipText = message,
                BalloonTipIcon = ToolTipIcon.Info,
            };

            notifyIcon.ShowBalloonTip(5000);

            Timer disposeTimer = new Timer();
            disposeTimer.Interval = 6000;
            disposeTimer.Tick += (s, e) =>
            {
                notifyIcon.Dispose();
                disposeTimer.Stop();
                disposeTimer.Dispose();
            };
            disposeTimer.Start();
        }
    }
}
