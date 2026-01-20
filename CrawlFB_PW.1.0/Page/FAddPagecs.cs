using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ClosedXML.Excel;
using DevExpress.XtraGrid.Views.Grid;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Helper;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using DevExpress.XtraBars;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using Ads = CrawlFB_PW._1._0.DAO.AdsPowerPlaywrightManager;
using CrawlFB_PW._1._0.Service;
using DevExpress.XtraPrinting;
using CrawlFB_PW._1._0.ViewModels;
using CrawlFB_PW._1._0.Enums;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
namespace CrawlFB_PW._1._0.Page
{
    public partial class FAddPagecs : Form
    {
        //private BindingList<PageInfo> _data = new BindingList<PageInfo>();
        private BindingList<PageInfoViewModel> _data = new BindingList<PageInfoViewModel>();
        const string module = nameof(FAddPagecs);
        public FAddPagecs()
        {
            InitializeComponent();
            InitGrid();        
        }
        // =====================================================
        // GRID SHOW: URL | NAME   
        private void InitGrid()
        {
            gridControl1.DataSource = _data;
            gridView1.PopulateColumns();

            // ===== Base grid behavior (CHUNG TOÀN APP) =====
            UIGridHelper.EnableRowIndicatorSTT(gridView1); // STT
            UIGridHelper.ApplySelect(gridView1, gridControl1);//Select
            UIGridHelper.EnableRowClickToggleSelect(gridView1);// click ăn select
            UIGridHelper.EnableStatusDisplay(gridView1);// hiển thị trạng thái
            UIGridHelper.ApplyVietnameseCaption(gridView1);// header tiếng việt
            UICommercialHelper.StyleGrid(gridView1);// style chung
            UIGridHelper.ApplyRowColorByStatus(gridView1, "Status");         
            UIPageInfoGridHelper.ApplyPageColumnAdjust(gridView1);
            // ===== Thứ tự cột =====
            gridView1.Columns[nameof(BaseViewModel.Select)].VisibleIndex = 0;
            gridView1.Columns[nameof(PageInfoViewModel.PageName)].VisibleIndex = 1;
            gridView1.Columns[nameof(PageInfoViewModel.PageLink)].VisibleIndex = 2;
            gridView1.Columns[nameof(BaseViewModel.Status)].VisibleIndex = 3;
            // ẩn cột k cần
            gridView1.Columns[nameof(PageInfoViewModel.PageTimeSave)].Visible = false;
            gridView1.Columns[nameof(PageInfoViewModel.PageInteraction)].Visible = false;
            gridView1.Columns[nameof(PageInfoViewModel.PageID)].Visible = false;
            gridView1.Columns[nameof(PageInfoViewModel.TimeLastPost)].Visible = false;
            gridView1.Columns[nameof(PageInfoViewModel.PageEvaluation)].Visible = false;
            //=========captiom
            // ===== UI option =====
            gridView1.OptionsSelection.EnableAppearanceFocusedCell = false;
            gridView1.OptionsSelection.EnableAppearanceFocusedRow = true;
            gridView1.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
        }

        string Clean(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Trim();                     // bỏ đầu + cuối
            s = Regex.Replace(s, @"\s+", " "); // gom nhiều space thành 1
            return s;
        }
        // =====================================================
        // THÊM DÒNG VÀO LIST
        // =====================================================
        private void AddRow(string pageLink, string pageName)
        {
            if (string.IsNullOrWhiteSpace(pageLink))
                return;

            // 1️⃣ Chuẩn hoá link (nếu chưa làm trước đó)
            pageLink = ProcessingHelper.NormalizeInputUrl(pageLink);

            // 2️⃣ Trùng trong GRID (chưa lưu)
            if (_data.Any(x => x.PageLink == pageLink))
            {
                Libary.Instance.LogForm(
                    module,
                    $"⚠ Page đã tồn tại trong danh sách: {pageLink}"
                );
                return;
            }

            // 3️⃣ Trùng trong DB (ĐÃ CÓ SẴN HÀM)
            if (SQLDAO.Instance.CheckPageExistsByLink(pageLink))
            {
                Libary.Instance.LogForm(
                    module,
                    $"⏭ Bỏ qua Page đã có trong DB: {pageLink}"
                );
                return;
            }

            // 4️⃣ OK → thêm vào grid
            var vm = new PageInfoViewModel
            {
                PageLink = pageLink,
                PageName = pageName ?? "N/A",
                Status = UIStatus.Added,
                Select = false
            };

            _data.Add(vm);
        }
        // =====================================================
        // 1️⃣ Load Excel (Cột 1 = STT, Cột 2 = URL, Cột 3 = Name)
        // =====================================================
        private void btnLoad_file_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog
                {
                    Filter = "Excel file|*.xlsx",
                    Title = "Chọn file danh sách Page"
                };
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;
                using (var stream = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var wb = new XLWorkbook(stream))
                {
                    var ws = wb.Worksheet(1);
                    var rows = ws.RangeUsed().RowsUsed().Skip(1); // bỏ header
                    foreach (var row in rows)
                    {
                        string url = row.Cell(2).GetString().Trim();      // Cột B
                        string name = row.Cell(3).GetString().Trim();     // Cột C
                        if (Regex.IsMatch(url, @"^\d+$"))   // Chỉ cần kiểm tra toàn số
                        {
                            url = "https://Fb.com/" + url;
                        }
                        if (!string.IsNullOrEmpty(url))
                            AddRow(url, name);
                        Libary.Instance.LogForm(module, $"Nạp Page Từ File: "+name);
                    }
                    Libary.Instance.LogForm(module, $"Nạp Tổng: "+rows.Count());
                }          
                MessageBox.Show("✔ Đã nạp file Excel thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi đọc file Excel: " + ex.Message);
            }
        }

        // =====================================================
        // 2️⃣ Chạy Check
        // =====================================================
        private async void btnRun_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_data.Count == 0)
            {
                MessageBox.Show("⚠ Không có dữ liệu để check!");
                return;
            }


            // 🔄 Reset trạng thái
            foreach (var row in _data)
            {
                row.Status = UIStatus.Pending;
            }
            gridView1.RefreshData();
            // 🟢 MỞ FORM CHỌN PROFILE
            var frm = new CrawlFB_PW._1._0.Profile.SelectProfileDB();
            if (frm.ShowDialog() != DialogResult.OK)
            {
                MessageBox.Show("⚠ Bạn chưa chọn profile!");
                return;
            }
            var selected = frm.Tag as List<ProfileDB>;
            if (selected == null || selected.Count == 0)
            {
                MessageBox.Show("⚠ Không nhận được profile!");
                return;
            }
            // Lấy profile LIVE (hoặc đã chọn)
            List<ProfileDB> profiles = selected
                .Where(p => p.ProfileStatus == "Live")
                .ToList();
            if (profiles.Count == 0)
            {
                MessageBox.Show("⚠ Không có profile LIVE!");
                return;
            }       
            // 🟢 LẤY DANH SÁCH PAGE ĐƯỢC CHỌN (VIEWMODEL)
            var runItems = _data
                .Where(x => x.Select)
                .ToList();

            if (runItems.Count == 0)
            {
                MessageBox.Show("⚠ Bạn chưa chọn Page nào để chạy!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ===============================
            // 2️⃣ FORM LOG (UI)
            // ===============================
            Libary.Instance.LogForm(module, "▶ Bắt đầu chia page cho profile");
            // ===============================
            // 3️⃣ GỌI SERVICE (KHÔNG LOG UI)
            // ===============================
            var service = new PageDistributionService();
            // ⚠️ CHỈ TRUYỀN PAGE ĐƯỢC CHỌN
            var result = service.Distribute(
                profiles,
                runItems,                 // ✅ CHỈ PAGE ĐƯỢC SELECT
                p => p.PageLink,
                "page"
            );
            // ===============================
            // 4️⃣ FORM LOG (UI)
            // ===============================
            Libary.Instance.LogForm(
                module,
                $"✔ Hoàn tất chia page cho {result.Count} profile"
            );

            List<Task> tasks = new List<Task>();
            // 🟢 CHẠY SONG SONG MỖI PROFILE
            foreach (var kv in result)
            {
                var profile = kv.Key;
                var pagesOfThisProfile = kv.Value;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        // 1️⃣ Đảm bảo profile chỉ còn 1 tab (tab gốc)
                        var mainPage = await Ads.Instance
                            .GetPageEnsureSingleTabAsync(profile.IDAdbrowser);
                        if (mainPage == null)
                            return;
                       // 2️⃣ Nếu CẦN tab phụ thì mới mở
                        var tab = await Ads.Instance
                            .OpenNewTabAsync(profile.IDAdbrowser);
                        // ⭐ GẮN PROFILE CONTEXT để log theo profile
                        Libary.Instance.SetProfileContext(profile.IDAdbrowser,profile.ProfileName);
                        if (tab == null)
                        {
                            Libary.Instance.LogForm(module, $"Profile {profile.ProfileName} mở tab thất bại!");
                            return;
                        }

                        foreach (var item in pagesOfThisProfile)
                        {
                            try
                            {
                                item.Status = UIStatus.Running;
                                gridView1.RefreshData();
                                Libary.Instance.LogTech("Link vao: " + item.PageLink,AppConfig.ENABLE_LOG);
                                var info = await ScanCheckPageDAO.Instance.ScanPageInfoAsync(tab, item.PageLink);
                                if (info != null)
                                Libary.Instance.LogForm(module,"Scan thành công: " + item.PageLink);
                                item.PageName = info.PageName;
                                item.PageType = info.PageType;
                                item.PageMembers = info.PageMembers;
                                item.PageInfoText = info.PageInfoText;
                                item.TimeLastPost = info.TimeLastPost;                           
                                item.IDFBPage = info.IDFBPage;

                                if (!string.IsNullOrEmpty(info.PageLink))
                                    item.PageLink = info.PageLink;

                                if (!string.IsNullOrEmpty(info.IDFBPage)
                                    && info.IDFBPage != "N/A")
                                {
                                    item.PageID = info.IDFBPage;
                                }

                                item.Status = UIStatus.Done;
                                gridView1.RefreshData();
                            }
                            catch (Exception ex)
                            {
                                item.Status = UIStatus.Error;
                                Libary.Instance.LogForm(module,
                                    $"[Scan ERROR] {ex.Message}");
                                gridView1.RefreshData();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Libary.Instance.LogForm(module,
                            "[Profile Task Error] " + ex.Message);
                    }
                    finally
                    {
                        // ⭐⭐ BẮT BUỘC PHẢI CÓ
                        Libary.Instance.ClearProfileContext();
                    }
                }));
            }

            // ⏳ CHỜ TẤT CẢ PROFILE LÀM XONG
            await Task.WhenAll(tasks);
            Libary.Instance.ClearProfileContext();
            MessageBox.Show("✔ Check xong toàn bộ Page!");
        }
        // =====================================================
        //34️ SAVE → TablePageInfo (DB1)
        // =====================================================    
        private void btnSave_DB_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_data == null || _data.Count == 0)
            {
                MessageBox.Show("⚠ Không có dữ liệu để lưu!");
                return;
            }

            // ⭐ LẤY CÁC PAGE ĐƯỢC CHỌN
            var selectedItems = _data
                .Where(x => x.Select)
                .ToList();

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("⚠ Bạn chưa chọn Page nào!");
                return;
            }

            if (MessageBox.Show(
                    "Bạn có chắc muốn lưu danh sách Page vào DB?",
                    "Xác nhận",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                ) != DialogResult.Yes)
            {
                return;
            }

            int successCount = 0;

            foreach (var vm in selectedItems)
            {
                try
                {
                    // ⭐ MAP VM → DTO
                    var dto = new CrawlFB_PW._1._0.DTO.PageInfo
                    {
                        PageID = vm.PageID,
                        IDFBPage = vm.IDFBPage,
                        PageLink = vm.PageLink,
                        PageName = vm.PageName,
                        PageType = vm.PageType,
                        PageMembers = vm.PageMembers,
                        PageInteraction = vm.PageInteraction,
                        PageEvaluation = vm.PageEvaluation,
                        PageInfoText = vm.PageInfoText,
                        TimeLastPost = vm.TimeLastPost,
                        PageTimeSave = vm.PageTimeSave
                    };

                    SQLDAO.Instance.InsertOrIgnorePageInfo(dto);
                    successCount++;
                }
                catch (Exception ex)
                {
                    Libary.Instance.LogForm(
                        module,
                        $"[FAddPage] ❌ Lỗi lưu page ({vm.PageLink}): {ex.Message}"
                    );
                }
            }

            MessageBox.Show($"✔ Đã lưu {successCount} Page được chọn!");
        }
        // =====================================================
        //4 RESET
        // =====================================================
        private void btn_reset_ItemClick(object sender, ItemClickEventArgs e)
        {
            _data.Clear();
        }
        private void btnAddGrid_Click_1(object sender, EventArgs e)
        {
            string url = ProcessingHelper.NormalizeInputUrl(Clean(txbPageLink.Text));
            string name = Clean(txbPageName.Text);
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(name))
            {
                MessageBox.Show("⚠ Hãy nhập đủ URL và tên Page!");
                return;
            }
            // Nếu nhập toàn số => convert sang facebook.com/ID
            if (Regex.IsMatch(url, @"^\d+$"))   // Chỉ cần kiểm tra toàn số
            {
                url = "Fb.com/" + url;
                MessageBox.Show(url);
            }

            AddRow(url, name);

            txbPageLink.Clear();
            txbPageName.Clear();
        }
        // SelectAll
        private void barButtonItem2_ItemClick(object sender, ItemClickEventArgs e)
        {

            if (_data == null || _data.Count == 0)
            {
                MessageBox.Show("⚠ Chưa có dữ liệu để chọn!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            UIGridHelper.SelectAll(gridControl1, true);
            gridView1.RefreshData();
        }
        private void btnRemoveAll_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(_data == null || _data.Count == 0)
    {
                MessageBox.Show("⚠ Chưa có dữ liệu để bỏ chọn!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            UIGridHelper.SelectAll(gridControl1, false);
            gridView1.RefreshData();
        }
    }
}
