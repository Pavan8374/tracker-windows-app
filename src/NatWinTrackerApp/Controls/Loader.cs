using System.Drawing.Drawing2D;

namespace NSTracker.Controls
{
    public class Loader: Control
    {
        private float _progress;
        private System.Windows.Forms.Timer _animationTimer;
        private string _loadingText = "Loading...";
        private float _percentage = 49;

        public float Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                Invalidate();
            }
        }

        public Loader()
        {
            SetStyle(ControlStyles.DoubleBuffer |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.OptimizedDoubleBuffer, true);

            Size = new Size(150, 150);
            BackColor = Color.White;

            _animationTimer = new System.Windows.Forms.Timer();
            _animationTimer.Interval = 50;
            _animationTimer.Tick += (s, e) =>
            {
                Progress = (Progress + 10) % 360;
                Invalidate();
            };
            _animationTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw outer circle (light gray)
            using (var pen = new Pen(Color.FromArgb(230, 230, 230), 8))
            {
                e.Graphics.DrawEllipse(pen, 10, 10, Width - 20, Height - 20);
            }

            // Draw progress arc (blue)
            using (var pen = new Pen(Color.FromArgb(47, 128, 237), 8))
            {
                e.Graphics.DrawArc(pen, 10, 10, Width - 20, Height - 20, -90, Progress);
            }

            // Draw loading text
            using (var font = new Font("Arial", 12))
            {
                var textSize = e.Graphics.MeasureString(_loadingText, font);
                e.Graphics.DrawString(_loadingText, font, Brushes.Gray,
                    (Width - textSize.Width) / 2,
                    Height - 30);
            }

            // Draw percentage
            using (var font = new Font("Arial", 16, FontStyle.Bold))
            {
                var percentText = $"{_percentage}%";
                var textSize = e.Graphics.MeasureString(percentText, font);
                e.Graphics.DrawString(percentText, font, Brushes.DarkGray,
                    (Width - textSize.Width) / 2,
                    (Height - textSize.Height) / 2);
            }

            base.OnPaint(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animationTimer?.Stop();
                _animationTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
