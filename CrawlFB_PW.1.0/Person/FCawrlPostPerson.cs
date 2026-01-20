using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Profile;
using DevExpress.DocumentView;
using Microsoft.Playwright;
using static CrawlFB_PW._1._0.DAO.PageDAO;
using IPage = Microsoft.Playwright.IPage;
using Ads = CrawlFB_PW._1._0.DAO.AdsPowerPlaywrightManager;
using perDAO = CrawlFB_PW._1._0.DAO.CrawlPostPersonDAO;
using DevExpress.XtraPrinting;
using FBType = CrawlFB_PW._1._0.Enums.FBType;
using DocumentFormat.OpenXml.Wordprocessing;
using DevExpress.XtraPrinting.Native;
using CrawlFB_PW._1._0.Helper;
using DevExpress.Data.TreeList;
using ClosedXML.Excel;
namespace CrawlFB_PW._1._0.Person
{

    public partial class FCawrlPostPerson : Form
    {
        const string module = nameof(FCawrlPostPerson);
        private List<ProfileDB> _selectedProfiles = new List<ProfileDB>();
        private List<PersonInfo> _selectedPerson = new List<PersonInfo>();
        private Dictionary<string, List<PostPage>> _resultDict = new Dictionary<string, List<PostPage>>();
        private Dictionary<string, List<ShareItem>> _shareMap = new Dictionary<string, List<ShareItem>>();
        private readonly object _shareMapLock = new object();
        private string _currentPersonName = "";
        private string _currentPersonUrl = "";
        private DateTime? _minDay = null;
        private DateTime? _maxDay = null;
        // biến lưu phục vụ filter
        private List<PersonPostVM> _allPosts = new List<PersonPostVM>();
        public FCawrlPostPerson()
        {
            InitializeComponent();
            gridView1.IndicatorWidth = 45;
            gridView1.CustomDrawRowIndicator += gridView1_CustomDrawRowIndicator;
            btn_FilterFind.EditValueChanged += (s, e) => ApplyResultFilter();
            btn_filterOneKey.EditValueChanged += (s, e) => ApplyResultFilter();
            date_FilterFromday.EditValueChanged += (s, e) => ApplyResultFilter();
            date_FilterToday.EditValueChanged += (s, e) => ApplyResultFilter();
            btn_FilterOnlyPersonPost.ItemClick += (s, e) =>
            {
                _filterMode = FilterMode.OnlyPersonPost;
                ApplyResultFilter();
            };
            btn_OnlyPersonShare.ItemClick += (s, e) =>
            {
                _filterMode = FilterMode.OnlyPersonShare;
                ApplyResultFilter();
            };
            btn_OnlyOriginalPost.ItemClick += (s, e) =>
            {
                _filterMode = FilterMode.OnlyOriginalPost;
                ApplyResultFilter();
            };
        }
        private (int maxPost, DateTime? maxDay, DateTime? minDay) GetSetupScanConfig()
        {
            // ===============================
            // 1️⃣ DEFAULT
            // ===============================
            int maxPost = 50;
            DateTime? maxDay = DateTime.Today.AddDays(-7); // mặc định 7 ngày trước
            DateTime? minDay = null;

            // ===============================
            // 2️⃣ MAX POST (luôn lấy)
            // ===============================
            if (int.TryParse(txd_maxpost.EditValue?.ToString(), out int mp) && mp > 0)
                maxPost = mp;

            // ===============================
            // 3️⃣ DATE EDIT (ƯU TIÊN CAO NHẤT)
            // ===============================
            bool hasMaxDay = DateEditFromDay.EditValue is DateTime;
            bool hasMinDay = DateEditToDay.EditValue is DateTime;

            if (hasMaxDay)
            {
                maxDay = ((DateTime)DateEditFromDay.EditValue).Date;

                if (hasMinDay)
                {
                    minDay = ((DateTime)DateEditToDay.EditValue).Date;

                    // đảm bảo maxDay <= minDay
                    if (minDay < maxDay)
                    {
                        var tmp = maxDay;
                        maxDay = minDay;
                        minDay = tmp;
                    }
                }

                return (maxPost, maxDay, minDay);
            }

            // ===============================
            // 4️⃣ KHÔNG DATE → DÙNG MAXTIME
            // ===============================
            if (int.TryParse(txd_maxtime.EditValue?.ToString(), out int mt) && mt > 0)
            {
                maxDay = DateTime.Today.AddDays(-mt);
                minDay = null;
            }

            return (maxPost, maxDay, minDay);
        }
        private bool SelectProfilesForScan(out List<ProfileDB> profiles)
        {
            profiles = null;

            try
            {
                using (var frm = new SelectProfileDB())
                {
                    if (frm.ShowDialog() != DialogResult.OK)
                    {
                        Libary.Instance.LogForm(module, "Người dùng hủy chọn profile");
                        return false;
                    }

                    var selected = frm.Tag as List<ProfileDB>;
                    if (selected == null || selected.Count == 0)
                    {
                        MessageBox.Show("❌ Chưa chọn profile nào!");
                        Libary.Instance.LogForm(module, "Không có profile nào được chọn");
                        return false;
                    }

                    // 🚦 Check UseTab
                    var invalid = selected.Where(p => p.UseTab >= 3).ToList();
                    if (invalid.Count > 0)
                    {
                        string msg = string.Join("\n",
                            invalid.Select(p =>
                                $"{p.ProfileName} (Đang chạy {p.UseTab}/3 tab)"));

                        MessageBox.Show(
                            $"❌ Các profile sau đã đạt giới hạn 3 tab:\n\n{msg}",
                            "Không thể chọn profile",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        Libary.Instance.LogForm(
                            module,
                            $"Profile vượt quá tab: {string.Join(", ", invalid.Select(x => x.ProfileName))}"
                        );

                        return false;
                    }

                    profiles = selected;

                    Libary.Instance.LogForm(
                        module,
                        $"Đã chọn {profiles.Count} profile để chạy FirstScan"
                    );

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi khi chọn profile: " + ex.Message);
                Libary.Instance.LogForm(module, "Exception chọn profile: " + ex.Message);
                return false;
            }
        }
        private void LoadPersonFromText()
        {
            string rawUrl = txd_UrlPerson.EditValue?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(rawUrl))
                return;

            // Chuẩn hóa chữ thường để check
            string url = rawUrl.ToLowerInvariant();

            bool isValidFacebookUrl =
                url.StartsWith("https://www.facebook.com") ||
                url.StartsWith("https://fb.com") ||
                url.StartsWith("https://www.fb.com");

            if (!isValidFacebookUrl)
            {
                MessageBox.Show(
                    "❌ URL không hợp lệ!\n\nChỉ chấp nhận:\n" +
                    "• https://www.facebook.com/...\n" +
                    "• https://fb.com/...\n" +
                    "• https://www.fb.com/...",
                    "URL không hợp lệ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                Libary.Instance.LogForm(
                    module,
                    $"❌ Reject URL không hợp lệ: {rawUrl}"
                );
                return;
            }

            // Gán vào _selectedPages
            _selectedPerson = new List<PersonInfo>
    {
        new PersonInfo
        {
            PersonID= Guid.NewGuid().ToString(),
            PersonLink = rawUrl, // giữ nguyên chữ hoa chữ thường
            PersonName = "Manual URL"
        }
    };

            Libary.Instance.LogForm(
                module,
                $"📌 Nhận URL từ textbox: {rawUrl}"
            );
        }
        private void ConfigGrid()
        {
            // 1️⃣ Sinh cột từ ViewModel
            gridView1.PopulateColumns();

            // 2️⃣ Ẩn cột kỹ thuật
            if (gridView1.Columns[nameof(PersonPostVM.RealPostTime)] != null)
                gridView1.Columns[nameof(PersonPostVM.RealPostTime)].Visible = false;

            if (gridView1.Columns[nameof(PersonPostVM.InteractionTotal)] != null)
                gridView1.Columns[nameof(PersonPostVM.InteractionTotal)].Visible = false;

            // 3️⃣ Caption
            gridView1.Columns[nameof(PersonPostVM.TimeView)].Caption = "Thời gian";
            gridView1.Columns[nameof(PersonPostVM.PostLink)].Caption = "Địa chỉ";
            gridView1.Columns[nameof(PersonPostVM.IDFBPost)].Caption = "ID FB Post";
            gridView1.Columns[nameof(PersonPostVM.Content)].Caption = "Nội dung";
            gridView1.Columns[nameof(PersonPostVM.PosterName)].Caption = "Người/Page đăng";
            gridView1.Columns[nameof(PersonPostVM.PosterLink)].Caption = "Địa chỉ người đăng";
            gridView1.Columns[nameof(PersonPostVM.PosterIDFB)].Caption = "ID FB";
            gridView1.Columns[nameof(PersonPostVM.PosterFBType)].Caption = "Loại";
            gridView1.Columns[nameof(PersonPostVM.Like)].Caption = "Like";
            gridView1.Columns[nameof(PersonPostVM.Share)].Caption = "Share";
            gridView1.Columns[nameof(PersonPostVM.Comment)].Caption = "Comment";
            gridView1.Columns[nameof(PersonPostVM.PostStatus)].Caption = "Trạng thái";

            // 4️⃣ Thứ tự cột (STT là RowIndicator)
            gridView1.Columns[nameof(PersonPostVM.Select)].VisibleIndex = 0;
            gridView1.Columns[nameof(PersonPostVM.TimeView)].VisibleIndex = 1;
            gridView1.Columns[nameof(PersonPostVM.PostLink)].VisibleIndex = 2;
            gridView1.Columns[nameof(PersonPostVM.IDFBPost)].VisibleIndex = 3;
            gridView1.Columns[nameof(PersonPostVM.Content)].VisibleIndex = 4;
            gridView1.Columns[nameof(PersonPostVM.PosterName)].VisibleIndex = 5;
            gridView1.Columns[nameof(PersonPostVM.PosterLink)].VisibleIndex = 6;
            gridView1.Columns[nameof(PersonPostVM.PosterIDFB)].VisibleIndex =7;
            gridView1.Columns[nameof(PersonPostVM.PosterFBType)].VisibleIndex = 8;
            gridView1.Columns[nameof(PersonPostVM.Like)].VisibleIndex = 9;
            gridView1.Columns[nameof(PersonPostVM.Share)].VisibleIndex = 10;
            gridView1.Columns[nameof(PersonPostVM.Comment)].VisibleIndex = 11;
            gridView1.Columns[nameof(PersonPostVM.PostStatus)].VisibleIndex = 12;

            // 5️⃣ Sort theo thời gian THẬT
            gridView1.Columns[nameof(PersonPostVM.RealPostTime)].SortOrder =
                DevExpress.Data.ColumnSortOrder.Descending;

            // 6️⃣ Hyperlink “Xem link”
            UIPostGridHelper.ApplyHyperlinkBehavior(gridView1);
        }
        class PersonPostResult
        {
            public PostPage Post { get; set; }
            public string OriginalPostLink { get; set; }   // dùng cho TablePostShare
            public string ShareTime { get; set; }
        }
        private async Task<List<PostPage>> CrawlPersonPostAsync(string url, DateTime? maxDays, DateTime? minDays, int maxPost, ProfileDB profile, IPage page)
        {
            var results = new List<PostPage>();
            var uniqueLinks = new HashSet<string>();

            Libary.Instance.LogForm(module, $"▶ Crawl PERSON: {url}");

            // =================================================
            // 1️⃣ GOTO PROFILE
            // =================================================
            await page.GotoAsync(url, new PageGotoOptions
            {
                Timeout = AppConfig.DEFAULT_TIMEOUT,
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

            await page.WaitForTimeoutAsync(2000);

            // =================================================
            // 2️⃣ GET FEED
            // =================================================
            // ⭐ LẤY TÊN PERSON – DAO PERSON
            string personName = await CrawlPostPersonDAO.Instance.GetFacebookNamePersonAsync(page);
            _currentPersonName = personName;
            Libary.Instance.LogForm(module, $"GetFacebookNamePersonAsync 👤 PersonName = {personName}");
            var feed = await CrawlPostPersonDAO.Instance.GetFeedContainerAsync(page);
            if (feed == null)
            {
                Libary.Instance.LogForm(module, "❌ Không tìm thấy feed person");
                return results;
            }

            int processedIndex = 0;
            int scrollRound = 0;
            int oldCount = 0;

            // =================================================
            // 3️⃣ SCROLL + PARSE
            // =================================================
            while (results.Count < maxPost && scrollRound < 50)
            {
                var postNodes = await feed.QuerySelectorAllAsync("div[class='x1n2onr6 x1ja2u2z']");

                Libary.Instance.LogForm(module, $"🔍 Scroll {scrollRound} – nodes={postNodes.Count}, processed={processedIndex}");

                for (int i = processedIndex; i < postNodes.Count; i++)
                {
                    if (results.Count >= maxPost)
                        break;

                    var node = postNodes[i];
                    processedIndex = i + 1;

                    // =================================================
                    // 🔥 1 NODE → 1 PostResult (POST + SHARE)
                    // =================================================
                    var result = await CrawlPostPersonDAO.Instance.GetPostPerson(page, node, url, personName);

                    if (result == null)
                    {
                        Libary.Instance.LogForm(
                            module,
                            $"{Libary.IconWarn} Bỏ qua PostResult null: {personName}"
                        );
                        continue;
                    }

                    var posts = result.Posts;
                    var shares = result.Shares;

                    // =================================================
                    // 🔗 HANDLE SHARE MAP (GIỐNG PAGE)
                    // =================================================
                    if (shares != null && shares.Count > 0)
                    {
                        lock (_shareMapLock)
                        {
                            if (!_shareMap.ContainsKey(url))
                                _shareMap[url] = new List<ShareItem>();

                            _shareMap[url].AddRange(shares);
                        }
                        Libary.Instance.LogTech($"[PERSON-SHARE] +{shares.Count} share item"
                        );
                    }

                    // =================================================
                    // ⚠️ KHÔNG CÓ POST → NEXT
                    // =================================================
                    if (posts == null || posts.Count == 0)
                    {
                        Libary.Instance.LogForm(
                            module,
                            $"{Libary.IconWarn} Không có post hợp lệ: {personName}"
                        );
                        continue;
                    }

                    // =================================================
                    // 📝 DUYỆT TỪNG POST
                    // =================================================
                    foreach (var post in posts)
                    {                       
                        if (results.Count >= maxPost)
                            break;

                        if (post == null || string.IsNullOrEmpty(post.PostLink))
                            continue;
                        string realTimeText = TimeHelper.NormalizeTime(post.RealPostTime);

                        string interactText =
                            $"👍 {post.LikeCount ?? 0}  " +
                            $"💬 {post.CommentCount ?? 0}  " +
                            $"🔁 {post.ShareCount ?? 0}";

                        // =================================================
                        // 🖨 LOGFORM PREVIEW
                        // =================================================
                        Libary.Instance.LogForm(
                       module,
                       $"{Libary.IconOK} [{post.PostType}]\n" +
                       $"👤 Người đăng : {post.PosterName}\n" +
                       $"⏰ Thời gian  : {post.PostTime}\n" +
                       $"📅 RealTime  : {realTimeText}\n" +
                       $"🔗 Link      : {post.PostLink}\n" +
                       $"📊 Tương tác : {interactText}\n" +
                       $"📝 Nội dung  : {ProcessingDAO.Instance.PreviewText(post.Content)}\n"
                   );
                        // =================================================
                        // ⏱ CHECK MAX DAY (CHỈ Ở ĐÂY)
                        // =================================================
                        if (post.RealPostTime.HasValue)
                        {
                            DateTime postTime = post.RealPostTime.Value;
                       
                            // ⛔ 1️⃣ QUÁ MAXDAY → tính oldCount để dừng crawl
                            if (maxDays.HasValue && postTime < maxDays.Value)
                            {                              
                                oldCount++;
                                if (oldCount >= 3)
                                {
                                    Libary.Instance.LogForm(
                                        module,
                                        "⛔ 3 bài quá MAXDAY liên tiếp → dừng crawl"
                                    );
                                    return results;
                                }

                                continue; // bỏ bài này
                            }

                            // ✅ reset vì gặp bài hợp lệ
                            oldCount = 0;

                            // 🚫 2️⃣ MỚI HƠN MINDAY → chỉ bỏ qua, KHÔNG dừng crawl
                            if (minDays.HasValue && postTime > minDays.Value)
                            {                           
                                continue;
                            }
                        }
                        // =================================================
                        // 🚫 DUPLICATE CHECK
                        // =================================================
                        if (uniqueLinks.Contains(post.PostLink))
                            continue;

                        uniqueLinks.Add(post.PostLink);
                        results.Add(post);
                    }
                }

                // =================================================
                // 4️⃣ SCROLL TIẾP
                // =================================================
                await ProcessingDAO.Instance.ScrollToLoadPostsAsync(page, 1);
                await page.WaitForTimeoutAsync(800);
                scrollRound++;
            }

            return results;
        }
        private async void btn_Run_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!SelectProfilesForScan(out _selectedProfiles))
            {
                MessageBox.Show("⚠ Chưa chọn profile hợp lệ!");
                return;
            }

            if (!string.IsNullOrWhiteSpace(txd_UrlPerson.EditValue?.ToString()))
            {
                LoadPersonFromText();
            }

            if (_selectedPerson == null || _selectedPerson.Count == 0)
            {
                MessageBox.Show("⚠ Bạn chưa nhập URL person hợp lệ!");
                return;
            }
            // mấy cái sau để hiện dòng đầu trong excell thoio
            string personUrl = _selectedPerson[0].PersonLink;
            _currentPersonUrl = personUrl;
            var (maxPosts, maxdays, mindays) = GetSetupScanConfig();
            _maxDay = maxdays;
            _minDay = mindays;
            // hết cái hiện đầu excell
            var profile = _selectedProfiles.First();

            Libary.Instance.SetProfileContext(profile.IDAdbrowser, profile.ProfileName);

            // đảm bảo 1 tab chính
            var mainPage = await Ads.Instance.GetPageEnsureSingleTabAsync(profile.IDAdbrowser);
            if (mainPage == null)
                return;

            // tab crawl
            var crawlPage = await Ads.Instance.OpenNewTabAsync(profile.IDAdbrowser);
            if (crawlPage == null)
                return;

            Libary.Instance.LogForm(module, $"🚀 Bắt đầu crawl PERSON: {personUrl}");

            // ✅ GỌI HÀM ĐÚNG
            var posts = await CrawlPersonPostAsync(
                personUrl,
                maxdays,
                mindays,
                maxPosts,
                profile,
                crawlPage
            );

            await Ads.Instance.ClosePageAsync(crawlPage);

            MessageBox.Show($"🎉 Hoàn tất quét – {posts.Count} bài");
            // xuất dữ liệu
            var vmList = new List<PersonPostVM>();

            foreach (var p in posts)
            {
                if (p == null || string.IsNullOrWhiteSpace(p.PostLink))
                    continue;
                // ✅ DAO đã gán PostStatus rồi → chỉ phân loại lại để HIỂN THỊ
                string statusUI = SQLDAO.Instance.GetPostStatusUI(p.PostType);
                vmList.Add(new PersonPostVM
                {
                    Select = false,

                    PosterName = p.PosterName,
                    PosterLink = p.PosterLink,

                    // ✅ HIỂN THỊ ĐẸP
                    TimeView = TimeHelper.NormalizeTime(p.RealPostTime),

                    // ✅ LỌC / SORT
                    RealPostTime = p.RealPostTime,

                    PostLink = p.PostLink,
                    PostStatus = statusUI,

                    Content = p.Content,

                    Like = p.LikeCount ?? 0,
                    Comment = p.CommentCount ?? 0,
                    Share = p.ShareCount ?? 0
                });
            }
            //binding
            _allPosts = vmList;
            gridControl1.DataSource = _allPosts;
            ConfigGrid();
        }
        private void btn_ExportFull_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (gridView1.RowCount == 0)
            {
                MessageBox.Show("Không có dữ liệu export");
                return;
            }

            var exportList = new List<(int STT, PersonPostVM Data)>();

            // 👉 LẤY DỮ LIỆU THEO GRID (đúng filter + sort)
            for (int i = 0; i < gridView1.RowCount; i++)
            {
                int rowHandle = gridView1.GetVisibleRowHandle(i);
                if (rowHandle < 0) continue;

                var row = gridView1.GetRow(rowHandle) as PersonPostVM;
                if (row == null) continue;

                exportList.Add((i + 1, row)); // STT = thứ tự hiển thị
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel File (*.xlsx)|*.xlsx";
                sfd.FileName = "Post_Person.xlsx";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                ExcellHeper.ExportPostPersonWithSTT(exportList, sfd.FileName);
                MessageBox.Show("✅ Xuất Excel thành công!");
            }

        }     
        private void btn_ExportFullAndPrint_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_allPosts == null || _allPosts.Count == 0)
            {
                MessageBox.Show("⚠ Không có dữ liệu");
                return;
            }

            // 🔥 CHỈ LẤY DÒNG ĐƯỢC CHỌN
            var selected = _allPosts
                .Where(x => x.Select)
                .ToList();

            if (selected.Count == 0)
            {
                MessageBox.Show("⚠ Bạn chưa chọn dòng nào");
                return;
            }

            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() != DialogResult.OK)
                    return;

                string folder = fbd.SelectedPath;

                string filePost = Path.Combine(folder, "Post.xlsx");
                string filePostInfo = Path.Combine(folder, "PostInfor.xlsx");

                ExportPostExcel(selected, filePost);
                ExportPostInforExcel(selected, filePostInfo);

                MessageBox.Show(
                    $"✅ Xuất Excel thành công!\n\n{filePost}\n{filePostInfo}"
                );
            }
        }
        private void ExportPostExcel(List<PersonPostVM> data, string filePath)
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("POST");

                int startRow = WriteExcelHeader(
                    ws,
                    _currentPersonName,
                    _currentPersonUrl,
                    _minDay,
                    _maxDay
                );

                // HEADER BẢNG
                ws.Cell(startRow, 1).Value = "STT";
                ws.Cell(startRow, 2).Value = "Thời gian";
                ws.Cell(startRow, 3).Value = "IDFB POST";
                ws.Cell(startRow, 4).Value = "Địa chỉ";

                ws.Row(startRow).Style.Font.SetBold();

                int row = startRow + 1;
                int stt = 1;

                foreach (var p in data)
                {
                    ws.Cell(row, 1).Value = stt++;
                    ws.Cell(row, 2).Value = p.TimeView;
                    ws.Cell(row, 3).Value = p.IDFBPost;
                    ws.Cell(row, 4).Value = p.PostLink;
                    row++;
                }

                ws.Columns().AdjustToContents();
                int headerRow = startRow;
                int startDataRow = startRow + 1;
                int endDataRow = startDataRow + data.Count - 1;

                ApplyTableStyle(
                    ws,
                    headerRow,
                    startDataRow,
                    endDataRow,
                    totalCols: 4
                );
                ws.Rows(startDataRow, endDataRow).AdjustToContents();

                wb.SaveAs(filePath);
            }
        }
        private void ExportPostInforExcel(List<PersonPostVM> data, string filePath)
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("POSTINFOR");

                int startRow = WriteExcelHeader(
                    ws,
                    _currentPersonName,
                    _currentPersonUrl,
                    _minDay,
                    _maxDay
                );

                ws.Cell(startRow, 1).Value = "STT";
                ws.Cell(startRow, 2).Value = "Thời gian";
                ws.Cell(startRow, 3).Value = "IDFB";
                ws.Cell(startRow, 4).Value = "Nội dung";
                ws.Cell(startRow, 5).Value = "Like";
                ws.Cell(startRow, 6).Value = "Share";
                ws.Cell(startRow, 7).Value = "Comment";
                ws.Cell(startRow, 8).Value = "Người đăng";
                ws.Cell(startRow, 9).Value = "IDFB người đăng";
                ws.Cell(startRow, 10).Value = "Kiểu bài";

                ws.Row(startRow).Style.Font.SetBold();

                int row = startRow + 1;
                int stt = 1;

                foreach (var p in data)
                {
                    ws.Cell(row, 1).Value = stt++;
                    ws.Cell(row, 2).Value = p.TimeView;
                    ws.Cell(row, 3).Value = p.IDFBPost;
                    ws.Cell(row, 4).Value = p.Content;
                    ws.Cell(row, 5).Value = p.Like;
                    ws.Cell(row, 6).Value = p.Share;
                    ws.Cell(row, 7).Value = p.Comment;
                    ws.Cell(row, 8).Value = p.PosterName;
                    ws.Cell(row, 9).Value = p.PosterIDFB;
                    ws.Cell(row, 10).Value = p.PostStatus;
                    row++;
                }

                ws.Columns().AdjustToContents();
                int headerRow = startRow;
                int startDataRow = startRow + 1;
                int endDataRow = startDataRow + data.Count - 1;

                ApplyTableStyle(
                    ws,
                    headerRow,
                    startDataRow,
                    endDataRow,
                    totalCols: 10,
                    contentColIndex: 4
                );
                ws.Rows(startDataRow, endDataRow).AdjustToContents();

                wb.SaveAs(filePath);
            }
        }

        private int WriteExcelHeader(IXLWorksheet ws, string personName,string url,DateTime? minDay,DateTime? maxDay)
        {
            ws.Cell(1, 1).Value = $"Danh sách bài viết của: {personName}";
            ws.Cell(2, 1).Value = $"Địa chỉ: {url}";
            ws.Cell(3, 1).Value =
                $"Thời gian: {minDay:dd/MM/yyyy} → {maxDay:dd/MM/yyyy}";

            ws.Range("A1:D1").Merge().Style.Font.SetBold();
            ws.Range("A2:D2").Merge();
            ws.Range("A3:D3").Merge();

            ws.Row(1).Height = 22;

            return 4; // ⭐ dòng bắt đầu HEADER bảng
        }
        private void ApplyTableStyle(IXLWorksheet ws,int headerRow,int startDataRow,int endDataRow,int totalCols,int contentColIndex = -1)
        {
            // ===== Header style =====
            var headerRange = ws.Range(headerRow, 1, headerRow, totalCols);
            headerRange.Style.Font.SetBold();
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // ===== Data style =====
            var dataRange = ws.Range(startDataRow, 1, endDataRow, totalCols);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            // ===== Content column =====
            if (contentColIndex > 0)
            {
                ws.Column(contentColIndex).Width = 60;
                ws.Column(contentColIndex).Style.Alignment.WrapText = true;
            }
        }

        // hết phần xuất excell
        //STT ĐỘNG THEO DỮ LIỆU
        private void gridView1_CustomDrawRowIndicator(object sender, DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
        {
            if (!e.Info.IsRowIndicator) return;
            if (e.RowHandle < 0)
            {
                e.Info.DisplayText = "STT";
                return;
            }

            // 👉 STT THEO GRID (đã filter / sort)
            e.Info.DisplayText = (e.RowHandle + 1).ToString();
        }
        // LỌC DỮ LIỆU
        private void ApplyResultFilter()
        {
            IEnumerable<PersonPostVM> query = _allPosts;

            // =========================
            // 1️⃣ TEXT FILTER
            // =========================
            string findText = btn_FilterFind.EditValue?.ToString()?.Trim();
            string exactText = btn_filterOneKey.EditValue?.ToString()?.Trim();

            if (!string.IsNullOrWhiteSpace(exactText))
            {
                query = query.Where(x =>
                    string.Equals(x.Content, exactText, StringComparison.OrdinalIgnoreCase));
            }
            else if (!string.IsNullOrWhiteSpace(findText))
            {
                query = query.Where(x =>
                    (x.Content ?? "").IndexOf(findText, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // =========================
            // 2️⃣ DATE FILTER
            // =========================
            DateTime? maxDay = date_FilterFromday.EditValue as DateTime?;
            DateTime? minDay = date_FilterToday.EditValue as DateTime?;

            if (maxDay.HasValue)
                query = query.Where(x => x.RealPostTime.HasValue &&
                                         x.RealPostTime.Value.Date >= maxDay.Value.Date);

            if (minDay.HasValue)
                query = query.Where(x => x.RealPostTime.HasValue &&
                                         x.RealPostTime.Value.Date <= minDay.Value.Date);

            // =========================
            // 3️⃣ POST TYPE FILTER
            // =========================
            if (_filterMode == FilterMode.OnlyPersonPost)
                query = query.Where(x => x.PostStatus == "Tự đăng");

            else if (_filterMode == FilterMode.OnlyPersonShare)
                query = query.Where(x => x.PostStatus == "Bài share");

            else if (_filterMode == FilterMode.OnlyOriginalPost)
                query = query.Where(x => x.PostStatus == "Bài gốc");

            // =========================
            // 4️⃣ REBIND GRID
            // =========================
            gridControl1.DataSource = query.ToList();
            ConfigGrid();
        }
        private FilterMode _filterMode = FilterMode.All;
        private enum FilterMode
        {
            All,
            OnlyPersonPost,
            OnlyPersonShare,
            OnlyOriginalPost
        }      
        private void btn_ClearFilter_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _filterMode = FilterMode.All;

            btn_FilterFind.EditValue = null;
            btn_filterOneKey.EditValue = null;
            date_FilterFromday.EditValue = null;
            date_FilterToday.EditValue = null;

            gridControl1.DataSource = _allPosts;
            ConfigGrid();
        }

        private async void btn_ScanIDFBPost_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_allPosts == null || _allPosts.Count == 0)
            {
                MessageBox.Show("⚠ Chưa có dữ liệu post");
                return;
            }

            var profile = _selectedProfiles?.FirstOrDefault();
            if (profile == null)
            {
                MessageBox.Show("⚠ Chưa chọn profile");
                return;
            }

            // cache PostLink → IDFBPost
            var cache = new Dictionary<string, string>();

            foreach (var post in _allPosts)
            {
                string postLink = post.PostLink;
                if (string.IsNullOrWhiteSpace(postLink) || postLink == "N/A")
                    continue;

                // ===== cache =====
                if (cache.TryGetValue(postLink, out var cachedId))
                {
                    post.IDFBPost = cachedId;
                    continue;
                }

                string postId = null;

                // =========================
                // 1️⃣ THỬ LẤY TỪ URL
                // =========================
                postId = ProcessingDAO.Instance.ExtractPostIdFromUrl(postLink);

                // =========================
                // 2️⃣ KHÔNG CÓ → MỞ TAB
                // =========================
                if (string.IsNullOrWhiteSpace(postId))
                {
                    var page = await Ads.Instance.OpenNewTabAsync(profile.IDAdbrowser);
                    if (page == null) continue;

                    try
                    {
                        await page.GotoAsync(postLink);
                        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                        string html = await page.ContentAsync();
                        postId = ProcessingDAO.Instance.ExtractPostIdFromHtml(html);
                    }
                    catch (Exception ex)
                    {
                        Libary.Instance.LogTech($"[ScanIDFBPost] {postLink} | {ex.Message}");
                    }
                    finally
                    {
                        await Ads.Instance.ClosePageAsync(page);
                    }
                }

                // =========================
                // 3️⃣ GÁN KẾT QUẢ
                // =========================
                post.IDFBPost = string.IsNullOrWhiteSpace(postId) ? "N/A" : postId;
                cache[postLink] = post.IDFBPost;
            }

            // =========================
            // 4️⃣ REFRESH GRID
            // =========================
            gridControl1.DataSource = null;
            gridControl1.DataSource = _allPosts;
            ConfigGrid();

            MessageBox.Show("✅ Hoàn tất lấy IDFB Post");
        }
        private async void btn_ScanIDFBPagePerson_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_allPosts == null || _allPosts.Count == 0)
            {
                MessageBox.Show("⚠ Chưa có dữ liệu post");
                return;
            }

            var profile = _selectedProfiles?.FirstOrDefault();
            if (profile == null)
            {
                MessageBox.Show("⚠ Chưa chọn profile");
                return;
            }

            // ⭐ CHỈ LẤY POST ĐƯỢC TICK
            var selectedPosts = _allPosts.Where(x => x.Select).ToList();
            if (selectedPosts.Count == 0)
            {
                MessageBox.Show("⚠ Chưa chọn post nào");
                return;
            }

            // cache: PosterLink → (IDFB, FBType)
            var cache = new Dictionary<string, (string id, FBType type)>();

            foreach (var post in selectedPosts)
            {
                string link = post.PosterLink;
                if (string.IsNullOrWhiteSpace(link) || link == "N/A")
                    continue;

                // ===== cache =====
                if (cache.TryGetValue(link, out var cached))
                {
                    post.PosterIDFB = cached.id;
                    post.PosterFBType = cached.type;
                    continue;
                }

                string idfb = null;
                FBType fbType = FBType.Unknown;

                // =========================
                // 1️⃣ CHECK DB – PERSON
                // =========================
                idfb = SQLDAO.Instance.GetIDFBPersonByLink(link);
                if (!string.IsNullOrWhiteSpace(idfb))
                {
                    fbType = FBType.Person;
                }
                else
                {
                    // =========================
                    // 2️⃣ CHECK DB – PAGE
                    // =========================
                    idfb = SQLDAO.Instance.GetIDFBPageByLink(link);
                    if (!string.IsNullOrWhiteSpace(idfb))
                    {
                        fbType = FBType.Fanpage;
                    }
                }

                // =========================
                // 3️⃣ DB CHƯA CÓ → MỞ TAB
                // =========================
                if (string.IsNullOrWhiteSpace(idfb))
                {
                    var page = await Ads.Instance.OpenNewTabAsync(profile.IDAdbrowser);
                    if (page == null) continue;

                    try
                    {
                        await page.GotoAsync(link);
                        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                        fbType = await PageDAO.Instance.CheckTypeCachedAsync(page, link);

                        if (fbType == FBType.Person ||
                            fbType == FBType.PersonKOL ||
                            fbType == FBType.PersonHidden)
                        {
                            idfb = await ScanCheckPageDAO.Instance
                                .ExtractProfileIdFromPhotoHrefAsync(page);

                            if (!string.IsNullOrWhiteSpace(idfb))
                            {
                                SQLDAO.Instance.ExecuteNonQuery(@"
                            UPDATE TablePersonInfo
                            SET IDFBPerson = @idfb
                            WHERE PersonLink = @link",
                                    new Dictionary<string, object>
                                    {
                                { "@idfb", idfb },
                                { "@link", link }
                                    });
                            }
                        }
                        else if (fbType == FBType.Fanpage ||
                                 fbType == FBType.GroupOn ||
                                 fbType == FBType.GroupOff)
                        {
                            var info = await ScanCheckPageDAO.Instance
                                .ScanPageInfoAsync(page, link);

                            if (!string.IsNullOrWhiteSpace(info?.IDFBPage))
                            {
                                idfb = info.IDFBPage;
                                SQLDAO.Instance.InsertOrIgnorePageInfo(info);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Libary.Instance.LogTech($"[ScanIDFB] {link} | {ex.Message}");
                    }
                    finally
                    {
                        await Ads.Instance.ClosePageAsync(page);
                    }
                }

                // =========================
                // 4️⃣ GÁN KẾT QUẢ
                // =========================
                post.PosterIDFB = string.IsNullOrWhiteSpace(idfb) ? "N/A" : idfb;
                post.PosterFBType = fbType;

                cache[link] = (post.PosterIDFB, fbType);
            }

            // =========================
            // 5️⃣ REFRESH GRID
            // =========================
            gridView1.RefreshData();

            MessageBox.Show("✅ Hoàn tất lấy IDFB (theo Select)");
        }
    }
}
