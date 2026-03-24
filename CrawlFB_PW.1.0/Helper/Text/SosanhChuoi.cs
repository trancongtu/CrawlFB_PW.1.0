using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CrawlFB_PW._1._0.Helper.Text
{
    public static class SosanhChuoi
    {
        /* =========================
         *  LEVEL 1 – STRICT (ký tự) so sánh được chữ và từ
         * ========================= */
        public static bool EqualsVietnameseStrict(string a, string b)
        {
            if (a == null || b == null) return false;

            a = a.Normalize(NormalizationForm.FormC);
            b = b.Normalize(NormalizationForm.FormC);

            return string.Equals(a, b, StringComparison.Ordinal);
        }
        // so sánh từng đoan trong keyword với đích
        public static bool ContainsExactVietnamesePhraseStrict( string content, string keyword)
        {
            if (string.IsNullOrWhiteSpace(content) ||
                string.IsNullOrWhiteSpace(keyword))
                return false;

            // 1️⃣ Chuẩn hoá Unicode (tránh lỗi tổ hợp)
            content = content.Normalize(NormalizationForm.FormC);
            keyword = keyword.Normalize(NormalizationForm.FormC);

            // 2️⃣ Tách từ (giữ nguyên dấu)
            var contentTokens = content.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var keywordTokens = keyword.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (keywordTokens.Length > contentTokens.Length)
                return false;

            // 3️⃣ Sliding window – so sánh tuyệt đối
            for (int i = 0; i <= contentTokens.Length - keywordTokens.Length; i++)
            {
                bool match = true;

                for (int j = 0; j < keywordTokens.Length; j++)
                {
                    if (!EqualsVietnameseStrict(
                            contentTokens[i + j],
                            keywordTokens[j]))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    return true;
            }

            return false;
        }

        /* =========================
         *  LEVEL 2 – NORMALIZE (bỏ dấu)
         * ========================= */
        public static string NormalizeVietnamese(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";

            string s = input.ToLowerInvariant()
                .Normalize(NormalizationForm.FormD);

            var sb = new StringBuilder();
            foreach (char c in s)
            {
                var uc = Char.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString()
                .Normalize(NormalizationForm.FormC);
        }
        //==== so sánh keyword trong topic để add
        public static bool SosanhkeywordAddTopic(string source,string keyword)
        {
            if (string.IsNullOrWhiteSpace(source) ||
                string.IsNullOrWhiteSpace(keyword))
                return false;

            // 1️⃣ Chuẩn hoá: hạ hoa + chuẩn Unicode (KHÔNG bỏ dấu)
            source = TextNormalizeHelper.ToLowerVietnamese(source);
            keyword = TextNormalizeHelper.ToLowerVietnamese(keyword);

            // 2️⃣ Tách từ
            var sourceTokens = source
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var keywordTokens = keyword
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (keywordTokens.Length > sourceTokens.Length)
                return false;

            // 3️⃣ Sliding window – so sánh tuyệt đối từng token
            for (int i = 0; i <= sourceTokens.Length - keywordTokens.Length; i++)
            {
                bool match = true;

                for (int j = 0; j < keywordTokens.Length; j++)
                {
                    // so sánh tuyệt đối ký tự
                    if (!EqualsVietnameseStrict(
                            sourceTokens[i + j],
                            keywordTokens[j]))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    return true;
            }

            return false;
        }
        // tìm từ khóa trong bài viết và trả về list vị trí
        public static List<(int start, int length)>FindKeywordPositions(string source, string keyword)
        {
            var results = new List<(int start, int length)>();

            if (string.IsNullOrWhiteSpace(source) ||
                string.IsNullOrWhiteSpace(keyword))
                return results;

            // Chuẩn hoá giống lúc phân tích
            string normalizedSource = TextNormalizeHelper.ToLowerVietnamese(source);
            string normalizedKeyword = TextNormalizeHelper.ToLowerVietnamese(keyword);

            var sourceTokens = normalizedSource
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var keywordTokens = normalizedKeyword
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (keywordTokens.Length > sourceTokens.Length)
                return results;

            // Ta cũng cần token gốc để map vị trí thật
            var originalTokens = source
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            int charPointer = 0;

            for (int i = 0; i < sourceTokens.Length; i++)
            {
                for (int j = 0; j < keywordTokens.Length; j++)
                {
                    if (i + j >= sourceTokens.Length ||
                        !EqualsVietnameseStrict(
                            sourceTokens[i + j],
                            keywordTokens[j]))
                    {
                        goto NextLoop;
                    }
                }

                // Nếu match
                string phrase = string.Join(" ",
                    originalTokens.Skip(i).Take(keywordTokens.Length));

                int startIndex = source.IndexOf(
                    phrase,
                    charPointer,
                    StringComparison.OrdinalIgnoreCase);

                if (startIndex >= 0)
                {
                    results.Add((startIndex, phrase.Length));
                    charPointer = startIndex + phrase.Length;
                }

            NextLoop:;
            }

            return results;
        }
    }
}
