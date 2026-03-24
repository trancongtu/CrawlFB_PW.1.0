using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.DAO.phantich;
using CrawlFB_PW._1._0.ViewModels;

namespace CrawlFB_PW._1._0.DAO
{
    public class KeywordDAO
    {
        private static KeywordDAO _instance;
        public static KeywordDAO Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new KeywordDAO();
                return _instance;
            }
        }
        private KeywordDAO() { }
        public List<KeywordViewModel> GetKeywordViewModels()
        {
            var keywords = SQLDAO.Instance.GetAllKeyword();
            var excludedIds = AnalyzeSQLDAO.Instance.GetExcludedKeywordIds();

            var result = new List<KeywordViewModel>();

            foreach (var k in keywords)
            {
                int countTopic = SQLDAO.Instance.CountTopicByKeywordId(k.KeywordId);

                int? att = SQLDAO.Instance.GetAttentionScoreByKeywordId(k.KeywordId);
                int? attLevel = SQLDAO.Instance.GetTrackingLevelByKeywordId(k.KeywordId);
                int? neg = SQLDAO.Instance.GetNegativeScoreByKeywordId(k.KeywordId);
                int? negLevel = SQLDAO.Instance.GetNegativeLevelByKeywordId(k.KeywordId);

                result.Add(new KeywordViewModel
                {
                    Select = false,

                    KeywordId = k.KeywordId,
                    KeywordName = k.KeywordName,
                    CountTopic = countTopic,

                    // ===== ATTENTION =====
                    AttentionScore = att ?? 0,
                    TrackingLevel = attLevel,

                    // ===== NEGATIVE =====
                    NegativeScore = neg ?? 0,
                    NegativeLevel = negLevel,
                    IsCritical = false,

                    // ===== EXCLUDE =====
                    IsExcluded = excludedIds.Contains(k.KeywordId)
                });

            }

            return result;
        }




    }
}
