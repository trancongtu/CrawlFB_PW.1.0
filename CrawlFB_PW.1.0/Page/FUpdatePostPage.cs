using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DTO;
using PageInfoDTO = CrawlFB_PW._1._0.DTO.PageInfo;
using CrawlFB_PW._1._0.Enums;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.Profile;
using CrawlFB_PW._1._0.ViewModels;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraPrinting;
using CrawlFB_PW._1._0.Helpers;
using DevExpress.XtraBars;
using CrawlFB_PW._1._0.Service;
using System.Security.Cryptography;
using Ads = CrawlFB_PW._1._0.DAO.AdsPowerPlaywrightManager;
using DevExpress.XtraGrid;
using System.Diagnostics;
using CrawlFB_PW._1._0.Helper.UI;
using System.Security.Policy;
using DevExpress.Utils.About;
using CrawlFB_PW._1._0.DAO.Page;
using DevExpress.DocumentView;
namespace CrawlFB_PW._1._0.Page
{
    public partial class FUpdatePostPage : Form
    {
        const string module = nameof(FUpdatePostPage);   
        private List<PageInfoDTO> _selectedPages = new List<PageInfoDTO>();
        private BindingList<PageInfoViewModel> _pageList = new BindingList<PageInfoViewModel>();
        private BindingList<PostInfoViewModel> _postList = new BindingList<PostInfoViewModel>();
        private List<ProfileDB> _selectedProfiles = new List<ProfileDB>(); //lưu danh sách profile
        private List<string> _listUrls = new List<string>();// lưu danh sách page
        private Dictionary<string, List<ShareItem>> _shareMap = new Dictionary<string, List<ShareItem>>();// lưa để thêm bảng share
        // Lock để tránh race condition khi chạy đa profile
        private readonly object _shareMapLock = new object();
        public FUpdatePostPage()
        {
            InitializeComponent();
            this.Load += FUpdatePostPage_Load;
            UICommercialHelper.StyleGrid(gridViewPost);          
            // UIStyleHelper.StyleBarManager(barManager1);          
        }
        private void FUpdatePostPage_Load(object sender, EventArgs e)
        {
            gridViewPage.OptionsBehavior.AutoPopulateColumns = false;
            gridViewPost.OptionsBehavior.AutoPopulateColumns = false;
            gridViewPage.FocusedRowChanged -= gridViewPage_FocusedRowChanged;
            gridViewPage.FocusedRowChanged += gridViewPage_FocusedRowChanged;
            gridControlPost.DataSourceChanged += GridControlPost_DataSourceChanged;
        }
        //============================================================================/////
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
        private void barButtonItemLoadPage_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (var f = new FSelectPageScanNew())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    _selectedPages = f.SelectedPages;

                    LoadPageList(); // ⭐ load lại grid

                    MessageBox.Show($"✔ Đã chọn {_selectedPages.Count} page!");
                }
            }
        }
        private void InitPageGrid()
        {
            var gv = gridViewPage;

            gv.BeginUpdate();
            try
            {
                gv.PopulateColumns();

                // ===== STT =====
                UIGridHelper.EnableRowIndicatorSTT(gv);

                // ===== Select =====
                UIGridHelper.ApplySelect(gv, gridControlPage);
                UIGridHelper.LockAllColumnsExceptSelect(gv);
                // ===== Caption tiếng Việt =====
                UIGridHelper.ApplyVietnameseCaption(gv);

                // ===== ẨN CỘT KHÔNG CẦN =====
                UIGridHelper.ShowOnlyColumns(
                    gv,
                    "Select",
                    "PageName",
                    "TimeLastPostView",
                    "Status"
                );
                // ===== Status =====
                UIGridHelper.EnableStatusDisplay(gv);
                UIGridHelper.ApplyRowColorByStatus(gv, "Status");
                // 3️⃣ (Khuyến nghị) giảm indicator
                gv.IndicatorWidth = 25; // Độ rộng STT                                      
                gv.BestFitColumns();
            }
            finally
            {
                gv.EndUpdate();
            }
        }
        private void LoadPageList()
        {
            _pageList.Clear();

            if (_selectedPages == null || _selectedPages.Count == 0)
            {
                gridControlPage.DataSource = _pageList;
                return;
            }

            foreach (var p in _selectedPages)
            {
                var vm = new PageInfoViewModel
                {
                    PageID = p.PageID,
                    PageName = p.PageName,
                    PageLink = p.PageLink,
                    TimeLastPost = p.TimeLastPost,                 
                    Status = UIStatus.Pending,   // trạng thái ban đầu
                    Select = false
                };

                _pageList.Add(vm);
            }

            gridControlPage.DataSource = _pageList;

            InitPageGrid(); // ⭐ cấu hình grid
        }
        private void InitPagePost()
        {
            var gv = gridViewPost;

            if (gv.Tag as string == "INIT_DONE")
                return;

            gv.BeginUpdate();
            try
            {
                UIGridHelper.EnableRowIndicatorSTT(gv);
                UIGridHelper.ApplyVietnameseCaption(gv);

             UIGridHelper.ShowOnlyColumns(
                gv,
                "PostLink",
                "TimeView",
                "Content",
                "AttachmentView",   // 🔥 THÊM
                "Like",
                "Share",
                "Comment",
                "PostType",
                "PosterName",
                "PosterLink",
                "PosterNote",
                "PageName",
                "PageLink"
            );


                UIGridHelper.ApplyLinkDisplayText(gridViewPost);
                UIGridHelper.EnableLinkClickByRowCell(gridViewPost);
                UIGridHelper.ApplyLinkTooltip(gridViewPost, gridControlPost);
                UIGridHelper.ApplyAttachmentLink(gridViewPost, gridControlPost, "AttachmentView");
                // UIGridHelper.LockAllColumnsExceptLinks(gv);
                gv.OptionsSelection.EnableAppearanceFocusedCell = false;
                gv.FocusRectStyle = DrawFocusRectStyle.RowFocus;

                gv.Tag = "INIT_DONE";
            }
            finally
            {
                gv.EndUpdate();
            }
        }
        private void LoadPostsForPage(PageInfoViewModel page)
        {
            if (page == null)
            {
                _postList.Clear();
                return;
            }

            _postList = new BindingList<PostInfoViewModel>(page.Posts);
            gridControlPost.DataSource = _postList;

            // 🔥 ÉP GRID RENDER
            gridControlPost.ForceInitialize();
            gridViewPost.RefreshData();

            // reset init flag
            gridViewPost.Tag = null;

            InitPagePost();
        }
        private void GridControlPost_DataSourceChanged(object sender, EventArgs e)
        {
            var gv = gridViewPost;

            gv.BeginUpdate();
            try
            {
                // 🔥 BẮT BUỘC: tạo cột từ ViewModel
                gv.PopulateColumns();
                // 4️⃣ Thứ tự cột (STT là RowIndicator)
                gv.Columns[nameof(PostInfoViewModel.TimeView)].VisibleIndex = 1;

                // ép render
                gridControlPost.ForceInitialize();
                gv.RefreshData();

                // reset init flag
                gv.Tag = null;

                // init UI (show/hide, hyperlink, caption…)
                InitPagePost();
            }
            finally
            {
                gv.EndUpdate();
            }
        }
        private void gridViewPage_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle < 0)
                return;

            var page = gridViewPage.GetRow(e.FocusedRowHandle)
                as PageInfoViewModel;

            if (page == null)
                return;

            // bind post theo page đang chọn
            _postList = new BindingList<PostInfoViewModel>(page.Posts);
            gridControlPost.DataSource = _postList;
        }
        private async void barButtonItemRun_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (!SelectProfilesForScan(out _selectedProfiles))
                {
                    MessageBox.Show("⚠ Chưa chọn profile hợp lệ!");
                    return;
                }
                // ===========================================
                // 1 LẤY PAGE ĐƯỢC CHỌN TỪ GRID (VIEWMODEL)
                // ===========================================
                var runPages = _pageList
                    .Where(x => x.Select)
                    .ToList();

                if (runPages.Count == 0)
                {
                    MessageBox.Show("⚠ Bạn chưa chọn Page nào để quét!");
                    return;
                }
                if (!CheckPageNotFirstScannedByDB(runPages))
                    return;
                // ===========================================
                // 4️⃣ CHIA PAGE THEO PROFILE (SERVICE)
                // ===========================================
                var service = new PageDistributionService();

                var jobMap = service.Distribute(
                    _selectedProfiles,
                    runPages,                // ⚠️ CHỈ PAGE ĐƯỢC SELECT
                    x => x.PageName,          // key để log / debug
                    "FirstScan"
                );

                // ===========================================
                // 4️⃣ CHẠY TỪNG PROFILE – PAGE
                // ===========================================
                List<Task> allTasks = new List<Task>();

                foreach (var kv in jobMap)
                {
                    var profile = kv.Key;
                    var pages = kv.Value;

                    // Mỗi profile chạy 1 task song song
                    var t = Task.Run(async () =>
                    {
                        foreach (var page in pages)
                        {
                            string pageId = page.PageID;
                            Libary.Instance.LogForm(module, "chạy PageLink: " + page.PageLink);
                            // update UI trước khi quét
                            this.Invoke(new Action(() =>
                            {
                                page.Status = UIStatus.Running;
                            }));

                            var mainPage = await Ads.Instance.GetPageEnsureSingleTabAsync(profile.IDAdbrowser);
                            if (mainPage == null)
                                continue;
                            var crawlPage = await Ads.Instance.OpenNewTabAsync(profile.IDAdbrowser);
                            Libary.Instance.SetProfileContext(profile.IDAdbrowser, profile.ProfileName);                           
                            var result = await UpdatePostPageDAO.Instance.UpdatePostPageAsync(crawlPage, page.PageLink, pageId, page.TimeLastPost);
                            var posts = result.Posts;
                            var shares = result.Shares;                         
                            var postVMs = posts.Select(p => new PostInfoViewModel
                            {
                                PostID = p.PostID,
                                PostLink = p.PostLink,
                                Content = p.Content,

                                PosterName = p.PosterName,
                                PosterLink = p.PosterLink,
                                PosterNote = p.PosterNote,

                                PageName = p.PageName,
                                PageLink = p.PageLink,

                                Like = p.LikeCount ?? 0,
                                Comment = p.CommentCount ?? 0,
                                Share = p.ShareCount ?? 0,
                                PostType = ProcessingHelper.MapPostType(p.PostType),
                                
                                Attachment = p.Attachment,
                                AttachmentView = AttachmentHelper.GetAttachmentForView(p.Attachment),

                                // 🔥 FLAG DÙNG CHO ICON
                                HasReel = p.PostType.Contains("Reel"),
                                HasVideo = p.PostType.Contains("Video"),
                                HasPhoto = p.PostType.Contains("Photo"),

                                PostTimeRaw = p.PostTime,
                                RealPostTime = p.RealPostTime,
                                Status = UIStatus.Done,     // hoặc Pending
                                ParentPage = page           // ⭐ BÂY GIỜ GÁN ĐƯỢC
                            }).ToList();

                            Libary.Instance.LogForm(module, "Chạy " + page.PageLink + " được tổng: " + posts.Count() + " bài viết");
                            foreach ( var post in posts )
                            {
                                LogPostResult(module, post);
                            }                             
                            this.Invoke(new Action(() =>
                            {
                                // gán post cho page
                                page.Posts.Clear();
                                foreach (var vm in postVMs)
                                    page.Posts.Add(vm);
                                // update trạng thái page
                                page.Status = UIStatus.Done;
                                // nếu page đang focus thì load post grid
                                var focusedPage = gridViewPage.GetFocusedRow() as PageInfoViewModel;
                                if (focusedPage == page)
                                {
                                    _postList = new BindingList<PostInfoViewModel>(page.Posts);
                                    gridControlPost.DataSource = _postList;
                                }
                            }));
                            await Ads.Instance.ClosePageAsync(crawlPage);
                        }
                    });

                    allTasks.Add(t);
                }
                // ⭐ CHỜ TẤT CẢ PROFILE CHẠY XONG SONG SONG
                await Task.WhenAll(allTasks);
                MessageBox.Show("🎉 Hoàn tất quét đồng thời!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Lỗi: " + ex.Message);
                Libary.Instance.LogForm(module, "[FirstScan] ❌ " + ex.Message);
            }
        }
        private bool CheckPageNotFirstScannedByDB(IEnumerable<PageInfoViewModel> runPages)
        {
            var notScanned = new List<string>();

            foreach (var p in runPages)
            {
                int isScanned = SQLDAO.Instance.GetIsScanned(p.PageID);
                if (isScanned == 0)
                {
                    notScanned.Add(p.PageName);
                }
            }

            if (notScanned.Count == 0)
                return true;

            string msg = string.Join(
                "\n",
                notScanned.Select(x => "• " + x)
            );

            MessageBox.Show(
                "⚠ Các page sau CHƯA chạy FIRST SCAN:\n\n" +
                msg +
                "\n\n👉 Vui lòng chạy FIRST SCAN trước khi UPDATE.",
                "Chưa đủ điều kiện",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            return false;
        }
        private void barButtonItemSelectAll_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
         UIGridHelper.SelectAll(gridControlPage, true);
        }
        private void barButtonItemremoveAll_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            UIGridHelper.SelectAll(gridControlPage, false);
        }
        private void barButtonItemSaveDB_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // 1️⃣ Lấy page được chọn theo ViewModel
            var selectedPages = _pageList
                .Where(x => x.Select)
                .ToList();

            if (selectedPages.Count == 0)
            {
                MessageBox.Show("⚠ Vui lòng chọn Page cần lưu!");
                return;
            }

            // 2️⃣ Confirm
            if (MessageBox.Show(
                $"Lưu kết quả quét cho {selectedPages.Count} page?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            int totalPages = 0;
            int totalPosts = 0;
            int totalShares = 0;
            foreach (var page in selectedPages) // đang load VM
            {
                totalPages++;

                // 1️⃣ LƯU POST
                foreach (var vm in page.Posts)
                {
                    var dto = vm.ToPostPage(); // VM → DTO
                    SQLDAO.Instance.InsertOrIgnorePost(dto);
                    totalPosts++;
                }

                // 2️⃣ LƯU SHARE
                if (!_shareMap.ContainsKey(page.PageLink))
                    continue;

                foreach (var share in _shareMap[page.PageLink])
                {
                    if (string.IsNullOrEmpty(share.PostLinkB))
                        continue;

                    string postId = SQLDAO.Instance.GetPostIdByPostLink(share.PostLinkB);
                    if (string.IsNullOrEmpty(postId))
                        continue;

                    SQLDAO.Instance.InsertTablePostShare(
                        postId,
                        page.PageID,
                        null,
                        TimeHelper.NormalizeTime(share.ShareTimeReal),
                        share.ShareTimeReal
                    );

                    totalShares++;
                }
                // 🔄 CẬP NHẬT TimeLastPost
                DateTime? newestTime = SQLDAO.Instance.GetNewestPostTime(page.PageID);

                if (newestTime.HasValue)
                {
                    SQLDAO.Instance.UpdatePageLastPostTime(page.PageID, newestTime);
                    // update VM để UI phản ánh ngay
                    page.TimeLastPost = newestTime;
                }

            }
            MessageBox.Show(
        $"Lưu database thành công!\n\n" +
        $"Page  : {totalPages}\n" +
        $"Post  : {totalPosts}\n" +
        $"Share : {totalShares}",
        "Save DB",
        MessageBoxButtons.OK,
        MessageBoxIcon.Information
    );

        }
        private void LogPostResult(string module, PostPage post)
        {
            var sb = new StringBuilder();

            sb.AppendLine("📝 POST RESULT");
            sb.AppendLine($"Page Chứa: {post.PageName}");
            sb.AppendLine($"{Libary.IconOK} Poster: {post.PosterName}");
            sb.AppendLine($"🔗 Link: {post.PostLink}");
            sb.AppendLine($"⏱ Time: {post.PostTime}");
            sb.AppendLine($"⏱ RealTime: {post.RealPostTime.ToString()}");
            sb.AppendLine($"🧾 ContentLen: {post.Content?.Length ?? 0}");
            string contentView = ProcessingHelper.PreviewText(post.Content);
            sb.AppendLine($"⏱ ContentView: {post.Content}");
            bool hasInteract =(post.LikeCount + post.CommentCount + post.ShareCount) > 0;
            sb.AppendLine($"{Libary.BoolIcon(hasInteract)} Interact: " +
                $"👍={post.LikeCount} 💬={post.CommentCount} 🔁={post.ShareCount}"
            );
            sb.AppendLine($"📌 Status: {post.PostType.ToString()}");
            Libary.Instance.LogForm(module, sb.ToString());
        }
    }
}
