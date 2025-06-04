using NatWinTracker.Services.DPRSService.Auth;
using NatWinTracker.Services.DPRSService.TokenManagement;
using NatWinTracker.Services.DPRSService.Users;
using NatWinTracker.Services.Utility;
using NSTracker.Common;
using NSTracker.Services.Navigation;
using ReaLTaiizor.Colors;
using ReaLTaiizor.Controls;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;
using ReaLTaiizor.Util;
using System.Text.RegularExpressions;

namespace NSTracker
{
    public partial class LoginForm : MaterialForm
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly INavigationService _navigationService;

        private Label titleLabel;
        private Label emailValidatorlabel;
        private Label passwordValidatorlabel;
        private MaterialButton loginButton;
        private MaterialTextBoxEdit emailTextBox;
        private MaterialTextBoxEdit passwordTextBox;
        private PictureBox logoBox;
        private PictureBox titleLogoBox;
        private MaterialLabel passwordToggleLabel;
        private MaterialDivider divider;
        private PictureBox passwordToggle;

        public LoginForm(
             IUserService userService
            , IAuthenticationService authenticationService
            , ITokenManager tokenManager
            , INavigationService navigationService
            )
        {
            InitializeComponent();

            _authenticationService = authenticationService;
            _navigationService = navigationService;

            System.Windows.Forms.Panel innerPanel = new System.Windows.Forms.Panel
            {
                BackColor = Color.White,
                Location = new Point(1, 64), 
                Size = new Size(this.ClientSize.Width - 2, this.ClientSize.Height - 100),
                Dock = DockStyle.None
            };
            this.Controls.Add(innerPanel);

            this.Size = new Size(300, 550);
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.FormClosed += MainForm_FormClosed;

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new MaterialColorScheme(
                Color.White,
                Color.FromArgb(33, 150, 243),
                Color.FromArgb(79, 195, 247),
                Color.FromArgb(33, 150, 243),
                MaterialTextShade.WHITE
            );

            string timerFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Icons", "NatrixLogo.ico");
            if (File.Exists(timerFilePath))
            {
                this.Icon = new Icon(timerFilePath);
            }
            StyleControls(innerPanel);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
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
        private void StyleControls(Control container)
        {
            SetupLogo(container);

            titleLabel = new Label();
            loginButton = new MaterialButton();
            emailTextBox = new MaterialTextBoxEdit();
            passwordTextBox = new MaterialTextBoxEdit();
            titleLogoBox = new PictureBox();
            logoBox = new PictureBox();
            divider = new MaterialDivider();
            var materialSkinManager = MaterialSkinManager.Instance;

            titleLabel.Text = "Welcome";
            titleLabel.Font = new Font("Roboto", 12, FontStyle.Bold);
            titleLabel.Location = new Point(100, 120);
            titleLabel.Size = new Size(100, 40);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;

            emailTextBox.Location = new Point(35, 180);
            emailTextBox.Size = new Size(220, 45);
            emailTextBox.UseAccent = true;
            emailTextBox.AnimateReadOnly = true;
            emailTextBox.Hint = "Enter your email *";
            emailTextBox.BackColor = Color.FromArgb(255, 255, 255);
            emailTextBox.BackColor = materialSkinManager.BackgroundColor;
            emailTextBox.ForeColor = Color.Black;

            emailValidatorlabel = new Label
            {
                AutoSize = true,
                Location = new Point(35, 230),
                ForeColor = Color.Red,
                Visible = false,
                Font = new Font("Roboto", 10, FontStyle.Regular),
                BackColor = Color.Transparent
            };

            // Add event handlers
            emailTextBox.TextChanged += EmailTextBox_TextChanged;
            emailTextBox.Leave += EmailTextBox_Leave;

            passwordTextBox.Location = new Point(35, 260);
            passwordTextBox.Size = new Size(220, 45);
            passwordTextBox.UseAccent = true;
            passwordTextBox.AnimateReadOnly = true;
            passwordTextBox.UseSystemPasswordChar = true;
            passwordTextBox.Hint = "Enter your password *";
            passwordTextBox.BackColor = Color.FromArgb(255, 255, 255); // Set background color to white
            passwordTextBox.ForeColor = Color.Black;

            SetupPasswordToggle();

            passwordValidatorlabel = new Label
            {
                AutoSize = true,
                Location = new Point(35, 310),
                ForeColor = Color.Red,
                Visible = false,
                Font = new Font("Roboto", 10, FontStyle.Regular),
                BackColor = Color.Transparent
            };
            passwordTextBox.Leave += PasswordTextBox_Leave;


            loginButton.Text = "    LOG IN    ";
            loginButton.Location = new Point(100, 350);
            loginButton.Size = new Size(220, 45);
            loginButton.Type = MaterialButton.MaterialButtonType.Contained;
            loginButton.UseAccentColor = true;
            loginButton.Click += LoginButton_Click;
            this.AcceptButton = loginButton;

            divider.Location = new Point(0, 1);
            divider.Size = new Size(300, 1);
            divider.BackColor = Color.FromArgb(79, 195, 247);

            container.Controls.AddRange(new Control[] {
                titleLabel,
                emailTextBox,
                passwordTextBox,
                loginButton,
                emailValidatorlabel,
                passwordValidatorlabel,
                divider,
                titleLogoBox
            });
        }
        private void PasswordToggleLabel_Click(object sender, EventArgs e)
        {
            passwordTextBox.UseSystemPasswordChar = !passwordTextBox.UseSystemPasswordChar;

            passwordToggleLabel.Text = passwordTextBox.UseSystemPasswordChar ? "👁️‍🗨️" : "👁️";
        }

        private void SetupLogo(Control container)
        {
            titleLogoBox = new PictureBox();
            titleLogoBox.Size = new Size(100, 30);
            titleLogoBox.Location = new Point(100, 30);
            titleLogoBox.SizeMode = PictureBoxSizeMode.Zoom;
            string titleLogoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", "Nstracker-logo.png");
            if (File.Exists(titleLogoPath))
            {
                titleLogoBox.Image = Image.FromFile(titleLogoPath);
            }

            Controls.Add(titleLogoBox);

            logoBox = new PictureBox();
            logoBox.Size = new Size(100, 100);
            logoBox.Location = new Point(100, 20);
            logoBox.SizeMode = PictureBoxSizeMode.Zoom;

            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Icons", "NatrixLogo.ico");
            if (File.Exists(logoPath))
            {
                logoBox.Image = Image.FromFile(logoPath);
            }

            container.Controls.Add(logoBox);
        }
        private async Task LoginButtonClickAsync(object sender, EventArgs e)
        {
            var email = emailTextBox.Text.Trim();
            var password = passwordTextBox.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                //MaterialMessageBox.Show(
                //    text: "Please enter both email and password.",
                //    caption: "Validation Error",
                //    buttons: MessageBoxButtons.OK,
                //    icon: MessageBoxIcon.Warning,
                //    buttonsPosition: MaterialFlexibleForm.ButtonsPosition.Center
                //);
                CustomMessageBox.Show("Please enter both email and password.", "Validation Error", MessageBoxButtons.OK, MessageBoxType.Warning);

                //AutoClosingMessageBox.Show(
                //    $"Please enter both email and password.",
                //  "Validation Error",
                //     MessageBoxButtons.OK,
                //    MessageBoxIcon.Warning,
                //   ButtonsPosition.Center,
                //    2000
                //);

                return;
            }

            var (response, isSuccess) = await _authenticationService.LoginAsync(email, password);
            if (response != null && isSuccess == true)
            {
                //MaterialMessageBox.Show(
                //    text: "Login successfully",
                //    caption: "Success",
                //    buttons: MessageBoxButtons.OK,
                //    icon: MessageBoxIcon.Information,
                //    buttonsPosition: MaterialFlexibleForm.ButtonsPosition.Center
                //);
                CustomMessageBox.Show("Login successfully.", "Login", MessageBoxButtons.OK, null);

                //AutoClosingMessageBox.Show(
                //   text: $"Login successful!",
                //   caption: "Success",
                //   timeout: 2000
                //);

                this.Hide();
                _navigationService.ShowTrackerForm();
            }
            else
            {
                //MaterialMessageBox.Show(
                //    text: "Invalid email or password. Please try again.",
                //    caption: "Login Failed",
                //    buttons: MessageBoxButtons.OK,
                //    icon: MessageBoxIcon.Error,
                //    buttonsPosition: MaterialFlexibleForm.ButtonsPosition.Center
                //);
                CustomMessageBox.Show("Invalid email or password. Please try again.", "Invalid Error", MessageBoxButtons.OK, MessageBoxType.Error);

                //AutoClosingMessageBox.Show(
                //   text: $"Invalid email or password. Please try again.",
                //   caption: "Login Failed",
                //   timeout: 2000
                //);
            }
        }
        private void EmailTextBox_TextChanged(object sender, EventArgs e)
        {
            string email = emailTextBox.Text;
            if (string.IsNullOrWhiteSpace(email))
            {
                emailValidatorlabel.Text = "Email is required";
                emailValidatorlabel.ForeColor = Color.Red;
                emailValidatorlabel.Visible = true;
            }
            else if (!IsValidEmail(email))
            {
                emailValidatorlabel.Text = "Invalid email format";
                emailValidatorlabel.ForeColor = Color.Red;
                emailValidatorlabel.Visible = true;
            }
            else
            {
                emailValidatorlabel.Visible = false;
            }
        }
        private void EmailTextBox_Leave(object sender, EventArgs e)
        {
            string email = emailTextBox.Text;
            if (string.IsNullOrWhiteSpace(email))
            {
                emailValidatorlabel.Text = "Email is required";
                emailValidatorlabel.Visible = true;
            }
            else if (!IsValidEmail(email))
            {
                emailValidatorlabel.Text = "Invalid email format";
                emailValidatorlabel.Visible = true;
            }
            else
            {
                emailValidatorlabel.Visible = false;
            }
        }
        private void PasswordTextBox_Leave(object sender, EventArgs e)
        {
            string password = passwordTextBox.Text;
            if (string.IsNullOrWhiteSpace(password))
            {
                passwordValidatorlabel.Text = "Password is required";
                passwordValidatorlabel.ForeColor = Color.Red;
                passwordValidatorlabel.Visible = true;
            }
            else
            {
                passwordValidatorlabel.Visible = false;
            }
        }
        private void SetupPasswordToggle()
        {
            passwordToggle = new PictureBox
            {
                Cursor = Cursors.Hand,
                Size = new Size(25, 25),
                Location = new Point(220, 340),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = passwordTextBox.BackColor, // Match the text box's background color
                BackgroundImageLayout = ImageLayout.Stretch // Ensure background fills entire picturebox
            };

            string eyeHidePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", "not-visible.png");
            string eyeShowPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", "visual.png");

            // Set initial state as hidden password
            try
            {
                passwordToggle.Image = File.Exists(eyeShowPath)
                    ? Image.FromFile(eyeShowPath)
                    : null;
            }
            catch (Exception ex)
            {
                // Handle potential file loading errors
                Console.WriteLine($"Error loading eye show image: {ex.Message}");
            }

            passwordToggle.Click += (s, e) =>
            {
                try
                {
                    if (passwordTextBox.UseSystemPasswordChar)
                    {
                        passwordTextBox.UseSystemPasswordChar = false;
                        passwordToggle.Image = File.Exists(eyeHidePath)
                            ? Image.FromFile(eyeHidePath)
                            : null;
                    }
                    else
                    {
                        passwordTextBox.UseSystemPasswordChar = true;
                        passwordToggle.Image = File.Exists(eyeShowPath)
                            ? Image.FromFile(eyeShowPath)
                            : null;
                    }
                }
                catch (Exception ex)
                {
                    // Handle potential file loading errors
                    Console.WriteLine($"Error toggling password visibility: {ex.Message}");
                }
            };

            Controls.Add(passwordToggle);
            passwordToggle.BringToFront();
        }
        private bool IsValidEmail(string email)
        {
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }
        private void LoginButton_Click(object sender, EventArgs e)
        {
            _ = LoginButtonClickAsync(sender, e);
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }
    }
}