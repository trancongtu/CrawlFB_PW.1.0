using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.DAO;
using System.Security.Cryptography;
using Ads = CrawlFB_PW._1._0.DAO.AdsPowerPlaywrightManager;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Helper.Log;
using System.Linq;
using System.Text.Json;
namespace CrawlFB_PW._1._0.Check
{
    public class DomCrawlerTester
    {
        public static async Task RunTestPage(ProfileDB profile,string url,Action<string> log)
        {
            try
            {
                TestLogHelper.Section(log, "OPEN PAGE");

                TestLogHelper.Step(
                    log,
                    "DomCrawlerTester",
                    "RunTestPage",
                    "Goto",
                    url);

                var mainPage =
                    await Ads.Instance
                    .GetPageEnsureSingleTabAsync(profile.IDAdbrowser);

                if (mainPage == null)
                {
                    TestLogHelper.Step(
                        log,
                        "DomCrawlerTester",
                        "RunTestPage",
                        "MainTab",
                        "FAIL");

                    return;
                }

                var page =
                    await Ads.Instance
                    .OpenNewTabAsync(profile.IDAdbrowser);

                if (page == null)
                {
                    TestLogHelper.Step(
                        log,
                        "DomCrawlerTester",
                        "RunTestPage",
                        "OpenTab",
                        "FAIL");

                    return;
                }

                await page.GotoAsync(url);

                await page.WaitForLoadStateAsync(
                    LoadState.DOMContentLoaded);

                await page.WaitForTimeoutAsync(1500);

                TestLogHelper.Step(
                    log,
                    "DomCrawlerTester",
                    "RunTestPage",
                    "PageLoaded",
                    "OK");

                // ===============================
                // DETECT PAGE TYPE
                // ===============================

                TestLogHelper.Section(log, "DETECT PAGE TYPE");

                var type =
                    await CrawlBaseDAO.Instance
                    .CheckFBTypeAsync(page);

                TestLogHelper.Step(
                    log,
                    "CrawlBaseDAO",
                    "CheckFBTypeAsync",
                    "DetectType",
                    type.ToString());

                // ===============================
                // SCROLL LOAD POSTS
                // ===============================

                TestLogHelper.Section(log, "LOAD POSTS");

                List<IElementHandle> posts = new List<IElementHandle>();

                for (int i = 0; i < 5; i++)
                {
                    var current =
                        await page.QuerySelectorAllAsync("div[role='article']");

                    posts = current.ToList();

                    TestLogHelper.Step(
                        log,
                        "DomCrawlerTester",
                        "CollectPosts",
                        "PostsFound",
                        posts.Count.ToString());

                    if (posts.Count >= 10)
                        break;

                    await ProcessingDAO.Instance.HumanScrollAsync(page);

                    await page.WaitForTimeoutAsync(800);
                }

                if (posts.Count == 0)
                {
                    TestLogHelper.Step(
                        log,
                        "DomCrawlerTester",
                        "CollectPosts",
                        "Result",
                        "NO POSTS");

                    return;
                }

                // ===============================
                // TEST POSTS
                // ===============================

                int maxTest = Math.Min(posts.Count, 10);

                TestLogHelper.Section(
                    log,
                    $"TEST {maxTest} POSTS");

                for (int i = 0; i < maxTest; i++)
                {
                    var post = posts[i];

                    TestLogHelper.Section(
                        log,
                        $"POST {i + 1}");

                    await TestSinglePost(
                        page,
                        post,
                        log);
                }

                TestLogHelper.Section(log, "TEST DONE");

                await Ads.Instance.ClosePageAsync(page);
            }
            catch (Exception ex)
            {
                log("ERROR: " + ex.Message);
            }
        }
        static async Task TestSinglePost(IPage page,IElementHandle post,Action<string> log)
        {
            var postinfor = await post.QuerySelectorAllAsync("div.xu06os2.x1ok221b");

            TestLogHelper.Step(
                log,
                "DomCrawlerTester",
                "TestSinglePost",
                "postinfor",
                postinfor.Count.ToString());

            var (timeList, linkList) =
                await CrawlBaseDAO.Instance
                .ExtractTimeAndLinksAsync(postinfor);

            TestLogHelper.Step(
                log,
                "CrawlBaseDAO",
                "ExtractTimeAndLinksAsync",
                "TimeList",
                timeList.Count.ToString());

            TestLogHelper.Step(
                log,
                "CrawlBaseDAO",
                "ExtractTimeAndLinksAsync",
                "LinkList",
                linkList.Count.ToString());

            var result =
                await CrawlBaseDAO.Instance
                .PostTypeDetectorAsync(
                    timeList,
                    linkList,
                    postinfor);

            TestLogHelper.Step(
                log,
                "CrawlBaseDAO",
                "PostTypeDetectorAsync",
                "PostLink",
                result.postLink);

            var poster =
                await CrawlBaseDAO.Instance
                .GetPosterFromProfileNameAsync(post);

            TestLogHelper.Step(
                log,
                "CrawlBaseDAO",
                "GetPosterFromProfileNameAsync",
                "Poster",
                poster.name);

            string content =
                await CrawlBaseDAO.Instance
                .GetContentTextAsync(page, post);

            TestLogHelper.Step(
                log,
                "CrawlBaseDAO",
                "GetContentTextAsync",
                "ContentLength",
                (content?.Length ?? 0).ToString());

            var inter =
                await CrawlBaseDAO.Instance
                .ExtractPostInteractionsAsync(post);

            TestLogHelper.Step(
                log,
                "CrawlBaseDAO",
                "ExtractPostInteractionsAsync",
                "Like",
                inter.likes.ToString());

            TestLogHelper.Step(
                log,
                "CrawlBaseDAO",
                "ExtractPostInteractionsAsync",
                "Comment",
                inter.comments.ToString());

            TestLogHelper.Step(
                log,
                "CrawlBaseDAO",
                "ExtractPostInteractionsAsync",
                "Share",
                inter.shares.ToString());

            var reel =
                await CrawlBaseDAO.Instance
                .DetectReelFromPostAsync(post);

            TestLogHelper.Step(
                log,
                "CrawlBaseDAO",
                "DetectReelFromPostAsync",
                "HasReel",
                reel.hasReel.ToString());

            var video =
                await CrawlBaseDAO.Instance
                .DetectVideoFromPostAsync(post);

            TestLogHelper.Step(
                log,
                "CrawlBaseDAO",
                "DetectVideoFromPostAsync",
                "HasVideo",
                video.hasVideo.ToString());

            var photos =
                await CrawlBaseDAO.Instance
                .DetectPhotosFromPostAsync(post);

            TestLogHelper.Step(
                log,
                "CrawlBaseDAO",
                "DetectPhotosFromPostAsync",
                "PhotoCount",
                photos.Count.ToString());
        }
    }
}