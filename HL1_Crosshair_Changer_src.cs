using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace HL1CrosshairChanger
{
    public class MainForm : Form
    {
        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblVer;
        private Button btnLang;

        private Button btnGameHL;
        private Button btnGameOF;
        private Button btnGameBS;
        private Button btnGameCustom;
        private Label lblCustomMod;
        private TextBox txtCustomMod;

        private Label lblPath;
        private TextBox txtGamePath;
        private Button btnBrowse;
        private Label lblRes;
        private ComboBox cmbResolution;
        private Label lblStyle;
        private ComboBox cmbStyle;
        private Label lblColor;
        private ComboBox cmbColor;
        private Button btnCustomColor;
        private Label lblPrev;
        private Panel pnlPreview;
        private Label lblStatus;
        private Button btnInstall;
        private Button btnRestore;
        private Label lblEasterEgg;
        private Label lblCredits;

        private bool isRussian = false; // Default: English
        private int secretClickCount = 0;
        private int selectedGameMode = 0; // 0 = HL (valve), 1 = OF (gearbox), 2 = BS (bshift), 3 = Custom

        // Base Dark Theme & Game Mode Colors
        private readonly Color hlBgDark = Color.FromArgb(24, 21, 18);
        private readonly Color hlPanelBg = Color.FromArgb(36, 31, 26);
        private readonly Color hlTextWhite = Color.FromArgb(240, 230, 215);

        private readonly Color colHL = Color.FromArgb(255, 150, 0);      // Classic HL1 Orange
        private readonly Color colOF = Color.FromArgb(50, 205, 50);      // Opposing Force Military Green
        private readonly Color colBS = Color.FromArgb(30, 144, 255);     // Blue Shift Security Blue
        private readonly Color colCustom = Color.FromArgb(255, 215, 0);  // Custom Mod Gold

        private Color activeAccent = Color.FromArgb(255, 150, 0);
        private Color customRgb = Color.FromArgb(0, 255, 128);

        private readonly string[] styleNamesEN = {
            "Large Circle + Dot",
            "Medium Circle + Dot",
            "Small Circle + Dot",
            "Large Circle",
            "Medium Circle",
            "Small Circle",
            "Large Brackets (.) + Dot",
            "Medium Brackets (.) + Dot",
            "Small Brackets (.) + Dot",
            "Large V-Chevron",
            "Medium V-Chevron",
            "Small V-Chevron",
            "Medium V-Chevron + Dot",
            "Bold Dot",
            "Small Crisp Dot"
        };

        private readonly string[] styleNamesRU = {
            "Большой круг с точкой (Large Circle + Dot)",
            "Средний круг с точкой (Medium Circle + Dot)",
            "Малый круг с точкой (Small Circle + Dot)",
            "Большой круг (Large Circle)",
            "Средний круг (Medium Circle)",
            "Малый круг (Small Circle)",
            "Большие дужки (.) с точкой (Large (.) Brackets)",
            "Средние дужки (.) с точкой (Medium (.) Brackets)",
            "Малые дужки (.) с точкой (Small (.) Brackets)",
            "Большой V-образный (Large V-Chevron)",
            "Средний V-образный (Medium V-Chevron)",
            "Малый V-образный (Small V-Chevron)",
            "V-образный с точкой (Medium V-Chevron + Dot)",
            "Крупная точка (Bold Dot)",
            "Малая четкая точка (Small Dot)"
        };

        private readonly string[] colorNamesEN = {
            "Classic HL1 Amber",
            "Green",
            "Cyan",
            "Blue",
            "Red",
            "Yellow",
            "Magenta",
            "White",
            "Custom RGB Color..."
        };

        private readonly string[] colorNamesRU = {
            "Янтарный HL1 (Classic Amber)",
            "Зеленый (Green)",
            "Голубой (Cyan)",
            "Синий (Blue)",
            "Красный (Red)",
            "Желтый (Yellow)",
            "Розовый (Magenta)",
            "Белый (White)",
            "Свой цвет... (Custom RGB)"
        };

        private readonly string[] resNamesEN = {
            "All Screen Resolutions (Recommended)",
            "Standard / HD (< 1280px)",
            "Full HD / Quad HD (1280px - 2559px)",
            "4K Ultra HD (2560px and above)"
        };

        private readonly string[] resNamesRU = {
            "Все разрешения экрана (Универсально)",
            "Стандартное / HD (до 1280px)",
            "Full HD / Quad HD (1280px - 2559px)",
            "4K Ultra HD (2560px и выше)"
        };

        private readonly Color[] presetColors = {
            Color.FromArgb(255, 160, 0),   // Classic HL1 Amber
            Color.FromArgb(0, 255, 0),     // Green
            Color.FromArgb(0, 255, 255),   // Cyan
            Color.FromArgb(0, 110, 255),   // Blue
            Color.FromArgb(255, 35, 35),   // Red
            Color.FromArgb(255, 255, 0),   // Yellow
            Color.FromArgb(255, 0, 255),   // Magenta
            Color.FromArgb(255, 255, 255)  // White
        };

        public MainForm()
        {
            this.Text = "Half-Life Crosshair Changer v5.1";
            this.Size = new Size(610, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = hlBgDark;
            this.ForeColor = hlTextWhite;
            this.Font = new Font("Tahoma", 9.25f, FontStyle.Regular);

            try
            {
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch { }

            BuildUI();
            ApplyLanguage();
            SelectGameMode(0);
            AutoDetectGameAndResolution();
        }

        private Color GetSelectedCrosshairColor()
        {
            int idx = cmbColor.SelectedIndex;
            if (idx >= 0 && idx < presetColors.Length)
                return presetColors[idx];
            return customRgb;
        }

        private string GetTargetModFolder()
        {
            if (selectedGameMode == 0) return "valve";
            if (selectedGameMode == 1) return "gearbox";
            if (selectedGameMode == 2) return "bshift";
            string custom = txtCustomMod.Text.Trim();
            return string.IsNullOrEmpty(custom) ? "cstrike" : custom;
        }

        private void BuildUI()
        {
            // Top Header Banner
            pnlHeader = new Panel() { Location = new Point(0, 0), Size = new Size(610, 48), BackColor = Color.FromArgb(18, 15, 12) };
            pnlHeader.Paint += delegate(object s, PaintEventArgs e) {
                using (Pen p = new Pen(activeAccent, 2)) {
                    e.Graphics.DrawLine(p, 0, 46, 610, 46);
                }
            };
            lblTitle = new Label() {
                Text = "HALF-LIFE  //  CROSSHAIR CHANGER",
                Location = new Point(18, 13),
                AutoSize = true,
                Font = new Font("Trebuchet MS", 13.5f, FontStyle.Bold),
                ForeColor = activeAccent
            };
            lblVer = new Label() {
                Text = "v5.1",
                Location = new Point(445, 16),
                AutoSize = true,
                Font = new Font("Consolas", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(140, 110, 60)
            };
            btnLang = CreateStyledButton("Lang: EN", new Point(495, 11), new Size(85, 25), activeAccent, false);
            btnLang.Click += delegate(object s, EventArgs e) {
                isRussian = !isRussian;
                ApplyLanguage();
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblVer);
            pnlHeader.Controls.Add(btnLang);

            // 4 Game / Mod Selection Buttons at Top of Interface
            btnGameHL = CreateStyledButton("Half-Life", new Point(20, 60), new Size(130, 32), colHL, true);
            btnGameHL.Click += delegate(object s, EventArgs e) { SelectGameMode(0); };

            btnGameOF = CreateStyledButton("Opposing Force", new Point(158, 60), new Size(135, 32), colOF, false);
            btnGameOF.Click += delegate(object s, EventArgs e) { SelectGameMode(1); };

            btnGameBS = CreateStyledButton("Blue Shift", new Point(301, 60), new Size(125, 32), colBS, false);
            btnGameBS.Click += delegate(object s, EventArgs e) { SelectGameMode(2); };

            btnGameCustom = CreateStyledButton("Custom Mod", new Point(434, 60), new Size(146, 32), colCustom, false);
            btnGameCustom.Click += delegate(object s, EventArgs e) { SelectGameMode(3); };

            // Custom Mod Subfolder Input Row (Shown/Enabled when Custom is chosen, or shows active folder)
            lblCustomMod = new Label() { Location = new Point(20, 101), AutoSize = true, ForeColor = colCustom };
            txtCustomMod = new TextBox() {
                Text = "cstrike",
                Location = new Point(255, 98),
                Width = 160,
                BackColor = hlPanelBg,
                ForeColor = colCustom,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 9.5f, FontStyle.Bold)
            };

            // Game Path
            lblPath = new Label() { Location = new Point(20, 130), AutoSize = true, ForeColor = activeAccent };
            txtGamePath = new TextBox() { Location = new Point(20, 152), Width = 455, BackColor = hlPanelBg, ForeColor = hlTextWhite, BorderStyle = BorderStyle.FixedSingle };
            btnBrowse = CreateStyledButton("Browse...", new Point(485, 150), new Size(95, 25), activeAccent, false);
            btnBrowse.Click += delegate(object s, EventArgs e) {
                using (var fbd = new FolderBrowserDialog()) {
                    if (fbd.ShowDialog() == DialogResult.OK) txtGamePath.Text = fbd.SelectedPath;
                }
            };

            // Resolution
            lblRes = new Label() { Location = new Point(20, 190), AutoSize = true, ForeColor = activeAccent };
            cmbResolution = new ComboBox() { Location = new Point(20, 212), Width = 360, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = hlPanelBg, ForeColor = hlTextWhite, FlatStyle = FlatStyle.Flat };

            // Crosshair Style
            lblStyle = new Label() { Location = new Point(20, 250), AutoSize = true, ForeColor = activeAccent };
            cmbStyle = new ComboBox() { Location = new Point(20, 272), Width = 360, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = hlPanelBg, ForeColor = hlTextWhite, FlatStyle = FlatStyle.Flat };
            cmbStyle.SelectedIndexChanged += delegate(object s, EventArgs e) { pnlPreview.Invalidate(); };

            // Crosshair Color + Custom Picker
            lblColor = new Label() { Location = new Point(20, 310), AutoSize = true, ForeColor = activeAccent };
            cmbColor = new ComboBox() { Location = new Point(20, 332), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = hlPanelBg, ForeColor = hlTextWhite, FlatStyle = FlatStyle.Flat };
            cmbColor.SelectedIndexChanged += delegate(object s, EventArgs e) {
                if (cmbColor.SelectedIndex == presetColors.Length) {
                    PickCustomColor();
                }
                pnlPreview.Invalidate();
            };

            btnCustomColor = CreateStyledButton("RGB Palette...", new Point(270, 331), new Size(110, 24), activeAccent, false);
            btnCustomColor.Click += delegate(object s, EventArgs e) {
                PickCustomColor();
                cmbColor.SelectedIndex = presetColors.Length;
                pnlPreview.Invalidate();
            };

            // Preview Box
            lblPrev = new Label() { Location = new Point(405, 190), AutoSize = true, ForeColor = activeAccent };
            pnlPreview = new Panel() { Location = new Point(405, 212), Size = new Size(175, 150), BackColor = Color.FromArgb(14, 13, 11), BorderStyle = BorderStyle.FixedSingle };
            pnlPreview.Paint += DrawPreview;

            // Main Action Buttons
            btnInstall = CreateStyledButton("INSTALL CROSSHAIR", new Point(20, 385), new Size(280, 44), activeAccent, true);
            btnInstall.Click += InstallCrosshair;

            btnRestore = CreateStyledButton("RESTORE DEFAULT", new Point(315, 385), new Size(265, 44), activeAccent, false);
            btnRestore.Click += RestoreBackup;

            // Status & Footer Credits
            lblStatus = new Label() { Location = new Point(20, 442), Size = new Size(560, 22), ForeColor = activeAccent };

            // Secret Bottom-Left Easter Egg Zone (5 clicks -> "Kosh=💩")
            lblEasterEgg = new Label() {
                Text = "",
                Location = new Point(18, 476),
                Size = new Size(160, 32),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI Emoji", 10.5f, FontStyle.Bold),
                ForeColor = activeAccent,
                BackColor = Color.Transparent
            };
            lblEasterEgg.MouseDown += delegate(object s, MouseEventArgs e) {
                TriggerEasterEggClick();
            };

            this.MouseDown += delegate(object s, MouseEventArgs e) {
                if (e.X <= 150 && e.Y >= this.ClientSize.Height - 60)
                {
                    TriggerEasterEggClick();
                }
            };

            lblCredits = new Label() {
                Text = "By FFBeaver and Gemini",
                Location = new Point(395, 482),
                Size = new Size(185, 22),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Trebuchet MS", 9.5f, FontStyle.Bold),
                ForeColor = activeAccent
            };

            this.Controls.AddRange(new Control[] {
                pnlHeader,
                btnGameHL, btnGameOF, btnGameBS, btnGameCustom,
                lblCustomMod, txtCustomMod,
                lblPath, txtGamePath, btnBrowse,
                lblRes, cmbResolution, lblStyle, cmbStyle,
                lblColor, cmbColor, btnCustomColor,
                lblPrev, pnlPreview, btnInstall, btnRestore,
                lblStatus, lblEasterEgg, lblCredits
            });
        }

        private Button CreateStyledButton(string text, Point loc, Size sz, Color themeCol, bool isFilled)
        {
            Button btn = new Button() {
                Text = text,
                Location = loc,
                Size = sz,
                BackColor = isFilled ? Color.FromArgb(160, themeCol.R * 2 / 3, themeCol.G * 2 / 3, themeCol.B * 2 / 3) : hlPanelBg,
                ForeColor = isFilled ? Color.White : themeCol,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", 8.75f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = themeCol;
            btn.FlatAppearance.BorderSize = isFilled ? 2 : 1;
            return btn;
        }

        private void StyleGameButton(Button btn, Color col, bool isSelected)
        {
            btn.BackColor = isSelected ? Color.FromArgb(col.R / 3, col.G / 3, col.B / 3) : Color.FromArgb(28, 25, 22);
            btn.ForeColor = isSelected ? Color.White : col;
            btn.FlatAppearance.BorderColor = col;
            btn.FlatAppearance.BorderSize = isSelected ? 2 : 1;
        }

        private void SelectGameMode(int mode)
        {
            selectedGameMode = mode;
            if (mode == 0) activeAccent = colHL;
            else if (mode == 1) activeAccent = colOF;
            else if (mode == 2) activeAccent = colBS;
            else activeAccent = colCustom;

            StyleGameButton(btnGameHL, colHL, mode == 0);
            StyleGameButton(btnGameOF, colOF, mode == 1);
            StyleGameButton(btnGameBS, colBS, mode == 2);
            StyleGameButton(btnGameCustom, colCustom, mode == 3);

            // Toggle Custom Mod folder box visibility/state
            lblCustomMod.Visible = (mode == 3);
            txtCustomMod.Visible = (mode == 3);

            // Update UI Accent colors to match the selected game!
            lblTitle.ForeColor = activeAccent;
            lblPath.ForeColor = activeAccent;
            lblRes.ForeColor = activeAccent;
            lblStyle.ForeColor = activeAccent;
            lblColor.ForeColor = activeAccent;
            lblPrev.ForeColor = activeAccent;
            lblCredits.ForeColor = activeAccent;
            lblStatus.ForeColor = activeAccent;

            btnLang.ForeColor = activeAccent;
            btnLang.FlatAppearance.BorderColor = activeAccent;

            btnBrowse.ForeColor = activeAccent;
            btnBrowse.FlatAppearance.BorderColor = activeAccent;

            btnCustomColor.ForeColor = activeAccent;
            btnCustomColor.FlatAppearance.BorderColor = activeAccent;

            btnInstall.BackColor = Color.FromArgb(activeAccent.R / 2, activeAccent.G / 2, activeAccent.B / 2);
            btnInstall.ForeColor = (mode == 3) ? Color.White : Color.White;
            btnInstall.FlatAppearance.BorderColor = activeAccent;

            btnRestore.ForeColor = activeAccent;
            btnRestore.FlatAppearance.BorderColor = activeAccent;

            pnlHeader.Invalidate();
            pnlPreview.Invalidate();
            UpdateStatusForTarget();
        }

        private void UpdateStatusForTarget()
        {
            string modFolder = GetTargetModFolder();
            if (isRussian)
                lblStatus.Text = string.Format("Целевая папка мода: {0}/sprites", modFolder);
            else
                lblStatus.Text = string.Format("Target mod folder: {0}/sprites", modFolder);
        }

        private void TriggerEasterEggClick()
        {
            secretClickCount++;
            if (secretClickCount >= 5)
            {
                lblEasterEgg.Text = "Kosh=\uD83D\uDCA9";
            }
        }

        private void ApplyLanguage()
        {
            int resIdx = Math.Max(0, cmbResolution.SelectedIndex);
            int styleIdx = (cmbStyle.SelectedIndex >= 0) ? cmbStyle.SelectedIndex : 6;
            int colIdx = Math.Max(0, cmbColor.SelectedIndex);

            cmbResolution.Items.Clear();
            cmbStyle.Items.Clear();
            cmbColor.Items.Clear();

            if (isRussian)
            {
                btnLang.Text = "Язык: RU";
                btnGameCustom.Text = "Свой Вариант";
                lblCustomMod.Text = "Папка мода (например cstrike, tfc):";
                lblPath.Text = "Корневая папка Half-Life:";
                btnBrowse.Text = "Обзор...";
                lblRes.Text = "Масштаб интерфейса (Разрешение):";
                lblStyle.Text = "Форма прицела:";
                lblColor.Text = "Цвет прицела:";
                btnCustomColor.Text = "Палитра RGB...";
                lblPrev.Text = "Предпросмотр:";
                btnInstall.Text = "УСТАНОВИТЬ ПРИЦЕЛ";
                btnRestore.Text = "ВЕРНУТЬ СТАНДАРТНЫЙ";

                cmbResolution.Items.AddRange(resNamesRU);
                cmbStyle.Items.AddRange(styleNamesRU);
                cmbColor.Items.AddRange(colorNamesRU);
            }
            else
            {
                btnLang.Text = "Lang: EN";
                btnGameCustom.Text = "Custom Mod";
                lblCustomMod.Text = "Mod folder name (e.g. cstrike, tfc):";
                lblPath.Text = "Half-Life Root Directory:";
                btnBrowse.Text = "Browse...";
                lblRes.Text = "HUD Scale (Screen Resolution):";
                lblStyle.Text = "Crosshair Shape:";
                lblColor.Text = "Crosshair Color:";
                btnCustomColor.Text = "RGB Palette...";
                lblPrev.Text = "Live Preview:";
                btnInstall.Text = "INSTALL CROSSHAIR";
                btnRestore.Text = "RESTORE DEFAULT";

                cmbResolution.Items.AddRange(resNamesEN);
                cmbStyle.Items.AddRange(styleNamesEN);
                cmbColor.Items.AddRange(colorNamesEN);
            }

            cmbResolution.SelectedIndex = resIdx;
            cmbStyle.SelectedIndex = styleIdx;
            cmbColor.SelectedIndex = colIdx;
            UpdateStatusForTarget();
        }

        private void PickCustomColor()
        {
            using (ColorDialog cd = new ColorDialog())
            {
                cd.Color = customRgb;
                cd.FullOpen = true;
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    if (cd.Color.R == 0 && cd.Color.G == 0 && cd.Color.B == 255)
                        customRgb = Color.FromArgb(0, 10, 250);
                    else
                        customRgb = cd.Color;
                }
            }
        }

        private void AutoDetectGameAndResolution()
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            if (File.Exists(Path.Combine(currentDir, "hl.exe")) || Directory.Exists(Path.Combine(currentDir, "valve")))
            {
                txtGamePath.Text = currentDir.TrimEnd('\\');
            }
            else
            {
                string[] commonPaths = {
                    @"C:\Program Files (x86)\Steam\steamapps\common\Half-Life",
                    @"C:\Program Files\Steam\steamapps\common\Half-Life",
                    @"D:\SteamLibrary\steamapps\common\Half-Life",
                    @"E:\SteamLibrary\steamapps\common\Half-Life",
                    @"F:\SteamLibrary\steamapps\common\Half-Life",
                    @"H:\SteamLibrary\steamapps\common\Half-Life"
                };
                foreach (var p in commonPaths)
                {
                    if (Directory.Exists(p)) { txtGamePath.Text = p; break; }
                }
            }
        }

        private byte[] GenerateCrosshairMask(int styleIndex)
        {
            int size = 64;
            int cx = 32, cy = 32;
            byte[] mask = new byte[size * size];

            double radius = 0;
            bool sideArcsOnly = false;
            int dotRadius = -1;
            int vSpan = 0;

            switch (styleIndex)
            {
                case 0: radius = 20.0; dotRadius = 1; break;
                case 1: radius = 14.0; dotRadius = 1; break;
                case 2: radius = 9.0;  dotRadius = 1; break;
                case 3: radius = 20.0; break;
                case 4: radius = 14.0; break;
                case 5: radius = 9.0;  break;
                case 6: radius = 20.0; dotRadius = 1; sideArcsOnly = true; break;
                case 7: radius = 14.0; dotRadius = 1; sideArcsOnly = true; break;
                case 8: radius = 9.0;  dotRadius = 1; sideArcsOnly = true; break;
                case 9: vSpan = 16;    break;
                case 10: vSpan = 11;   break;
                case 11: vSpan = 7;    break;
                case 12: vSpan = 12;   dotRadius = 1; break;
                case 13: dotRadius = 2; break;
                case 14: dotRadius = 1; break;
            }

            if (radius > 0)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        double dx = x - cx;
                        double dy = y - cy;
                        double dist = Math.Sqrt(dx * dx + dy * dy);
                        if (Math.Abs(dist - radius) <= 1.0)
                        {
                            if (!sideArcsOnly || Math.Abs(dy) <= Math.Abs(dx))
                            {
                                mask[y * size + x] = 1;
                            }
                        }
                    }
                }
            }

            if (vSpan > 0)
            {
                int vertexY = (dotRadius >= 0) ? (cy + 10) : (cy + vSpan / 2);
                for (int i = 0; i <= vSpan; i++)
                {
                    int y = vertexY - i;
                    int xLeft = cx - i;
                    int xRight = cx + i;

                    for (int t = 0; t <= 1; t++)
                    {
                        if (y >= 0 && y < size)
                        {
                            if (xLeft + t >= 0 && xLeft + t < size) mask[y * size + (xLeft + t)] = 1;
                            if (xRight - t >= 0 && xRight - t < size) mask[y * size + (xRight - t)] = 1;
                        }
                    }
                }
            }

            if (dotRadius >= 0)
            {
                for (int dy = -dotRadius; dy <= dotRadius; dy++)
                {
                    for (int dx = -dotRadius; dx <= dotRadius; dx++)
                    {
                        mask[(cy + dy) * size + (cx + dx)] = 1;
                    }
                }
            }

            return mask;
        }

        private byte[] BuildGoldSrcSpr(byte[] mask, Color col)
        {
            int width = 64, height = 64;
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(new byte[] { (byte)'I', (byte)'D', (byte)'S', (byte)'P' });
                bw.Write((int)2);
                bw.Write((int)2);
                bw.Write((int)3);
                float boundingRadius = (float)Math.Sqrt((width / 2.0) * (width / 2.0) + (height / 2.0) * (height / 2.0));
                bw.Write(boundingRadius);
                bw.Write((int)width);
                bw.Write((int)height);
                bw.Write((int)1);
                bw.Write((float)0.0f);
                bw.Write((int)0);

                bw.Write((short)256);
                byte[] palette = new byte[768];
                palette[3] = col.R;
                palette[4] = col.G;
                palette[5] = col.B;
                palette[255 * 3 + 0] = 0;
                palette[255 * 3 + 1] = 0;
                palette[255 * 3 + 2] = 255;
                bw.Write(palette);

                bw.Write((int)0);
                bw.Write((int)(-width / 2));
                bw.Write((int)(height / 2));
                bw.Write((int)width);
                bw.Write((int)height);

                byte[] pixels = new byte[width * height];
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = (mask[i] == 1) ? (byte)1 : (byte)255;
                }
                bw.Write(pixels);

                return ms.ToArray();
            }
        }

        private void DrawPreview(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            byte[] mask = GenerateCrosshairMask(cmbStyle.SelectedIndex);
            Color col = GetSelectedCrosshairColor();

            using (Pen gridPen = new Pen(Color.FromArgb(28, 26, 22), 1))
            {
                g.DrawLine(gridPen, pnlPreview.Width / 2, 0, pnlPreview.Width / 2, pnlPreview.Height);
                g.DrawLine(gridPen, 0, pnlPreview.Height / 2, pnlPreview.Width, pnlPreview.Height / 2);
            }

            int zoom = 2;
            int offsetX = (pnlPreview.Width - 64 * zoom) / 2;
            int offsetY = (pnlPreview.Height - 64 * zoom) / 2;

            using (Brush br = new SolidBrush(col))
            {
                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        if (mask[y * 64 + x] == 1)
                        {
                            g.FillRectangle(br, offsetX + x * zoom, offsetY + y * zoom, zoom, zoom);
                        }
                    }
                }
            }

            using (Pen borderPen = new Pen(activeAccent, 1))
            {
                g.DrawRectangle(borderPen, 0, 0, pnlPreview.Width - 1, pnlPreview.Height - 1);
            }
        }

        private void PatchWeaponTxtFiles(string spritesDir)
        {
            string[] txtFiles = Directory.GetFiles(spritesDir, "*.txt");
            Regex rxCrosshairAndZoom = new Regex(@"^(\s*(?:crosshair|zoom|autoaim)\s+\d+\s+)\S+\s+\d+\s+\d+\s+\d+\s+\d+(.*)$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            foreach (string txtPath in txtFiles)
            {
                string bakTxt = txtPath + ".bak";
                string sourceToRead = File.Exists(bakTxt) ? bakTxt : txtPath;
                string content = File.ReadAllText(sourceToRead);

                if (rxCrosshairAndZoom.IsMatch(content))
                {
                    if (!File.Exists(bakTxt))
                    {
                        File.Copy(txtPath, bakTxt, false);
                    }
                    string updated = rxCrosshairAndZoom.Replace(content, "${1}crosshairs\t0\t0\t64\t64${2}");
                    File.WriteAllText(txtPath, updated);
                }
            }
        }

        private string ResolveSpritesDir(string baseGameDir, string modFolder, out string resolvedModDir)
        {
            // Check baseGameDir/modFolder/sprites first
            string candidateModDir = Path.Combine(baseGameDir, modFolder);
            string candidateSprites = Path.Combine(candidateModDir, "sprites");
            if (Directory.Exists(candidateSprites))
            {
                resolvedModDir = candidateModDir;
                return candidateSprites;
            }

            // Or if the user selected the mod directory directly (e.g., .../Half-Life/cstrike)
            string directSprites = Path.Combine(baseGameDir, "sprites");
            if (Directory.Exists(directSprites))
            {
                resolvedModDir = baseGameDir;
                return directSprites;
            }

            resolvedModDir = candidateModDir;
            return candidateSprites;
        }

        private void InstallCrosshair(object sender, EventArgs e)
        {
            string gameDir = txtGamePath.Text.Trim();
            string modFolder = GetTargetModFolder();
            string resolvedModDir;
            string spritesDir = ResolveSpritesDir(gameDir, modFolder, out resolvedModDir);

            // For Blue Shift (bshift), if bshift/sprites doesn't exist or lacks crosshairs, create bshift/sprites or copy weapon_*.txt from valve/sprites if needed
            if (!Directory.Exists(spritesDir))
            {
                if (Directory.Exists(resolvedModDir))
                {
                    Directory.CreateDirectory(spritesDir);
                }
                else
                {
                    string errMsg = isRussian
                        ? string.Format("Папка '{0}/sprites' не найдена! Проверьте путь к Half-Life и наличие выбранного мода ({0}).", modFolder)
                        : string.Format("Folder '{0}/sprites' not found! Please check your Half-Life path and make sure the mod ({0}) is installed.", modFolder);
                    MessageBox.Show(errMsg, isRussian ? "Ошибка" : "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            byte[] mask = GenerateCrosshairMask(cmbStyle.SelectedIndex);
            Color col = GetSelectedCrosshairColor();
            byte[] sprBytes = BuildGoldSrcSpr(mask, col);

            string[] targets;
            if (cmbResolution.SelectedIndex == 1) targets = new[] { "crosshairs.spr", "640crosshairs.spr" };
            else if (cmbResolution.SelectedIndex == 2) targets = new[] { "1280crosshairs.spr" };
            else if (cmbResolution.SelectedIndex == 3) targets = new[] { "2560crosshairs.spr" };
            else targets = new[] { "crosshairs.spr", "640crosshairs.spr", "1280crosshairs.spr", "2560crosshairs.spr" };

            try
            {
                // If installing for an expansion (gearbox/bshift) that inherits base weapons from valve/sprites,
                // copy missing weapon_*.txt and hud.txt from valve/sprites so we can patch all weapon crosshairs cleanly
                string valveSprites = Path.Combine(gameDir, "valve", "sprites");
                if (Directory.Exists(valveSprites) && ! string.Equals(spritesDir, valveSprites, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (string vTxt in Directory.GetFiles(valveSprites, "*.txt"))
                    {
                        string destTxt = Path.Combine(spritesDir, Path.GetFileName(vTxt));
                        if (!File.Exists(destTxt))
                        {
                            File.Copy(vTxt, destTxt, false);
                        }
                    }
                }

                foreach (string t in targets)
                {
                    string dest = Path.Combine(spritesDir, t);
                    string bak = dest + ".bak";
                    if (File.Exists(dest) && !File.Exists(bak))
                    {
                        File.Copy(dest, bak, false);
                    }
                    File.WriteAllBytes(dest, sprBytes);
                }

                PatchWeaponTxtFiles(spritesDir);

                string userCfg = Path.Combine(resolvedModDir, "userconfig.cfg");
                if (!File.Exists(userCfg))
                {
                    File.WriteAllText(userCfg, "crosshair \"1\"\r\n");
                }

                lblStatus.ForeColor = Color.FromArgb(120, 255, 120);
                lblStatus.Text = isRussian
                    ? string.Format("Прицел успешно установлен в [{0}]! Перезапустите игру.", modFolder)
                    : string.Format("Crosshair installed into [{0}]! Please restart the game.", modFolder);
                MessageBox.Show(
                    isRussian
                        ? string.Format("Прицел успешно сгенерирован и установлен для [{0}]!", modFolder)
                        : string.Format("Selected crosshair has been generated and installed for [{0}]!", modFolder),
                    "Half-Life Crosshair Changer v5.1",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show((isRussian ? "Ошибка при установке прицела: " : "Error installing crosshair: ") + ex.Message, isRussian ? "Ошибка" : "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RestoreBackup(object sender, EventArgs e)
        {
            string gameDir = txtGamePath.Text.Trim();
            string modFolder = GetTargetModFolder();
            string resolvedModDir;
            string spritesDir = ResolveSpritesDir(gameDir, modFolder, out resolvedModDir);

            if (!Directory.Exists(spritesDir)) return;

            int restored = 0;
            foreach (string bakFile in Directory.GetFiles(spritesDir, "*.bak"))
            {
                string origFile = bakFile.Substring(0, bakFile.Length - 4);
                File.Copy(bakFile, origFile, true);
                restored++;
            }

            if (restored > 0)
            {
                lblStatus.ForeColor = activeAccent;
                lblStatus.Text = isRussian
                    ? string.Format("Стандартный прицел для [{0}] восстановлен.", modFolder)
                    : string.Format("Default crosshair for [{0}] restored.", modFolder);
                MessageBox.Show(
                    isRussian
                        ? string.Format("Стандартные прицелы для [{0}] успешно восстановлены из резервной копии.", modFolder)
                        : string.Format("Default crosshairs for [{0}] have been restored from backup.", modFolder),
                    isRussian ? "Восстановлено" : "Restored",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else
            {
                MessageBox.Show(
                    isRussian ? "Резервные копии (.bak) в выбранном моде не найдены." : "No backup files (.bak) found in the selected mod folder.",
                    isRussian ? "Информация" : "Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
