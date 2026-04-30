using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Enums;
namespace CrawlFB_PW._1._0.Service
{
    public class PageInfoCacheService
    {
        private static Dictionary<string, PageInfo> _cache;
        private static DateTime _lastLoadTime = DateTime.MinValue;

        // thời gian cache (phút)
        private const int CACHE_MINUTES = 5;

        private static void EnsureCache()
        {
            // reload nếu chưa có hoặc hết hạn
            if (_cache == null || (DateTime.Now - _lastLoadTime).TotalMinutes > CACHE_MINUTES)
            {
                LoadCache();
            }
        }

        private static void LoadCache()
        {
            _cache = new Dictionary<string, PageInfo>();

            var dt = SQLDAO.Instance.GetAllPagesDB();

            foreach (DataRow r in dt.Rows)
            {
                var page = new PageInfo
                {
                    PageID = r["PageID"] == DBNull.Value ? null : r["PageID"].ToString(),
                    PageName = r["PageName"] == DBNull.Value ? null : r["PageName"].ToString(),
                    PageLink = r["PageLink"] == DBNull.Value ? null : r["PageLink"].ToString(),
                    IDFBPage = r["IDFBPage"] == DBNull.Value ? null : r["IDFBPage"].ToString(),
                    PageMembers = r["PageMembers"] == DBNull.Value ? null : r["PageMembers"].ToString(),
                    PageInteraction = r["PageInteraction"] == DBNull.Value ? null : r["PageInteraction"].ToString(),
                    PageEvaluation = r["PageEvaluation"] == DBNull.Value ? null : r["PageEvaluation"].ToString(),
                    PageInfoText = r["PageInfoText"] == DBNull.Value ? null : r["PageInfoText"].ToString(),

                    // 🔥 enum
                    PageType = r["PageType"] == DBNull.Value
                        ? FBType.Unknown
                        : Enum.TryParse<FBType>(r["PageType"].ToString(), true, out var t)
                            ? t
                            : FBType.Unknown,

                    PageTimeSave = r["PageTimeSave"] == DBNull.Value ? null : r["PageTimeSave"].ToString(),

                    IsScanned = r.Table.Columns.Contains("IsScanned") &&
                                r["IsScanned"] != DBNull.Value &&
                                Convert.ToInt32(r["IsScanned"]) == 1,

                    TimeLastPost = r["TimeLastPost"] == DBNull.Value
                        ? (DateTime?)null
                        : Convert.ToDateTime(r["TimeLastPost"])
                };

                if (!string.IsNullOrWhiteSpace(page.PageID))
                    _cache[page.PageID] = page;
            }

            _lastLoadTime = DateTime.Now;
        }

        public static PageInfo Get(string pageId)
        {
            EnsureCache();

            if (string.IsNullOrEmpty(pageId))
                return null;

            PageInfo page;
            if (_cache.TryGetValue(pageId, out page))
                return page;

            return null;
        }

        // 👉 gọi khi bạn insert/update page
        public static void Clear()
        {
            _cache = null;
        }
    }
}
