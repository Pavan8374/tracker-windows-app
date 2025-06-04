using System.Drawing.Drawing2D;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace NSTracker.Common
{
    public class CustomModalDialog : Form
    {
        public class RichTextData
        {
            public string PlainText { get; set; }
            public List<TextFormatting> Formatting { get; set; } = new List<TextFormatting>();
        }
        public class TextFormatting
        {
            public int StartIndex { get; set; }
            public int Length { get; set; }
            public string FontName { get; set; }
            public float FontSize { get; set; }
            public FontStyle FontStyle { get; set; }
            public string TextColorArgb { get; set; }
            public string BackColorArgb { get; set; }
            public HorizontalAlignment Alignment { get; set; }
            public void SetTextColor(Color color)
            {
                TextColorArgb = $"{color.A},{color.R},{color.G},{color.B}";
            }
            public void SetBackColor(Color color)
            {
                BackColorArgb = $"{color.A},{color.R},{color.G},{color.B}";
            }
            public Color GetTextColor()
            {
                if (string.IsNullOrEmpty(TextColorArgb)) return Color.Black;
                var parts = TextColorArgb.Split(',').Select(int.Parse).ToArray();
                return Color.FromArgb(parts[0], parts[1], parts[2], parts[3]);
            }
            public Color GetBackColor()
            {
                if (string.IsNullOrEmpty(BackColorArgb)) return Color.White;
                var parts = BackColorArgb.Split(',').Select(int.Parse).ToArray();
                return Color.FromArgb(parts[0], parts[1], parts[2], parts[3]);
            }
        }
        private bool mouseDown;
        private Point lastLocation;
        private Panel titleBar;
        public RichTextBox richTextBox;
        private Dictionary<Button, Color> originalButtonColors = new Dictionary<Button, Color>();
        private int currentListNumber = 1;
        private string currentListType = "";
        public string Description = "";
        public Button closeButton;
        public Button SaveButton;
        public Button CancelButton;
        private string autoSavePath = Path.Combine(Path.GetTempPath(), "richtext_autosave.xml");
        private System.Windows.Forms.Timer autoSaveTimer;
        private bool isLoading = false;
        private ComboBox currentListDropDown;
        public CustomModalDialog()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;
            this.Size = new Size(400, 300); // Slightly larger for better content visibility
            this.BackColor = Color.White;
            this.Padding = new Padding(1);
            this.Paint += CustomDialog_Paint;

            InitializeModalComponents();
            InitializeAutoSave();
            LoadSavedContent();
        }

        private void InitializeAutoSave()
        {
            autoSaveTimer = new System.Windows.Forms.Timer();
            autoSaveTimer.Interval = 30000; 
            autoSaveTimer.Tick += (s, e) =>
            {
                SaveContent(); 
                SaveFormattedText(); 
            };
            autoSaveTimer.Start();
            richTextBox.TextChanged += (s, e) =>
            {
                if (!isLoading)
                {
                    autoSaveTimer.Stop();
                    autoSaveTimer.Start();
                }
            };
        }
        private void SaveContent()
        {
            try
            {
                var richTextData = new RichTextData
                {
                    PlainText = richTextBox.Rtf, // Save as RTF instead of plain text
                    Formatting = new List<TextFormatting>()
                };
                int currentSelStart = richTextBox.SelectionStart;
                int currentSelLength = richTextBox.SelectionLength;

                for (int i = 0; i < richTextBox.TextLength; i++)
                {
                    richTextBox.SelectionStart = i;
                    richTextBox.SelectionLength = 1;

                    if (richTextBox.SelectionFont != null)
                    {
                        var currentFont = richTextBox.SelectionFont;
                        var currentColor = richTextBox.SelectionColor;
                        var currentBackColor = richTextBox.SelectionBackColor;
                        var currentAlignment = richTextBox.SelectionAlignment;
                        int length = 1;

                        while (i + length < richTextBox.TextLength)
                        {
                            richTextBox.SelectionStart = i + length;
                            richTextBox.SelectionLength = 1;

                            if (richTextBox.SelectionFont == null ||
                                !FontFormattingEquals(richTextBox.SelectionFont, currentFont) ||
                                richTextBox.SelectionColor != currentColor ||
                                richTextBox.SelectionBackColor != currentBackColor ||
                                richTextBox.SelectionAlignment != currentAlignment)
                                break;

                            length++;
                        }

                        var formatting = new TextFormatting
                        {
                            StartIndex = i,
                            Length = length,
                            FontName = currentFont.Name,
                            FontSize = currentFont.Size,
                            FontStyle = currentFont.Style,
                            Alignment = currentAlignment
                        };

                        formatting.SetTextColor(currentColor);
                        formatting.SetBackColor(currentBackColor);
                        richTextData.Formatting.Add(formatting);
                        i += length - 1;
                    }
                }

                richTextBox.SelectionStart = currentSelStart;
                richTextBox.SelectionLength = currentSelLength;

                // Save to file
                using (var writer = XmlWriter.Create(autoSavePath, new XmlWriterSettings { Indent = true }))
                {
                    var serializer = new XmlSerializer(typeof(RichTextData));
                    serializer.Serialize(writer, richTextData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Auto-save error: {ex.Message}");
            }
        }
        public void SaveFormattedText()
        {
            try
            {
                StringBuilder formattedText = new StringBuilder();
                string savePath = Path.Combine(Path.GetTempPath(), "richtext_content.txt");
                int currentSelStart = richTextBox.SelectionStart;
                int currentSelLength = richTextBox.SelectionLength;

                formattedText.Append("<p><span style=\"color:rgb(29, 28, 29);\"><span style=\"background-color:rgb(248, 248, 248);\">");

                Font lastFont = null;
                Color lastColor = Color.Black;
                Color lastBackColor = Color.White;

                for (int i = 0; i < richTextBox.TextLength; i++)
                {
                    char currentChar = richTextBox.Text[i];

                    if (char.IsWhiteSpace(currentChar))
                    {
                        if (currentChar == ' ')
                        {
                            formattedText.Append("&nbsp;");
                        }
                        else if (currentChar == '\n')
                        {
                            formattedText.Append("</span></span></p><p><span style=\"color:rgb(29, 28, 29);\"><span style=\"background-color:rgb(248, 248, 248);\">");
                        }
                        else if (currentChar == '\t')
                        {
                            formattedText.Append("&nbsp;&nbsp;&nbsp;&nbsp;");
                        }
                        continue;
                    }

                    richTextBox.Select(i, 1);

                    var currentFont = richTextBox.SelectionFont ?? richTextBox.Font;
                    var currentColor = richTextBox.SelectionColor;
                    var currentBackColor = richTextBox.SelectionBackColor;

                    bool fontChanged = lastFont == null ||
                        (currentFont.Bold != lastFont.Bold ||
                         currentFont.Italic != lastFont.Italic ||
                         currentFont.Underline != lastFont.Underline);

                    bool colorChanged = currentColor != lastColor;
                    bool backColorChanged = currentBackColor != lastBackColor;

                    if (fontChanged || colorChanged || backColorChanged)
                    {
                        if (lastFont != null)
                        {
                            if (lastFont.Underline) formattedText.Append("</u>");
                            if (lastFont.Italic) formattedText.Append("</i>");
                            if (lastFont.Bold) formattedText.Append("</b>");
                            if (lastColor != Color.Black) formattedText.Append("</span>");
                            if (lastBackColor != Color.White) formattedText.Append("</span>");
                        }

                        if (currentColor != Color.Black)
                        {
                            formattedText.Append($"<span style='color: rgb({currentColor.R},{currentColor.G},{currentColor.B});'>");
                        }

                        if (currentBackColor != Color.White)
                        {
                            formattedText.Append($"<span style='background-color: rgb({currentBackColor.R},{currentBackColor.G},{currentBackColor.B});'>");
                        }

                        if (currentFont.Bold) formattedText.Append("<b>");
                        if (currentFont.Italic) formattedText.Append("<i>");
                        if (currentFont.Underline) formattedText.Append("<u>");
                    }

                    formattedText.Append(HttpUtility.HtmlEncode(currentChar.ToString()));

                    lastFont = currentFont;
                    lastColor = currentColor;
                    lastBackColor = currentBackColor;
                }

                if (lastFont != null)
                {
                    if (lastFont.Underline) formattedText.Append("</u>");
                    if (lastFont.Italic) formattedText.Append("</i>");
                    if (lastFont.Bold) formattedText.Append("</b>");
                    if (lastColor != Color.Black) formattedText.Append("</span>");
                    if (lastBackColor != Color.White) formattedText.Append("</span>");
                }

                formattedText.Append("</span></span></p>");

                richTextBox.SelectionStart = currentSelStart;
                richTextBox.SelectionLength = currentSelLength;

                File.WriteAllText(savePath, formattedText.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Format save error: {ex.Message}");
            }
        }
        private bool FontFormattingEquals(Font font1, Font font2)
        {
            return font1.Name == font2.Name &&
                   Math.Abs(font1.Size - font2.Size) < 0.01 &&
                   font1.Style == font2.Style;
        }
        private void LoadSavedContent()
        {
            try
            {
                if (File.Exists(autoSavePath))
                {
                    isLoading = true;
                    using (var reader = XmlReader.Create(autoSavePath))
                    {
                        var serializer = new XmlSerializer(typeof(RichTextData));
                        var richTextData = (RichTextData)serializer.Deserialize(reader);

                        // First, load the RTF content
                        richTextBox.Rtf = richTextData.PlainText;

                        // Then apply additional formatting
                        foreach (var format in richTextData.Formatting)
                        {
                            if (format.StartIndex < richTextBox.TextLength)
                            {
                                richTextBox.SelectionStart = format.StartIndex;
                                richTextBox.SelectionLength = Math.Min(format.Length, richTextBox.TextLength - format.StartIndex);

                                // Apply font formatting
                                richTextBox.SelectionFont = new Font(format.FontName, format.FontSize, format.FontStyle);

                                // Apply colors
                                richTextBox.SelectionColor = format.GetTextColor();
                                richTextBox.SelectionBackColor = format.GetBackColor();

                                // Apply alignment
                                richTextBox.SelectionAlignment = format.Alignment;
                            }
                        }

                        // Reset selection
                        richTextBox.SelectionStart = 0;
                        richTextBox.SelectionLength = 0;
                    }
                    isLoading = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load error: {ex.Message}");
                isLoading = false;
            }
        }
        public void ResetContent()
        {
            richTextBox.Clear();
            if (File.Exists(autoSavePath))
            {
                try
                {
                    File.Delete(autoSavePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Reset error: {ex.Message}");
                }
            }
        }
        private void CustomDialog_Paint(object sender, PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle,
                Color.FromArgb(235, 245, 255), 
                Color.FromArgb(255, 255, 255),
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }

            using (var shadowBrush = new SolidBrush(Color.FromArgb(25, 0, 0, 0)))
            {
                e.Graphics.FillRectangle(shadowBrush, new Rectangle(-2, -2, this.Width + 4, this.Height + 4));
            }
        }
        private void InitializeModalComponents()
        {
            titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.FromArgb(33, 150, 243) 
            };

            Label titleLabel = new Label
            {
                Text = "Add Description",
                Font = new Font("Segoe UI Semibold", 13),
                ForeColor = Color.White,
                Location = new Point(15, 10),
                AutoSize = true
            };

            closeButton = new Button
            {
                Text = "✕",
                Size = new Size(32, 32),
                Location = new Point(this.Width - 42, 6),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += Click_Cancel;

            titleBar.Controls.AddRange(new Control[] { titleLabel, closeButton });
            SetupTitleBarDragging();

            Panel mainContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(18,0,18,60)
            };

            Panel toolbarPanel = CreateEnhancedToolbar();
            toolbarPanel.Dock = DockStyle.Top;

            Panel contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0, 10, 0, 0)
            };

            Panel textBoxContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(1),
                BackColor = Color.FromArgb(210, 220, 235)
            };

            richTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(45, 55, 75),
                Padding = new Padding(10)
            };

            SaveButton = new Button()
            {
                Text = "Save",
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                Size = new Size(120, 40),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(15, 200),
            };
            SaveButton.Click += Click_Cancel;

            CancelButton = new Button()
            {
                Text = "Cancel",
                BackColor = Color.Black,
                ForeColor = Color.White,
                Size = new Size(120, 40),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(260, 200),
            };

            CancelButton.Click += Click_Cancel;
            richTextBox.KeyDown += RichTextBox_KeyDown;
            richTextBox.SelectionChanged += RichTextBox_SelectionChanged;

            textBoxContainer.Controls.Add(richTextBox);
            contentPanel.Controls.Add(textBoxContainer);
            mainContainer.Controls.AddRange(new Control[] { contentPanel, toolbarPanel, SaveButton, CancelButton });
            this.Controls.AddRange(new Control[] { mainContainer, titleBar });
        }
        private async void Click_Cancel(object? sender, EventArgs e)
        {
            SaveContent();
            SaveFormattedText(); 
            this.Close();
        }
        private void RichTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;

                // Check if the current line starts with a list marker
                int currentLineIndex = richTextBox.GetLineFromCharIndex(richTextBox.SelectionStart);
                string currentLineText = GetLineText(currentLineIndex);

                if (!string.IsNullOrEmpty(currentListType))
                {
                    // If the current line is just a list marker, reset the list
                    if (currentLineText.Trim() == currentListType.Trim() ||
                        (currentListType.StartsWith("1") && currentLineText.Trim().EndsWith(". ")))
                    {
                        ResetListFormatting();
                        richTextBox.SelectedText = Environment.NewLine;
                        return;
                    }

                    // Continue the list formatting
                    if (currentListType.StartsWith("1")) // Numbered list
                    {
                        richTextBox.SelectedText = Environment.NewLine + $"{currentListNumber++}. ";
                    }
                    else // Bullet or dash list
                    {
                        richTextBox.SelectedText = Environment.NewLine + currentListType;
                    }
                }
                else
                {
                    // Normal line break without list formatting
                    richTextBox.SelectedText = Environment.NewLine;
                }
            }
        }

        private string GetLineText(int lineIndex)
        {
            if (lineIndex >= 0 && lineIndex < richTextBox.Lines.Length)
            {
                return richTextBox.Lines[lineIndex];
            }
            return string.Empty;
        }
        private Panel CreateEnhancedToolbar()
        {
            Panel toolbarPanel = new Panel
            {
                Height = 42,
                BackColor = Color.FromArgb(245, 248, 252),
                Padding = new Padding(8, 4, 8, 4)
            };

            FlowLayoutPanel buttonContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true
            };

            Control[] formattingButtons = new Control[]
            {
                CreateToolbarButton("B", "Bold", () => ApplyFormatting(FontStyle.Bold), FontStyle.Bold),
                CreateToolbarButton("I", "Italic", () => ApplyFormatting(FontStyle.Italic), FontStyle.Italic),
                CreateToolbarButton("U", "Underline", () => ApplyFormatting(FontStyle.Underline), FontStyle.Underline)
            };

            Control[] colorButtons = new Control[]
            {
                CreateToolbarButton("A", "Text Color", () => ShowColorDialog(true)),
                CreateToolbarButton("⬒", "Background Color", () => ShowColorDialog(false))
            };

            var alignmentItems = new (string Text, Action Handler)[]
            {
                ("Left", () => richTextBox.SelectionAlignment = HorizontalAlignment.Left),
                ("Center", () => richTextBox.SelectionAlignment = HorizontalAlignment.Center),
                ("Right", () => richTextBox.SelectionAlignment = HorizontalAlignment.Right)
            };

            var listItems = new (string Text, Action Handler)[]
            {
                ("Bullet Points", () => StartList("• ")),
                ("Numbers", () => StartList("1. ")),
                //("Dashes", () => StartList("- "))
            };

            var alignmentDropDown = CreateToolbarDropDown("Alignment", alignmentItems);
            var listDropDown = CreateToolbarDropDown("List", listItems);

            buttonContainer.Controls.AddRange(formattingButtons);
            buttonContainer.Controls.Add(CreateToolbarSeparator());
            buttonContainer.Controls.AddRange(colorButtons);
            buttonContainer.Controls.Add(CreateToolbarSeparator());
            buttonContainer.Controls.Add(alignmentDropDown);
            buttonContainer.Controls.Add(listDropDown);

            toolbarPanel.Controls.Add(buttonContainer);
            return toolbarPanel;
        }
        private void StartList(string marker)
        {
            // If the same list type is already active, deactivate it
            if (currentListType == marker)
            {
                ResetListFormatting();
                return;
            }

            currentListType = marker;
            currentListNumber = 1;

            // Get the selected text and split it into lines
            string[] lines = richTextBox.SelectedText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int selectionStart = richTextBox.SelectionStart;

            // Clear existing selection
            richTextBox.SelectedText = string.Empty;

            for (int i = 0; i < lines.Length; i++)
            {
                // Determine the prefix based on the list type
                string prefix;
                if (marker.StartsWith("1")) // Numbered list
                {
                    prefix = $"{currentListNumber++}. ";
                }
                else // Bullet or dash list
                {
                    prefix = marker;
                }

                // Append the formatted line back to the RichTextBox
                richTextBox.AppendText(prefix + lines[i]);

                // Add a new line after each line except for the last one
                if (i < lines.Length - 1)
                {
                    richTextBox.AppendText(Environment.NewLine);
                }
            }

            // Reset selection to the end of the newly added text
            richTextBox.SelectionStart = selectionStart;
        }

        private ComboBox CreateToolbarDropDown(string text, (string Text, Action Handler)[] items)
        {
            var dropDown = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size = new Size(95, 30),
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(2, 0, 2, 0),
                BackColor = Color.FromArgb(250, 252, 255)
            };

            dropDown.Items.Add(text);
            dropDown.Items.AddRange(items.Select(i => i.Text).ToArray());
            dropDown.SelectedIndex = 0;

            dropDown.SelectedIndexChanged += (s, e) =>
            {
                if (dropDown.SelectedIndex > 0)
                {
                    // Deselect previous list dropdown if exists
                    if (currentListDropDown != null && currentListDropDown != dropDown)
                    {
                        currentListDropDown.BackColor = Color.FromArgb(250, 252, 255);
                    }

                    // Handle the selected list type
                    items[dropDown.SelectedIndex - 1].Handler();

                    // Highlight the current dropdown
                    dropDown.BackColor = Color.FromArgb(226, 232, 240);
                    currentListDropDown = dropDown;
                }
                else
                {
                    // Reset list type if "List" is selected
                    ResetListFormatting();
                }
            };

            return dropDown;
        }

        private void ResetListFormatting()
        {
            // Reset list formatting
            currentListType = "";
            currentListNumber = 1;

            // Reset dropdown color
            if (currentListDropDown != null)
            {
                currentListDropDown.BackColor = Color.FromArgb(250, 252, 255);
                currentListDropDown = null;
            }
        }
        private void RichTextBox_SelectionChanged(object sender, EventArgs e)
        {
            UpdateToolbarState();
        }
        private void UpdateToolbarState()
        {
            foreach (var kvp in originalButtonColors)
            {
                var button = kvp.Key;
                var originalColor = kvp.Value;

                if (button.Tag is FontStyle style)
                {
                    bool isActive = richTextBox.SelectionFont != null &&
                                  (richTextBox.SelectionFont.Style & style) == style;
                    button.BackColor = isActive ? Color.FromArgb(226, 232, 240) : originalColor;
                }
            }
        }
        private Control CreateToolbarButton(string text, string tooltip, Action clickHandler = null, FontStyle? style = null)
        {
            Button button = new Button
            {
                Text = text,
                Size = new Size(34, 34),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", style.HasValue ? 10 : 12, style ?? FontStyle.Regular),
                ForeColor = Color.FromArgb(60, 75, 95),
                Cursor = Cursors.Hand,
                Margin = new Padding(1),
                UseVisualStyleBackColor = true,
                Tag = style
            };

            button.FlatAppearance.BorderSize = 0;
            originalButtonColors[button] = button.BackColor;

            button.MouseEnter += (s, e) => {
                button.BackColor = Color.FromArgb(220, 230, 245);
                button.ForeColor = Color.FromArgb(25, 35, 55);
            };

            button.MouseLeave += (s, e) => {
                if (!IsStyleActive(style))
                {
                    button.BackColor = originalButtonColors[button];
                    button.ForeColor = Color.FromArgb(60, 75, 95);
                }
            };

            if (clickHandler != null)
            {
                button.Click += (s, e) => {
                    clickHandler();
                    UpdateToolbarState();
                };
            }

            if (!string.IsNullOrEmpty(tooltip))
            {
                var toolTip = new ToolTip();
                toolTip.SetToolTip(button, tooltip);
            }

            return button;
        }
        private bool IsStyleActive(FontStyle? style)
        {
            return style.HasValue && richTextBox.SelectionFont != null &&
                   (richTextBox.SelectionFont.Style & style.Value) == style.Value;
        }
        private Control CreateToolbarSeparator() =>
            new Panel
            {
                Width = 1,
                Height = 24,
                Margin = new Padding(6, 4, 6, 4),
                BackColor = Color.FromArgb(226, 232, 240)
            };
        private void SetupTitleBarDragging()
        {
            titleBar.MouseDown += (s, e) =>
            {
                mouseDown = true;
                lastLocation = e.Location;
            };

            titleBar.MouseMove += (s, e) =>
            {
                if (mouseDown)
                {
                    this.Location = new Point(
                        (this.Location.X - lastLocation.X) + e.X,
                        (this.Location.Y - lastLocation.Y) + e.Y);
                }
            };

            titleBar.MouseUp += (s, e) => mouseDown = false;
        }
        private void ApplyFormatting(FontStyle style)
        {
            if (richTextBox.SelectionFont != null)
            {
                FontStyle newStyle = richTextBox.SelectionFont.Style ^ style;
                richTextBox.SelectionFont = new Font(richTextBox.SelectionFont, newStyle);
            }
        }
        private void ShowColorDialog(bool isTextColor)
        {
            using (var dialog = new ColorDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (isTextColor)
                        richTextBox.SelectionColor = dialog.Color;
                    else
                        richTextBox.SelectionBackColor = dialog.Color;
                }
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                autoSaveTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}