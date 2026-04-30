using System;
using System.Drawing;
using System.Windows.Forms;

namespace CrawlFB_PW._1._0.Page
{
    public partial class PopupAuto : Form
    {
        public static PopupAuto Instance;

        private Label lblTotal;
        private Label lblTab;
        private Label lblNew;
        private Label lblSaved;

        private ProgressBar progressBar;
        private NotifyIcon trayIcon;

        public PopupAuto()
        {
            InitializeComponent();
            Instance = this;

            BuildUI();
        }

        private void BuildUI()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(320, 220);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.ShowInTaskbar = false;

            var container = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(1)
            };
            this.Controls.Add(container);

            // ===== HEADER =====
            var header = new Panel()
            {
                Height = 40,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(0, 120, 255)
            };
            container.Controls.Add(header);

            var lblHeader = new Label()
            {
                Text = "🚀 AUTO MONITOR",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Left,
                Width = 200,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            header.Controls.Add(lblHeader);

            // ===== NÚT MINIMIZE (—) =====
            var btnMin = new Label()
            {
                Text = "—",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = false,
                Width = 30,
                Dock = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            btnMin.Click += (s, e) => HideToTray();
            header.Controls.Add(btnMin);

            // ===== NÚT CLOSE (X) =====
            var btnClose = new Label()
            {
                Text = "X",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = false,
                Width = 30,
                Dock = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => this.Hide(); // ❗ KHÔNG close app
            header.Controls.Add(btnClose);

            int y = 55;

            lblTotal = CreateCard(container, y, "📄 Tổng page"); y += 35;
            lblTab = CreateCard(container, y, "🧠 Tab đang chạy"); y += 35;
            lblNew = CreateCard(container, y, "🆕 Bài mới"); y += 35;
            lblSaved = CreateCard(container, y, "💾 Đã lưu"); y += 35;

            progressBar = new ProgressBar()
            {
                Width = 260,
                Height = 8,
                Location = new Point(30, y + 5)
            };
            container.Controls.Add(progressBar);

            InitTray();
            EnableDrag(header);
        }
        public void UpdateStartCountdown(int seconds)
        {
            this.Text = $"⏳ Chạy sau: {seconds}s";
        }
        private Label CreateCard(Control parent, int y, string title)
        {
            var lbl = new Label()
            {
                Text = $"{title}: 0",
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(30, y)
            };

            parent.Controls.Add(lbl);
            return lbl;
        }

        // ===== TRAY =====
        private void InitTray()
        {
            trayIcon = new NotifyIcon();
            trayIcon.Icon = SystemIcons.Application;
            trayIcon.Visible = true;
            trayIcon.Text = "Auto Monitor đang chạy";

            trayIcon.DoubleClick += (s, e) => ShowFromTray();

            var menu = new ContextMenuStrip();
            menu.Items.Add("Mở lại", null, (s, e) => ShowFromTray());
            menu.Items.Add("Thoát", null, (s, e) =>
            {
                trayIcon.Visible = false;
                Application.Exit();
            });

            trayIcon.ContextMenuStrip = menu;
        }

        private void HideToTray()
        {
            this.Hide();

            trayIcon.BalloonTipTitle = "Auto Monitor";
            trayIcon.BalloonTipText = "Đã thu nhỏ xuống khay hệ thống";
            trayIcon.ShowBalloonTip(1000);
        }

        private void ShowFromTray()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Activate();
        }

        // ===== UPDATE =====
        public void UpdateProgress(int totalPages, int runningTabs, int totalNew, int totalSaved)
        {
            lblTotal.Text = $"📄 Tổng page: {totalPages}";
            lblTab.Text = $"🧠 Tab đang chạy: {runningTabs}";
            lblNew.Text = $"🆕 Bài mới: {totalNew}";
            lblSaved.Text = $"💾 Đã lưu: {totalSaved}";

            int percent = totalPages == 0 ? 0 : Math.Min(100, (totalNew * 100) / (totalPages * 10));
            progressBar.Value = percent;

            if (!this.Visible)
                this.Show();
        }

        public void InitEmpty()
        {
            UpdateProgress(0, 0, 0, 0);
        }

        public static PopupAuto Ensure()
        {
            if (Instance == null || Instance.IsDisposed)
                Instance = new PopupAuto();

            return Instance;
        }

        // ===== DRAG WINDOW =====
        private void EnableDrag(Control ctrl)
        {
            bool dragging = false;
            Point start = Point.Empty;

            ctrl.MouseDown += (s, e) =>
            {
                dragging = true;
                start = new Point(e.X, e.Y);
            };

            ctrl.MouseMove += (s, e) =>
            {
                if (dragging)
                {
                    this.Left += e.X - start.X;
                    this.Top += e.Y - start.Y;
                }
            };

            ctrl.MouseUp += (s, e) => dragging = false;
        }
    }
}