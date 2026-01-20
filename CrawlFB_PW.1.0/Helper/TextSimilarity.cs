using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

            var vec1 = ToWordVector(Normalize(text1));
            var vec2 = ToWordVector(Normalize(text2));

            return CosineSimilarity(vec1, vec2);
        }

        // ============================
        // Normalize text
        // ============================
        private static string Normalize(string text)
        {
            text = text.ToLowerInvariant();

            // bỏ ký tự đặc biệt
            text = Regex.Replace(text, @"[^\p{L}\p{Nd}\s]", " ");

            // gom khoảng trắng
            text = Regex.Replace(text, @"\s+", " ").Trim();

            return text;
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
    }

}
