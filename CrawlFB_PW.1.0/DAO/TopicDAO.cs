using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.ViewModels;

namespace CrawlFB_PW._1._0.DAO
{
    public class TopicDAO
    {
        private static TopicDAO _instance;
        public static TopicDAO Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TopicDAO();
                return _instance;
            }
        }

        private TopicDAO() { }

        public List<TopicViewModel> GetTopicViewModels()
        {
            var topics = SQLDAO.Instance.GetAllTopic();

            return topics.Select(t => new TopicViewModel
            {
                Select = false,
                TopicId = t.TopicId,
                TopicName = t.TopicName,
                CountKeyword = SQLDAO.Instance.CountKeywordInTopic(t.TopicId)
            }).ToList();
        }
    }

}
