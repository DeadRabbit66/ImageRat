using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace ImageRat
{
    public partial class Form1 : Form
    {
        private TextBox? _txtInput;

        public Form1()
        {
            InitializeComponent();
            SetupCustomWindow();
            CreateControls();
        }

        private void SetupCustomWindow()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(550, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.Black;

            Panel titleBar = new Panel
            {
                Height = 35,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(40, 40, 40)
            };

            Label titleLabel = new Label
            {
                Text = "IMAGE RAT",
                ForeColor = Color.Lime,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(10, 8),
                AutoSize = true
            };

            Button minimizeButton = new Button
            {
                Text = "—",
                Size = new Size(35, 35),
                Location = new Point(this.Width - 70, 0),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Lime,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            minimizeButton.FlatAppearance.BorderSize = 0;
            minimizeButton.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            Button closeButton = new Button
            {
                Text = "✕",
                Size = new Size(35, 35),
                Location = new Point(this.Width - 35, 0),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Lime,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => Application.Exit();

            titleBar.Controls.Add(titleLabel);
            titleBar.Controls.Add(minimizeButton);
            titleBar.Controls.Add(closeButton);
            this.Controls.Add(titleBar);
            this.AllowDrop = true;
            this.DragEnter += Form_DragEnter;
            this.DragDrop += Form_DragDrop;
            this.Icon = Properties.Resources.Rat;

            titleBar.MouseDown += TitleBar_MouseDown;
        }
                    private void Form_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data!.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void Form_DragDrop(object? sender, DragEventArgs e)
        {
            string[]? files = e.Data!.GetData(DataFormats.FileDrop) as string[];
            if (files == null || files.Length == 0) return;

            string filePath = files[0];
            string ext = Path.GetExtension(filePath).ToLower();

            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp")
            {
                ExtractTextFromImage(filePath);
            }
            else
            {
                MessageBox.Show("Пожалуйста, перетащите файл изображения (PNG, JPG, BMP).",
                    "Неверный формат", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ExtractTextFromImage(string filePath)
        {
            try
            {
                byte[] imageBytes = File.ReadAllBytes(filePath);
                string extractedText = SteganographyHelper.ExtractText(imageBytes);

                if (string.IsNullOrEmpty(extractedText))
                {
                    MessageBox.Show("В этой картинке не найдено скрытых сообщений",
                        "Не найдено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _txtInput!.Text = extractedText;
                MessageBox.Show("Текст успешно извлечён", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при извлечении: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        

        private void CreateControls()
        {
            _txtInput = new TextBox
            {
                Location = new Point(20, 50),
                Size = new Size(500, 180),
                Multiline = true,
                Font = new Font("Consolas", 10),
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                BorderStyle = BorderStyle.FixedSingle
            };

            Button btnHide = new Button
            {
                Text = "BURY text into image",
                Size = new Size(220, 50),
                Location = new Point(20, 270),
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                FlatStyle = FlatStyle.Flat
            };
            btnHide.FlatAppearance.BorderColor = Color.Lime;
            btnHide.FlatAppearance.BorderSize = 1;
            btnHide.Click += BtnHide_Click;

            Button btnExtract = new Button
            {
                Text = "DIG from image",
                Size = new Size(220, 50),
                Location = new Point(300, 270),
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                FlatStyle = FlatStyle.Flat
            };
            btnExtract.FlatAppearance.BorderColor = Color.Lime;
            btnExtract.FlatAppearance.BorderSize = 1;
            btnExtract.Click += BtnExtract_Click;

            Button btnHelp = new Button
            {
                Text = "❓ Help",
                Size = new Size(80, 30),
                Location = new Point(460, 460),
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                FlatStyle = FlatStyle.Flat
            };
            btnHelp.FlatAppearance.BorderColor = Color.Lime;
            btnHelp.FlatAppearance.BorderSize = 1;
            btnHelp.Click += (s, e) =>
            {
                string path = System.IO.Path.Combine(Application.StartupPath, "README.txt");
                if (System.IO.File.Exists(path))
                    System.Diagnostics.Process.Start("notepad.exe", path);
                else
                    MessageBox.Show("Файл README.txt не найден!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };
            this.BackgroundImage = Properties.Resources.background;
            this.BackgroundImageLayout = ImageLayout.Stretch;

            this.Controls.Add(_txtInput);
            this.Controls.Add(btnHide);
            this.Controls.Add(btnExtract);
            this.Controls.Add(btnHelp);
        }

        private void BtnHide_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtInput?.Text))
            {
                MessageBox.Show("Введите текст для сокрытия", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Выберите картинку";
            ofd.Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp|Все файлы|*.*";

            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                Image img = Image.FromFile(ofd.FileName);

                if (img.RawFormat.Equals(ImageFormat.Jpeg))
                {
                    Bitmap png = new Bitmap(img.Width, img.Height);
                    using (Graphics g = Graphics.FromImage(png))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(img, 0, 0, img.Width, img.Height);
                    }
                    img.Dispose();
                    img = png;
                }

                if (!SteganographyHelper.CanHide(_txtInput.Text, img, out string error))
                {
                    MessageBox.Show(error, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                byte[] result = SteganographyHelper.HideText(_txtInput.Text, img);

                using SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "Сохранить картинку";
                sfd.Filter = "PNG Image|*.png";
                sfd.FileName = Path.GetFileNameWithoutExtension(ofd.FileName) + "_enc.png";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(sfd.FileName, result);
                    MessageBox.Show("Текст успешно спрятан!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void BtnExtract_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Выберите картинку со скрытым текстом";
            ofd.Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp|Все файлы|*.*";

            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                byte[] imageBytes = File.ReadAllBytes(ofd.FileName);
                string extractedText = SteganographyHelper.ExtractText(imageBytes);

                if (string.IsNullOrEmpty(extractedText))
                {
                    MessageBox.Show("В этой картинке не найдено скрытого текста!",
                        "Не найдено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _txtInput!.Text = extractedText;
                MessageBox.Show("Текст успешно извлечён!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }
    }
}