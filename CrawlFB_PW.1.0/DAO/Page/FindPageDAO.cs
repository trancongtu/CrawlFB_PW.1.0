using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Playwright;
using CrawlFB_PW._1._0.DTO;
using System.Text.RegularExpressions;
using CrawlFB_PW._1._0.Enums;
namespace CrawlFB_PW._1._0.DAO
{
    public class FindPageDAO
    {
        private static FindPageDAO _instance;
        public static FindPageDAO Instance => _instance ?? (_instance = new FindPageDAO());

        private FindPageDAO() { }

        private IPage _page;

        /// <summary>
        /// Khởi tạo session Playwright cho FindPage
        /// </summary>
        public async Task<bool> InitAsync(string profileId)
        {
            _page = await AdsPowerPlaywrightManager.Instance.GetPageAsync(profileId);
            if (_page == null)
            {
                Libary.Instance.LogTech(" ❌ Không lấy được Playwright Page!");
                return false;
            }
            return true;
        }
        public async Task<bool> OpenSearchPageAsync(string keyword, string searchType)
        {
            if (_page == null)
                return false;

            try
            {
                string url = $"https://www.facebook.com/search/{searchType}/?q={Uri.EscapeDataString(keyword)}";

                await _page.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle,
                    Timeout = 20000
                });
                Libary.Instance.LogTech(" Open Shearch thành công");
                return true;
            }
            catch
            {
                Libary.Instance.LogTech(" Open Shearch lỗi");
                return false;
            }
        }

        public async Task<List<PageInfo>> RunFastSearchAsync(FBType searchType, int maxPost, int MinFlow)
        {
            var result = new List<PageInfo>();
            int noNewCount = 0;
            int lastCount = 0;

            for (int round = 1; round <= 20; round++)
            {
                Libary.Instance.LogTech($"[FAST] Round {round} bắt đầu…");
                // 1️⃣ LẤY DANH SÁCH HIỆN TẠI
                var feed = _page.Locator("div[aria-label='Kết quả tìm kiếm'][role='main']");
                if (await feed.CountAsync() == 0)
                {
                    Libary.Instance.LogTech("[FAST] ❌ Không tìm thấy FEED.");
                    return result;
                }

                var items = feed.Locator("div[role='article']");
                int count = await items.CountAsync();

                Libary.Instance.LogTech($"[FAST] Hiện có {count} item.");

                // 2️⃣ LẤY TỪNG ITEM CHƯA CÓ TRONG result
                for (int i = lastCount; i < count; i++)
                {
                    var node = items.Nth(i);

                    // TÊN PAGE
                    var (name, rawHref) = await ExtractNameAndLink(node);
                    string pageLink = ProcessingDAO.Instance.ShortLinkPage(rawHref);

                    // MEMBERS

                    string members = await ExtractMembersFromItem(node);
                    int mem = ConvertMembersToInt(members);

                    if (mem >= MinFlow)
                    {
                        result.Add(new PageInfo
                        {
                            PageName = name,
                            PageLink = pageLink,
                            PageMembers = members,
                            PageType = searchType
                        });

                        Libary.Instance.LogTech($"[FAST] + {name} | {members} (OK)", AppConfig.ENABLE_LOG);
                    }
                    else
                    {
                        Libary.Instance.LogTech($"[FAST] - {name} | {members} (BỎ vì < MinFlow)", AppConfig.ENABLE_LOG);
                    }

                    Libary.Instance.LogTech($"[FAST] + {i + 1}: {name} | {pageLink} | {members}", AppConfig.ENABLE_LOG);

                    if (result.Count >= maxPost)
                    {
                        Libary.Instance.LogTech("[FAST] 🔥 Đã đủ maxpost → dừng.");
                        return result;
                    }
                }

                // 3️⃣ KIỂM TRA TĂNG ITEM?
                if (count == lastCount)
                {
                    noNewCount++;
                    Libary.Instance.LogTech($"[FAST] Không thêm item mới ({noNewCount}/3)");

                    if (noNewCount >= 3)
                    {
                        Libary.Instance.LogTech("[FAST] ❌ Không load thêm → dừng scroll.");
                        break;
                    }
                }
                else
                {
                    noNewCount = 0;
                }

                lastCount = count;

                // 4️⃣ SCROLL HUMAN (Quan trọng)
                try
                {
                    var html = await _page.QuerySelectorAsync("html");
                    await ProcessingDAO.Instance.HumanScrollAndClickAsync(_page, html, "Scroll load thêm");
                }
                catch (Exception ex)
                {
                    Libary.Instance.LogTech($"[FAST] ⚠ Scroll lỗi: {ex.Message}");
                }

                await _page.WaitForTimeoutAsync(new Random().Next(400, 800));
            }

            Libary.Instance.LogTech($"[FAST] ✔ Hoàn tất. Tổng thu được {result.Count}");
            return result;
        }
        private async Task<(string name, string link)> ExtractNameAndLink(ILocator node)
        {
            var aTags = node.Locator("a");
            int total = await aTags.CountAsync();

            for (int i = 0; i < total; i++)
            {
                var a = aTags.Nth(i);
                string href = await a.GetAttributeAsync("href");

                if (string.IsNullOrWhiteSpace(href))
                    continue;

                // chọn link dẫn đến page thật
                if (href.Contains("facebook.com") && !href.Contains("privacy") && !href.Contains("__"))
                {
                    string name = await SafeInnerText(a);
                    return (name, href);
                }
            }

            return ("N/A", "");
        }
        private async Task<string> ExtractMembersFromItem(ILocator item)
        {
            try
            {
                var span = item.Locator("span.x1lliihq.x6ikm8r.x10wlt62.x1n2onr6");
                int count = await span.CountAsync();

                if (count == 0)
                    return "N/A";

                // luôn lấy span đầu tiên chứa metadata
                string raw = (await span.First.InnerTextAsync())?.Trim() ?? "";          
                return ExtractMembersText(raw);
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech("Error ExtractMembers: " + ex.Message);
                return "N/A";
            }
        }

        private string ExtractMembersText(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "N/A";

            raw = raw.ToLower();

            string keyword = null;

            if (raw.Contains("người theo dõi"))
                keyword = "người theo dõi";
            else if (raw.Contains("thành viên"))
                keyword = "thành viên";
            else
                return "N/A";

            // Lấy phần trước keyword
            int idx = raw.IndexOf(keyword);
            if (idx < 0)
                return "N/A";

            string before = raw.Substring(0, idx);

            // Tách theo dấu ·
            var parts = before.Split('·');
            if (parts.Length == 0)
                return "N/A";

            // phần gần keyword nhất
            string segment = parts.Last().Trim();

            // Regex tìm số gần nhất
            var match = Regex.Match(segment, @"([\d.,]+)\s*(k)?");

            if (!match.Success)
                return "N/A";

            string num = match.Groups[1].Value.Replace(",", ".").Trim();
            string k = match.Groups[2].Success ? "K" : "";

            return num + k;
        }

        private int ConvertMembersToInt(string members)
        {
            if (string.IsNullOrWhiteSpace(members))
                return 0;

            members = members.ToLower().Trim();

            // Trường hợp có K
            if (members.EndsWith("k"))
            {
                string num = members.Replace("k", "").Trim();
                if (double.TryParse(num, out double v))
                    return (int)(v * 1000);
            }

            // Trường hợp số bình thường
            if (double.TryParse(members.Replace(",", ".").Trim(), out double val))
                return (int)val;

            return 0;
        }

        private async Task<string> SafeInnerText(ILocator loc)
        {
            try { return (await loc.InnerTextAsync())?.Trim(); }
            catch { return ""; }
        }

        //=============
        public async Task<List<PageInfo>> RunAndCheckAsync(FBType searchType, int maxPost, int minFlow)
        {
            // 1️⃣ lấy danh sách page như RUN bình thường
            var list = await RunFastSearchAsync(searchType, maxPost, minFlow);

            var result = new List<PageInfo>();

            foreach (var p in list)
            {
                try
                {
                    // mở tab page
                    var newPage = await _page.Context.NewPageAsync();
                    await newPage.GotoAsync(p.PageLink, new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.NetworkIdle,
                        Timeout = 20000
                    });

                    // lấy last post time
                    DateTime? lastTime = await ScanCheckPageDAO.Instance.GetPostTimeAsync(newPage);
                    p.TimeLastPost = lastTime == DateTime.MinValue? (DateTime?)null: lastTime;


                    Libary.Instance.LogTech($"[CHECK] {p.PageName} | Time = {p.TimeLastPost}");

                    await newPage.CloseAsync();
                }
                catch (Exception ex)
                {
                    p.TimeLastPost = null;
                    Libary.Instance.LogTech($"[CHECK ERROR] {p.PageName}: {ex.Message}");
                }

                result.Add(p);
            }
            return result;
        }




    }
}
