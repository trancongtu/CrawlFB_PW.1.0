using System;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Helper;

namespace CrawlFB_PW._1._0.Profile
{
    public partial class FsetupProfile : Form
    {
        private readonly ProfileInfoDAO profileDao;

        public FsetupProfile()
        {
            InitializeComponent();
            profileDao = new ProfileInfoDAO();
            
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                string idAdbrowser = txbProfileId.Text.Trim();
                string name = txbName.Text.Trim();

                if (string.IsNullOrWhiteSpace(idAdbrowser))
                {
                    MessageBox.Show("⚠ Vui lòng nhập ID Adsbrowser!");
                    return;
                }

                if (profileDao.ExistsAdbrowser(idAdbrowser))
                {
                    MessageBox.Show("❌ ID Adsbrowser đã tồn tại trong DB!");
                    return;
                }

                // Profile tối giản
                var newP = new ProfileDB
                {
                    IDAdbrowser = idAdbrowser,
                    ProfileName = string.IsNullOrWhiteSpace(name) ? idAdbrowser : name,

                    // Mặc định
                    ProfileLink = "N/A",
                    ProfileStatus = "Die", // Khi Startup sẽ check login thực tế
                    UseTab = 0,
                    ProfileType = "N/A"   // Sau này crawler sẽ update
                };

                bool ok = profileDao.InsertProfile(newP);

                if (!ok)
                {
                    MessageBox.Show("❌ Không thể thêm profile vào DB!");
                    return;
                }

                MessageBox.Show($"✅ Đã thêm profile: {newP.ProfileName}");

                // Reset form
                txbProfileId.Clear();
                txbName.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi khi thêm profile: " + ex.Message);
            }
        }
    }
}
