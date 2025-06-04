using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinForms;
using Microsoft.Win32;
using NatWinTracker.Models.Auth;
using NatWinTracker.Models.DPRS;
using NatWinTracker.Models.Users;
using NatWinTracker.Services;
using NatWinTracker.Services.DPRSService.TokenManagement;
using NatWinTracker.Services.DPRSService.Users;
using NatWinTracker.Services.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSTracker.Common;
using NSTracker.Controls;
using NSTracker.Services.AutoSuggestions;
using NSTracker.Services.Navigation;
using NSTracker.Utility;
using ReaLTaiizor.Colors;
using ReaLTaiizor.Controls;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;
using ReaLTaiizor.Util;
using SkiaSharp;
using System.Text.Json;
using static ReaLTaiizor.Forms.MaterialFlexibleForm;
using Button = System.Windows.Forms.Button;

namespace NSTracker
{
    public partial class TrackerForm : MaterialForm
    {
        private NotifyIcon notifyIcon;

        private readonly ITokenManager _tokenManager;
        private readonly IUserService _userService;
        private readonly INavigationService _navigationService;
        private readonly ISuggestionService _suggestionService;

        private PieChart pieChart;
        private Random colorGenerator = new Random();
        private DateTime startTime;
        private PictureBox titleLogoBox;
        private MaterialDivider divider;
        private System.Windows.Forms.Timer timer;
        private Label timeLabel;
        private ComboBox projectComboBox;
        private MaterialButton logoutButton;
        private MaterialTextBoxEdit trackedHoursTextBox;
        private TimeSpan elapsedTime = TimeSpan.Zero;
        private TimeSpan totalTrackedTime = TimeSpan.Zero;
        private Button toggleButton;
        private Button addDescriptionButton;
        private Button addDPRS;
        private Label todayWorkedHours;
        private PictureBox userProfilePicture;
        private Label userNameLabel;
        private readonly SKColor REMAINING_HOURS_COLOR = new SKColor(200, 200, 200);
        private List<(int Duration, SKColor Color, string ProjectName)> sessionData = new List<(int Duration, SKColor Color, string ProjectName)>();
        private GlobalHook globalHook;
        private int mouseClickCount = 0;
        private int keyboardClickCount = 0;
        private DateTime lastSaveTime;
        private string jsonFilePath =  Path.Combine(Path.GetTempPath(), "activity.json");
        private ActivityLog currentLog;
        private System.Windows.Forms.Panel slidePanel;
        private PoisonComboBox workModeComboBox;
        private int currentHeight = 0;
        private const int PANEL_HEIGHT = 120; 
        private const int PANEL_WIDTH = 300;
        private bool isPanelExpanded = false;
        private bool isTracking = false;
        private System.Windows.Forms.Timer animationTimer;
        TimeSpan TOTAL_WORK_HOURS = TimeSpan.FromHours(8);
        private TimeSpan currentProjectTotalTime = TimeSpan.Zero;
        private string currentWorkMode = "WFO";
        private int XPosition = 20;
        public string CurrentWorkMode 
        {
            get { return currentWorkMode; }
            private set { currentWorkMode = value; }
        }
        private bool isMinimizedToTray = false;
        private LoaderControl _loaderControl;
        private bool isSavingInProgress = false;

        public TrackerForm(
            ITokenManager tokenManager
            ,IUserService userService 
            ,INavigationService navigationService
            ,ISuggestionService suggestionService
            )
        {

            _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _suggestionService = suggestionService ?? throw new ArgumentNullException(nameof(suggestionService));

            InitializeSvgLoader();

            SystemEvents.SessionEnding += OnSessionEnding;
            Application.ApplicationExit += Application_ApplicationExit;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;


            System.Windows.Forms.Panel innerPanel = new System.Windows.Forms.Panel
            {
                BackColor = Color.White,
                Location = new Point(1, 64), 
                Size = new Size(300 - 2, 550 - 2),
                Dock = DockStyle.None
            };
            this.Controls.Add(innerPanel);

            this.Size = new Size(300, 550);
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;
            this.StartPosition = FormStartPosition.CenterScreen;
            //this.FormClosing += TrackerForm_FormClosing;

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new MaterialColorScheme(
                Color.White,
                Color.FromArgb(33, 150, 243),
                Color.FromArgb(79, 195, 247),
                Color.Black,
                MaterialTextShade.WHITE
            );


            LoadAppIcon();
            SetupLogo();
            InitializeMenu();
            InitializeComponent();
            InitializeNotifyIcon();
            InitializeAsync(innerPanel);
            InitializeActivityTracking();
            InitializeHooks();
            CustomizeButtonAppearance();
            TrackingNotStarted();
        }

        private void LoadAppIcon()
        {
            string timerFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Icons", "NatrixLogo.ico");
            if (File.Exists(timerFilePath))
            {
                this.Icon = new Icon(timerFilePath);
            }
        }
        private void InitializeSvgLoader()
        {

            _loaderControl = new LoaderControl
            {
                Size = new Size(300, 550),
                Location = new Point(0, 0)
            };

            this.Controls.Add(_loaderControl);
            _loaderControl.BringToFront();
            // Bring loader to front if needed
            _loaderControl.StartLoader();
        }
        private void SetupLogo()
        {
            titleLogoBox = new PictureBox();
            titleLogoBox.Size = new Size(100, 30);
            titleLogoBox.Location = new Point(20, 30);
            titleLogoBox.SizeMode = PictureBoxSizeMode.Zoom;
            string titleLogoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", "Nstracker-logo.png");
            if (File.Exists(titleLogoPath))
            {
                titleLogoBox.Image = Image.FromFile(titleLogoPath);
            }

            Controls.Add(titleLogoBox);
        }
        private void InitializeNotifyIcon()
        {
            string timerFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Icons", "NatrixLogo.ico");

            notifyIcon = new NotifyIcon
            {
                Icon = new Icon(timerFilePath),
                Visible = true,
                Text = "NSTracker"
            };

            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem openItem = new ToolStripMenuItem("Open", null, OpenItem_Click);
            contextMenu.Items.Add(openItem);

            ToolStripMenuItem logoutItem = new ToolStripMenuItem("Logout", null, LogoutItem_Click);
            contextMenu.Items.Add(logoutItem);

            ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit", null, ExitItem_ClickAsync);
            contextMenu.Items.Add(exitItem);

            notifyIcon.ContextMenuStrip = contextMenu;
        }
        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            OpenApplication();
        }
        private void OpenItem_Click(object sender, EventArgs e)
        {
            OpenApplication();
        }
        private async void LogoutItem_Click(object sender, EventArgs e)
        {
            await HandleAutoSaveDPRSEntry();
            PerformLogout();
        }
        private async void ExitItem_ClickAsync(object sender, EventArgs e)
        {
            var result = await HandleAutoSaveDPRSEntry();

            if (!result)
            {
                return;
            }
            else
            {
                if (notifyIcon != null)
                {
                    notifyIcon.Visible = false;
                    notifyIcon.Dispose();
                }
                Application.Exit();
            }
        }
        private void OpenApplication()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate(); // Bring it to the front
            isMinimizedToTray = false;
        }
        private async void PerformLogout()
        {
            try
            {
                await _tokenManager.ClearTokenAsync();
                _navigationService.ShowLoginForm();
                this.Close();
            }
            catch (Exception ex)
            {
                AutoClosingMessageBox.Show(
                   $"Error initializing form: {ex.Message}",
                 "Initialization Error",
                    MessageBoxButtons.OK,
                   MessageBoxIcon.Error,
                  ButtonsPosition.Center,
                   2000
               );
            }
        }
        private void InitializeHooks()
        {
            globalHook = new GlobalHook();
            globalHook.KeyPressed += GlobalHook_KeyPressed;
            globalHook.MouseClicked += GlobalHook_MouseClicked;
        }
        private async void InitializeAsync(Control container)
        {
            try
            {
                await System.Threading.Tasks.Task.Delay(500);
                _loaderControl.StopLoader();

                //_svgLoader.HideLoader();
                //_svgLoader.Visible = false;
                InitializeSlider();
                InitializePieChart(container);
                await InitializeCustomComponents(container);
            }
            catch (Exception ex)
            {
                MaterialMessageBox.Show(
                   text: $"Error initializing form: {ex.Message}",
                   caption: "Initialization Error",
                   buttons: MessageBoxButtons.OK,
                   icon: MessageBoxIcon.Error,
                   buttonsPosition: MaterialFlexibleForm.ButtonsPosition.Center
               );
            }
        }
        private void InitializeSlider()
        {
            slidePanel = new System.Windows.Forms.Panel()
            {
                Size = new Size(PANEL_WIDTH, 0),
                Location = new Point(260 - (PANEL_WIDTH / 2), 65),
                BackColor = Color.White,
                Visible = true,
            };
            Controls.Add(slidePanel);

            InitializeSliderControls();
        }
        private void SliderCollapse_Click(object sender, EventArgs e)
        {
            if (isPanelExpanded)
            {
                CollapseSlider(); 
            }
        }
        private void InitializeSliderControls()
        {
            var workModeLabel = new ReaLTaiizor.Controls.DungeonLabel()
            {
                Text = "Work Mode",
                Location = new Point(10, 15),
                Size = new Size(120, 20),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 11, FontStyle.Regular)
            };

            workModeComboBox = new ReaLTaiizor.Controls.PoisonComboBox()
            {
                Location = new Point(10, 35),
                Size = new Size(170, 30),
                ForeColor = Color.White,
                DrawMode = DrawMode.OwnerDrawFixed
            };
            workModeComboBox.Items.AddRange(new object[] { "WFO", "WFH", "Hybrid" });
            workModeComboBox.SelectedIndex = 0;

            workModeComboBox.SelectedIndexChanged += WorkMode_SelectedIndexChanged;

            // Logout Button
            var logoutButton = new ReaLTaiizor.Controls.LostCancelButton()
            {
                Text = "Logout",
                Location = new Point(10, 75),
                Size = new Size(60, 30),
                BackColor = Color.FromArgb(255, 44, 164, 239), // Set blue color
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            logoutButton.Click += LogoutButton_Click;

            // Cancel Button
            var cancelButton = new Button()
            {
                Text = "Cancel",
                Location = new Point(120, 75), // Adjust location to align side by side
                Size = new Size(60, 30),
                BackColor = Color.Black,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            cancelButton.FlatStyle = FlatStyle.Flat;
            //cancelButton.FlatAppearance.BorderColor = Color.FromArgb(33, 150, 243);
            cancelButton.Click += (s, e) => CollapseSlider();

            slidePanel.Controls.AddRange(new Control[] {
        workModeLabel,
        workModeComboBox,
        logoutButton,
        cancelButton
    });
        }
        private void WorkMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (workModeComboBox.SelectedItem != null)
            {
                CurrentWorkMode = workModeComboBox.SelectedItem.ToString();
            }
        }
        private void ProfilePicture_Click(object sender, EventArgs e)
        {
            if (!isPanelExpanded)
            {
                ExpandSlider();
            }
            else
            {
                CollapseSlider();
            }
        }
        private async Task InitializeCustomComponents(Control container)
        {
            divider = new MaterialDivider();
            divider.Location = new Point(0, 1);
            divider.Size = new Size(300, 1);
            divider.BackColor = Color.FromArgb(79, 195, 247);
            container.Controls.Add(divider);

            todayWorkedHours = new Label
            {
                Font = new Font("Verdana", 12, FontStyle.Bold),
                ForeColor = Color.Black,
                Size = new Size(250, 20),
                Location = new Point(40, 10), 
                Text = $"Today's Hours: {0}H : {0}M",
                TextAlign = ContentAlignment.MiddleLeft
            };
            container.Controls.Add(todayWorkedHours);

            InitializeTimerPanel(container);
            //InitializeMenu();

            InitializeTrackedHoursTextBox(container);

            addDescriptionButton = new Button
            {
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Text = "+ Description",
                Location = new Point(155, 420),
                Size = new Size(125, 40),
                ForeColor = Color.White
            };
            addDescriptionButton.BackColor = Color.Black;
            addDescriptionButton.FlatStyle = FlatStyle.Flat;
            addDescriptionButton.FlatAppearance.BorderColor = Color.Black;

            addDescriptionButton.Click += AddDescriptionButton_Click;
            container.Controls.Add(addDescriptionButton);

            addDPRS = new Button
            {
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Text = "Save",
                Location = new Point(XPosition, 420),
                Size = new Size(80, 40),
                ForeColor = Color.White
            };
            addDPRS.BackColor = Color.FromArgb(33, 150, 243);
            addDPRS.FlatStyle = FlatStyle.Flat;
            addDPRS.FlatAppearance.BorderColor = Color.FromArgb(33, 150, 243);
            addDPRS.Click += EndButton_Click;
            container.Controls.Add(addDPRS);

            projectComboBox = new MaterialComboBox
            {
                Location = new Point(XPosition, 290),
                Size = new Size(260, 20),
                DrawMode = DrawMode.OwnerDrawVariable
            };
            container.Controls.Add(projectComboBox);
            projectComboBox.TabIndex = 4;

            projectComboBox.SelectedIndexChanged += projectComboBox_SelectedIndexChanged;

            await PopulateProjectComboBox();

            if (projectComboBox.SelectedItem != null)
            {
                var selectedProject = (UserProject)projectComboBox.SelectedItem;
                trackedHoursTextBox.Enabled = selectedProject.IsTrackable;
            }

        }
        private void InitializeTrackedHoursTextBox(Control container)
        {
            trackedHoursTextBox = new MaterialTextBoxEdit
            {
                Location = new Point(XPosition, 350),
                Size = new Size(260, 30),
                Text = "00 : 00",
                Hint = "Tracked Hours",
                LeadingIcon = null,
                UseAccent = true,
                Enabled = false,
                PrefixSuffixText = "HH : MM"
            };

            trackedHoursTextBox.KeyPress += (sender, e) =>
            {
                if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                {
                    e.Handled = true;
                }
            };

            trackedHoursTextBox.TextChanged += (sender, e) =>
            {
                string digits = new string(trackedHoursTextBox.Text.Where(char.IsDigit).ToArray());

                if (digits.Length > 4)
                {
                    digits = digits.Substring(0, 4);
                }

                string formattedText = "";
                if (digits.Length > 0)
                {
                    digits = digits.PadRight(4, '0');

                    int hours = int.Parse(digits.Substring(0, 2));
                    if (hours > 23)
                    {
                        digits = "23" + digits.Substring(2);
                    }

                    int minutes = int.Parse(digits.Substring(2, 2));
                    if (minutes > 59)
                    {
                        digits = digits.Substring(0, 2) + "59";
                    }

                    formattedText = $"{digits.Substring(0, 2)} : {digits.Substring(2, 2)}";
                }

                if (trackedHoursTextBox.Text != formattedText)
                {
                    int selectionStart = trackedHoursTextBox.SelectionStart;
                    trackedHoursTextBox.Text = formattedText;
                    trackedHoursTextBox.SelectionStart = Math.Min(selectionStart, formattedText.Length);
                }
            };

            container.Controls.Add(trackedHoursTextBox);
        }
        private void InitializePieChart(Control container)
        {
            int totalSeconds = (int)TOTAL_WORK_HOURS.TotalSeconds;

            pieChart = new PieChart
            {
                Series = new List<int> { totalSeconds }.AsPieSeries((value, series) =>
                {
                    series.Fill = new SolidColorPaint(REMAINING_HOURS_COLOR);
                    series.InnerRadius = 60;
                    series.ToolTipLabelFormatter = (chartPoint) =>
                    {
                        TimeSpan duration = TimeSpan.FromSeconds(value);
                        return $"Remaining Hours: {duration:hh\\:mm\\:ss}";
                    };
                    series.HoverPushout = 0;
                }),
                Location = new System.Drawing.Point(60, 35),
                Size = new System.Drawing.Size(180, 170),

            };
            container.Controls.Add(pieChart);
        }
        private void InitializeTimerPanel(Control container)
        {

            timer = new System.Windows.Forms.Timer { Interval = 1000 };
            timer.Tick += Timer_Tick;
            timeLabel = new Label
            {
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.Black,
                Size = new Size(100, 30),
                Location = new Point(100, 205),
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "00:00:00"
            };
            container.Controls.Add(timeLabel);

            

            toggleButton = new Button();
            toggleButton.Text = "START";
            toggleButton.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            toggleButton.Location = new Point(90, 240);
            toggleButton.Size = new Size(120, 40);
            toggleButton.BackColor = Color.FromArgb(33, 150, 243);
            toggleButton.FlatStyle = FlatStyle.Flat;
            toggleButton.FlatAppearance.BorderColor = Color.FromArgb(33, 150, 243);

            toggleButton.Click += ToggleButton_Click;
            container.Controls.Add(toggleButton);
        }
        private async void InitializeMenu()
        {
            userProfilePicture = new PictureBox();
            userNameLabel = new Label();

            userProfilePicture = new PictureBox
            {
                Size = new Size(35, 35),
                Location = new Point(260, 25),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Cursor = Cursors.Hand 
            };


            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddEllipse(0, 0, userProfilePicture.Width, userProfilePicture.Height);
            userProfilePicture.Region = new Region(gp);

            userProfilePicture.Click += ProfilePicture_Click;
            userProfilePicture.MouseEnter += (s, e) => userProfilePicture.BackColor = Color.LightGray; 
            userProfilePicture.MouseLeave += (s, e) => userProfilePicture.BackColor = Color.Transparent;

            userNameLabel.Font = new Font("inter", 8);
            userNameLabel.ForeColor = Color.White;
            //userNameLabel.Location = new Point(150, 450);
            //userNameLabel.AutoSize = true;
            userNameLabel.TextAlign = ContentAlignment.MiddleLeft;

            await LoadUserProfile();
           // this.Controls.Add(userNameLabel);
            this.Controls.Add(userProfilePicture);


        }
        private void ExpandSlider()
        {
            if (slidePanel == null) return;

            isPanelExpanded = true;
            slidePanel.BringToFront();
            currentHeight = 0;

            animationTimer = new System.Windows.Forms.Timer { Interval = 1 };
            animationTimer.Tick += (s, e) => {
                if (currentHeight < PANEL_HEIGHT)
                {
                    currentHeight += 8; 
                    slidePanel.Size = new Size(PANEL_WIDTH, currentHeight);
                }
                else
                {
                    animationTimer.Stop();
                }
            };
            animationTimer.Start();
            slidePanel.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.SkyBlue, 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, 190 - 1, 120 - 1);
                }
            };
        }
        private void CollapseSlider()
        {
            isPanelExpanded = false;
            animationTimer = new System.Windows.Forms.Timer { Interval = 1 };
            animationTimer.Tick += (s, e) => {
                if (currentHeight > 0)
                {
                    currentHeight -= 8; 
                    slidePanel.Size = new Size(PANEL_WIDTH, currentHeight);
                }
                else
                {
                    animationTimer.Stop();
                }
            };
            animationTimer.Start();
        }
        private async Task LoadUserProfile()
        {
            try
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string imagePath = Path.Combine(appDirectory, "Assets", "Images", "profile.png");

                var userDetails = await GetUserDetails();
                if (userDetails != null)
                {
                    userNameLabel.Text = userDetails.UserName ?? userDetails.UserName;
                    string profileUrl = Endpoint.ProfileImage + $"{userDetails.MemberId}.jpg";

                    try
                    {
                        using (var client = new HttpClient())
                        {
                            var response = await client.GetAsync(profileUrl);
                            if (response.IsSuccessStatusCode)
                            {
                                using (var stream = await response.Content.ReadAsStreamAsync())
                                {
                                    userProfilePicture.Image = Image.FromStream(stream);
                                }
                            }
                            else
                            {
                                userProfilePicture.Image = Image.FromFile(imagePath);
                            }
                        }
                    }
                    catch
                    {
                        userProfilePicture.Image = Image.FromFile(imagePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user profile: {ex.Message}");
            }
        }
        private void CustomizeButtonAppearance()
        {
            if (toggleButton != null && toggleButton is Button materialToggleButton)
            {
                if (isTracking)
                {
                    materialToggleButton.BackColor = Color.Red; 
                    materialToggleButton.ForeColor = Color.White;
                    materialToggleButton.Text = "STOP";
                    materialToggleButton.FlatStyle = FlatStyle.Flat;
                    materialToggleButton.FlatAppearance.BorderColor = Color.Red;
                }
                else
                {
                    materialToggleButton.BackColor = Color.FromArgb(33, 150, 243);
                    materialToggleButton.ForeColor = Color.White;
                    materialToggleButton.Text = "START";
                    materialToggleButton.FlatStyle = FlatStyle.Flat;
                    materialToggleButton.FlatAppearance.BorderColor = Color.FromArgb(33, 150, 243);
                }
            }
        }
        private void InitializeActivityTracking()
        {
            InitializeActivityLog();
            lastSaveTime = DateTime.Now;

            System.Windows.Forms.Timer activitySaveTimer = new System.Windows.Forms.Timer
            {
                Interval = 30 * 60 * 1000 // 30 minute in milliseconds
            };
            activitySaveTimer.Tick += ActivitySaveTimer_Tick;
            activitySaveTimer.Start();
        }
        private void InitializeActivityLog()
        {
                if (File.Exists(jsonFilePath))
                {
                    try
                    {
                        string jsonContent = File.ReadAllText(jsonFilePath);
                        currentLog = System.Text.Json.JsonSerializer.Deserialize<ActivityLog>(jsonContent);

                        if (currentLog == null)
                        {
                            CreateNewActivityLog();
                        }
                    }
                    catch (System.Text.Json.JsonException)
                    {
                        CreateNewActivityLog();
                    }
                }
                else
                {
                    CreateNewActivityLog();
                }           
        }
        private DateTime GetIndianStandardTime()
        {
            DateTime utcNow = DateTime.UtcNow;
            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, istZone);
        }
        private void CreateNewActivityLog()
        {
            try
            {
                currentLog = new ActivityLog
                {
                    WorkedDate = GetIndianStandardTime().ToString(),
                    Activity = new List<ActivityEntry>()
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = System.Text.Json.JsonSerializer.Serialize(currentLog, options);

                using (FileStream fs = new FileStream(jsonFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(jsonString);
                }
            }
            catch (Exception ex)
            {
                MaterialMessageBox.Show(
                   text: $"Error creating activity log: {ex.Message}",
                   caption: "Error",
                   buttons: MessageBoxButtons.OK,
                   icon: MessageBoxIcon.Error,
                   buttonsPosition: MaterialFlexibleForm.ButtonsPosition.Center
               );
            }
        }
        private void ActivitySaveTimer_Tick(object sender, EventArgs e)
        {
            SaveCurrentActivity();
        }
        private void SaveCurrentActivity()
        {
            if (isTracking) 
            {
                string timeRange = $"{lastSaveTime:HH:mm} - {DateTime.Now:HH:mm}";

                var entry = new ActivityEntry
                {
                    Time = timeRange,
                    MouseClicks = mouseClickCount,
                    KeyboardClicks = keyboardClickCount
                };

                if (currentLog != null)
                {
                    string today = GetIndianStandardTime().ToString();
                    if (currentLog.WorkedDate != today)
                    {
                        currentLog.WorkedDate = today; // Update to today's date
                    }

                    currentLog.Activity.Add(entry);
                    SaveActivityLog();
                    mouseClickCount = 0;
                    keyboardClickCount = 0;
                    lastSaveTime = DateTime.Now;
                }
            }
        }
        private void SaveActivity_IfTrackerStopped()
        {
            if (!isTracking)
            {
                string timeRange = $"{lastSaveTime:HH:mm} - {DateTime.Now:HH:mm}";

                var entry = new ActivityEntry
                {
                    Time = timeRange,
                    MouseClicks = mouseClickCount,
                    KeyboardClicks = keyboardClickCount
                };

                if (currentLog != null)
                {
                    string today = GetIndianStandardTime().ToString();
                    if (currentLog.WorkedDate != today)
                    {
                        currentLog.WorkedDate = today; // Update to today's date
                    }

                    currentLog.Activity.Add(entry);
                    SaveActivityLog();
                    mouseClickCount = 0;
                    keyboardClickCount = 0;
                    lastSaveTime = DateTime.Now;
                }
            }
        }
        private void SaveActivityLog()
        {
            string activityJsonPath = jsonFilePath;
            if (!File.Exists(activityJsonPath))
            {
                File.Create(activityJsonPath);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string jsonString = System.Text.Json.JsonSerializer.Serialize(currentLog, options);

            using (FileStream fs = new FileStream(jsonFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(jsonString);
            }
        }
        private void GlobalHook_KeyPressed(object sender, EventArgs e)
        {
            if (isTracking)
            {
                keyboardClickCount++;
            }
        }
        private void GlobalHook_MouseClicked(object sender, EventArgs e)
        {
            if (isTracking)
            {
                mouseClickCount++;
            }
        }
        private async void EndButton_Click(object sender, EventArgs e)
        {
            if(sessionData.Count() > 0 || isTracking)
            {
                timer.Stop();
                if (isTracking)
                {
                    UpdateSessions();
                }
                isTracking = false;
                SaveActivity_IfTrackerStopped();

                await AddDprs();
                Clear();
                currentProjectTotalTime = TimeSpan.Zero;
                elapsedTime = TimeSpan.Zero;
                timeLabel.Text = "00:00:00";
                InitializeActivityLog();
                CreateDesciptionFiles();
                CustomizeButtonAppearance();
                UpdateTimeDisplay();
                UpdatePieChart();
                this.Invalidate();
            }

            else
            {
                //ReaLTaiizor.Controls.MaterialMessageBox.Show("Please start tracking", MessageBoxIcon.Warning.ToString(),);

                //MaterialMessageBox.Show(
                //       text: "Please start tracking!",
                //       caption: "Tracker",
                //       buttons: MessageBoxButtons.OK,
                //       icon: MessageBoxIcon.Warning,
                //       buttonsPosition: MaterialFlexibleForm.ButtonsPosition.Center
                //   );

                CustomMessageBox.Show("Please start tracking!", "Tracker Warning", MessageBoxButtons.OK, MessageBoxType.Warning);

            }


        }
        private string UpdateSelectedProject()
        {
            var selectedProject = projectComboBox.SelectedItem as UserProject; 
            string selectedProjectName = selectedProject?.ProjectName ?? "Unknown Project";
            return selectedProjectName;
        }
        private void CreateDesciptionFiles()
        {
            string autoSavePath = Path.Combine(Path.GetTempPath(), "richtext_autosave.xml");
            string savePath = Path.Combine(Path.GetTempPath(), "richtext_content.txt");

            if(!File.Exists(autoSavePath))
                File.Create(autoSavePath).Dispose();

            if (!File.Exists(savePath))
                File.Create(savePath).Dispose();
        }
        private (int, int) GetSelectedProjectId()
        {
            if (projectComboBox.SelectedItem is UserProject selectedProject)
            {
                return (selectedProject.Id, selectedProject.ManageByMemberId);
            }
            return (-1, -1);
        }
        private void projectComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (projectComboBox.SelectedItem != null)
            {
                var selectedProject = (UserProject)projectComboBox.SelectedItem;

                trackedHoursTextBox.Enabled = selectedProject.IsTrackable;
            }
        }
        private async Task AddDprs()
        {
            try
            {
                string activityJsonPath = jsonFilePath;
                string jsonContent = File.ReadAllText(activityJsonPath);

                JToken jsonData = JToken.Parse(jsonContent);
                string activityData = Newtonsoft.Json.JsonConvert.SerializeObject(jsonData, Formatting.Indented);

                string autoSavePath = Path.Combine(Path.GetTempPath(), "richtext_content.txt");
                string stringContent = "";
                if (File.Exists(autoSavePath))
                {
                    stringContent = File.ReadAllText(autoSavePath);
                }

                var tokenDetails = await _tokenManager.GetTokenDetailsAsync();
                var (projectId, managerId) = GetSelectedProjectId();
                DateTime workedDateTime = DateTime.UtcNow;
                string workMode = CurrentWorkMode;

                string trackedHours = $"{trackedHoursTextBox.Text.Trim().Replace(" ", "")}:00";

                var request = new AddDPRSRequestModel()
                {
                    Id = 0,
                    ProjectID = projectId,
                    MemberID = tokenDetails.MemberId,
                    TrackedHours = trackedHours,
                    ManagementSupportHours = "00:00:00",
                    WorkedHours = currentProjectTotalTime.ToString(@"hh\:mm\:ss"),
                    Summary = stringContent, 
                    WorkedDate = workedDateTime.Date,
                    _strWorkedDate = workedDateTime.Date,
                    ManagerId = managerId,
                    KeyCountJSON = activityData,
                    WorkMode = workMode
                };

                bool result = await _userService.ManageDPRS(request);
                if (!result)
                {
                    //MaterialMessageBox.Show(
                    //    text: "DPRS entry failed!",
                    //    caption: "Failed",
                    //    buttons: MessageBoxButtons.OK,
                    //    icon: MessageBoxIcon.Error,
                    //    buttonsPosition: MaterialFlexibleForm.ButtonsPosition.Center
                    //);
                    CustomMessageBox.Show("DPRS entry failed!", "DPRS Entry", MessageBoxButtons.OK, MessageBoxType.Error);

                    return;
                }

                timeLabel.Text = "00:00:00";

                //MaterialMessageBox.Show(
                //       text: "DPRS entry added successfully!",
                //       caption: "Success",
                //       buttons: MessageBoxButtons.OK,
                //       icon: MessageBoxIcon.Information,
                //       buttonsPosition: MaterialFlexibleForm.ButtonsPosition.Center
                //   );

                CustomMessageBox.Show("DPRS entry added successfully!", "DPRS Entry", MessageBoxButtons.OK, null);

            }
            catch (Exception ex)
            {
                MaterialMessageBox.Show(
                       text: $"Error adding DPRS entry: {ex.Message}",
                       caption: "Error",
                       buttons: MessageBoxButtons.OK,
                       icon: MessageBoxIcon.Error,
                       buttonsPosition: MaterialFlexibleForm.ButtonsPosition.Center
                   );
            }
        }
        private class PieChartData
        {
            public int Value { get; set; }
            public SKColor Color { get; set; }
            public string Label { get; set; }
            public string ProjectName { get; set; }
        }
        private void UpdatePieChart()
        {
            try
            {
                List<PieChartData> chartData = new List<PieChartData>();

                // Add all tracked sessions
                foreach (var session in sessionData)
                {
                    chartData.Add(new PieChartData
                    {
                        Value = session.Duration,
                        Color = session.Color,
                        Label = $"{session.ProjectName.Trim()}\r\nWorked Hours: {TimeSpan.FromSeconds(session.Duration):hh\\:mm\\:ss}"
                    });
                }

                // Only add remaining time if total tracked time is less than TOTAL_WORK_HOURS
                int totalTrackedSeconds = sessionData.Sum(s => s.Duration);
                int remainingSeconds = (int)TOTAL_WORK_HOURS.TotalSeconds - totalTrackedSeconds;

                if (remainingSeconds > 0)
                {
                    chartData.Add(new PieChartData
                    {
                        Value = remainingSeconds,
                        Color = REMAINING_HOURS_COLOR,
                        Label = $"Remaining: {TimeSpan.FromSeconds(remainingSeconds):hh\\:mm\\:ss}"
                    });
                }

                // Update pie chart
                pieChart.Series = chartData.Select(data => data.Value).AsPieSeries((value, series) =>
                {
                    var data = chartData.FirstOrDefault(d => d.Value == value);
                    if (data != null)
                    {
                        series.Fill = new SolidColorPaint(data.Color);
                        series.InnerRadius = 60;
                        series.ToolTipLabelFormatter = (chartPoint) => data.Label;
                        series.HoverPushout = 0;
                    }
                });

                pieChart.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating pie chart: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void LogoutButton_Click(object sender, EventArgs e)
        {
            MessageBoxResult result = CustomMessageBox.Show("Are you sure you want to log out?", "Logout NSTracker", MessageBoxButtons.YesNo, MessageBoxType.Yes_No);

            if (result == MessageBoxResult.Yes)
            {
                Clear();
                await _tokenManager.ClearTokenAsync();
                _navigationService.ShowLoginForm();
                this.Close();

                if (notifyIcon != null)
                {
                    notifyIcon.Visible = false;
                    notifyIcon.Dispose();
                }
            }
        }
        private void ToggleButton_Click(object sender, EventArgs e)
        {
            if (!isTracking)
            {
                startTime = DateTime.Now;
                isTracking = true;
                timer.Start();
                InitializeActivityLog();
            }

            else
            {
                timer.Stop();
                isTracking = false;

                UpdateSessions();
                SaveActivity_IfTrackerStopped();
            }

            CustomizeButtonAppearance();
            UpdateTimeDisplay();
            UpdatePieChart();
            this.Invalidate();
        }
        private void UpdateSessions()
        {
            TimeSpan sessionDuration = DateTime.Now - startTime;
            totalTrackedTime += sessionDuration;
            currentProjectTotalTime += sessionDuration;

            SKColor sessionColor = new SKColor(
                (byte)colorGenerator.Next(256),
                (byte)colorGenerator.Next(256),
                (byte)colorGenerator.Next(256)
            );

            int sessionSeconds = (int)sessionDuration.TotalSeconds;
            if (sessionSeconds == 0) sessionSeconds = 1;

            string projectName = UpdateSelectedProject();
            sessionData.Add((sessionSeconds, sessionColor, projectName));
        }
        private async Task PopulateProjectComboBox()
        {
            try
            {
                projectComboBox.Items.Clear();
                var projects = await GetProjectItems();
                var recentProjects = await _userService.GetDPRSEntry();

                if (projects != null && projects.Length > 0)
                {
                    projectComboBox.DisplayMember = "ProjectName";
                    projectComboBox.ValueMember = "Id";
                    projectComboBox.DataSource = projects;
                    projectComboBox.Enabled = true;

                    if (recentProjects != null && recentProjects.Count > 0)
                    {
                        var maxWorkedProject = recentProjects
                            .OrderByDescending(p =>
                            {
                                decimal hours = 0;
                                decimal.TryParse(p.WorkedHours, out hours);
                                return hours;
                            })
                            .FirstOrDefault();

                        if (maxWorkedProject != null)
                        {
                            var matchingProjectIndex = -1;
                            for (int i = 0; i < projects.Length; i++)
                            {
                                if (projects[i].ProjectName.ToLower() == maxWorkedProject.ProjectName.ToLower())
                                {
                                    matchingProjectIndex = i;
                                    break;
                                }
                            }

                            if (matchingProjectIndex >= 0)
                            {
                                projectComboBox.SelectedIndex = matchingProjectIndex;
                            }
                        }
                    }
                }
                else
                {
                    projectComboBox.Items.Add("No projects available");
                    projectComboBox.SelectedIndex = 0;
                    projectComboBox.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading projects: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                projectComboBox.Items.Add("Error loading projects");
                projectComboBox.SelectedIndex = 0;
                projectComboBox.Enabled = false;
            }
        }
        private async Task<UserProject[]> GetProjectItems()
        {
            var userProjects = await _userService.GetUserProjects();
            if (userProjects == null || !userProjects.Any())
            {
                return Array.Empty<UserProject>();
            }

            return userProjects
                .Where(p => p != null && !string.IsNullOrEmpty(p.ProjectName))
                .ToArray();
        }
        private async Task<Login> GetUserDetails()
        {
            return await _tokenManager.GetTokenDetailsAsync();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            elapsedTime = DateTime.Now - startTime;
            UpdateTimeDisplay();
        }
        private void UpdateTimeDisplay()
        {
            if (timeLabel != null && !IsDisposed && timeLabel.IsHandleCreated)
            {
                string formattedTime = $"Today's Hours: {(int)totalTrackedTime.TotalHours}H : {totalTrackedTime.Minutes}M";

                if (timeLabel.InvokeRequired)
                {
                    timeLabel.Invoke(new Action(() => timeLabel.Text = elapsedTime.ToString(@"hh\:mm\:ss")));
                    todayWorkedHours.Invoke(new Action(() => todayWorkedHours.Text = formattedTime));
                }
                else
                {
                    timeLabel.Text = elapsedTime.ToString(@"hh\:mm\:ss");
                    todayWorkedHours.Text = formattedTime;
                }
            }
        }
        private void AddDescriptionButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new CustomModalDialog())
            {
                dialog.Owner = this;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    
                }
            }
        }
        private void TrackingNotStarted()
        {
            _suggestionService.TrackingNotStarted(isTracking);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen borderPen = new Pen(Color.SkyBlue, 1))
            {
                Rectangle rect = new Rectangle(0, 0, this.ClientSize.Width - 1, this.ClientSize.Height - 1);
                e.Graphics.DrawRectangle(borderPen, rect);
            }

        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; 
                this.Hide();
                return;
            }
            if (e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.TaskManagerClosing)
            {
                SaveDataBeforeShutdown();
            }
            base.OnFormClosing(e);
        } 

        // Save data if app crashes or end program or close app forcefully.
        private void SaveDataBeforeShutdown()
        {
            try
            {
                SaveDataAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save data: {ex.Message}");
            }
        }
        private async Task<bool> HandleAutoSaveDPRSEntry()
        {
            if (isSavingInProgress)
                return false;

            isSavingInProgress = true;

            try
            {
                if (isTracking || currentProjectTotalTime > TimeSpan.Zero)
                {
                    await AddDprs();
                    Clear();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during auto-save: {ex.Message}");
                return false;
            }
            finally
            {
                isSavingInProgress = false; // Reset flag after operation
            }
        }
        private async void Application_ApplicationExit(object sender, EventArgs e)
        {
            await HandleAutoSaveDPRSEntry();
        }
        private async void OnSessionEnding(object sender, SessionEndingEventArgs e)
        {
            await HandleAutoSaveDPRSEntry();
        }
        private async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            await HandleAutoSaveDPRSEntry();
        }
        private async Task SaveDataAsync()
        {
            await HandleAutoSaveDPRSEntry();
            await Task.Delay(2000); // Simulate delay
            Console.WriteLine("Data saved successfully.");
        }
        private void Clear()
        {
            string autoSavePath = Path.Combine(Path.GetTempPath(), "richtext_autosave.xml");
            string descriptionPath = Path.Combine(Path.GetTempPath(), "richtext_content.text");
            string activityPath = jsonFilePath;
            if (File.Exists(autoSavePath)) 
            {
                File.SetAttributes(autoSavePath, FileAttributes.Normal);
                File.Delete(autoSavePath);
            }
            if (File.Exists(descriptionPath))
            {
                File.SetAttributes(descriptionPath, FileAttributes.Normal);
                File.Delete(descriptionPath);
            }
            if (File.Exists(activityPath))
            {
                File.SetAttributes(activityPath, FileAttributes.Normal);
                File.Delete(activityPath);
            }
        }
    }
}