using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using CrawlFB_PW._1._0.ViewModels;
using CrawlFB_PW._1._0.DTO;
namespace CrawlFB_PW._1._0.DAO.phantich
{
    public class AnalyzeSQLDAO
    {
        private static AnalyzeSQLDAO _instance;

        public static AnalyzeSQLDAO Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AnalyzeSQLDAO();
                return _instance;
            }
        }

        private AnalyzeSQLDAO() { }
        public bool HasAnyEvaluation()
        {
            object v = SQLDAO.Instance.ExecuteScalar("SELECT TOP 1 1 FROM TablePostEvaluation");
            return v != null;
        }
        public bool HasPendingEvaluation()
        {
                int currentVersion = SQLDAO.Instance.GetKeywordVersion();

                object v = SQLDAO.Instance.ExecuteScalar(@"
            SELECT TOP 1 1
            FROM TablePostInfo p
            LEFT JOIN TablePostEvaluation e
                ON p.PostID = e.PostID
            WHERE 
                e.PostID IS NULL
                OR e.KeywordVersion < @ver
            ", new Dictionary<string, object>
            {
                { "@ver", currentVersion }
            });

                return v != null;
        }
        //==I. LOAD RAM
        public Dictionary<int, (int Score, int Level)> LoadAttentionDictionary()
        {
            var dict = new Dictionary<int, (int, int)>();

            var dt = SQLDAO.Instance.ExecuteQuery(@"
        SELECT KeywordId, Score, TrackingLevel
        FROM TableAttentionKeywordScore
    ");

            foreach (DataRow row in dt.Rows)
            {
                int id = Convert.ToInt32(row["KeywordId"]);
                int score = Convert.ToInt32(row["Score"]);
                int level = Convert.ToInt32(row["TrackingLevel"]);

                dict[id] = (score, level);
            }

            return dict;
        }
        public Dictionary<int, (int Score, int Level)> LoadNegativeDictionary()
        {
            var dict = new Dictionary<int, (int, int)>();

            var dt = SQLDAO.Instance.ExecuteQuery(@"
        SELECT KeywordId, Score, NegativeLevel
        FROM TableNegativeKeywordScore
    ");

            foreach (DataRow row in dt.Rows)
            {
                int id = Convert.ToInt32(row["KeywordId"]);
                int score = Convert.ToInt32(row["Score"]);
                int level = Convert.ToInt32(row["NegativeLevel"]);

                dict[id] = (score, level);
            }

            return dict;
        }
        public HashSet<int> GetExcludedKeywordIds()
        {
            var result = new HashSet<int>();

            var dt = SQLDAO.Instance.ExecuteQuery(@"
        SELECT KeywordId
        FROM TableExcludeKeyword
    ");

            foreach (DataRow row in dt.Rows)
            {
                result.Add(Convert.ToInt32(row["KeywordId"]));
            }

            return result;
        }
        // Bảng    PostEvaluation
        //1.Lưu
        public void SavePostEvaluation(string postId, int attentionScore, int attentionLevel,
        int negativeScore,
        int negativeLevel,
        int interactionScore,
        int totalScore,
        int resultLevel,
        string attentionJson,
        string negativeJson)
            {
                int currentVersion = SQLDAO.Instance.GetKeywordVersion();

                SQLDAO.Instance.ExecuteNonQuery(@"
            MERGE TablePostEvaluation AS t
            USING (SELECT @PostID AS PostID) s
            ON t.PostID = s.PostID

            WHEN MATCHED THEN
                UPDATE SET
                    AttentionScore = @AttentionScore,
                    AttentionLevel = @AttentionLevel,
                    NegativeScore = @NegativeScore,
                    NegativeLevel = @NegativeLevel,
                    InteractionScore = @InteractionScore,
                    TotalScore = @TotalScore,
                    ResultLevel = @ResultLevel,
                    AttentionKeywordIds = @AttentionJson,
                    NegativeKeywordIds = @NegativeJson,
                    KeywordVersion = @KeywordVersion,
                    EvaluatedAt = SYSDATETIME()

            WHEN NOT MATCHED THEN
                INSERT (
                    PostID,
                    AttentionScore,
                    AttentionLevel,
                    NegativeScore,
                    NegativeLevel,
                    InteractionScore,
                    TotalScore,
                    ResultLevel,
                    AttentionKeywordIds,
                    NegativeKeywordIds,
                    KeywordVersion
                )
                VALUES (
                    @PostID,
                    @AttentionScore,
                    @AttentionLevel,
                    @NegativeScore,
                    @NegativeLevel,
                    @InteractionScore,
                    @TotalScore,
                    @ResultLevel,
                    @AttentionJson,
                    @NegativeJson,
                    @KeywordVersion
                );
        ", new Dictionary<string, object>
        {
            { "@PostID", postId },
            { "@AttentionScore", attentionScore },
            { "@AttentionLevel", attentionLevel },
            { "@NegativeScore", negativeScore },
            { "@NegativeLevel", negativeLevel },
            { "@InteractionScore", interactionScore },
            { "@TotalScore", totalScore },
            { "@ResultLevel", resultLevel },
            { "@AttentionJson", (object)attentionJson ?? DBNull.Value },
            { "@NegativeJson", (object)negativeJson ?? DBNull.Value },
            { "@KeywordVersion", currentVersion }
        });
            }
        //2. GET
        public DataTable GetNegativeMonitorPosts(DateTime fromDate, int maxPost)
        {
            string sql = @"
    SELECT TOP (@MaxPost)
           p.PostID,
           p.PostContent,
           p.RealPostTime,
           e.AttentionScore,
           e.AttentionLevel,
           e.NegativeScore,
           e.NegativeLevel,
           e.ResultLevel,
           e.AttentionKeywordIds,
           e.NegativeKeywordIds
    FROM TablePostEvaluation e
    INNER JOIN TablePostInfo p
        ON p.PostID = e.PostID
    WHERE 
        p.RealPostTime >= @FromDate
        AND e.AttentionScore > 0
        AND e.NegativeScore > 0
    ORDER BY 
        e.NegativeScore DESC,
        e.NegativeLevel DESC,
        e.AttentionScore DESC
";

            return SQLDAO.Instance.ExecuteQuery(sql, new Dictionary<string, object>
    {
        { "@FromDate", fromDate },
        { "@MaxPost", maxPost }
    });
        }
    
        //TablePostEvaluation  
        public void DeletePostEvaluation(string postId)
            {
                    SQLDAO.Instance.ExecuteNonQuery(@"
                DELETE FROM TablePostEvaluation
                WHERE PostID = @pid;
                ", new Dictionary<string, object>
                {
                    { "@pid", postId }
                });
            }
        public DataTable GetEvaluatedPosts(DateTime fromDate, int maxPost)
            {
                string sql = @"
            SELECT TOP (@MaxPost)
                   p.PostID,
                   p.PostContent,
                   p.RealPostTime,
                   e.AttentionScore,
                   e.NegativeScore,
                   e.AttentionLevel,
                   e.NegativeLevel,
                   e.AttentionKeywordIds,
                   e.NegativeKeywordIds
                   e.InteractionScore,
                   e.TotalScore,
                   e.ResultLevel
            FROM TablePostEvaluation e
            INNER JOIN TablePostInfo p
                ON p.PostID = e.PostID
            WHERE p.RealPostTime >= @FromDate
            ORDER BY e.TotalScore DESC, e.ResultLevel DESC
        ";

                return SQLDAO.Instance.ExecuteQuery(sql, new Dictionary<string, object>
        {
            { "@FromDate", fromDate },
            { "@MaxPost", maxPost }
        });
            }
        public DataTable GetPostsForEvaluation(DateTime fromDate, int maxPost)
            {
                int currentVersion = SQLDAO.Instance.GetKeywordVersion();

                string sql = @"
            SELECT p.PostID,
                   p.PostContent,
                   p.RealPostTime,
                   p.LikeCount,
                   p.ShareCount,
                   p.CommentCount,
                   e.KeywordVersion
            FROM TablePostInfo p
            LEFT JOIN TablePostEvaluation e
                ON p.PostID = e.PostID
            WHERE p.RealPostTime >= @FromDate
              AND (
                    e.PostID IS NULL
                    OR e.KeywordVersion < @CurrentVersion
                  )
            ORDER BY p.RealPostTime DESC
            OFFSET 0 ROWS FETCH NEXT @MaxPost ROWS ONLY
        ";

                return SQLDAO.Instance.ExecuteQuery(sql, new Dictionary<string, object>
        {
            { "@FromDate", fromDate },
            { "@CurrentVersion", currentVersion },
            { "@MaxPost", maxPost }
        });
            }


        public void ClearPostEvaluation()
        {
            SQLDAO.Instance.ExecuteNonQuery(
                "DELETE FROM TablePostEvaluation",
                null);
        }
        public List<KeywordDTO> GetKeywordsByIds(List<int> ids)
        {
            var result = new List<KeywordDTO>();

            if (ids == null || ids.Count == 0)
                return result;

            var parameters = new Dictionary<string, object>();
            var paramNames = new List<string>();

            for (int i = 0; i < ids.Count; i++)
            {
                string paramName = "@id" + i;
                paramNames.Add(paramName);
                parameters[paramName] = ids[i];
            }

            string sql = $@"
        SELECT KeywordId, KeywordName
        FROM TableKeyword
        WHERE KeywordId IN ({string.Join(",", paramNames)})
    ";

            var dt = SQLDAO.Instance.ExecuteQuery(sql, parameters);

            foreach (DataRow row in dt.Rows)
            {
                result.Add(new KeywordDTO
                {
                    KeywordId = Convert.ToInt32(row["KeywordId"]),
                    KeywordName = row["KeywordName"].ToString()
                });
            }

            return result;
        }
        public PostPage GetFullPostById(string postId)
        {
            if (string.IsNullOrWhiteSpace(postId))
                return null;

            var dt = SQLDAO.Instance.ExecuteQuery(@"
            SELECT *
            FROM TablePostInfo
            WHERE PostID = @PostID",
            new Dictionary<string, object>
            {
        { "@PostID", postId }
            });

            if (dt.Rows.Count == 0)
                return null;

            var row = dt.Rows[0];

            return new PostPage
            {
                PostID = row["PostID"]?.ToString(),
                PostLink = row["PostLink"]?.ToString(),
                Content = row["PostContent"]?.ToString(),
                PostTime = row["PostTime"]?.ToString(),
                RealPostTime = row["RealPostTime"] as DateTime?,
                LikeCount = row["LikeCount"] as int?,
                ShareCount = row["ShareCount"] as int?,
                CommentCount = row["CommentCount"] as int?,
                Attachment = row["PostAttachment"]?.ToString(),
                PostType = row["PostStatus"]?.ToString()
            };
        }
    }
}
