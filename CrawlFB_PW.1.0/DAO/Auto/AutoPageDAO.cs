using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.Helper;
using Microsoft.Playwright;
using CrawlFB_PW._1._0.DTO;
namespace CrawlFB_PW._1._0.DAO.Auto
{
    public class AutoPageDAO
    {
        public static AutoPageDAO Instance = new AutoPageDAO();

        public async Task<AutoResult> RunAutoAsync(IPage page,PageInfo pageInfo)
        {
            AutoResult result = new AutoResult();

            try
            {
                await page.GotoAsync(pageInfo.PageLink);

                DateTime lastScan = DatabaseDAO.Instance.GetPageLastScan(pageInfo.PageID);

                await ProcessingDAO.Instance.ScrollToLoadPostsAsync(page, AppConfig.scrollCount);

                var feed = await PageDAO.Instance.GetFeedContainerAsync(page);
                if (feed == null) return result;

                var nodes = await feed.QuerySelectorAllAsync("div[class='x1n2onr6 x1ja2u2z']");

                HashSet<string> unique = new HashSet<string>();

                foreach (var node in nodes)
                {
                    var posts = await PageDAO.Instance.GetPostAutoPageAsyncV3(
                        page, node, pageInfo.PageName, pageInfo.PageLink);

                    foreach (var post in posts)
                    {
                        if (DatabaseDAO.Instance.ExistPostByLink(post.PostLink))
                            return result;

                        var realTime = TimeHelper.ParseFacebookTime(post.PostTime);
                        if (realTime <= lastScan)
                            return result;

                        if (unique.Contains(post.PostLink)) continue;

                        unique.Add(post.PostLink);
                        result.Posts.Add(post);
                        result.NewPosts++;
                    }
                }

                foreach (var p in result.Posts)
                    DatabaseDAO.Instance.InsertOrIgnorePost(p);

                DatabaseDAO.Instance.UpdatePageLastScan(pageInfo.PageID);
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("AUTO ERROR: " + ex.Message);
            }

            return result;
        }
    }
}
