using System;
using System.Drawing;
using System.Windows.Forms;

namespace CrawlFB_PW._1._0.Page
{
    public partial class PopupAuto : Form
    {
        public static PopupAuto Instance;

        private Label lblPage;
        private Label lblTotalPages;
        private Label lblCompleted;
        private Label lblPosts;
        private Label lblCountdown;

        private ProgressBar progressBar;

        private Timer countdownTimer;
        private int remainingSeconds = 0;
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
            this.Size = new Size(360, 260);
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;
            this.BackColor = Color.White;
            this.ShowInTaskbar = false;

            // Shadow effect
            this.Padding = new Padding(1);
            this.BackColor = Color.Gray;

            var container = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
            };
            this.Controls.Add(container);

            // Header gradient
            var header = new Panel()
            {
                Height = 50,
                Dock = DockStyle.Top,
                BackColor = ColorTranslator.FromHtml("#0078FF")
            };
            container.Controls.Add(header);

            var lblHeader = new Label()
            {
                Text = "AUTO GIÁM SÁT",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            header.Controls.Add(lblHeader);

            lblPage = CreateLabel(container, 65);
            lblTotalPages = CreateLabel(container, 95);
            lblCompleted = CreateLabel(container, 125);
            lblPosts = CreateLabel(container, 155);

            lblCountdown = new Label()
            {
                Text = "Chạy lại sau: --",
                ForeColor = ColorTranslator.FromHtml("#FF6600"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 190)
            };
            container.Controls.Add(lblCountdown);

            progressBar = new ProgressBar()
            {
                Width = 300,
                Height = 12,
                Location = new Point(20, 220),
                Style = ProgressBarStyle.Continuous
            };
            container.Controls.Add(progressBar);

            countdownTimer = new Timer();
            countdownTimer.Interval = 1000; // 1s
            countdownTimer.Tick += CountdownTick;
            trayIcon = new NotifyIcon();
            trayIcon.Icon = SystemIcons.Information;
            trayIcon.Visible = true;
            trayIcon.Text = "Auto Giám Sát đang chạy";

            trayIcon.DoubleClick += (s, e) =>
            {
                this.Show();
                this.TopMost = true;
                this.BringToFront();
            };

            // close form click
            this.Click += (s, e) => this.Hide();
            header.Click += (s, e) => this.Hide();
            lblHeader.Click += (s, e) => this.Hide();
        }

        private Label CreateLabel(Control parent, int y)
        {
            var lbl = new Label()
            {
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(20, y),
                Text = "--"
            };

            parent.Controls.Add(lbl);
            return lbl;
        }

        private void CountdownTick(object sender, EventArgs e)
        {
            if (remainingSeconds > 0)
            {
                remainingSeconds--;
                lblCountdown.Text = $"Chạy lại sau: {remainingSeconds}s";

                progressBar.Value = Math.Max(0, Math.Min(100,
                    (int)((remainingSeconds * 1.0 / countdownMax) * 100)));
            }
            else
            {
                countdownTimer.Stop();
                lblCountdown.Text = $"Đang chờ tác vụ kế tiếp...";
            }
        }

        private int countdownMax = 0;

        public void StartCountdown(int seconds)
        {
            countdownMax = seconds;
            remainingSeconds = seconds;
            countdownTimer.Start();
        }
        public void ShowPopup()
        {
            try
            {
                if (!this.Visible)
                {
                    this.Show();
                    this.BringToFront();
                }
                else
                {
                    // cửa sổ đang hiện → chỉ đưa lên trước
                    this.BringToFront();
                }
            }
            catch { }
        }
        public void InitEmpty()
        {
            lblPage.Text = "Đang chạy: --";
            lblTotalPages.Text = "Tổng Page: --";
            lblCompleted.Text = "Hoàn thành: --";
            lblPosts.Text = "Tổng bài mới: --";
            lblCountdown.Text = "Chạy lại sau: --";

            progressBar.Value = 0;
        }

        public void UpdateProgress(string runningPage, int totalPages, int completedPages, int totalPosts)
        {
            lblPage.Text = $"Đang chạy: {runningPage}";
            lblTotalPages.Text = $"Tổng Page: {totalPages}";
            lblCompleted.Text = $"Hoàn thành: {completedPages}";
            lblPosts.Text = $"Tổng bài mới: {totalPosts}";

            this.Show();
            this.BringToFront();
        }
        public static PopupAuto Ensure()
        {
            if (Instance == null || Instance.IsDisposed)
                Instance = new PopupAuto();

            return Instance;
        }
        private bool dragging = false;
        private Point dragCursor;
        private Point dragForm;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            dragging = true;
            dragCursor = Cursor.Position;
            dragForm = this.Location;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (dragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(dragCursor));
                this.Location = Point.Add(dragForm, new Size(diff));
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            dragging = false;
        }
        private void btnHide_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
