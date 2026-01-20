using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Helper;
namespace CrawlFB_PW._1._0.Profile
{
    public partial class FSlotManagerProfile : Form
    {
        private readonly ProfileInfoDAO profileDao;
        private readonly ManagerProfileDAO managerDao;
        private readonly DatabaseDAO dbDao;
        private System.Timers.Timer realtimeTimer;
        public FSlotManagerProfile()
        {
            InitializeComponent();
            profileDao = new ProfileInfoDAO();
            managerDao = new ManagerProfileDAO();
            dbDao = DatabaseDAO.Instance;

            InitGrid();
            this.Load += FSlotManagerProfile_Load;
        }
        private void InitGrid()
        {
            UICommercialHelper.StyleGrid(gridView1);
            gridView1.OptionsBehavior.Editable = false;
            gridView1.OptionsView.ShowGroupPanel = false;
            gridView1.OptionsView.ShowIndicator = false;

            gridView1.Columns.Clear();

            // =======================
            // ⭐ STT cố định đẹp
            // =======================
            var colSTT = gridView1.Columns.AddVisible("STT", "STT");
            colSTT.Width = 50;
            colSTT.OptionsColumn.FixedWidth = true;
            colSTT.OptionsColumn.AllowSize = false;
            colSTT.OptionsColumn.AllowMove = false;
            colSTT.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            colSTT.OptionsFilter.AllowFilter = false;
            colSTT.Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;

            // =======================
            gridView1.Columns.AddVisible("IDAdbrowser", "ID Browser").Width = 150;
            gridView1.Columns.AddVisible("ProfileName", "FB Name").Width = 200;
            gridView1.Columns.AddVisible("UseTab", "Tabs").Width = 60;

            gridView1.Columns.AddVisible("Page1", "Page 1").Width = 250;
            gridView1.Columns.AddVisible("Page2", "Page 2").Width = 250;
            gridView1.Columns.AddVisible("Page3", "Page 3").Width = 250;
        }

        class SlotRow
        {
            public int STT { get; set; }
            public string IDAdbrowser { get; set; }
            public string ProfileName { get; set; }
            public int UseTab { get; set; }

            public string Page1 { get; set; }
            public string Page2 { get; set; }
            public string Page3 { get; set; }
        }
        private void FSlotManagerProfile_Load(object sender, EventArgs e)
        {
            InitGrid();
            LoadProfilesRealtime();
            realtimeTimer = new System.Timers.Timer(3000); // 3 giây
            realtimeTimer.Elapsed += (s, ev) =>
            {
                try
                {
                    this.Invoke(new Action(() => LoadProfilesRealtime()));
                }
                catch (Exception ex)
                {
                    Libary.Instance.LogForm(
                        nameof(FSlotManagerProfile),
                        "❌ Invoke LoadProfilesRealtime failed: " + ex.Message
                    );
                }
            };
            realtimeTimer.Start();
        }

        private void LoadProfilesRealtime()
        {
            try
            {
                Libary.Instance.LogTech($"LoadProfilesRealtime: start", AppConfig.ENABLE_LOG);
                var profileDao = new ProfileInfoDAO();
                var managerDao = new ManagerProfileDAO();
                var dbDao = DatabaseDAO.Instance;

                var profiles = profileDao.GetAllProfiles();
                Libary.Instance.LogTech($"LoadProfilesRealtime: profiles={profiles.Count}", AppConfig.ENABLE_LOG);

                var list = new List<SlotRow>();

                int stt = 1;

                foreach (var p in profiles)
                {
                    var row = new SlotRow
                    {
                        STT = stt++,
                        IDAdbrowser = p.IDAdbrowser,
                        ProfileName = p.ProfileName,
                        UseTab = p.UseTab
                    };
                    Libary.Instance.LogTech($"Profile {p.IDAdbrowser} | UseTab={p.UseTab}", AppConfig.ENABLE_LOG);
                    // 1️⃣ Lấy danh sách page đã mapping
                    var mappings = managerDao.GetMappingByProfile(p.ID); // List<ManagerProfileDTO>

                    string[] pages = new string[3];

                    for (int i = 0; i < mappings.Count && i < 3; i++)
                    {
                        var mp = mappings[i];

                        // PageID trong DB2
                        string pageId = mp.PageIDCrawl;
                        Libary.Instance.LogTech($"Profile {p.IDAdbrowser} mappings={mappings.Count}", AppConfig.ENABLE_LOG);

                        // 2️⃣ Lấy PageInfo từ TablePageInfo (DB1)
                        var pageInfo = dbDao.GetPageInfoByID(pageId);
                        string pageName = pageInfo?.PageName ?? "(Không tìm thấy)";

                        // 3️⃣ Lấy trạng thái Auto/NewScan/Idle từ TablePageMonitor
                        var monitor = dbDao.GetMonitorRow(pageId);

                        if (monitor != null)
                        {
                            Libary.Instance.LogTech($"Page {pageId} status={monitor.Status}", AppConfig.ENABLE_LOG);
                            if (monitor.Status.Contains("Running"))
                                pageName += " (Auto)";
                            else if (monitor.Status.Contains("NewScan"))
                                pageName += " (NewScan)";
                            else
                                pageName += " (Idle)";
                        }

                        pages[i] = pageName;
                    }
                    row.Page1 = (mappings.Count > 0) ? pages[0] : "";
                    row.Page2 = (mappings.Count > 1) ? pages[1] : "";
                    row.Page3 = (mappings.Count > 2) ? pages[2] : "";
                    list.Add(row);
                }
                gridControl1.DataSource = list;
                gridView1.RefreshData();
            }
            catch (Exception ex)
            {
                Libary.Instance.LogForm( nameof(FSlotManagerProfile),"❌ LoadProfilesRealtime error: " + ex.Message);
            }

        }

        public void ForceReload()
        {
            try
            {
                LoadProfilesRealtime();   // 🔥 Hàm bạn đang dùng để load lại grid
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog(nameof(FSlotManagerProfile),"[SlotManager] ❌ ForceReload lỗi: " + ex.Message);
            }
        }

    }
}
