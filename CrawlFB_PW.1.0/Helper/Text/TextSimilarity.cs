using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.Helper.Text;

namespace CrawlFB_PW._1._0.Helper
{
    public static class TextSimilarity
    {
        public static double Similarity(string text1, string text2)
        {
            // ❌ nếu lỗi crawl thì KHÔNG so
            if (text1 == "N/A" || text2 == "N/A")
                return -1; // marker: không dùng được

            // ✅ caption rỗng là hợp lệ
            text1 = text1 ?? "";
            text2 = text2 ?? "";

            if (text1.Length == 0 && text2.Length == 0)
                return 1; // cả 2 đều không caption → giống nhau

            if (text1.Length == 0 || text2.Length == 0)
                return 0;

            var vec1 = ToWordVector(TextNormalizeHelper.Normalize(text1));
            var vec2 = ToWordVector(TextNormalizeHelper.Normalize(text2));

            return CosineSimilarity(vec1, vec2);
        }

        // ============================
        // Convert to word-frequency vector
        // ============================
        private static Dictionary<string, int> ToWordVector(string text)
        {
            var dict = new Dictionary<string, int>();

            foreach (var word in text.Split(' '))
            {
                if (word.Length < 2) continue; // bỏ từ quá ngắn

                if (!dict.ContainsKey(word))
                    dict[word] = 0;

                dict[word]++;
            }

            return dict;
        }

        // ============================
        // Cosine Similarity
        // ============================
        private static double CosineSimilarity(
            Dictionary<string, int> v1,
            Dictionary<string, int> v2)
        {
            double dot = 0;
            double mag1 = 0;
            double mag2 = 0;

            foreach (var kv in v1)
            {
                mag1 += kv.Value * kv.Value;

                if (v2.TryGetValue(kv.Key, out int v))
                    dot += kv.Value * v;
            }

            foreach (var v in v2.Values)
                mag2 += v * v;

            if (mag1 == 0 || mag2 == 0)
                return 0;

            return dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2));
        }

        public static int MatchScoreVietnameseAdvanced(string source, string keyword)
        {
            if (string.IsNullOrWhiteSpace(source) ||
                string.IsNullOrWhiteSpace(keyword))
                return 0;

            string s = TextNormalizeHelper.Normalize(source);
            string k = TextNormalizeHelper.Normalize(keyword);

            var words = s.Split(
             new[] { ' ' },
             StringSplitOptions.RemoveEmptyEntries
         );


            // trùng tuyệt đối 1 âm tiết
            if (words.Any(w => w == k))
                return 100;

            // ghép chuẩn: họa sĩ, hỏa hoạn
            if (words.Any(w => w.StartsWith(k) && w.Length > k.Length))
            {
                foreach (var w in words)
                {
                    if (!w.StartsWith(k) || w.Length == k.Length)
                        continue;

                    char nextChar = w[k.Length];
                    if ("aeiouy".Contains(nextChar))
                        return 80;
                }
            }

            return 0;
        }

        public static bool MatchVietnameseSyllable(string source, string keyword)
        {
            if (string.IsNullOrWhiteSpace(source) ||
                string.IsNullOrWhiteSpace(keyword))
                return false;

            string s = TextNormalizeHelper.Normalize(source);
            string k = TextNormalizeHelper.Normalize(keyword);

            // tách từ theo khoảng trắng
                    var words = s.Split(
              new[] { ' ' },
              StringSplitOptions.RemoveEmptyEntries
          );


            foreach (var w in words)
            {
                // 1️⃣ trùng chính xác: hoa, hóa, họa...
                if (w == k)
                    return true;

                // 2️⃣ từ ghép bắt đầu bằng keyword
                // nhưng PHẢI dài hơn và keyword là 1 âm tiết hợp lệ
                if (w.StartsWith(k) && w.Length > k.Length)
                {
                    // chặn các trường hợp sai: hoanh, hoang, hoat...
                    // rule: ký tự sau "hoa" KHÔNG được là phụ âm nối vô nghĩa
                    // mà phải là dấu kết thúc âm tiết (thường là nguyên âm có dấu)
                    // → tiếng Việt: a, o, e, i, u, y
                    char nextChar = w[k.Length];
                    if ("aeiouy".Contains(nextChar))
                        return true;
                }
            }

            return false;
        }

    }

}
