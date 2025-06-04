using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace NSTracker.Controls
{
    public class LoaderControl : UserControl
    {
        private PictureBox pictureBoxLoader;

        public LoaderControl()
        {
            // Initialize components
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            pictureBoxLoader = new PictureBox
            {
                // Change SizeMode to Zoom to resize without cropping
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill
            };

            // Load the GIF
            string loaderPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "Gifs",
                "loader.gif"
            );
            pictureBoxLoader.Image = Image.FromFile(loaderPath);

            // Make the user control the same size as the form it will be on
            this.Controls.Add(pictureBoxLoader);
            this.BackColor = Color.White;
            this.Visible = false; // Initially hidden
        }

        public void StartLoader()
        {
            // Set the size and position of the loader
            this.Size = new Size(100, 100); // Desired smaller size
            this.Location = new Point(100, 150);
            this.BringToFront();
            this.Visible = true;
        }

        public void StopLoader()
        {
            this.Visible = false;
        }
    }
}