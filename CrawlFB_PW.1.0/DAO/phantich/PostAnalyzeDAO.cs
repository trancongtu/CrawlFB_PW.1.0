using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.Helper.Text;
using CrawlFB_PW._1._0.ViewModels;
using CrawlFB_PW._1._0.ViewModels.Keyword;

namespace CrawlFB_PW._1._0.DAO.phantich
{
    public class PostAnalyzeDAO
    {
        // 🔒 Singleton
        private static readonly Lazy<PostAnalyzeDAO> _instance =
            new Lazy<PostAnalyzeDAO>(() => new PostAnalyzeDAO());

        public static PostAnalyzeDAO Instance => _instance.Value;

        // ❌ không cho new từ ngoài
        private PostAnalyzeDAO() { }

        // ===============================
        // PHÂN TÍCH TIÊU CỰC BÀI VIẾT
        // ===============================
        public int AnalyzeAndSavePosts(
     DataTable posts,
     List<KeywordDTO> keywords,
     Dictionary<int, (int Score, int Level)> attentionDict,
     Dictionary<int, (int Score, int Level)> negativeDict,
     HashSet<int> excludeSet)
        {
            int count = 0;

            foreach (DataRow row in posts.Rows)
            {
                string postId = row["PostID"]?.ToString();
                if (string.IsNullOrWhiteSpace(postId))
                    continue;

                string content = row["PostContent"]?.ToString() ?? "";

                int attentionScore = 0;
                int negativeScore = 0;
                int interactionScore = 0;

                int attentionLevel = 0;
                int negativeLevel = 0;

                var attentionMatched = new List<(int id, int start, int length)>();
                var negativeMatched = new List<(int id, int start, int length)>();

                bool isExcluded = false;

                foreach (var kw in keywords)
                {
                    var positions = SosanhChuoi.FindKeywordPositions(content, kw.KeywordName);

                    if (positions.Count == 0)
                        continue;

                    if (excludeSet.Contains(kw.KeywordId))
                    {
                        isExcluded = true;
                        break;
                    }

                    // ===== ATTENTION =====
                    if (attentionDict.TryGetValue(kw.KeywordId, out var att) && att.Score > 0)
                    {
                        attentionScore += att.Score;
                        attentionLevel = Math.Max(attentionLevel, att.Level);

                        foreach (var p in positions)
                        {
                            attentionMatched.Add((kw.KeywordId, p.start, p.length));
                        }
                    }

                    // ===== NEGATIVE =====
                    if (negativeDict.TryGetValue(kw.KeywordId, out var neg) && neg.Score > 0)
                    {
                        negativeScore += neg.Score;
                        negativeLevel = Math.Max(negativeLevel, neg.Level);

                        foreach (var p in positions)
                        {
                            negativeMatched.Add((kw.KeywordId, p.start, p.length));
                        }
                    }
                }

                if (isExcluded)
                    continue;

                attentionScore = Math.Min(attentionScore, 30);
                negativeScore = Math.Min(negativeScore, 50);

                int totalScore = attentionScore + negativeScore + interactionScore;
                int resultLevel = Math.Max(attentionLevel, negativeLevel);

                var attentionJson = attentionMatched.Count > 0? JsonHelper.Serialize(
                 attentionMatched.Select(x => new KeywordMatchDTO
                 {
                     Id = x.id,
                     Start = x.start,
                     Length = x.length
                 }).ToList()): null;

                var negativeJson = negativeMatched.Count > 0
                    ? JsonHelper.Serialize(
                        negativeMatched.Select(x => new KeywordMatchDTO
                        {
                            Id = x.id,
                            Start = x.start,
                            Length = x.length
                        }).ToList())
                    : null;

                AnalyzeSQLDAO.Instance.SavePostEvaluation(
                    postId,
                    attentionScore,
                    attentionLevel,
                    negativeScore,
                    negativeLevel,
                    interactionScore,
                    totalScore,
                    resultLevel,
                    attentionJson,
                    negativeJson
                );

                count++;
            }

            return count;
        }
        
    }

}
