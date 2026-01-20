using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DTO;


namespace CrawlFB_PW._1._0.Profile
{
    public partial class ProfileStartup : Form
    {
        private ProgressBar progress;
        private Label lblStatus;

        public ProfileStartup()
        {
            BuildUI();
            this.Shown += async (s, e) => await RunStartupAsync(); // ✅ tự chạy khi form hiển thị
        }

        private void BuildUI()
        {
            this.Text = "Cài đặt hệ thống (Profile Startup)";
            this.Size = new Size(500, 200);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblStatus = new Label
            {
                AutoSize = true,
                Location = new Point(40, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.Gray,
                Text = "🔄 Đang chuẩn bị kiểm tra trạng thái profile..."
            };
            this.Controls.Add(lblStatus);

            progress = new ProgressBar
            {
                Location = new Point(40, 80),
                Size = new Size(400, 25),
                Minimum = 0,
                Maximum = 100
            };
            this.Controls.Add(progress);
        }

        private async Task RunStartupAsync()
        {
            try
            {
                string profileDbPath = Path.Combine(
                    PathHelper.Instance.GetProfilesFolder(),
                    "ProfileInfo.db"
                );

                Libary.Instance.CreateLog("📌 DB2 path = " + profileDbPath);

                var profileDao = new ProfileInfoDAO();
                var profiles = profileDao.GetAllProfiles();

                // ❗ Nếu không có profile → mở FSetupProfile
                if (profiles == null || profiles.Count == 0)
                {
                    MessageBox.Show("⚠ Chưa có profile nào. Hãy tạo mới!", "Thông báo");

                    this.Hide();
                    using (var f = new FsetupProfile())
                        f.ShowDialog();

                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    return;
                }

                var adsMgr = AdsPowerPlaywrightManager.Instance;
                int total = profiles.Count;
                int count = 0;

                foreach (var p in profiles)
                {
                    count++;
                    lblStatus.Text = $"🔍 Kiểm tra {p.ProfileName} ({count}/{total})...";
                    Application.DoEvents();

                    try
                    {
                        // 1️⃣ Check login
                        bool logged = await adsMgr.CheckFacebookLoginAsync(p.IDAdbrowser);
                        p.ProfileStatus = logged ? "Live" : "Die";

                        // 2️⃣ Close extra tabs
                        await adsMgr.CloseExtraTabsAsync(p.IDAdbrowser);
                        int remain = await adsMgr.CountTabsAsync(p.IDAdbrowser);

                        p.UseTab = remain - 1;

                        Libary.Instance.CreateLog(
                            $"[Startup] {p.IDAdbrowser} → {p.ProfileStatus}, Tab còn lại: {remain}"
                        );

                        // 3️⃣ Update DB
                        profileDao.UpdateProfileStatus(p);
                    }
                    catch (Exception ex)
                    {
                        // ⭐ Dùng ex ⇒ ghi log lỗi rõ ràng
                        Libary.Instance.CreateLog(
                            $"[Startup][ERROR] Profile {p.IDAdbrowser} bị lỗi: {ex.Message}\nStack: {ex.StackTrace}"
                        );

                        // ⭐ Cập nhật DB
                        profileDao.UpdateProfileStatus(p);
                    }

                    progress.Value = (int)((count * 100.0) / total);
                    await Task.Delay(150);
                }

                lblStatus.Text = "✔ Kiểm tra xong!";
                await Task.Delay(500);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Startup lỗi: " + ex.Message);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }


    }
}
