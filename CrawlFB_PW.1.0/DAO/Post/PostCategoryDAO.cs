using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CrawlFB_PW._1._0.Helper;
using CrawlFB_PW._1._0.Helper.Text;
namespace CrawlFB_PW._1._0.DAO
{

    public class PostCategoryDAO
    {
        private static PostCategoryDAO instance;
        public static PostCategoryDAO Instance
        {
            get
            {
                if (instance == null)
                    instance = new PostCategoryDAO();
                return instance;
            }
        }

        private PostCategoryDAO() { }

        private bool MatchKeyword(string content, string keyword)
        {
            return SosanhChuoi.SosanhkeywordAddTopic(content, keyword);
        }

        // =========================
        // 1️⃣ REBUILD ALL
        // =========================
        public void RebuildAllTopicPost()
        {
            SQLDAO.Instance.ClearAllTopicPost();

            var posts = SQLDAO.Instance.GetAllPosts();
            var map = SQLDAO.Instance.GetAllKeywordTopic();

            Convert(posts, map, checkExist: false);
        }

        // =========================
        // 2️⃣ CONVERT BÀI MỚI
        // =========================
        public void ConvertNewPosts()
        {
            var posts = SQLDAO.Instance.GetPostsWithoutTopic();
            var map = SQLDAO.Instance.GetAllKeywordTopic();

            Convert(posts, map, checkExist: false);
        }

        // =========================
        // 3️⃣ UPDATE KHI THÊM KEYWORD / TOPIC
        // =========================
        public void RebuildByNewKeywordOrTopic()
        {
            var posts = SQLDAO.Instance.GetAllPosts();
            var map = SQLDAO.Instance.GetAllKeywordTopic();

            Convert(posts, map, checkExist: true);
        }

        // =========================
        // CORE CONVERT
        // =========================
        private void Convert(
            DataTable posts,
            DataTable map,
            bool checkExist)
        {
            foreach (DataRow p in posts.Rows)
            {
                string postId = p["PostID"].ToString();
                string content = p["PostContent"].ToString();

                foreach (DataRow r in map.Rows)
                {
                    int topicId = (int)r["TopicId"];
                    string keyword = r["KeywordName"].ToString();

                    if (!MatchKeyword(content, keyword))
                        continue;

                    if (checkExist &&
                        SQLDAO.Instance.TopicPostExists(topicId, postId))
                        continue;

                    SQLDAO.Instance.InsertTopicPost(topicId, postId);
                }
            }
        }


    }
 
  
}
