using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.Utils.Svg;
using CrawlFB_PW._1._0.DAO.Server;

namespace CrawlFB_PW._1._0
{
    public partial class FLogin : XtraForm
    {
        private bool isShowPass = false;
        private SvgImage eyeOpen;
        private SvgImage eyeClose;

        public FLogin()
        {
            InitializeComponent();
            InitUI();
        }

        #region UI INIT

        private PanelControl panelLogin;
        private TextEdit txtUser;
        private TextEdit txtPass;
        private SimpleButton btnLogin;
        private SimpleButton btnRegister;
        private SimpleButton btnShowPass;
        private LabelControl lblTitle;

        private void InitUI()
        {
            this.Size = new Size(500, 400);
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(240, 242, 245);

            // bo góc form
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            // ===== CARD =====
            panelLogin = new PanelControl();
            panelLogin.Size = new Size(320, 260);
            panelLogin.Location = new Point((this.Width - 320) / 2, (this.Height - 260) / 2);
            panelLogin.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            panelLogin.Appearance.BackColor = Color.White;

            panelLogin.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 320, 260, 20, 20));

            this.Controls.Add(panelLogin);

            // ===== TITLE =====
            lblTitle = new LabelControl();
            lblTitle.Text = "Đăng nhập";
            lblTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTitle.Location = new Point(110, 20);

            panelLogin.Controls.Add(lblTitle);

            Panel lineUser = new Panel();
            lineUser.BackColor = Color.Gray;
            lineUser.Size = new Size(240, 1);
            lineUser.Location = new Point(40, 100);
            lineUser.BackColor = Color.FromArgb(180, 180, 180);
            panelLogin.Controls.Add(lineUser);

            Panel linePass = new Panel();
            linePass.BackColor = Color.Gray;
            linePass.Size = new Size(240, 1);
            linePass.Location = new Point(40, 140);
            linePass.BackColor = Color.FromArgb(180, 180, 180);
            panelLogin.Controls.Add(linePass);
            // ===== USER =====
            txtUser = new TextEdit();
            txtUser.Size = new Size(240, 30);
            txtUser.Location = new Point(40, 70);
            txtUser.Properties.NullValuePrompt = "Tài khoản";
            txtUser.Properties.NullValuePromptShowForEmptyValue = true;
            txtUser.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            txtUser.GotFocus += (s, e) => lineUser.BackColor = Color.FromArgb(0, 120, 215);
            txtUser.LostFocus += (s, e) => lineUser.BackColor = Color.Gray;
            panelLogin.Controls.Add(txtUser);

            // ===== PASS =====
            txtPass = new TextEdit();
            txtPass.Size = new Size(210, 30); // 🔥 giảm width
            txtPass.Location = new Point(40, 110);
            txtPass.Properties.UseSystemPasswordChar = true;
            txtPass.Properties.NullValuePrompt = "Mật khẩu";
            txtPass.Properties.NullValuePromptShowForEmptyValue = true;
            txtPass.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            txtPass.GotFocus += (s, e) => linePass.BackColor = Color.FromArgb(0, 120, 215);
            txtPass.LostFocus += (s, e) => linePass.BackColor = Color.Gray;
            panelLogin.Controls.Add(txtPass);

            // ===== BUTTON EYE =====
            btnShowPass = new SimpleButton();
            btnShowPass.Size = new Size(28, 30);

            // 🔥 đặt sát mép textbox
            btnShowPass.Location = new Point(
                txtPass.Right + 2,
                txtPass.Top
            );

            btnShowPass.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            btnShowPass.Appearance.BackColor = Color.Transparent;

            panelLogin.Controls.Add(btnShowPass);

            // ===== LOGIN =====
            btnLogin = new SimpleButton();
            btnLogin.Text = "Đăng nhập";
            btnLogin.Size = new Size(240, 35);
            btnLogin.Location = new Point(40, 160);
            btnLogin.Appearance.BackColor = Color.FromArgb(0, 120, 215);
            btnLogin.Appearance.ForeColor = Color.White;
            btnLogin.Appearance.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnLogin.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            btnLogin.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 240, 35, 10, 10));
            btnLogin.AppearanceHovered.BackColor = Color.FromArgb(0, 100, 200);
          
            panelLogin.Controls.Add(btnLogin);
           
            // ===== REGISTER =====
            btnRegister = new SimpleButton();
            btnRegister.Text = "Đăng ký";
            btnRegister.Size = new Size(240, 35);
            btnRegister.Location = new Point(40, 205);
            btnRegister.Appearance.BackColor = Color.FromArgb(40, 167, 69);
            btnRegister.Appearance.ForeColor = Color.White;
            btnRegister.Appearance.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnRegister.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            btnRegister.AppearanceHovered.BackColor = Color.FromArgb(30, 150, 60);
            btnRegister.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 240, 35, 10, 10));

            panelLogin.Controls.Add(btnRegister);

            // ===== SVG ICON =====
            eyeOpen = LoadSvg(svgEyeOpen);
            eyeClose = LoadSvg(svgEyeClose);
            btnShowPass.ImageOptions.SvgImage = eyeClose;

            // ===== EVENTS =====
            btnShowPass.Click += TogglePassword;
            btnLogin.Click += Login_Click;

            this.AcceptButton = btnLogin;
            LabelControl lblBrand = new LabelControl();
            lblBrand.Text = "Tool GSM By TutjPro";
            lblBrand.Appearance.Font = new Font("Segoe UI", 8, FontStyle.Italic);
            lblBrand.Appearance.ForeColor = Color.Gray;

            // căn giữa dưới form
            lblBrand.AutoSizeMode = LabelAutoSizeMode.Horizontal;
            lblBrand.Location = new Point(
                (this.Width - lblBrand.Width) / 2,
                this.Height - 40
            );

            this.Controls.Add(lblBrand);
            this.MouseDown += DragForm;
            panelLogin.MouseDown += DragForm;
        }

        #endregion

        #region LOGIN

        private void Login_Click(object sender, EventArgs e)
        {
            string user = txtUser.Text.Trim();
            string pass = txtPass.Text.Trim();

            var userId = SQLDAO_Server.Instance.Login(user, pass);

            if (userId == null)
            {
                XtraMessageBox.Show("Sai tài khoản!", "Lỗi");
                return;
            }

            Session.CurrentUserId = userId.Value;

            string role = SQLDAO_Server.Instance.GetRole(userId.Value);

            this.Hide();

            if (role == "Admin")
                new FMain().Show();
            else
                new FUser_Main().Show();
        }

        #endregion

        #region PASSWORD TOGGLE

        private void TogglePassword(object sender, EventArgs e)
        {
            isShowPass = !isShowPass;

            txtPass.Properties.UseSystemPasswordChar = !isShowPass;
            btnShowPass.ImageOptions.SvgImage = isShowPass ? eyeOpen : eyeClose;
        }

        #endregion

        #region SVG

        string svgEyeOpen = @"<svg viewBox='0 0 24 24'><path fill='#555' d='M12 5C7 5 2.73 8.11 1 12c1.73 3.89 6 7 11 7s9.27-3.11 11-7c-1.73-3.89-6-7-11-7zm0 12a5 5 0 1 1 0-10 5 5 0 0 1 0 10zm0-8a3 3 0 1 0 0 6 3 3 0 0 0 0-6z'/></svg>";

        string svgEyeClose = @"<svg viewBox='0 0 24 24'><path fill='#555' d='M2 5.27L3.28 4 20 20.72 18.73 22l-2.05-2.05C15.1 20.62 13.58 21 12 21c-5 0-9.27-3.11-11-7 1.1-2.48 3.18-4.5 5.73-5.73L2 5.27z'/></svg>";

        private SvgImage LoadSvg(string svg)
        {
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(svg)))
            {
                return SvgImage.FromStream(ms);
            }
        }

        #endregion

        #region BO GÓC

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect,
            int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);

        #endregion
        [DllImport("user32.DLL")]
        private static extern void ReleaseCapture();

        [DllImport("user32.DLL")]
        private static extern void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);
        private void DragForm(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }
    }
}