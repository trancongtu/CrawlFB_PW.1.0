using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DAO.Post;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Enums;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.Profile;
using CrawlFB_PW._1._0.ViewModels;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Playwright;
using static System.Net.WebRequestMethods;
using Ads = CrawlFB_PW._1._0.DAO.AdsPowerPlaywrightManager;
namespace CrawlFB_PW._1._0.Post
{
    public partial class FscanCommenPostFull : Form
    {
        const string module = nameof(FscanCommenPostFull);
        private List<ProfileDB> _selectedProfiles = new List<ProfileDB>();
        private List<string> _ListPostLink = new List<string>();
        private volatile bool _stopScanRequested = false;
        //private List<CommentGridRow> _allComments = new List<CommentGridRow>();
       
        private List<CommentGridRow> _data = new List<CommentGridRow>();
        string _currentPostLink, _currentPosterName = "";
        
        public FscanCommenPostFull()
        {
            InitializeComponent();
       
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
        private void ConfigGrid()
        {
            gridView1.PopulateColumns();
            if (gridView1.Columns[nameof(CommentGridRow.RealPostTime)] != null)
                gridView1.Columns[nameof(CommentGridRow.RealPostTime)].Visible = false;
            // ===== CAPTION =====
            gridView1.Columns[nameof(CommentGridRow.STT)].Caption = "STT";
            gridView1.Columns[nameof(CommentGridRow.Select)].Caption = "";
            gridView1.Columns[nameof(CommentGridRow.ActorName)].Caption = "Người bình luận";
            gridView1.Columns[nameof(CommentGridRow.IDFBPerson)].Caption = "ID FB";
            gridView1.Columns[nameof(CommentGridRow.PosterFBType)].Caption = "Loại";
            gridView1.Columns[nameof(CommentGridRow.Time)].Caption = "Thời gian";
            gridView1.Columns[nameof(CommentGridRow.LinkView)].Caption = "Link";         
            gridView1.Columns[nameof(CommentGridRow.Content)].Caption = "Nội dung";

            // ===== THỨ TỰ CỘT =====
            gridView1.Columns[nameof(CommentGridRow.Select)].VisibleIndex = 0;
            gridView1.Columns[nameof(CommentGridRow.STT)].VisibleIndex = 1;
            gridView1.Columns[nameof(CommentGridRow.ActorName)].VisibleIndex = 2;
            gridView1.Columns[nameof(CommentGridRow.IDFBPerson)].VisibleIndex = 3;
            gridView1.Columns[nameof(CommentGridRow.PosterFBType)].VisibleIndex = 4;
            gridView1.Columns[nameof(CommentGridRow.Time)].VisibleIndex = 5;
            gridView1.Columns[nameof(CommentGridRow.LinkView)].VisibleIndex = 6;        
            gridView1.Columns[nameof(CommentGridRow.Content)].VisibleIndex = 7;

            // Ẩn link thật (chỉ dùng nội bộ)
            gridView1.Columns[nameof(CommentGridRow.Link)].Visible = false;
            // 5️⃣ Sort theo thời gian THẬT
            UIGridHelper.ApplyCommentGridStyle(gridView1);
            UIGridHelper.ApplySelect(gridView1, gridControl1);
            // 6️⃣ Hyperlink “Xem link”
            UIGridHelper.ApplyCommentLink(gridView1, gridControl1);
            // load json can
            gridView1.OptionsBehavior.Editable = true;
            gridView1.OptionsBehavior.ReadOnly = false;        
            gridView1.Columns[nameof(CommentGridRow.Select)]
                .OptionsColumn.AllowEdit = true;
           

        }
       
        private void LoadPostLinkFromText()
        {
            string rawUrl = txd_PostLink.EditValue?.ToString()?.Trim();

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
            _ListPostLink.Add(url);


            Libary.Instance.LogForm(
                module,
                $"📌 Nhận URL từ textbox: {rawUrl}"
            );
        }
        private async void btn_RunScan_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!SelectProfilesForScan(out _selectedProfiles))
            {
                MessageBox.Show("⚠ Chưa chọn profile hợp lệ!");
                return;
            }

            if (!string.IsNullOrWhiteSpace(txd_PostLink.EditValue?.ToString()))
            {
                LoadPostLinkFromText();
            }

            if (_ListPostLink == null || _ListPostLink.Count == 0)
            {
                MessageBox.Show("⚠ Bạn chưa nhập URL hợp lệ!");
                return;
            }
            var profile = _selectedProfiles.First();
            Libary.Instance.SetProfileContext(profile.IDAdbrowser, profile.ProfileName);
            string Link = _ListPostLink[0];
            _currentPostLink = _ListPostLink[0];
            bool isReel = Link.Contains("/reel/");
            bool isWatch =
                Link.Contains("/watch") ||
                Link.Contains("/videos/");
            bool isNormal = Link.Contains("/posts/");
            string safeLink = AdsPowerPlaywrightManager.Instance.NormalizePostLinkToOpen(Link, null);
            // đảm bảo 1 tab chính
            var mainPage = await Ads.Instance.GetPageEnsureSingleTabAsync(profile.IDAdbrowser);
            if (mainPage == null)
                return;
            var Page =
                await AdsPowerPlaywrightManager.Instance.OpenNewTabAsync(
                    profile.IDAdbrowser,
                    applyViewport: false
                );

            _stopScanRequested = false;
            List<CommentItem> comments;

            if (isReel)
            {
                comments = await PostReelCommentDAO.Instance.ScanPostRellFulllAsync(
                    Page,
                    Link,
                    () => _stopScanRequested
                );
            }
            else if (isWatch)
            {
                comments = await PostWatchCommentDAO.Instance.ScanWatchsCommentsAsync(
                    Page,
                    Link,
                    () => _stopScanRequested
                );
            }
            else if (isNormal)
            {
                comments = await PostNormalCommentDAO.Instance.ScanPostNormalFullAsync(
                    Page,
                    Link,
                    () => _stopScanRequested
                );
            }
            else
            {
                MessageBox.Show("⚠ URL không hỗ trợ (chỉ hỗ trợ Reel / Watch / Post thường)");
                return;
            }

            var parents = comments.Where(x => string.IsNullOrEmpty(x.ParentCommentId)).ToList();
            //GÁN GRID
            var gridRows = new List<CommentGridRow>();

            int parentIndex = 0;

            foreach (var parent in parents)
            {
                parentIndex++;
                string sttParent = parentIndex.ToString();

                // ===== COMMENT CHA =====
                gridRows.Add(new CommentGridRow
                {
                    Select = true,
                    STT = sttParent,
                    ActorName = parent.PosterName,
                    Time = TimeHelper.NormalizeTime(parent.RealCommentTime),
                    RealPostTime = parent.RealCommentTime,
                    Link = parent.PosterLink,      // link thật
                    LinkView = "Xem link",          // 👉 quyết định HIỂN THỊ Ở ĐÂY
                    IDFBPerson = "",
                    Content = parent.Content,
                    PosterFBType = FBType.Unknown,
                    Level = 0
                });


                // 2️⃣ Lấy comment con
                var replies = comments
                    .Where(x => x.ParentCommentId == parent.CommentId)
                    .ToList();

                int replyIndex = 0;

                foreach (var reply in replies)
                {
                    replyIndex++;
                    string sttReply = $"{sttParent}.{replyIndex}";

                    gridRows.Add(new CommentGridRow
                    {
                        Select = true,
                        STT = sttReply,
                        ActorName = reply.PosterName,
                        Time = TimeHelper.NormalizeTime(reply.RealCommentTime),
                        RealPostTime = reply.RealCommentTime,
                        Link = reply.PosterLink,   // 🔗 link THẬT
                        LinkView = "Xem link",     // 👁 hiển thị ngắn trên grid
                        Content = reply.Content,
                        PosterFBType = FBType.Unknown,
                        IDFBPerson = "",
                        Level = 1
                    });

                    // 3️⃣ Nếu có comment cháu
                    var subReplies = comments
                        .Where(x => x.ParentCommentId == reply.CommentId)
                        .ToList();

                    int subIndex = 0;

                    foreach (var sub in subReplies)
                    {
                        subIndex++;
                        gridRows.Add(new CommentGridRow
                        {
                            Select = true,
                            STT = $"{sttReply}.{subIndex}",
                            ActorName = sub.PosterName,
                            Time = TimeHelper.NormalizeTime(sub.RealCommentTime),
                            RealPostTime = sub.RealCommentTime,
                            Link = sub.PosterLink,      // 🔗 link THẬT
                            LinkView = "↳↳ Xem link",   // 👁 hiển thị cho level 2
                            IDFBPerson = "",
                            PosterFBType = FBType.Unknown,
                            Content = sub.Content,
                            Level = 2
                        });

                    }
                }
            }
            //bìn data
            //  _allComments = gridRows; // hoặc build trực tiếp vào _allComments
            _data = gridRows;              // gridRows là List<CommentGridRow>
            gridControl1.DataSource = _data;
            ConfigGrid();
            UIGridHelper.ApplySelect(gridView1, gridControl1);
            // style chung
            // style riêng comment (override)         
            //LOG
            foreach (var parent in parents)
            {
                parentIndex++;

                // ===== LOG COMMENT CHA =====
                Libary.Instance.LogForm(
                    module,
                    $"🧱 COMMENT CHA #{parentIndex}\n" +
                    $"  🆔 {parent.CommentId}\n" +
                    $"  👤 {parent.PosterName}\n" +
                    $"  🔗 {parent.PosterLink}\n" +
                    $"  ⏰ {parent.RealCommentTime}\n" +
                    $"  📝 {ProcessingDAO.Instance.PreviewText(parent.Content)}"
                );

                // 2️⃣ Lấy các phản hồi của comment này
                var replies = comments
                    .Where(x => x.ParentCommentId == parent.CommentId)
                    .ToList();

                int replyIndex = 0;

                foreach (var reply in replies)
                {
                    replyIndex++;

                    // ===== LOG COMMENT CON =====
                    Libary.Instance.LogForm(
                        module,
                        $"    ↳ 💬 PHẢN HỒI #{parentIndex}.{replyIndex}\n" +
                        $"       🆔 {reply.CommentId}\n" +
                        $"       👤 {reply.PosterName}\n" +
                        $"       🔗 {reply.PosterLink}\n" +
                        $"       ⏰ {reply.RealCommentTime}\n" +
                        $"       📝 {ProcessingDAO.Instance.PreviewText(reply.Content)}"
                    );
                }
            }

        }
        private async Task<string> WaitForUserOpenPostAsync(
    IPage page,
    int timeoutMs = 120000) // 2 phút
        {
            var start = DateTime.Now;

            while ((DateTime.Now - start).TotalMilliseconds < timeoutMs)
            {
                string url = page.Url ?? "";

                // 👉 Thoát khi không còn là feed
                if (!string.IsNullOrWhiteSpace(url) &&
                    url.Contains("facebook.com") &&
                    !url.TrimEnd('/').Equals("https://www.facebook.com", StringComparison.OrdinalIgnoreCase) &&
                    !url.TrimEnd('/').Equals("https://www.facebook.com/", StringComparison.OrdinalIgnoreCase))
                {
                    return url;
                }

                await Task.Delay(500); // poll nhẹ
            }

            return null;
        }

        private void btn_StopScan_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            _stopScanRequested = true;

            Libary.Instance.LogForm( module,"⛔ Người dùng yêu cầu DỪNG quét comment");
        }

        private void btn_ExportFull_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (_data == null || _data.Count == 0)
                {
                    MessageBox.Show("⚠ Không có dữ liệu comment");
                    return;
                }

                // ⭐ CHỈ LẤY COMMENT ĐƯỢC CHỌN
                var selectedRows = _data.Where(x => x.Select).ToList();
                if (selectedRows.Count == 0)
                {
                    MessageBox.Show("⚠ Chưa chọn comment nào");
                    return;
                }

                // 3️⃣ Chọn nơi lưu file
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Excel File (*.xlsx)|*.xlsx";
                    sfd.FileName = $"Comment_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                    if (sfd.ShowDialog() != DialogResult.OK)
                        return;
                    string currentPostLink = _ListPostLink.FirstOrDefault() ?? "";
                    // 4️⃣ Gọi helper xuất Excel
                    ExcellHeper.ExportCommentsFullToExcel( selectedRows,currentPostLink, sfd.FileName);

                    MessageBox.Show("✅ Xuất Excel thành công!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi xuất Excel:\n" + ex.Message);
            }
        }

        private void btn_SelectAll_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            UIGridHelper.SelectAll(gridControl1, true);
        }

        private void barButtonItem2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            UIGridHelper.SelectAll(gridControl1, false);  // bỏ chọn tất cả
        }

        private async void btn_ScanIDFB_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!SelectProfilesForScan(out _selectedProfiles))
            {
                MessageBox.Show("⚠ Chưa chọn profile hợp lệ!");
                return;
            }

            if (_data == null || _data.Count == 0)
            {
                MessageBox.Show("⚠ Không có dữ liệu comment");
                return;
            }

            var selectedComments = _data.Where(x => x.Select).ToList();
            if (selectedComments.Count == 0)
            {
                MessageBox.Show("⚠ Chưa chọn comment nào");
                return;
            }

            Libary.Instance.LogForm(
                module,
                $"{Libary.IconInfo} Bắt đầu quét IDFB cho {selectedComments.Count} comment"
            );

            // 1️⃣ Chia comment cho profile
            var jobMap = DistributeComments(_selectedProfiles, selectedComments);

            Libary.Instance.LogForm(
                module,
                $"📦 Chia job | Profile={jobMap.Count} | Comment={jobMap.Sum(x => x.Value.Count)}"
            );

            // cache chung: link → (idfb, type)
            var cache = new ConcurrentDictionary<string, (string id, FBType type)>();
            var tasks = new List<Task>();

            foreach (var kv in jobMap)
            {
                var profile = kv.Key;
                var rows = kv.Value;

                if (rows.Count == 0)
                    continue;

                var task = Task.Run(async () =>
                {
                    Libary.Instance.SetProfileContext(profile.IDAdbrowser, profile.ProfileName);                
                    var page = await Ads.Instance.GetPageEnsureSingleTabAsync(profile.IDAdbrowser);
                    if (page == null)
                    {
                        Libary.Instance.LogForm(
                            module,
                            $"❌ [TAB FAIL] Không lấy được main tab | Profile={profile.ProfileName}"
                        );
                        return; // ✅ ĐÚNG: thoát task, KHÔNG dùng continue
                    }
                    try
                    {
                        foreach (var row in rows)
                        {
                            string link = row.Link;

                            // ❌ link lỗi
                            if (string.IsNullOrWhiteSpace(link))
                            {
                                Libary.Instance.LogForm( module,$"❌ [SKIP] Link rỗng | STT={row.STT}"
                                );
                                continue;
                            }

                            // ♻ CACHE
                            if (cache.TryGetValue(link, out var cached))
                            {
                                row.IDFBPerson = cached.id;
                                row.PosterFBType = cached.type;

                                Libary.Instance.LogForm(
                                    module,
                                    $"♻ [CACHE] {cached.type} | STT={row.STT} | IDFB={cached.id}"
                                );
                                continue;
                            }

                            string idfb = "";
                            FBType fbType = FBType.Unknown;

                            // 2️⃣ DB – PERSON
                            idfb = SQLDAO.Instance.GetIDFBPersonByLink(link);
                            if (!string.IsNullOrWhiteSpace(idfb))
                            {
                                fbType = FBType.Person;
                                Libary.Instance.LogForm(module,$"🗄️ [DB] PERSON | STT={row.STT} | IDFB={idfb}"
                                );
                            }
                            else
                            {
                                // 3️⃣ DB – PAGE
                                idfb = SQLDAO.Instance.GetIDFBPageByLink(link);
                                if (!string.IsNullOrWhiteSpace(idfb))
                                {
                                    fbType = FBType.Fanpage; 
                                    Libary.Instance.LogForm(module, $"🗄️ [DB] PAGE | STT={row.STT} | IDFB={idfb}"
                                    );
                                }
                            }

                            // 4️⃣ CHƯA CÓ → MỞ TAB
                            if (string.IsNullOrWhiteSpace(idfb))
                            {
                                Libary.Instance.LogForm(
                                    module,
                                    $"🌐 [SCAN] Mở tab | STT={row.STT}"
                                );

                                try
                                {
                                    // 🔹 0️⃣ Ưu tiên lấy ID từ URL trước
                                    string idFromUrl = ProcessingDAO.Instance.ExtractIdFromUrl(link);
                                    if (!string.IsNullOrWhiteSpace(idFromUrl))
                                    {
                                        idfb = idFromUrl;
                                        Libary.Instance.LogForm(
                                            module,
                                            $"🔗 [URL ID] STT={row.STT} | IDFB={idfb}"
                                        );
                                    }

                                    // 🔹 1️⃣ Mở trang
                                    await page.GotoAsync(link);
                                    await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                                    // 🔹 2️⃣ Check type CHUNG
                                    fbType = await PageDAO.Instance.CheckTypeCachedAsync(page, link);

                                    // 🔹 3️⃣ PERSON / PAGE / GROUP → dùng hàm mới selectedID
                                    string idFromSelected = await ProcessingDAO.Instance
                                        .ExtractProfileIdFromSelectedIdAsync(page);

                                    if (!string.IsNullOrWhiteSpace(idFromSelected))
                                    {
                                        Libary.Instance.LogForm(
                                            module,
                                            $"🧩 [selectedID] STT={row.STT} | IDFB={idFromSelected}"
                                        );

                                        // 🔹 4️⃣ Nếu URL đã có ID → so sánh
                                        if (!string.IsNullOrWhiteSpace(idfb))
                                        {
                                            if (idfb == idFromSelected)
                                            {
                                                // trùng → chốt
                                                Libary.Instance.LogForm(
                                                    module,
                                                    $"✅ [CONFIRM] URL ID = selectedID | STT={row.STT} | IDFB={idfb}"
                                                );
                                            }
                                            else
                                            {
                                                // khác → ưu tiên selectedID
                                                Libary.Instance.LogForm(
                                                    module,
                                                    $"⚠️ [MISMATCH] URL ID={idfb} | selectedID={idFromSelected} → dùng selectedID"
                                                );
                                                idfb = idFromSelected;
                                            }
                                        }
                                        else
                                        {
                                            // URL không có → lấy selectedID
                                            idfb = idFromSelected;
                                        }
                                    }

                                    // 🔹 5️⃣ LOG KẾT QUẢ CUỐI
                                    Libary.Instance.LogForm(
                                        module,
                                        string.IsNullOrWhiteSpace(idfb)
                                            ? $"❌ [SCAN FAIL] {fbType} | STT={row.STT}"
                                            : $"✅ [SCAN OK] {fbType} | STT={row.STT} | IDFB={idfb}"
                                    );
                                }
                                catch (Exception ex)
                                {
                                    Libary.Instance.LogForm(
                                        module,
                                        $"❌ [ERROR] STT={row.STT} | {ex.Message}"
                                    );
                                }
                            }


                            // 5️⃣ GÁN KẾT QUẢ → GRID
                            row.IDFBPerson = idfb ?? "";
                            row.PosterFBType = fbType;

                            cache.TryAdd(link, (row.IDFBPerson, row.PosterFBType));

                            // 👉 refresh từng dòng (nhìn thấy chạy)
                            gridView1.RefreshRow(_data.IndexOf(row));
                        }
                    }
                    finally
                    {
                        await Ads.Instance.ClosePageAsync(page); // ✅ ĐÚNG CHỖ
                    }
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            gridView1.RefreshData();
            MessageBox.Show("✅ Hoàn tất quét IDFB comment");
        }
        // chia url cho profile
        private Dictionary<ProfileDB, List<CommentGridRow>> DistributeComments(List<ProfileDB> profiles,List<CommentGridRow> comments)
        {
            var result = new Dictionary<ProfileDB, List<CommentGridRow>>();

            foreach (var p in profiles)
                result[p] = new List<CommentGridRow>();

            int index = 0;

            foreach (var c in comments)
            {
                var profile = profiles[index];
                result[profile].Add(c);
                index = (index + 1) % profiles.Count;
            }

            return result;
        }

        private void btn_SaveJson_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_data == null || _data.Count == 0)
            {
                MessageBox.Show("⚠ Không có dữ liệu để lưu");
                return;
            }

            // ⭐ chỉ lưu những dòng được chọn (nếu muốn lưu hết thì bỏ Where)
            var selected = _data.Where(x => x.Select).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("⚠ Chưa chọn dòng nào");
                return;
            }
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "JSON file (*.json)|*.json";
                sfd.Title = "Lưu kết quả scan (JSON)";
                sfd.FileName = $"comment_scan_{DateTime.Now:yyyyMMdd_HHmmss}.json";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                JsonHelper.SaveCommentsJson(
                    sfd.FileName,
                    _currentPostLink,
                    _currentPosterName,
                    selected
                );

                MessageBox.Show("✅ Đã lưu JSON thành công");
            }

        }

        private void btn_LoadDataJson_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "JSON file (*.json)|*.json";
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                // 1️⃣ Load JSON FILE (object đầy đủ)
                var jsonData = JsonHelper.LoadCommentsJson(ofd.FileName);
                if (jsonData == null || jsonData.Comments == null || jsonData.Comments.Count == 0)
                {
                    MessageBox.Show("⚠ File JSON rỗng hoặc không hợp lệ");
                    return;
                }

                // 2️⃣ RESTORE CONTEXT
                _currentPostLink = jsonData.PostLink ?? "";
                _currentPosterName = jsonData.PosterName ?? "";

                // 3️⃣ CONVERT COMMENTS → GRID ROW
                _data = jsonData.Comments.Select(c => new CommentGridRow
                {
                    Select = false,
                    STT = c.STT,
                    ActorName = c.ActorName,
                    Link = c.Link,
                    IDFBPerson = c.IDFB,
                    PosterFBType = c.FBType,
                    Time = c.Time,
                    Content = c.Content,
                    Level = c.Level
                }).ToList();

                // 4️⃣ Bind grid
                gridControl1.DataSource = null;
                gridControl1.DataSource = _data;

                ConfigGrid();

                UIGridHelper.ApplySelect(gridView1, gridControl1);
                UIGridHelper.ApplyCommentLink(gridView1, gridControl1);

                gridView1.RefreshData();

                MessageBox.Show("✅ Load JSON thành công");
            }
        }


        private void btn_ExportFullAndPrint_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_data == null || _data.Count == 0)
            {
                MessageBox.Show("⚠ Không có dữ liệu comment");
                return;
            }

            // ⭐ CHỈ LẤY COMMENT ĐƯỢC CHỌN
            var selected = _data.Where(x => x.Select).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("⚠ Chưa chọn comment nào");
                return;
            }
           

           
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel File (*.xlsx)|*.xlsx";
                sfd.FileName = "BinhLuan_Full.xlsx";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                // 🔹 các thông tin header (có thể thay sau)
                string postLink = _currentPostLink;          // link bài viết
                string posterName = _currentPosterName;      // người đăng bài viết

                ExcellHeper.ExportCommentsFullForPrint(
                    selected,
                    postLink,
                    posterName,
                    sfd.FileName
                );

                MessageBox.Show("✅ Xuất Excel bình luận (in được) thành công!");
            }
        }
    }
}
