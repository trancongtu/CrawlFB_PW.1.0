using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.Helper.Text
{
    public static class TextNormalizeHelper
    {
        //bỏ viết hoa và bỏ dấu
        public static string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            text = text.ToLowerInvariant();

            // bỏ dấu tiếng Việt
            string normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (char c in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString();
        }
        // chỉ bó viết hoa
        public static string ToLowerVietnamese(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            // Normalize Unicode để tránh lỗi tổ hợp
            return text.Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }


        public static bool ContainsIgnoreCaseAndAccent(
    string source,
    string keyword)
        {
            if (string.IsNullOrWhiteSpace(source) ||
                string.IsNullOrWhiteSpace(keyword))
                return false;

            return Normalize(source)
                .Contains(Normalize(keyword));
        }
        public static bool ContainsWordIgnoreCaseAndAccent(
    string source,
    string keyword)
        {
            if (string.IsNullOrWhiteSpace(source) ||
                string.IsNullOrWhiteSpace(keyword))
                return false;

            string src = Normalize(source);
            string key = Normalize(keyword);

            // split theo khoảng trắng (đã normalize)
            var words = src
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            return words.Any(w => w == key);
        }
        // so sánh từ dùng cho gán chủ đề
       


    }
}
