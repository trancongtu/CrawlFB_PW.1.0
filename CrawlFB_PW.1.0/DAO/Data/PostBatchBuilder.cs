using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.DTO;
using System.Data;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.Enums;
namespace CrawlFB_PW._1._0.DAO.Data
{
    public class PostBatchBuilder
    {
        public DataTable BuildPostTable(List<PostPage> posts, Dictionary<PostPage, string> idMap)
        {
            var dt = new DataTable();

            dt.Columns.Add("PostID");
            dt.Columns.Add("PostLink");
            dt.Columns.Add("PostContent");
            dt.Columns.Add("PostTime");
            dt.Columns.Add("RealPostTime", typeof(DateTime));
            dt.Columns.Add("LikeCount", typeof(int));
            dt.Columns.Add("ShareCount", typeof(int));
            dt.Columns.Add("CommentCount", typeof(int));
            dt.Columns.Add("PostAttachment");
            dt.Columns.Add("PostStatus");

            foreach (var p in posts)
            {
                dt.Rows.Add(
                 idMap[p],
                 p.PostLink,
                 p.Content ?? "",
                 p.PostTime ?? "",
                 p.RealPostTime.HasValue ? (object)p.RealPostTime.Value : DBNull.Value,
                 p.LikeCount ?? 0,
                 p.ShareCount ?? 0,
                 p.CommentCount ?? 0,
                 p.Attachment ?? "",
                 p.PostType
             );
            }

            return dt;
        }

        public DataTable BuildPageTable(
    List<PostPage> posts,
    Dictionary<string, string> pageMap)
        {
            var dt = new DataTable();

            dt.Columns.Add("PageID");
            dt.Columns.Add("IDFBPage");
            dt.Columns.Add("PageLink");
            dt.Columns.Add("PageName");

            var added = new HashSet<string>();

            foreach (var p in posts)
            {
                // =========================================
                // 🔥 1. CONTAINER PAGE
                // =========================================
                string containerKey = !string.IsNullOrEmpty(p.ContainerIdFB)
                    ? p.ContainerIdFB
                    : UrlHelper.NormalizeFacebookUrl(p.PageLink);

                if (!string.IsNullOrEmpty(containerKey) && pageMap.ContainsKey(containerKey))
                {
                    var pageId = pageMap[containerKey];

                    if (!added.Contains(pageId))
                    {
                        added.Add(pageId);

                        dt.Rows.Add(
                            pageId,
                            string.IsNullOrEmpty(p.ContainerIdFB) ? (object)DBNull.Value : p.ContainerIdFB,
                            UrlHelper.NormalizeFacebookUrl(p.PageLink),
                            p.PageName ?? ""
                        );
                    }
                }

                // =========================================
                // 🔥 2. POSTER FANPAGE
                // =========================================
                if (p.PosterNote == FBType.Fanpage && !string.IsNullOrEmpty(p.PosterIdFB))
                {
                    string posterKey = p.PosterIdFB;

                    if (pageMap.ContainsKey(posterKey))
                    {
                        var pageIdPoster = pageMap[posterKey];

                        if (!added.Contains(pageIdPoster))
                        {
                            added.Add(pageIdPoster);

                            dt.Rows.Add(
                                pageIdPoster,
                                p.PosterIdFB,
                                UrlHelper.NormalizeFacebookUrl(p.PosterLink),
                                p.PosterName ?? ""
                            );
                        }
                    }
                }
            }

            return dt;
        }

        public DataTable BuildPersonTable(
     Dictionary<string, string> personMap,
     List<PostPage> posts)
        {
            var dt = new DataTable();

            dt.Columns.Add("PersonID");
            dt.Columns.Add("PersonLink");
            dt.Columns.Add("PersonName");
            dt.Columns.Add("PersonNote");

            foreach (var kv in personMap)
            {
                var key = kv.Key;
                var id = kv.Value;

                // 🔥 bỏ key bẩn
                if (string.IsNullOrWhiteSpace(key) || key == "N/A")
                    continue;

                // 🔥 ANONYMOUS
                if (id == SystemIds.PERSON_ANONYMOUS_ID)
                {
                    dt.Rows.Add(id, "", SystemIds.PERSON_ANONYMOUS_NAME, FBType.PersonHidden);
                    continue;
                }

                var p = posts.FirstOrDefault(x => x.PosterIdFB == key);

                dt.Rows.Add(
                    id,
                    p?.PosterLink,
                    p?.PosterName,
                    p?.PosterNote ?? FBType.Unknown
                );
            }

            return dt;
        }

        public DataTable BuildPostMapTable_V4(
    List<PostPage> posts,
    Dictionary<PostPage, string> idMap,
    Dictionary<string, string> pageMap,
    Dictionary<string, string> personMap,
    string pageCrawlId)
        {
            Libary.Instance.LogForm("savelog",
    $"[BUILD_MAP_V4] Start | Total Posts = {posts.Count}");
            var dt = new DataTable();

            dt.Columns.Add("PostID");
            dt.Columns.Add("PageIDCreate");
            dt.Columns.Add("PageIDContainer");
            dt.Columns.Add("PersonIDCreate");

            foreach (var p in posts)
            {
                // 🔥 KEY container phải ưu tiên IDFB
                string containerKey = !string.IsNullOrEmpty(p.ContainerIdFB)
                    ? p.ContainerIdFB
                    : UrlHelper.NormalizeFacebookUrl(p.PageLink);

                string pageContainer = pageMap.ContainsKey(containerKey)
                    ? pageMap[containerKey]
                    : pageCrawlId;

                string pageCreate = null;
                string person = null;

                if (p.ContainerType == FBType.Fanpage)
                {
                    pageCreate = pageContainer;
                    Libary.Instance.LogForm("savelog",$"[CREATE][FANPAGE_CONTAINER] PageCreate = {pageCreate}");
                }
                else if (p.ContainerType == FBType.GroupOn)
                {
                    string key = !string.IsNullOrEmpty(p.PosterIdFB)? p.PosterIdFB: UrlHelper.NormalizeFacebookUrl(p.PosterLink);
                    switch (p.PosterNote)
                    {
                        case FBType.Person:
                        case FBType.PersonKOL:                        
                            personMap.TryGetValue(key, out person);
                            Libary.Instance.LogForm("savelog",$"[CREATE][PERSON] Key: {key} | Found: {personMap.ContainsKey(key)} | PersonID: {person}");
                            break;

                        case FBType.Fanpage:
                            bool found = !string.IsNullOrEmpty(p.PosterIdFB) && pageMap.ContainsKey(p.PosterIdFB);

                            if (found)
                            {
                                pageCreate = pageMap[p.PosterIdFB];
                                Libary.Instance.LogForm("savelog", $"[CREATE][FANPAGE_POSTER] PosterIdFB: {p.PosterIdFB} | Found: {found} | PageCreate: {pageCreate}");
                            }                         
                            if (!found)
                            {
                                foreach (var k in pageMap.Keys)
                                {
                                    if (k.Contains(key) || key.Contains(k))
                                    {
                                        Libary.Instance.LogForm("savelog",
                                            $"[NEAR_MATCH] key='{key}' ~ mapKey='{k}'");
                                    }
                                }
                            }
                            break;
                        case FBType.PersonHidden:
                        case FBType.Unknown:
                            person = SystemIds.PERSON_ANONYMOUS_ID;
                            break;

                        default:
                            // không xác định → để null
                            break;
                    }
                }
                Libary.Instance.LogForm("savelog",
    $"[BUILD_MAP_V4] PostID: {p.PostID} | ContainerType: {p.ContainerType} | PosterNote: {p.PosterNote}");
                dt.Rows.Add(
                    idMap[p],
                    pageCreate,
                    pageContainer,
                    person
                );
                Libary.Instance.LogForm("savelog",
    $"[RESULT] PostID: {p.PostID} | PageCreate: {pageCreate} | PageContainer: {pageContainer} | Person: {person}");
            }

            return dt;
        }

        public static string GenerateStableId(string input)
        {
            if (string.IsNullOrEmpty(input))
                return Guid.NewGuid().ToString("N").Substring(0, 16);

            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input.Trim().ToLowerInvariant());
                var hash = sha1.ComputeHash(bytes);

                var sb = new StringBuilder(16);
                for (int i = 0; i < 8; i++)
                {
                    sb.Append(hash[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
      
        public DataTable BuildShareTable(List<ShareItem> shares)
        {
            var dt = new DataTable();

            dt.Columns.Add("PostID");
            dt.Columns.Add("PageID");
            dt.Columns.Add("PersonID");
            dt.Columns.Add("TimeShare");
            dt.Columns.Add("RealTimeShare", typeof(DateTime));

            if (shares == null) return dt;

            foreach (var s in shares)
            {
                dt.Rows.Add(
                    s.PostID_B,
                    s.PageID_A,
                    s.PersonIDShare,
                    s.ShareTimeRaw,
                    s.ShareTimeReal.HasValue ? (object)s.ShareTimeReal.Value : DBNull.Value
                );
            }

            return dt;
        }
    }
}
