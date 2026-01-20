using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

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

        // ===================== DICTIONARY =====================
        public readonly Dictionary<string, (string[] contains, string[] regex)> TopicRules =
     new Dictionary<string, (string[], string[])>
     {
         ["Tin tức – Sự kiện – Thời sự"] = (
         new[]{ "tin mới","tin nóng","thời sự","cập nhật","sự kiện","thông báo","báo cáo","hiện trường",
               "phát hiện","tình hình","vụ việc","điều tra","cháy","cháy nổ","tai nạn","khẩn cấp",
               "bão","lũ","sạt lở","ùn tắc","dự báo","họp báo","diễn biến","tổng hợp","ghi nhận",
               "triển khai","phong tỏa","thiên tai","cấp cứu","cứu hộ","cứu nạn","đột xuất",
               "truy vết","xác minh","sơ tán","đang xử lý","điều tra","xảy ra","chỉ đạo","đề xuất",
               "công bố","động đất","bình luận thời sự","biến động"
         },
         new[] { @"\b(tin|thời sự|cập nhật|breaking|thông báo|báo cáo|hiện trường|tai nạn|cháy|bão|lũ)\b" }
     ),

         ["Chính trị – Xã hội – Đảng – Nhà nước"] = (
         new[]{ "chính phủ","quốc hội","đảng","nhà nước","nghị định","thông tư","quy hoạch","ban hành",
               "xử lý","kiểm tra","bộ trưởng","ubnd","sở","tỉnh","huyện","pháp luật","văn bản",
               "dân số","hành chính","bổ nhiệm","miễn nhiệm","tuyên bố","chỉ thị","chính sách",
               "cơ quan","tổ công tác","thanh tra","giám sát","kiểm toán","an ninh","trật tự",
               "công an","công vụ","hội đồng","hội nghị","hội thảo","tái cơ cấu","cải cách",
               "tài khóa","ngân sách","đầu tư công","quy hoạch đô thị","định hướng","phát triển"
         },
         new[] { @"\b(chính phủ|quốc hội|đảng|nghị định|thông tư|ubnd|bộ|sở|chính sách|pháp luật)\b" }
     ),

         ["Cảnh báo – Nhạy cảm – Nổi cộm – Bất thường"] = (
         new[]{ "cảnh báo","nghiêm trọng","bất thường","nổi cộm","bắt giữ","bạo lực","xô xát","đâm chém",
               "đâm xe","tai nạn nghiêm trọng","tạm giam","truy nã","truy bắt","lừa đảo","scam",
               "giả mạo","thuốc giả","thực phẩm bẩn","ngộ độc","dịch bệnh","bùng phát","bạo loạn",
               "phong tỏa","đình chỉ","đóng cửa","nổ lớn","xác chết","mất tích","báo động đỏ",
               "tham nhũng","tiêu cực","đột kích","khám xét","bắt quả tang","cướp ngân hàng","cướp giật",
               "vỡ nợ","sụp đổ","sập cầu","sập nhà","ung dung","sự cố", "giết người", "bạo hành"
         },
         new[] { @"\b(cảnh báo|lừa đảo|scam|bắt giữ|nghiêm trọng|bất thường|tai nạn|đình chỉ|bạo lực|mất tích)\b" }
     ),

         ["Giáo dục – Kiến thức"] = (
         new[]{ "giáo dục","kiến thức","kỹ năng","tự học","bài học","bài giảng","học tập","giảng viên",
               "nghiên cứu","bài kiểm tra","bài tập","luyện thi","mẹo học","học sinh","sinh viên",
               "trường học","đại học","ebook","giáo trình","khoá học","tài liệu","seminar","workshop",
               "học phí","bài viết chuyên môn","kỹ thuật","phân tích","đề cương","học nhanh","hiểu sâu",
               "tư duy","khoa học","thử nghiệm","sáng tạo","logic"
         },
         new[] { @"\b(giáo dục|kiến thức|kỹ năng|bài học|tự học|bài giảng|học tập)\b" }
     ),

         ["Bán hàng – Tư vấn – Mua bán – Tuyển dụng"] = (
         new[]{ "giảm giá","sale","flash sale","deal","ưu đãi","khuyến mãi","freeship","mua ngay",
               "đặt hàng","tư vấn","inbox","liên hệ","combo","voucher","thanh lý","tồn kho",
               "bán sỉ","bán lẻ","order","lên đơn","chốt đơn","đại lý","nhà phân phối",
               "tuyển dụng","tuyển gấp","ứng tuyển","phỏng vấn","mức lương","đãi ngộ",
               "đi làm ngay","job","cv","hr","cộng tác viên","part-time","full-time"
         },
         new[] { @"\b(giảm giá|sale|ưu đãi|mua ngay|đặt hàng|tư vấn|tuyển dụng|ứng tuyển|lương)\b" }
     ),

         ["Giải trí – Thể thao"] = (
         new[]{ "showbiz","drama","scandal","ca sĩ","diễn viên","idol","fan","phim","trailer",
               "tập mới","teaser","mv","album","livestream","reaction","viral","hot trend",
               "bóng đá","thể thao","đội tuyển","trận đấu","giải đấu","cầu thủ","huấn luyện viên",
               "bàn thắng","tỉ số","world cup","highlight","var","penalty","sân vận động"
         },
         new[] { @"\b(showbiz|drama|phim|viral|bóng đá|thể thao|đội tuyển|bàn thắng)\b" }
     )
     };

        public class ClusterGroup
        {
            public int ClusterId { get; set; }
            public string TopicName { get; set; }
            public string Summary { get; set; }
            public int Count { get; set; }
            public List<ClusterPost> Posts { get; set; }
        }

        public class ClusterPost
        {
            public string PostID { get; set; }
            public string Content { get; set; }
            public DateTime Time { get; set; }
        }


        // ===================== Classify =====================
        public string Classify(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return "Khác";

            string txt = content.ToLower();

            foreach (var t in TopicRules)
            {
                // bước 1: Contains để lọc rộng
                foreach (var kw in t.Value.contains)
                {
                    if (txt.Contains(kw))
                    {
                        // bước 2: Regex để xác nhận
                        foreach (var pattern in t.Value.regex)
                        {
                            if (Regex.IsMatch(txt, pattern))
                                return t.Key;
                        }
                    }
                }
            }

            return "Khác";
        }
        public List<string> ClassifyMulti(string content)
        {
            List<string> topics = new List<string>();

            if (string.IsNullOrWhiteSpace(content))
                return topics;

            string clean = NormalizeContent(content);

            foreach (var entry in TopicRules)
            {
                foreach (string kw in entry.Value.contains)
                {
                    string cleanKw = NormalizeContent(kw);

                    if (clean.Contains(cleanKw))
                    {
                        topics.Add(entry.Key);
                        break;
                    }
                }
            }

            return topics;
        }

        // ===================== INSERT Topic =====================
        private int InsertTopic(string topicName)
        {
            object exist = SQLDAO.Instance.ExecuteScalar(
                "SELECT TopicId FROM TableTopic WHERE TopicName=@n",
                new Dictionary<string, object> { { "@n", topicName } });

            if (exist != null)
                return Convert.ToInt32(exist);

            return Convert.ToInt32(
                SQLDAO.Instance.ExecuteScalar(
                @"INSERT INTO TableTopic(TopicName) VALUES(@name);
                  SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new Dictionary<string, object> { { "@name", topicName } })
            );
        }

        // ===================== INSERT Keyword =====================
        private int InsertKeyword(string kw)
        {
            object exist = SQLDAO.Instance.ExecuteScalar(
                "SELECT KeywordId FROM TableKeyword WHERE KeywordName=@kw",
                new Dictionary<string, object> { { "@kw", kw } });

            if (exist != null)
                return Convert.ToInt32(exist);

            return Convert.ToInt32(
                SQLDAO.Instance.ExecuteScalar(
                @"INSERT INTO TableKeyword(KeywordName) VALUES(@kw);
                  SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new Dictionary<string, object> { { "@kw", kw } })
            );
        }

        // ===================== INSERT TopicKey =====================
        private void InsertTopicKey(int topicId, int keywordId)
        {
            SQLDAO.Instance.ExecuteNonQuery(@"
        IF NOT EXISTS (
            SELECT * FROM TableTopicKey
            WHERE TopicId=@t AND KeywordId=@k
        )
        INSERT INTO TableTopicKey(TopicId, KeywordId)
        VALUES(@t, @k)",
                new Dictionary<string, object>
                {
            { "@t", topicId },
            { "@k", keywordId }
                });
        }

        


        // ===================== Insert Dictionary vào DB =====================
        public void InsertTopicRulesToDB()
        {
            foreach (var entry in TopicRules)
            {
                string topicName = entry.Key;

                // lấy danh sách keyword Contains
                string[] keywords = entry.Value.contains;

                // 1) Insert topic
                int topicId = InsertTopic(topicName);

                // 2) Insert Keyword
                foreach (string kw in keywords)
                {
                    int keywordId = InsertKeyword(kw);
                    InsertTopicKey(topicId, keywordId);
                }
            }
        }


        // ===================== Convert tất cả bài viết =====================
        public void ConvertAllPosts()
        {
            var dt = SQLDAO.Instance.ExecuteQuery("SELECT PostID, PostContent FROM TablePostInfo");

            foreach (DataRow r in dt.Rows)
            {
                string postId = r["PostID"].ToString();
                string content = r["PostContent"].ToString();

                var topics = ClassifyMulti(content);

                // Nếu không khớp topic nào → gán vào Khác
                if (topics.Count == 0)
                {
                    int otherId = InsertTopic("Khac");
                    InsertTopicPost(otherId, postId);
                    continue;
                }

                // Multi-topic
                foreach (string topicName in topics)
                {
                    int topicId = InsertTopic(topicName);
                    InsertTopicPost(topicId, postId);
                }
            }
        }
        private void InsertTopicPost(int topicId, string postId)
        {
            SQLDAO.Instance.ExecuteNonQuery(@"
        IF NOT EXISTS (
            SELECT * FROM TableTopicPost
            WHERE TopicId = @t AND PostId = @p
        )
        INSERT INTO TableTopicPost(TopicId, PostId)
        VALUES(@t, @p)",
                new Dictionary<string, object>
                {
            { "@t", topicId },
            { "@p", postId }
                });
        }


        // ===================== GET PAGE (for GridControl) =====================
        public DataTable GetPage(int page, int pageSize)
        {
            int offset = (page - 1) * pageSize;

            string sql = @"
                SELECT tp.Id,
                       tp.PostId,
                       t.TopicName,
                       LEFT(p.PostContent, 200) AS Content
                FROM TableTopicPost tp
                JOIN TableTopic t ON tp.TopicId = t.TopicId
                JOIN TablePostInfo p ON tp.PostId = p.PostID
                ORDER BY tp.Id
                OFFSET @off ROWS FETCH NEXT @size ROWS ONLY";

            return SQLDAO.Instance.ExecuteQuery(sql,
                new Dictionary<string, object>
                {
                    { "@off", offset },
                    { "@size", pageSize }
                });
        }

        public int CountAll()
        {
            return Convert.ToInt32(
                SQLDAO.Instance.ExecuteScalar("SELECT COUNT(*) FROM TableTopicPost"));
        }
        public void ClearTopicData()
        {
            SQLDAO.Instance.ExecuteNonQuery(@"
        DELETE FROM TableTopicPost;
        DELETE FROM TableTopicKey;
        DELETE FROM TableKeyword;
        DELETE FROM TableTopic;
    ");
        }
        public DataTable GetPostsByTopic(string topicName)
        {
            string sql = @"
        SELECT 
            tp.PostId,
            pi.PostLink,
            pi.PostContent,
            pg.PageName AS PageNameContainer,
            pi.PostTime,
            pi.LikeCount,
            pi.ShareCount,
            pi.CommentCount
        FROM TableTopicPost tp
        INNER JOIN TableTopic t ON tp.TopicId = t.TopicId
        INNER JOIN TablePostInfo pi ON tp.PostId = pi.PostID
        LEFT JOIN TablePost p ON p.PostID = pi.PostID
        LEFT JOIN TablePageInfo pg ON pg.PageID = p.PageIDContainer
        WHERE t.TopicName = @topic
        ORDER BY tp.Id ASC";

            return SQLDAO.Instance.ExecuteQuery(sql,
                new Dictionary<string, object> { { "@topic", topicName } });
        }
        public string NormalizeContent(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            string text = input.ToLower();

            // bỏ dấu tiếng Việt
            text = RemoveVietnameseDiacritics(text);

            // bỏ emoji + ký tự đặc biệt
            text = Regex.Replace(text, @"[^a-z0-9\s]", " ");

            // bỏ nhiều dấu cách
            text = Regex.Replace(text, @"\s+", " ").Trim();

            return text;
        }

        public string RemoveVietnameseDiacritics(string str)
        {
            string[] signs = new string[]
            {
        "aAeEoOuUiIdDyY",
        "áàảãạăắằẳẵặâấầẩẫậ",
        "ÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬ",
        "éèẻẽẹêếềểễệ",
        "ÉÈẺẼẸÊẾỀỂỄỆ",
        "óòỏõọôốồổỗộơớờởỡợ",
        "ÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢ",
        "úùủũụưứừửữự",
        "ÚÙỦŨỤƯỨỪỬỮỰ",
        "íìỉĩị",
        "ÍÌỈĨỊ",
        "đ",
        "Đ",
        "ýỳỷỹỵ",
        "ÝỲỶỸỴ"
            };

            for (int i = 1; i < signs.Length; i++)
            {
                for (int j = 0; j < signs[i].Length; j++)
                {
                    str = str.Replace(signs[i][j], signs[0][i - 1]);
                }
            }

            return str;
        }
        //======================
        public DataTable GetPostsForClustering(int topicId, int days = 2)
        {
            string sql = @"
        SELECT 
            pi.PostID,
            pi.PostContent,
            pi.RealPostTime
        FROM TableTopicPost tp
        JOIN TablePostInfo pi ON tp.PostId = pi.PostID
        WHERE tp.TopicId = @topic
          AND pi.RealPostTime >= DATEADD(day, -@d, GETDATE())
        ORDER BY pi.RealPostTime DESC";

            return SQLDAO.Instance.ExecuteQuery(sql,
                new Dictionary<string, object>
                {
            {"@topic", topicId},
            {"@d", days}
                });
        }
        public string[] Tokenize(string text)
        {
            string clean = NormalizeContent(text);
            return clean.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public double JaccardSimilarity(string[] t1, string[] t2)
        {
            var set1 = new HashSet<string>(t1);
            var set2 = new HashSet<string>(t2);

            int intersection = set1.Intersect(set2).Count();
            int union = set1.Union(set2).Count();

            return (double)intersection / union;
        }
        public List<List<DataRow>> ClusterPosts(DataTable dt)
        {
            double threshold = 0.4; // mức giống nhau được coi là cùng vụ

            var clusters = new List<List<DataRow>>();
            var used = new HashSet<string>();

            foreach (DataRow rowA in dt.Rows)
            {
                if (used.Contains(rowA["PostID"].ToString()))
                    continue;

                var cluster = new List<DataRow>();
                cluster.Add(rowA);

                var tokA = Tokenize(rowA["PostContent"].ToString());

                foreach (DataRow rowB in dt.Rows)
                {
                    if (rowA == rowB) continue;
                    if (used.Contains(rowB["PostID"].ToString())) continue;

                    var tokB = Tokenize(rowB["PostContent"].ToString());

                    double sim = JaccardSimilarity(tokA, tokB);
                    if (sim >= threshold)
                    {
                        cluster.Add(rowB);
                        used.Add(rowB["PostID"].ToString());
                    }
                }

                clusters.Add(cluster);
            }

            return clusters;
        }
        //================
        public List<ClusterGroup> BuildClusterDisplay(List<List<DataRow>> clusters, string topicName)
        {
            List<ClusterGroup> list = new List<ClusterGroup>();
            int id = 1;

            foreach (var group in clusters)
            {
                var first = group.First();
                string summary = MakeSummary(first["PostContent"].ToString());

                list.Add(new ClusterGroup
                {
                    ClusterId = id++,
                    TopicName = topicName,
                    Summary = summary,
                    Count = group.Count,
                    Posts = group.Select(r => new ClusterPost
                    {
                        PostID = r["PostID"].ToString(),
                        Content = r["PostContent"].ToString(),
                        Time = Convert.ToDateTime(r["RealPostTime"])
                    }).ToList()
                });
            }

            return list;
        }
        // cần: using System.Linq;
        private string MakeSummary(string content, int wordCount = 6)
        {
            if (string.IsNullOrWhiteSpace(content))
                return "";

            // Nếu bạn có NormalizeContent, dùng nó để loại dấu & ký tự
            string clean = NormalizeContent(content); // nếu không có, dùng content.ToLower()

            var parts = clean.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                             .Take(wordCount);

            string summary = string.Join(" ", parts);
            if (summary.Length == 0) return "";

            return summary + "...";
        }

    }
}
