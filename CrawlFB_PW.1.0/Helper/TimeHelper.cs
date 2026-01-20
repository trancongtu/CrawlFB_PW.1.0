using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.Helper
{
    public class TimeHelper
    {
        // Xóa ký tự thừa chỉ để chuẩn mốc thời gian
        public static string CleanTimeString(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "N/A";

            // ==================================================
            // A. Chuẩn hoá ký tự vô hình Facebook
            // ==================================================
            raw = raw
                .Replace('\u00A0', ' ')
                .Replace('\u200B', ' ')
                .Replace('\u200E', ' ')
                .Replace('\u202F', ' ')
                .Replace('\u2060', ' ')
                .Replace('\u200C', ' ')
                .Trim();

            string cleaned = raw;

            // ==================================================
            // 1️⃣ Chuẩn hoá whitespace
            // ==================================================
            cleaned = Regex.Replace(cleaned, @"[\r\n\t]+", " ");
            cleaned = Regex.Replace(cleaned, @"\s{2,}", " ");

            // ==================================================
            // 1️⃣.5️⃣ FIX GỐC: bỏ "," và "lúc"
            // ==================================================
            cleaned = cleaned.Replace(",", "");
            cleaned = Regex.Replace(cleaned, @"\blúc\b", "", RegexOptions.IgnoreCase).Trim();

            // ==================================================
            // 2️⃣ Xử lý dấu phân cách (· • ∙)
            // 👉 ƯU TIÊN DATE ĐẦY ĐỦ, KHÔNG ƯU TIÊN "x tháng"
            // ==================================================
            if (cleaned.IndexOfAny(new[] { '·', '•', '∙' }) >= 0)
            {
                var parts = cleaned
                    .Split(new[] { '·', '•', '∙' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .ToList();

                string timePart =
                    // dd tháng MM yyyy [HH:mm]
                    parts.FirstOrDefault(p =>
                        Regex.IsMatch(p,
                            @"\d{1,2}\s*tháng\s*\d{1,2}\s*\d{4}(\s*\d{1,2}:\d{2})?",
                            RegexOptions.IgnoreCase))
                    ??
                    // dd tháng MM [HH:mm]
                    parts.FirstOrDefault(p =>
                        Regex.IsMatch(p,
                            @"\d{1,2}\s*tháng\s*\d{1,2}(\s*\d{1,2}:\d{2})?",
                            RegexOptions.IgnoreCase))
                    ??
                    // hôm nay / hôm qua [HH:mm]
                    parts.FirstOrDefault(p =>
                        Regex.IsMatch(p,
                            @"hôm\s*(nay|qua)(\s*\d{1,2}:\d{2})?",
                            RegexOptions.IgnoreCase))
                    ??
                    // x phút / x giờ / x ngày / x tuần / x tháng / x năm
                    parts.FirstOrDefault(p =>
                        Regex.IsMatch(p,
                            @"^\d+\s*(phút|giờ|ngày|tuần|tháng|năm)$",
                            RegexOptions.IgnoreCase));

                cleaned = !string.IsNullOrWhiteSpace(timePart)
                    ? timePart
                    : parts.First();
            }

            // ==================================================
            // 3️⃣ Xoá unicode rác còn sót
            // ==================================================
            cleaned = Regex.Replace(cleaned, @"[\u200B\u200C\u200E\u202F\u2060]+", "");

            // ==================================================
            // 4️⃣ Xoá dấu "." thừa sau time
            // ==================================================
            cleaned = Regex.Replace(
                cleaned,
                @"(\d+\s*(giờ|phút|ngày|tuần|tháng|năm|hôm\s*qua))\s*\.",
                "$1",
                RegexOptions.IgnoreCase
            );

            // ==================================================
            // 5️⃣ Loại bỏ text nhiễu Facebook
            // ==================================================
            cleaned = Regex.Replace(
                cleaned,
                @"\b(Quản\s*trị\s*viên|Người\s*kiểm\s*duyệt|Tác\s*giả|Ban\s*quản\s*trị|Đang\s*xem|Đã\s*chia\s*sẻ|Đã\s*đăng|Chỉnh\s*sửa|Người\s*đóng\s*góp(?:\s*nổi\s*bật)?)\b",
                "",
                RegexOptions.IgnoreCase
            ).Trim();

            // ==================================================
            // 6️⃣ Chuẩn hoá cuối
            // ==================================================
            cleaned = Regex.Replace(cleaned, @"\s{2,}", " ").Trim();

            return string.IsNullOrWhiteSpace(cleaned) ? "N/A" : cleaned;
        }

        // chuyển time Raw sang Date Time (Lưu DB lưu biến RealPostTime,dùng so sánh thời gian)
        public static DateTime? ParseFacebookTime(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw) || raw == "N/A")
                return null;

            raw = CleanTimeString(raw).ToLower();
            DateTime now = DateTime.Now;

            Match m;

            // ==================================================
            // 1️⃣ dd tháng MM yyyy HH:mm   (ĐỦ NHẤT – ƯU TIÊN 1)
            // ==================================================
            m = Regex.Match(
                raw,
                @"(\d{1,2})\s*tháng\s*(\d{1,2})\s*(\d{4})\s*(\d{1,2}):(\d{2})",
                RegexOptions.IgnoreCase
            );
            if (m.Success)
            {
                return new DateTime(
                    int.Parse(m.Groups[3].Value),
                    int.Parse(m.Groups[2].Value),
                    int.Parse(m.Groups[1].Value),
                    int.Parse(m.Groups[4].Value),
                    int.Parse(m.Groups[5].Value),
                    0
                );
            }

            // ==================================================
            // 2️⃣ dd tháng MM HH:mm   (KHÔNG NĂM)
            // ==================================================
            m = Regex.Match(
                raw,
                @"(\d{1,2})\s*tháng\s*(\d{1,2})\s*(\d{1,2}):(\d{2})",
                RegexOptions.IgnoreCase
            );
            if (m.Success)
            {
                int day = int.Parse(m.Groups[1].Value);
                int month = int.Parse(m.Groups[2].Value);
                int hour = int.Parse(m.Groups[3].Value);
                int minute = int.Parse(m.Groups[4].Value);

                int year = now.Year;
                if (month > now.Month) year--; // FB không ghi năm

                return new DateTime(year, month, day, hour, minute, 0);
            }

            // ==================================================
            // 3️⃣ dd tháng MM yyyy   (KHÔNG GIỜ)
            // ==================================================
            m = Regex.Match(
                raw,
                @"(\d{1,2})\s*tháng\s*(\d{1,2})\s*(\d{4})",
                RegexOptions.IgnoreCase
            );
            if (m.Success)
            {
                return new DateTime(
                    int.Parse(m.Groups[3].Value),
                    int.Parse(m.Groups[2].Value),
                    int.Parse(m.Groups[1].Value),
                    0, 0, 0
                );
            }

            // ==================================================
            // 4️⃣ dd tháng MM   (KHÔNG NĂM – KHÔNG GIỜ)
            // ==================================================
            m = Regex.Match(
                raw,
                @"(\d{1,2})\s*tháng\s*(\d{1,2})",
                RegexOptions.IgnoreCase
            );
            if (m.Success)
            {
                int day = int.Parse(m.Groups[1].Value);
                int month = int.Parse(m.Groups[2].Value);

                int year = now.Year;
                if (month > now.Month) year--;

                return new DateTime(year, month, day, 0, 0, 0);
            }

            // ==================================================
            // 5️⃣ hôm nay lúc HH:mm
            // ==================================================
            m = Regex.Match(raw, @"hôm\s*nay\s*(\d{1,2}):(\d{2})");
            if (m.Success)
            {
                return new DateTime(
                    now.Year, now.Month, now.Day,
                    int.Parse(m.Groups[1].Value),
                    int.Parse(m.Groups[2].Value),
                    0
                );
            }

            // ==================================================
            // 6️⃣ hôm qua lúc HH:mm
            // ==================================================
            m = Regex.Match(raw, @"hôm\s*qua\s*(\d{1,2}):(\d{2})");
            if (m.Success)
            {
                DateTime d = now.AddDays(-1);
                return new DateTime(
                    d.Year, d.Month, d.Day,
                    int.Parse(m.Groups[1].Value),
                    int.Parse(m.Groups[2].Value),
                    0
                );
            }

            // ==================================================
            // 7️⃣ hôm nay / hôm qua (KHÔNG GIỜ)
            // ==================================================
            if (raw.Contains("hôm nay"))
                return DateTime.Today;

            if (raw.Contains("hôm qua"))
                return DateTime.Today.AddDays(-1);

            // ==================================================
            // 8️⃣ x phút / giờ / ngày / tuần / tháng / năm trước
            // ==================================================
            m = Regex.Match(raw, @"(\d+)\s*(phút|giờ|ngày|tuần|tháng|năm)");
            if (m.Success)
            {
                int value = int.Parse(m.Groups[1].Value);
                string unit = m.Groups[2].Value;

                switch (unit)
                {
                    case "phút":
                        return now.AddMinutes(-value);

                    case "giờ":
                        return now.AddHours(-value);

                    case "ngày":
                        return now.AddDays(-value).Date;

                    case "tuần":
                        return now.AddDays(-7 * value).Date;

                    case "tháng":
                        return now.AddMonths(-value).Date;

                    case "năm":
                        return now.AddYears(-value).Date;

                    default:
                        return null;
                }
            }
            // ==================================================
            // ❌ KHÔNG HIỂU → NULL + LOG
            // ==================================================
            Libary.Instance.LogService($"[TIME PARSE FAIL] raw='{raw}'");
            return null;
        }

        // Chuyển DateTime sang text đẹp, chỉ việc hiện lên grid
        public static string NormalizeTime(DateTime? dt)
        {
            if (dt == null)
                return "N/A";

            // 00:00:00 = không có giờ
            bool hasTime = dt.Value.TimeOfDay != TimeSpan.Zero;

            return hasTime
                ? dt.Value.ToString("dd/MM/yyyy HH:mm")
                : dt.Value.ToString("dd/MM/yyyy");
        }
        // Dùng cho chuyển combovox sang số ngày 
        public static int ConvertToDays(string sel)
        {
            if (string.IsNullOrEmpty(sel)) return 7;
            sel = sel.ToLower();
            if (sel.Contains("hôm nay") || sel.Contains("1 ngày")) return 1;
            if (sel.Contains("1 tuần") || sel.Contains("7 ngày")) return 7;
            if (sel.Contains("1 tháng") || sel.Contains("30 ngày")) return 30;
            int v;
            if (int.TryParse(new string(sel.Where(char.IsDigit).ToArray()), out v)) return v;
            return 7;
        }
       // public static bool IsPostInTimeRange(string postTime, int soNgay) { }
        // xem có giờ hay k từ việc xem có dấu :
        public static bool HasTime(string parsedTime)
        {
            if (string.IsNullOrWhiteSpace(parsedTime))
                return false;

            return parsedTime.Contains(":");   // có giờ nếu có dấu :
        }
        // Hàm xem text phải là thời gina
        public static bool IsTime(string txt)
        {
            if (string.IsNullOrWhiteSpace(txt))
                return false;

            string lower = txt.ToLower().Trim();

            string[] keywords =
            {
        "giờ", "phút", "ngày", "tháng", "năm",
        "lúc", "hôm nay", "hôm qua",
        "today", "yesterday"
    };

            foreach (var k in keywords)
            {
                if (lower.Contains(k))
                    return true;
            }

            // Regex: dạng "2 giờ", "15 phút", "1 ngày", "3 tháng", "5 năm"
            if (Regex.IsMatch(lower, @"\d+\s*(phút|giờ|ngày|tháng|năm)", RegexOptions.IgnoreCase))
                return true;

            // Regex: dạng "lúc 3:45" , "3:50 PM"
            if (Regex.IsMatch(lower, @"(lúc\s*\d+)|(\d{1,2}:\d{2})", RegexOptions.IgnoreCase))
                return true;

            return false;
        }
        // Hàm so sánh thời gian
        public static bool IsOldPost(DateTime? crawlTime, DateTime? lastPostTime)
        {
            // Không có mốc so sánh → coi là mới
            if (!lastPostTime.HasValue)
                return false;

            // Không parse được thời gian crawl → coi là mới
            if (!crawlTime.HasValue)
                return false;

            DateTime newest = lastPostTime.Value;
            DateTime current = crawlTime.Value;

            // xác định "không có giờ" = 00:00:00
            bool crawlHasTime = current.TimeOfDay != TimeSpan.Zero;
            bool newestHasTime = newest.TimeOfDay != TimeSpan.Zero;

            // 1️⃣ Nếu 1 trong 2 KHÔNG có giờ → so sánh NGÀY
            if (!crawlHasTime || !newestHasTime)
            {
                return current.Date < newest.Date;
            }

            // 2️⃣ Nếu cả 2 CÓ giờ → so sánh full DateTime
            return current < newest;
        }
    }
}
