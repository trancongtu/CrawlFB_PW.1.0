using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.Helper.Log;
using CrawlFB_PW._1._0.ViewModels.DOM;
using Microsoft.Playwright;
namespace CrawlFB_PW._1._0.Check
{
    public static class DOMCheck
    {
        // ===============================
        // SELECTOR REGISTRY
        // ===============================

        public static Dictionary<string, DOMSelectorModel> Selectors = new Dictionary<string, DOMSelectorModel>();
        public static string SelectorFile = Path.Combine(AppContext.BaseDirectory, "DOMSelectors.json");
        public static void LoadSelectors()
        {
            if (!File.Exists(SelectorFile))
            {
                // tạo file mặc định lần đầu
                Selectors = new Dictionary<string, DOMSelectorModel>()
        {
            { "POST_CONTAINER", new DOMSelectorModel{ selector="div[role='article']"} },
            { "POST_INFO", new DOMSelectorModel{ selector="div.xu06os2.x1ok221b"} },
            { "POSTER", new DOMSelectorModel{ selector="span.xjp7ctv a"} },
            { "CONTENT", new DOMSelectorModel{ selector="div[data-ad-rendering-role='story_message']"} },
            { "TIME_LINK", new DOMSelectorModel{ selector="a[href*='/posts/']"} },
            { "LIKE", new DOMSelectorModel{ selector="span.x1e558r4"} },
            { "INTERACTION", new DOMSelectorModel{ selector="span.html-span"} },
            { "PHOTO", new DOMSelectorModel{ selector="img[data-imgperflogname='feedPostPhoto']"} },
            { "REEL_LINK", new DOMSelectorModel{ selector="a[href*='/reel/']"} }
        };

                SaveSelectors();
                return;
            }

            string json = File.ReadAllText(SelectorFile);

            Selectors =
                JsonSerializer.Deserialize<
                    Dictionary<string, DOMSelectorModel>>(json);
        }
        // ===============================
        // SAVE DOM SNAPSHOT
        // ===============================
        public static void SaveSelectors()
        {
            string json =
                JsonSerializer.Serialize(
                    Selectors,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

            File.WriteAllText(SelectorFile, json);
        }
        public static async Task CheckCrawlerPipeline(
    IPage page,
    Action<string> log)
        {
            TestLogHelper.Section(log, "CHECK CƠ BẢN");

            // =========================
            // Container
            // =========================

            var containers =
                await page.QuerySelectorAllAsync("div[role='article']");

            log($"- Container: lấy được bảng post ({containers.Count})");

            if (containers.Count == 0)
                return;

            // =========================
            // Node
            // =========================

            var nodes =
                await page.QuerySelectorAllAsync("div.xu06os2.x1ok221b");

            log($"- Node: lấy được node post ({nodes.Count})");

            // =========================
            // PostInfor
            // =========================

            var firstPost = containers[0];

            var postInfor =
                await firstPost.QuerySelectorAllAsync(
                    "div.xu06os2.x1ok221b");

            log($"- PostInfor: lấy được postinfor ({postInfor.Count})");

            if (postInfor.Count == 0)
                return;

            // =========================
            // RawPostInfo
            // =========================

            var (timeList, linkList) =
                await CrawlBaseDAO.Instance
                .ExtractTimeAndLinksAsync(postInfor);

            log($"- RawPostInfo:");

            log($"  + TimeList: {timeList.Count}");
            log($"  + LinkList: {linkList.Count}");

            // =========================
            // Chi tiết
            // =========================

            TestLogHelper.Section(log, "CHECK CHI TIẾT");

            // Time
            if (timeList.Count > 0)
                log($"- time: OK ({timeList[0]})");
            else
                log($"- time: FAIL");

            // Link
            if (linkList.Count > 0)
                log($"- link: OK ({linkList[0]})");
            else
                log($"- link: FAIL");

            // Poster
            var poster =
                await CrawlBaseDAO.Instance
                .GetPosterFromProfileNameAsync(firstPost);

            log($"- người đăng: {poster.name}");

            // Content
            string content =
                await CrawlBaseDAO.Instance
                .GetContentTextAsync(page, firstPost);

            log($"- content: {(string.IsNullOrEmpty(content) ? "rỗng" : $"OK ({content.Length})")}");

            // Interaction
            var inter =
                await CrawlBaseDAO.Instance
                .ExtractPostInteractionsAsync(firstPost);

            log($"- tương tác:");

            log($"  + Like: {inter.likes}");
            log($"  + Comment: {inter.comments}");
            log($"  + Share: {inter.shares}");

            // Page chứa
            log($"- page chứa:");

            log($"  + PageName: {poster.name}");
        }
    }
}
