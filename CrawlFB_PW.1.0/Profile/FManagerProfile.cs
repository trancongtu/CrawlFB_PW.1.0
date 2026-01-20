using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Helpers;
using DevExpress.DocumentView;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.Playwright;
using PlayPage = Microsoft.Playwright.IPage;
namespace CrawlFB_PW._1._0.Profile
{
    public partial class FManagerProfile : Form
    {
        private readonly ProfileInfoDAO profileDao;
        private List<ProfileDB> profiles = new List<ProfileDB>();

        public FManagerProfile()
        {
            InitializeComponent();
            profileDao = new ProfileInfoDAO();

            InitGrid();
            this.Load += FManagerProfile_Load;
          
            // Nhấn nút mới check
        }

        // ❗ KHÔNG CHECK GÌ KHI LOAD FORM — chỉ load từ DB
        private void FManagerProfile_Load(object sender, EventArgs e)
        {
          
            LoadProfilesFromDB();
            UIStyleHelper.StyleBarManager(barManager1);
        }

        private void InitGrid()
        {
            gridView1.OptionsBehavior.Editable = false;
            gridView1.OptionsView.ShowGroupPanel = false;
            gridView1.OptionsView.ShowIndicator = false;

            gridView1.Columns.Clear();

            gridView1.Columns.AddVisible(nameof(ProfileDB.ID), "ID").Width = 50;
            gridView1.Columns.AddVisible(nameof(ProfileDB.IDAdbrowser), "IDAdbrowser").Width = 150;
            gridView1.Columns.AddVisible(nameof(ProfileDB.ProfileName), "Tên Profile").Width = 180;
            gridView1.Columns.AddVisible(nameof(ProfileDB.ProfileLink), "Link Facebook").Width = 220;
            gridView1.Columns.AddVisible(nameof(ProfileDB.ProfileStatus), "Trạng thái").Width = 100;
            gridView1.Columns.AddVisible(nameof(ProfileDB.UseTab), "Tabs").Width = 50;
            gridView1.Columns.AddVisible(nameof(ProfileDB.ProfileType), "Loại").Width = 100;
        }
        // ⭐ CHỈ LOAD DB, KHÔNG CHECK LOGIN
        private void LoadProfilesFromDB()
        {
            profiles = profileDao.GetAllProfiles();
            lblStatus.Text = $"📌 Bạn có {profiles.Count} profile";
            foreach (var p in profiles)
            {
                if (string.IsNullOrWhiteSpace(p.ProfileLink))
                    p.ProfileLink = "Chưa kiểm tra";

                if (string.IsNullOrWhiteSpace(p.ProfileStatus))
                    p.ProfileStatus = "Chưa kiểm tra";

                if (string.IsNullOrWhiteSpace(p.ProfileType))
                    p.ProfileType = "Chưa kiểm tra";

                if (p.UseTab <= 0)
                    p.UseTab = 0;
            }

            gridControl1.DataSource = profiles;
            gridView1.RefreshData();
        } 
        // ⭐ CHECK LOGIN TỪNG PROFILE
        private async Task CheckProfilesAsync()
        {
            var adsMgr = AdsPowerPlaywrightManager.Instance;
            int total = profiles.Count;
            int index = 0;
            foreach (var p in profiles)
            {
                index++;

                // 🟦 Cập nhật trạng thái đang check
                lblStatus.Text = $"🔍 Đang kiểm tra {index}/{total} profile...";
                lblStatus.Refresh();
                try
                {
                    // 1️⃣ Mở AdsPower profile (KHÔNG đóng nữa)
                    var page = await adsMgr.GetPageAsync(p.IDAdbrowser);
                    await page.GotoAsync("https://www.fb.com/");
                    await page.WaitForTimeoutAsync(800);
                    ///
                    bool login = await adsMgr.CheckFacebookLoginAsync(page);
                    p.ProfileStatus = login ? "Live" : "Die";
                    Libary.Instance.CreateLog("FB Link: " + p.ProfileStatus);
                    if (!login)
                        return;
                    ////
                    p.ProfileLink = await adsMgr.GetFacebookLinkAsync(page);
                    Libary.Instance.CreateLog("FB Link: " + p.ProfileLink);
                    await page.GotoAsync(p.ProfileLink);
                    await page.WaitForTimeoutAsync(800);
                    p.ProfileName = await adsMgr.GetFacebookNameAsync(page);
                    Libary.Instance.CreateLog("FB Name: " + p.ProfileName);                   
                    // 6️⃣ Giữ lại tab gốc → KHÔNG close profile nữa
                    await adsMgr.CloseExtraTabsAsync(p.IDAdbrowser);
                    int remainTabs = await adsMgr.CountTabsAsync(p.IDAdbrowser);
                    p.UseTab = remainTabs;

                    // 7️⃣ Update vào DB2
                    profileDao.UpdateProfile(p);

                    // 8️⃣ Refresh UI
                    gridView1.RefreshData();

                    await Task.Delay(200);
                }
                catch (Exception ex)
                {
                    p.ProfileStatus = "Die";
                    p.UseTab = 0;
                    profileDao.UpdateProfile(p);

                    Libary.Instance.CreateLog($"❌ Lỗi check profile {p.IDAdbrowser}: {ex.Message}");
                }
            }

            MessageBox.Show("✔ Đã kiểm tra đầy đủ tất cả profile!");
        }
        
        // ⭐ NHẤN NÚT DELETE
        private async void btnCheckProfile_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            await CheckProfilesAsync();
        }

        private void btnDeleteProfile_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                var row = gridView1.GetFocusedRow() as ProfileDB;
                if (row == null)
                {
                    MessageBox.Show("⚠ Chọn 1 profile để xóa!");
                    return;
                }

                if (MessageBox.Show($"Xóa profile: {row.IDAdbrowser} ?",
                    "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;

                // XÓA DB
                profileDao.DeleteProfile(row.ID);

                LoadProfilesFromDB();

                MessageBox.Show("🗑 Đã xóa profile!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi xóa profile: " + ex.Message);
            }
        }

        private void btnAddProfile_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            FsetupProfile f = new FsetupProfile();
            f.ShowDialog();
        }

        private async void barButtonItemCheckTime_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                var adsMgr = AdsPowerPlaywrightManager.Instance;

                // 1) Kiểm tra grid view
                var view = gridView1;
                if (view == null)
                {
                    MessageBox.Show("gridView1 == null");
                    return;
                }

                // 2) Lấy row đang chọn
                int rowHandle = view.FocusedRowHandle;
                if (rowHandle < 0 || !view.IsDataRow(rowHandle))
                {
                    MessageBox.Show("Chưa chọn hàng!");
                    return;
                }

                // 3) Lấy ID (string)
                string id = view.GetRowCellValue(rowHandle, "IDAdbrowser")?.ToString();
                if (string.IsNullOrWhiteSpace(id))
                {
                    MessageBox.Show("Không có IDAdbrowser!");
                    return;
                }

                // 4) Lấy page
                PlayPage page = await adsMgr.GetPageAsync(id);
                if (page == null)
                {
                    MessageBox.Show("Không lấy được page!");
                    return;
                }

                // 5) Mở Facebook
                await page.GotoAsync("https://facebook.com/");
                await page.WaitForTimeoutAsync(1500);

                // 6) Chờ Feed xuất hiện
                var feed = await PageDAO.Instance.GetFeedContainerAsync(page);
                if (feed == null)
                {
                    MessageBox.Show("Không load được Feed!");
                    return;
                }

                // 7) Scroll để load bài
                await ProcessingDAO.Instance.HumanScrollAsync(page);
                await page.WaitForTimeoutAsync(1500);

                // 8) Lấy tất cả thời gian
                List<string> times = await GetAllTimesAsync(page);
                if (times.Count > 0)
                {
                    // In 1–2 cái đầu ra log
                    var top2 = times.Take(2).ToList();
                    Libary.Instance.CreateLog("⏱ Time found: " + string.Join(" | ", top2));

                    MessageBox.Show("⏱ Thời gian OK – tìm thấy thời gian hợp lệ!", "Kết quả");
                }
                else
                {
                    Libary.Instance.CreateLog("❌ Không tìm thấy thời gian hợp lệ trên trang!");
                    MessageBox.Show("❌ Không tìm thấy thời gian hợp lệ trên trang!", "Kết quả");
                }
            }
            catch (Exception ex)
            {
                string msg = $"Exception in CheckTime: {ex.Message}\n{ex.StackTrace}";
                Libary.Instance.CreateLog(msg);
                MessageBox.Show(msg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //SETUP UI
        public async Task<List<string>> GetAllTimesAsync(PlayPage page)
        {
            string[] selectors =
            {
        "a[aria-label] time",
        "a[role='link'] time",
        "time",
        "span[aria-label]",
        "a[aria-label]",
        "abbr"
    };

            var results = new List<string>();

            foreach (var sel in selectors)
            {
                var nodes = await page.QuerySelectorAllAsync(sel);
                if (nodes == null) continue;

                foreach (var n in nodes)
                {
                    string t = (await n.InnerTextAsync() ?? "").Trim();

                    if (string.IsNullOrWhiteSpace(t))
                        continue;

                    // Lọc chuỗi thời gian hợp lệ
                    if (ProcessingDAO.Instance.IsTime(t))
                        results.Add(t);
                }

                if (results.Count > 0)
                    break;
            }

            return results;
        }


    }
}
