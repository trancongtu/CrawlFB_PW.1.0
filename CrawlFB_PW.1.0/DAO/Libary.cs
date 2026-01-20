using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using CrawlFB_PW._1._0.DTO;
using System.Text;

namespace CrawlFB_PW._1._0
{
    public class Libary
    {
        private static readonly Lazy<Libary> _inst = new Lazy<Libary>(() => new Libary());
        public static Libary Instance => _inst.Value;

        // ===============================
        // PROFILE CONTEXT (ASYNC SAFE)
        // ===============================
        private static readonly AsyncLocal<string> _profileId = new AsyncLocal<string>();
        private static readonly AsyncLocal<string> _profileName = new AsyncLocal<string>();

        // ===============================
        // LOG ROOT
        // ===============================
        private readonly string _logRoot;

        private Libary()
        {
            _logRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logfile");
            if (!Directory.Exists(_logRoot))
                Directory.CreateDirectory(_logRoot);
        }

        // ===============================
        // PROFILE CONTEXT API (GIỮ NGUYÊN)
        // ===============================
        public void SetProfileContext(string profileId, string profileName = null)
        {
            _profileId.Value = profileId;
            _profileName.Value = profileName;
        }

        public void ClearProfileContext()
        {
            _profileId.Value = null;
            _profileName.Value = null;
        }

        // ==================================================
        // CORE LOG – GIỮ NGUYÊN 100% (KHÔNG PHÁ CODE CŨ)
        // ==================================================
        public void CreateLog(string module, string message)
        {
            try
            {
                if (!AppConfig.ENABLE_LOG) return;

                string file = Path.Combine(_logRoot, $"{module}.log");
                string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - [{module}] - {message}";
                File.AppendAllText(file, line + Environment.NewLine);
            }
            catch { }
        }

        // Legacy – giữ nguyên
        public void CreateLog(string message)
        {
            CreateLog("Common", message);
        }

        // ==================================================
        // 1️⃣ SERVICE LOG (GIỮ NGUYÊN)
        // ==================================================
        public void LogService(string message)
        {
            CreateLog("Service", message);
        }

        // ==================================================
        // 2️⃣ FORM LOG (GIỮ NGUYÊN)
        // ==================================================
        public void LogForm(string formName, string message)
        {
            CreateLog(formName, message);
        }

        // ==================================================
        // 3️⃣ TECH / PROCESS LOG
        // 👉 LogTech CŨ = process.log (theo profile)
        // ==================================================
        public void LogTech(
            string message,
            bool? enableLog = null,
            [CallerMemberName] string memberName = ""
        )
        {
            try
            {
                if (enableLog.HasValue && enableLog.Value == false)
                    return;

                if (enableLog.HasValue && enableLog.Value == true && !AppConfig.ENABLE_LOG)
                    return;

                WriteProfileLog(
                    fileName: "process.log",
                    level: "INFO",
                    message: message,
                    memberName: memberName
                );
            }
            catch { }
        }

        // ==================================================
        // 4️⃣ DEBUG LOG – DÙNG CHO HÀM NHỎ
        // ==================================================
        public void LogDebug(
            string message,
            [CallerMemberName] string memberName = ""
        )
        {
            try
            {
                if (!AppConfig.ENABLE_DEBUG_LOG)
                    return;

                WriteProfileLog(
                    fileName: "debug.log",
                    level: "DEBUG",
                    message: message,
                    memberName: memberName
                );
            }
            catch { }
        }

        // ==================================================
        // 5️⃣ ERROR LOG – DỒN VÀO DEBUG.LOG
        // ==================================================
        public void LogError(
            string message,
            Exception ex = null,
            [CallerMemberName] string memberName = ""
        )
        {
            try
            {
                string full = ex == null
                    ? message
                    : $"{message}{Environment.NewLine}{ex}";

                WriteProfileLog(
                    fileName: "debug.log",
                    level: "ERROR",
                    message: full,
                    memberName: memberName
                );
            }
            catch { }
        }
        public void LogPost(
    PostPage post,
    string source, // "GROUP" | "FANPAGE"
    bool? enableLog = null,
    [CallerMemberName] string memberName = ""
)
        {
            try
            {
                if (!AppConfig.ENABLE_LOG_POST)
                    return;

                if (enableLog.HasValue && enableLog.Value == false)
                    return;

                if (post == null)
                    return;

                var sb = new StringBuilder();

                sb.AppendLine(StartPost(source));
                sb.AppendLine($"Source       : {source}");
                sb.AppendLine($"PostID       : {post.PostID}");
                sb.AppendLine($"PostLink     : {post.PostLink}");
                sb.AppendLine($"PostTime     : {post.PostTime}");
                sb.AppendLine($"PostStatus   : {post.PostType.ToString()}");
                sb.AppendLine();
                sb.AppendLine($"PosterName   : {post.PosterName}");
                sb.AppendLine($"PosterLink   : {post.PosterLink}");
                sb.AppendLine($"PosterNote   : {post.PosterNote}");
                sb.AppendLine();
                sb.AppendLine($"PageName     : {post.PageName}");
                sb.AppendLine($"PageLink     : {post.PageLink}");
                sb.AppendLine();
                sb.AppendLine($"LikeCount    : {post.LikeCount}");
                sb.AppendLine($"CommentCount : {post.CommentCount}");
                sb.AppendLine($"ShareCount   : {post.ShareCount}");
                sb.AppendLine();
                sb.AppendLine($"ContentLen   : {(post.Content ?? "").Length}");
                sb.AppendLine("Content:");
                sb.AppendLine(post.Content);
                sb.AppendLine();
                sb.AppendLine($"Attachment   : {post.Attachment}");
                sb.AppendLine($"Topic        : {post.Topic}");
                sb.AppendLine(EndPost(source));

                WriteProfileLog(
                    fileName: "logpost.log",
                    level: "POST",
                    message: sb.ToString(),
                    memberName: memberName
                );
            }
            catch { }
        }

        // ==================================================
        // CORE WRITE – 1 CHỖ DUY NHẤT GHI FILE
        // ==================================================
        private void WriteProfileLog(
            string fileName,
            string level,
            string message,
            string memberName
        )
        {
            if (!AppConfig.ENABLE_LOG)
                return;

            // xác định class gọi
            var stack = new StackTrace();
            var frame = stack.GetFrame(2);
            var method = frame?.GetMethod();
            var type = method?.DeclaringType;
            string className = type != null ? type.Name : "UnknownClass";

            // thư mục profile
            string profileFolder = string.IsNullOrEmpty(_profileId.Value)
                ? Path.Combine(_logRoot, "profile_COMMON")
                : Path.Combine(_logRoot, $"profile_{_profileId.Value}");

            if (!Directory.Exists(profileFolder))
                Directory.CreateDirectory(profileFolder);

            string file = Path.Combine(profileFolder, fileName);

            string shortProfileName = _profileName.Value;

            if (!string.IsNullOrEmpty(shortProfileName) && shortProfileName.Length > 4)
                shortProfileName = shortProfileName.Substring(0, 4);

            string profileInfo = string.IsNullOrEmpty(_profileId.Value)
                ? "[NoProfile]"
                : $"[Profile:{_profileId.Value}|{shortProfileName}]";
            string line =
                $"[{DateTime.Now:HH:mm:ss}]" +
                $"[{level}]" +
                $"{profileInfo}" +
                $"[{className}.{memberName}] " +
                message;

            File.AppendAllText(file, line + Environment.NewLine);
        }

        // ==================================================
        // CLEAR LOG (GIỮ NGUYÊN)
        // ==================================================
        public void ClearLog(string module)
        {
            try
            {
                string file = Path.Combine(_logRoot, $"{module}.log");
                if (File.Exists(file))
                {
                    File.WriteAllText(file, string.Empty);
                    CreateLog(module, "🧹 Log đã được xóa");
                }
            }
            catch { }
        }

        public void ClearAllLogs()
        {
            try
            {
                if (!Directory.Exists(_logRoot))
                    return;

                foreach (var file in Directory.GetFiles(_logRoot, "*.log", SearchOption.AllDirectories))
                    File.Delete(file);

                CreateLog("System", "🧹 Đã xóa toàn bộ log khi khởi động");
            }
            catch { }
        }
        // ===============================
        // ICON HELPER
        // ===============================
        public static string IconOK => "✅";
        public static string IconFail => "❌";
        public static string IconWarn => "⚠️";
        public static string IconInfo => "ℹ️";
        public static string IconStart => "▶";
        public static string IconEnd => "✔";

        public static string BoolIcon(bool value)
            => value ? IconOK : IconFail;

        public static string CountIcon(int count)
            => count > 0 ? IconOK : IconFail;
        // ===============================
        // POST PROCESS MARKER
        // ===============================
        public static string StartPost(string title = null)
        {
            return string.IsNullOrWhiteSpace(title)
                ? "══════════════════════ START POST ══════════════════════"
                : $"══════════════════════ START {title.ToUpper()} ══════════════════════";
        }

        public static string EndPost(string title = null)
        {
            return string.IsNullOrWhiteSpace(title)
                ? "══════════════════════ END POST ══════════════════════"
                : $"══════════════════════ END {title.ToUpper()} ══════════════════════";
        }

    }
}
