using Microsoft.Win32;

namespace NSTracker
{
    partial class TrackerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                if (globalHook != null)
                {
                    globalHook.Dispose();
                }
                if (components != null)
                {
                    components.Dispose();
                }
                if (disposing && (notifyIcon != null))
                {
                    notifyIcon.Dispose();
                }
                SystemEvents.SessionEnding -= OnSessionEnding;
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // TrackerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            this.Size = new Size(300, 550);
            Name = "TrackerForm";
            ResumeLayout(false);
        }

        #endregion
    }
}