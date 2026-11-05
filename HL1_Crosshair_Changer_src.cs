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
        private Button btnLang;
        private Label lblEasterEgg;

        private bool isRussian = false; // Default: English
        private int secretClickCount = 0;

        // HL1 Classic Color Palette
        private readonly Color hlBgDark = Color.FromArgb(24, 21, 18);
        private readonly Color hlPanelBg = Color.FromArgb(36, 31, 26);
        private readonly Color hlAmber = Color.FromArgb(255, 160, 0);
        private readonly Color hlAmberBright = Color.FromArgb(255, 195, 40);
        private readonly Color hlBorder = Color.FromArgb(120, 80, 20);
        private readonly Color hlTextWhite = Color.FromArgb(240, 230, 215);

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
            this.Text = "Half-Life Crosshair Changer v5.0";
            this.Size = new Size(600, 495);
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
            AutoDetectGameAndResolution();
        }

        private Color GetSelectedCrosshairColor()
        {
            int idx = cmbColor.SelectedIndex;
            if (idx >= 0 && idx < presetColors.Length)
                return presetColors[idx];
            return customRgb;
        }

        private void BuildUI()
        {
            // Top Header Banner
            Panel pnlHeader = new Panel() { Location = new Point(0, 0), Size = new Size(600, 48), BackColor = Color.FromArgb(18, 15, 12) };
            pnlHeader.Paint += delegate(object s, PaintEventArgs e) {
                using (Pen p = new Pen(hlAmber, 2)) {
                    e.Graphics.DrawLine(p, 0, 46, 600, 46);
                }
            };
            Label lblTitle = new Label() {
                Text = "HALF-LIFE  //  CROSSHAIR CHANGER",
                Location = new Point(18, 13),
                AutoSize = true,
                Font = new Font("Trebuchet MS", 13.5f, FontStyle.Bold),
                ForeColor = hlAmber
            };
            Label lblVer = new Label() {
                Text = "v5.0",
                Location = new Point(435, 16),
                AutoSize = true,
                Font = new Font("Consolas", 10f, FontStyle.Bold),
                ForeColor = hlBorder
            };
            btnLang = CreateHLButton("EN / RU", new Point(485, 11), new Size(85, 25), false);
            btnLang.Click += delegate(object s, EventArgs e) {
                isRussian = !isRussian;
                ApplyLanguage();
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblVer);
            pnlHeader.Controls.Add(btnLang);

            // Game Path
            lblPath = new Label() { Location = new Point(20, 62), AutoSize = true, ForeColor = hlAmber };
            txtGamePath = new TextBox() { Location = new Point(20, 85), Width = 445, BackColor = hlPanelBg, ForeColor = hlTextWhite, BorderStyle = BorderStyle.FixedSingle };
            btnBrowse = CreateHLButton("Browse...", new Point(475, 83), new Size(95, 25), false);
            btnBrowse.Click += delegate(object s, EventArgs e) {
                using (var fbd = new FolderBrowserDialog()) {
                    if (fbd.ShowDialog() == DialogResult.OK) txtGamePath.Text = fbd.SelectedPath;
                }
            };

            // Resolution
            lblRes = new Label() { Location = new Point(20, 124), AutoSize = true, ForeColor = hlAmber };
            cmbResolution = new ComboBox() { Location = new Point(20, 147), Width = 355, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = hlPanelBg, ForeColor = hlTextWhite, FlatStyle = FlatStyle.Flat };

            // Crosshair Style
            lblStyle = new Label() { Location = new Point(20, 186), AutoSize = true, ForeColor = hlAmber };
            cmbStyle = new ComboBox() { Location = new Point(20, 209), Width = 355, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = hlPanelBg, ForeColor = hlTextWhite, FlatStyle = FlatStyle.Flat };
            cmbStyle.SelectedIndexChanged += delegate(object s, EventArgs e) { pnlPreview.Invalidate(); };

            // Crosshair Color + Custom Picker
            lblColor = new Label() { Location = new Point(20, 248), AutoSize = true, ForeColor = hlAmber };
            cmbColor = new ComboBox() { Location = new Point(20, 271), Width = 235, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = hlPanelBg, ForeColor = hlTextWhite, FlatStyle = FlatStyle.Flat };
            cmbColor.SelectedIndexChanged += delegate(object s, EventArgs e) {
                if (cmbColor.SelectedIndex == presetColors.Length) {
                    PickCustomColor();
                }
                pnlPreview.Invalidate();
            };

            btnCustomColor = CreateHLButton("RGB Palette...", new Point(265, 270), new Size(110, 24), false);
            btnCustomColor.Click += delegate(object s, EventArgs e) {
                PickCustomColor();
                cmbColor.SelectedIndex = presetColors.Length;
                pnlPreview.Invalidate();
            };

            // Preview Box
            lblPrev = new Label() { Location = new Point(400, 124), AutoSize = true, ForeColor = hlAmber };
            pnlPreview = new Panel() { Location = new Point(400, 147), Size = new Size(170, 150), BackColor = Color.FromArgb(14, 13, 11), BorderStyle = BorderStyle.FixedSingle };
            pnlPreview.Paint += DrawPreview;

            // Main Action Buttons
            btnInstall = CreateHLButton("INSTALL CROSSHAIR", new Point(20, 325), new Size(275, 44), true);
            btnInstall.Click += InstallCrosshair;

            btnRestore = CreateHLButton("RESTORE DEFAULT", new Point(310, 325), new Size(260, 44), false);
            btnRestore.Click += RestoreBackup;

            // Status & Footer Credits
            lblStatus = new Label() { Location = new Point(20, 385), Size = new Size(550, 22), ForeColor = hlAmberBright };

            // Secret Bottom-Left Easter Egg Zone (5 clicks -> "Kosh=💩")
            lblEasterEgg = new Label() {
                Text = "",
                Location = new Point(18, 416),
                Size = new Size(160, 32),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI Emoji", 10.5f, FontStyle.Bold),
                ForeColor = hlAmberBright,
                BackColor = Color.Transparent
            };
            lblEasterEgg.MouseDown += delegate(object s, MouseEventArgs e) {
                TriggerEasterEggClick();
            };

            // Also capture clicks on the form's bottom-left corner area directly
            this.MouseDown += delegate(object s, MouseEventArgs e) {
                if (e.X <= 150 && e.Y >= this.ClientSize.Height - 60)
                {
                    TriggerEasterEggClick();
                }
            };

            Label lblCredits = new Label() {
                Text = "By FFBeaver and Gemini",
                Location = new Point(385, 422),
                Size = new Size(185, 22),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Trebuchet MS", 9.5f, FontStyle.Bold),
                ForeColor = hlAmber
            };

            this.Controls.AddRange(new Control[] {
                pnlHeader, lblPath, txtGamePath, btnBrowse,
                lblRes, cmbResolution, lblStyle, cmbStyle,
                lblColor, cmbColor, btnCustomColor,
                lblPrev, pnlPreview, btnInstall, btnRestore,
                lblStatus, lblEasterEgg, lblCredits
            });
        }

        private void TriggerEasterEggClick()
        {
            secretClickCount++;
            if (secretClickCount >= 5)
            {
                // Unicode surrogate pair for Pile of Poo emoji \uD83D\uDCA9
                lblEasterEgg.Text = "Kosh=\uD83D\uDCA9";
            }
        }

        private void ApplyLanguage()
        {
            int resIdx = Math.Max(0, cmbResolution.SelectedIndex);
            int styleIdx = (cmbStyle.SelectedIndex >= 0) ? cmbStyle.SelectedIndex : 6; // Default to (.) crosshair or Medium Circle
            int colIdx = Math.Max(0, cmbColor.SelectedIndex);

            cmbResolution.Items.Clear();
            cmbStyle.Items.Clear();
            cmbColor.Items.Clear();

            if (isRussian)
            {
                btnLang.Text = "Язык: RU";
                lblPath.Text = "Директория игры Half-Life:";
                btnBrowse.Text = "Обзор...";
                lblRes.Text = "Масштаб интерфейса (Разрешение):";
                lblStyle.Text = "Форма прицела:";
                lblColor.Text = "Цвет прицела:";
                btnCustomColor.Text = "Палитра RGB...";
                lblPrev.Text = "Предпросмотр:";
                btnInstall.Text = "УСТАНОВИТЬ ПРИЦЕЛ";
                btnRestore.Text = "ВЕРНУТЬ СТАНДАРТНЫЙ";
                lblStatus.Text = "Система готова к установке прицела.";

                cmbResolution.Items.AddRange(resNamesRU);
                cmbStyle.Items.AddRange(styleNamesRU);
                cmbColor.Items.AddRange(colorNamesRU);
            }
            else
            {
                btnLang.Text = "Lang: EN";
                lblPath.Text = "Half-Life Game Directory:";
                btnBrowse.Text = "Browse...";
                lblRes.Text = "HUD Scale (Screen Resolution):";
                lblStyle.Text = "Crosshair Shape:";
                lblColor.Text = "Crosshair Color:";
                btnCustomColor.Text = "RGB Palette...";
                lblPrev.Text = "Live Preview:";
                btnInstall.Text = "INSTALL CROSSHAIR";
                btnRestore.Text = "RESTORE DEFAULT";
                lblStatus.Text = "System ready to install crosshair.";

                cmbResolution.Items.AddRange(resNamesEN);
                cmbStyle.Items.AddRange(styleNamesEN);
                cmbColor.Items.AddRange(colorNamesEN);
            }

            cmbResolution.SelectedIndex = resIdx;
            cmbStyle.SelectedIndex = styleIdx;
            cmbColor.SelectedIndex = colIdx;
        }

        private Button CreateHLButton(string text, Point loc, Size sz, bool isPrimary)
        {
            Button btn = new Button() {
                Text = text,
                Location = loc,
                Size = sz,
                BackColor = isPrimary ? Color.FromArgb(185, 95, 0) : hlPanelBg,
                ForeColor = isPrimary ? Color.White : hlAmber,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Tahoma", isPrimary ? 10f : 8.75f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = hlAmber;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = isPrimary ? Color.FromArgb(225, 125, 10) : Color.FromArgb(60, 48, 32);
            return btn;
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

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Half-Life\Settings"))
                {
                    if (key != null)
                    {
                        object wObj = key.GetValue("ScreenWidth");
                        object hObj = key.GetValue("ScreenHeight");
                        if (wObj != null && hObj != null)
                        {
                            int w = Convert.ToInt32(wObj);
                            int h = Convert.ToInt32(hObj);
                            lblStatus.Text = isRussian
                                ? string.Format("Обнаружено игровое разрешение: {0}x{1}", w, h)
                                : string.Format("Detected in-game resolution: {0}x{1}", w, h);
                        }
                    }
                }
            }
            catch { }
        }

        // Generates the 64x64 pixel matrix for both the Preview and the GoldSrc .spr compiler
        private byte[] GenerateCrosshairMask(int styleIndex)
        {
            int size = 64;
            int cx = 32, cy = 32;
            byte[] mask = new byte[size * size];

            double radius = 0;
            bool sideArcsOnly = false; // True for (.) style (two side quarter-circles + center dot)
            int dotRadius = -1;        // -1 = no dot, 1 = 3x3 dot, 2 = 5x5 dot
            int vSpan = 0;             // >0 = V-shaped chevron arm length

            switch (styleIndex)
            {
                case 0: radius = 20.0; dotRadius = 1; break; // Large Circle + Dot
                case 1: radius = 14.0; dotRadius = 1; break; // Medium Circle + Dot
                case 2: radius = 9.0;  dotRadius = 1; break; // Small Circle + Dot
                case 3: radius = 20.0; break;                // Large Circle
                case 4: radius = 14.0; break;                // Medium Circle
                case 5: radius = 9.0;  break;                // Small Circle
                case 6: radius = 20.0; dotRadius = 1; sideArcsOnly = true; break; // Large (.) + Dot
                case 7: radius = 14.0; dotRadius = 1; sideArcsOnly = true; break; // Medium (.) + Dot
                case 8: radius = 9.0;  dotRadius = 1; sideArcsOnly = true; break; // Small (.) + Dot
                case 9: vSpan = 16;    break;                // Large V-Chevron
                case 10: vSpan = 11;   break;                // Medium V-Chevron
                case 11: vSpan = 7;    break;                // Small V-Chevron
                case 12: vSpan = 12;   dotRadius = 1; break; // Medium V-Chevron + Dot
                case 13: dotRadius = 2; break;               // Bold Dot (5x5)
                case 14: dotRadius = 1; break;               // Small Dot (3x3)
            }

            // Draw Full Circle or Side Quarter-Circle Brackets (.)
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
                            if (!sideArcsOnly)
                            {
                                mask[y * size + x] = 1;
                            }
                            else
                            {
                                // Left and right 90-degree quarter-circle arcs: |dy| <= |dx|
                                if (Math.Abs(dy) <= Math.Abs(dx))
                                {
                                    mask[y * size + x] = 1;
                                }
                            }
                        }
                    }
                }
            }

            // Draw V-Shaped Crosshair
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

            // Draw Center Dot
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

            using (Pen borderPen = new Pen(hlBorder, 1))
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

        private void InstallCrosshair(object sender, EventArgs e)
        {
            string gameDir = txtGamePath.Text.Trim();
            string spritesDir = Path.Combine(gameDir, "valve", "sprites");

            if (!Directory.Exists(spritesDir))
            {
                string errMsg = isRussian
                    ? "Папка 'valve/sprites' не найдена! Укажите корректный путь к папке Half-Life."
                    : "Folder 'valve/sprites' not found! Please select a valid Half-Life directory.";
                MessageBox.Show(errMsg, isRussian ? "Ошибка" : "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
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

                string userCfg = Path.Combine(gameDir, "valve", "userconfig.cfg");
                if (!File.Exists(userCfg))
                {
                    File.WriteAllText(userCfg, "crosshair \"1\"\r\n");
                }

                lblStatus.ForeColor = Color.FromArgb(120, 255, 120);
                lblStatus.Text = isRussian
                    ? "Прицел успешно установлен! Перезапустите Half-Life."
                    : "Crosshair installed successfully! Please restart Half-Life.";
                MessageBox.Show(
                    isRussian ? "Выбранный прицел успешно сгенерирован и установлен в игру!" : "Selected crosshair has been generated and installed into the game!",
                    "Half-Life Crosshair Changer",
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
            string spritesDir = Path.Combine(txtGamePath.Text.Trim(), "valve", "sprites");
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
                lblStatus.ForeColor = hlAmberBright;
                lblStatus.Text = isRussian ? "Стандартный прицел успешно восстановлен." : "Default crosshair restored successfully.";
                MessageBox.Show(
                    isRussian ? "Стандартные прицелы Half-Life успешно восстановлены из резервной копии." : "Default Half-Life crosshairs have been restored from backup.",
                    isRussian ? "Восстановлено" : "Restored",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else
            {
                MessageBox.Show(
                    isRussian ? "Резервные копии (.bak) не найдены." : "No backup files (.bak) found.",
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
