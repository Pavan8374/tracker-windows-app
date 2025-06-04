using Tracker.Services.Utility;
using System.Drawing.Drawing2D;
using System.Timers;

namespace NSTracker.Common
{
    public class CustomMessageBox : Form
    {
       private Panel pnlContent;
        private PictureBox picLogo;
        private Label lblMessage;
        private List<Button> buttons = new List<Button>();
        private System.Timers.Timer autoCloseTimer;
        private MessageBoxResult userResult = MessageBoxResult.None;

        public CustomMessageBox(string message, string title, MessageBoxType? messageBoxType, MessageBoxButtons buttonConfig)
        {
            this.Text = title;
            this.Size = new Size(350, 200); 
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None; 
            this.BackColor = Color.White;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            pnlContent = new RoundedPanel();
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.BackColor = Color.White;
            this.Controls.Add(pnlContent);

            picLogo = new PictureBox();
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.Size = new Size(60, 60);
            picLogo.Location = new Point(150, 20);
            string titleLogoPath = GetImagePath(messageBoxType);
            if (File.Exists(titleLogoPath))
            {
                picLogo.Image = Image.FromFile(titleLogoPath);
            }
            pnlContent.Controls.Add(picLogo);

            lblMessage = new Label();
            lblMessage.Text = message;
            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            lblMessage.AutoSize = true;
            lblMessage.Font = new Font("Segoe UI", 11, FontStyle.Bold, GraphicsUnit.Point);
            lblMessage.ForeColor = Color.Black;

            using (Graphics g = lblMessage.CreateGraphics())
            {
                SizeF textSize = g.MeasureString(message, lblMessage.Font);
                int labelWidth = (int)Math.Ceiling(textSize.Width);
                int panelWidth = pnlContent.ClientSize.Width;

                int xPosition = Math.Max(10, (panelWidth - labelWidth) / 2);
                int yPosition = picLogo.Bottom + 10;

                lblMessage.Location = new Point(xPosition, yPosition);
            }
            pnlContent.Controls.Add(lblMessage);

            CreateButtons(buttonConfig, lblMessage.Bottom + 20);

            if(MessageBoxButtons.OK == buttonConfig)
            {
                autoCloseTimer = new System.Timers.Timer(2000);
                autoCloseTimer.Elapsed += AutoCloseTimer_Elapsed;
                autoCloseTimer.AutoReset = false;
                autoCloseTimer.Start();
            }

        }

        private void CreateButtons(MessageBoxButtons buttonConfig, int yPosition)
        {
            switch (buttonConfig)
            {
                case MessageBoxButtons.OK:
                    AddButton("OK", MessageBoxResult.OK, yPosition);
                    break;

                case MessageBoxButtons.OKCancel:
                    AddButton("OK", MessageBoxResult.OK, yPosition, true);
                    AddButton("Cancel", MessageBoxResult.Cancel, yPosition, false);
                    break;

                case MessageBoxButtons.YesNo:
                    AddButton("Yes", MessageBoxResult.Yes, yPosition, true, true);
                    AddButton("No", MessageBoxResult.No, yPosition, false, false);
                    break;
                
                case MessageBoxButtons.RetryCancel:
                    AddButton("Retry", MessageBoxResult.Retry, yPosition, true);
                    AddButton("Cancel", MessageBoxResult.Cancel, yPosition, false);
                    break;

                //case MessageBoxButtons.YesNoCancel:
                //    AddButton("Yes", MessageBoxResult.Yes, yPosition, true, true);
                //    AddButton("No", MessageBoxResult.No, yPosition, false, false);
                //    AddButton("Cancel", MessageBoxResult.Cancel, yPosition, false, false);
                //    break;

                //case MessageBoxButtons.AbortRetryIgnore:
                //    AddButton("Abort", MessageBoxResult.Abort, yPosition, true);
                //    AddButton("Retry", MessageBoxResult.Retry, yPosition, false);
                //    AddButton("Ignore", MessageBoxResult.Ignore, yPosition, false);
                //    break;
            }
        }

        private void AddButton(string text, MessageBoxResult result, int yPosition, bool isFirst = false, bool isYesButton = false)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold, GraphicsUnit.Point);
            btn.Size = new Size(100, 35);

            // Button spacing and positioning
            int buttonWidth = btn.Width;

            if (isYesButton)
            {
                btn.Location = new Point(30, yPosition);
                btn.BackColor = Color.FromArgb(59, 158, 241); 
            }
            else if (text == "No" || text == "Cancel")
            {
                btn.BackColor = Color.Gray;
                btn.Location = new Point(
                    pnlContent.ClientSize.Width - buttonWidth - 30, yPosition);
            }
            else
            {
                btn.BackColor = Color.FromArgb(59, 158, 241);
                btn.Location = new Point(130, yPosition);
            }

            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;

            btn.Click += (sender, e) =>
            {
                userResult = result;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            pnlContent.Controls.Add(btn);
        }

        private string GetImagePath(MessageBoxType? type)
        {
            switch (type)
            {
                case MessageBoxType.Warning:
                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", "warning.png");

                case MessageBoxType.Error:
                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", "error.png");

                case MessageBoxType.Info:
                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", "info.png");

                case MessageBoxType.Yes_No:
                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", "question-circle.png");

                default:
                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", "checkmark-circle.png");
            }
        }

        private void AutoCloseTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(Close));
            }
            else
            {
                this.Close();
            }
        }

        public static MessageBoxResult Show(string message, string title, MessageBoxButtons buttons, MessageBoxType? messageBoxType = null)
        {
            using (var messageBox = new CustomMessageBox(message, title, messageBoxType, buttons))
            {
                messageBox.ShowDialog();
                return messageBox.userResult;
            }
        }
    }

    public class RoundedPanel : Panel
    {
        public Color BorderColor { get; set; } = Color.SkyBlue;
        public int BorderThickness { get; set; } = 5;
        public int CornerRadius { get; set; } = 20; 

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (Pen borderPen = new Pen(BorderColor, BorderThickness))
            {
                Rectangle rect = new Rectangle(0, 0, this.ClientSize.Width - 1, this.ClientSize.Height - 1);
                e.Graphics.DrawRectangle(borderPen, rect);
            }
        }
    }
}
