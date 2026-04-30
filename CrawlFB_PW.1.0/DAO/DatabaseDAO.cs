using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CrawlFB_PW._1._0.DTO;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using DocumentFormat.OpenXml.Office.Word;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CrawlFB_PW._1._0.Enums;
using CrawlFB_PW._1._0.Helper.Data;

namespace CrawlFB_PW._1._0.DAO
{
    public class DatabaseDAO
    {
        private static DatabaseDAO instance;
        public static DatabaseDAO Instance
        {
            get
            {
                if (instance == null)
                    instance = new DatabaseDAO();
                return instance;
            }
        }

        private DatabaseDAO() { }

        private const string MAIN_DB = "MainDatabase.db";

        // -------------------------
        // Utility: generate PostID
        // SHA1 hash then take first 16 hex chars (stable, short)
        // -------------------------
        public static string GeneratePostId(string postLink)
        {
            if (string.IsNullOrEmpty(postLink))
                return Guid.NewGuid().ToString("N").Substring(0, 16);

            using (var sha1 = SHA1.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(postLink.Trim().ToLowerInvariant());
                byte[] hash = sha1.ComputeHash(bytes);
                // take first 8 bytes -> 16 hex chars
                StringBuilder sb = new StringBuilder(16);
                for (int i = 0; i < 8; i++)
                {
                    sb.Append(hash[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
        private string GenerateHashId(string input)
        {
            if (string.IsNullOrEmpty(input))
                return Guid.NewGuid().ToString("N").Substring(0, 12);

            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input.Trim().ToLowerInvariant()));
                return BitConverter.ToString(hash, 0, 6).Replace("-", "").ToLower();
            }
        }

        private string BuildInteractionString(PostPage p)
        {
            try
            {
                var list = new System.Collections.Generic.List<string>();
                if (p.LikeCount.HasValue) list.Add("likes=" + p.LikeCount.Value);
                if (p.ShareCount.HasValue) list.Add("shares=" + p.ShareCount.Value);
                if (p.CommentCount.HasValue) list.Add("comments=" + p.CommentCount.Value);
                return string.Join(";", list);
            }
            catch { return ""; }
        }
        public void CreateDatabase(string dbName = "MainDatabase.db")
        {
            try
            {
                using (var connection = SqliteHelper.Instance.GetConnection(dbName))
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = @"
                PRAGMA foreign_keys = ON;
                PRAGMA journal_mode = WAL;
                PRAGMA synchronous = NORMAL;

                --------------------------------------------------------
                -- TABLE PAGE INFO
                --------------------------------------------------------
                            CREATE TABLE TablePageInfo (
                    PageID TEXT PRIMARY KEY,
                    IDFBPage TEXT DEFAULT 'N/A',
                    PageLink TEXT,
                    PageName TEXT,
                    PageType TEXT,
                    PageMembers TEXT DEFAULT 'N/A',
                    PageInteraction TEXT DEFAULT 'N/A',
                    PageEvaluation TEXT DEFAULT 'N/A',
                    PageInfoText TEXT DEFAULT 'N/A',
                    IsScanned INTEGER DEFAULT 0,
                    PageTimeSave DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                --------------------------------------------------------
                -- TABLE PERSON INFO
                --------------------------------------------------------
                CREATE TABLE IF NOT EXISTS TablePersonInfo (
                    PersonID TEXT PRIMARY KEY,
                    IDFBPerson TEXT DEFAULT 'N/A',
                    PersonLink TEXT,
                    PersonName TEXT,
                    PersonInfo TEXT DEFAULT 'N/A',
                    PersonNote TEXT DEFAULT 'N/A',
                    PersonTimeSave DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                --------------------------------------------------------
                -- TABLE POST INFO
                --------------------------------------------------------
                CREATE TABLE IF NOT EXISTS TablePostInfo (
                    PostID TEXT PRIMARY KEY,
                    IDFBPost TEXT DEFAULT 'N/A',
                    PostLink TEXT,
                    PostContent TEXT,
                    PostTime TEXT,
                    RealPostTime TEXT,
                    LikeCount INTEGER DEFAULT 0,
                    ShareCount INTEGER DEFAULT 0,
                    CommentCount INTEGER DEFAULT 0,
                    PostInteraction TEXT DEFAULT '',
                    PostAttachment TEXT DEFAULT 'N/A',
                    PostStatus TEXT DEFAULT 'N/A',
                    PostTimeSave DATETIME DEFAULT CURRENT_TIMESTAMP                  
                );

                --------------------------------------------------------
                -- TABLE POST (mapping)
                --------------------------------------------------------
                CREATE TABLE IF NOT EXISTS TablePost (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    PostID TEXT NOT NULL,
                    PageIDCreate TEXT,
                    PageIDContainer TEXT,
                    PersonIDCreate TEXT,
                    FOREIGN KEY(PostID) REFERENCES TablePostInfo(PostID) ON DELETE CASCADE,
                    FOREIGN KEY(PageIDCreate) REFERENCES TablePageInfo(PageID),
                    FOREIGN KEY(PageIDContainer) REFERENCES TablePageInfo(PageID),
                    FOREIGN KEY(PersonIDCreate) REFERENCES TablePersonInfo(PersonID)
                );

                --------------------------------------------------------
                -- TABLE POST SHARE (giữ nguyên)
                --------------------------------------------------------
                CREATE TABLE IF NOT EXISTS TablePostShare (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    PostID TEXT NOT NULL,
                    PageID TEXT,
                    PersonID TEXT,
                    TimeShare TEXT,
                    FOREIGN KEY(PostID) REFERENCES TablePostInfo(PostID) ON DELETE CASCADE,
                    FOREIGN KEY(PageID) REFERENCES TablePageInfo(PageID),
                    FOREIGN KEY(PersonID) REFERENCES TablePersonInfo(PersonID)
                );

                --------------------------------------------------------
                -- COMMENT + TOPIC giữ nguyên
                --------------------------------------------------------
                CREATE TABLE IF NOT EXISTS TableCommentInfo (
                    CommentID TEXT PRIMARY KEY,
                    PersonID TEXT,
                    PageID TEXT,
                    Content TEXT,
                    TimeSave TEXT,
                    FOREIGN KEY(PersonID) REFERENCES TablePersonInfo(PersonID),
                    FOREIGN KEY(PageID) REFERENCES TablePageInfo(PageID)
                );

                CREATE TABLE IF NOT EXISTS TablePostComment (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    PostID TEXT NOT NULL,
                    CommentID TEXT NOT NULL,
                    CommentTime TEXT,
                    UNIQUE(PostID, CommentID)
                );

                --------------------------------------------------------
                -- TOPIC/KEYWORD (giữ nguyên)
                --------------------------------------------------------
                CREATE TABLE IF NOT EXISTS TableTopic (
                    TopicId INTEGER PRIMARY KEY AUTOINCREMENT,
                    TopicName TEXT NOT NULL UNIQUE,
                    TopicInfor TEXT
                );

                CREATE TABLE IF NOT EXISTS TableKeyword (
                    KeywordId INTEGER PRIMARY KEY AUTOINCREMENT,
                    KeywordName TEXT NOT NULL UNIQUE
                );

                CREATE TABLE IF NOT EXISTS TableTopicKey (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TopicId INTEGER NOT NULL,
                    KeywordId INTEGER NOT NULL,
                    UNIQUE(TopicId, KeywordId)
                );

                CREATE TABLE IF NOT EXISTS TableTopicPost (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TopicId INTEGER NOT NULL,
                    PostId TEXT NOT NULL,
                    UNIQUE(TopicId, PostId)           
                );
               CREATE TABLE IF NOT EXISTS TablePageMonitor (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                PageID TEXT UNIQUE,          -- 🔥 Thêm UNIQUE ở đây
                IsAuto INTEGER DEFAULT 0,
                Status TEXT DEFAULT 'Chưa auto',
                FirstScanTime TEXT,
                LastScanTime TEXT,
                TotalPostsScanned INTEGER,
                TimeSave TEXT
            );
            CREATE TABLE IF NOT EXISTS TableManagerProfile (
                 ID INT IDENTITY(1,1) PRIMARY KEY,
                ProfileID INT NOT NULL,
                PageID NVARCHAR(200) NOT NULL,
                TimeSave DATETIME DEFAULT GETDATE(),
                FOREIGN KEY(ProfileID) REFERENCES TableProfileInfo(ID)
            );
          CREATE TABLE IF NOT EXISTS TableProfileInfo (
                ID INT IDENTITY(1,1) PRIMARY KEY,
                IDAdbrowser NVARCHAR(200),
                ProfileName NVARCHAR(500),
                ProfileLink NVARCHAR(MAX),
                ProfileStatus NVARCHAR(200) DEFAULT 'Die',
                UseTab INT DEFAULT 0,
                ProfileType NVARCHAR(200) DEFAULT 'Person'
            );
                    ";


                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ CreateDatabase new error: " + ex.Message);
            }
        }

        // -------------------------
        // CÁC HÀM POST
        
        // -------------------------
      
        public void DeleteAllPostsOfPage(string pageId)
        {
            try
            {
                string dbPath = PathHelper.Instance.GetMainDatabasePath();

                using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
                {
                    conn.Open();

                    List<string> postIds = new List<string>();

                    // 1️⃣ Lấy tất cả PostID thuộc PageID
                    string sqlGetPosts = @"
                SELECT PostID 
                FROM TablePost 
                WHERE PageIDCreate=@pid OR PageIDContainer=@pid
            ";

                    using (var cmd = new SQLiteCommand(sqlGetPosts, conn))
                    {
                        cmd.Parameters.AddWithValue("@pid", pageId);
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                                postIds.Add(rd["PostID"].ToString());
                        }
                    }

                    if (postIds.Count == 0)
                    {
                        Libary.Instance.CreateLog($"[DeletePageData] Không có bài nào của PageID={pageId}");
                        return;
                    }

                    Libary.Instance.CreateLog($"[DeletePageData] Tìm thấy {postIds.Count} bài cần xóa của PageID={pageId}");

                    foreach (var postId in postIds)
                    {
                        // 2️⃣ XÓA MAPPING POST
                        Execute(conn, "DELETE FROM TablePost WHERE PostID=@id", postId);

                        // 3️⃣ XÓA THÔNG TIN POST
                        Execute(conn, "DELETE FROM TablePostInfo WHERE PostID=@id", postId);

                        // 4️⃣ XÓA SHARE (nếu có)
                        Execute(conn, "DELETE FROM TablePostShare WHERE PostID=@id", postId);

                        // 5️⃣ XÓA COMMENT MAPPING
                        Execute(conn, "DELETE FROM TablePostComment WHERE PostID=@id", postId);

                        // 6️⃣ XÓA KEYWORD/TOPIC mapping
                        Execute(conn, "DELETE FROM TableTopicPost WHERE PostID=@id", postId);
                    }
                }

                Libary.Instance.CreateLog($"[DeletePageData] ✔ Đã xóa dữ liệu trang PageID={pageId}");
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"[DeletePageData] ❌ Lỗi: {ex.Message}");
            }
        }

        private void Execute(SQLiteConnection conn, string sql, string postId)
        {
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", postId);
                cmd.ExecuteNonQuery();
            }
        }

        public void InsertOrIgnorePost(PostPage p, SQLiteConnection externalConn = null)
        {
            if (p == null) return;
            string postId = GeneratePostId(p.PostLink);
            // Page chứa = page đang crawl
            if (string.IsNullOrWhiteSpace(p.PageLink) || p.PageLink == "N/A")
            {
                // Không có Page chứa thì post không hợp lệ → bỏ qua
                return;
            }
            string pageContainerId = GenerateHashId(p.PageLink);
            // Người đăng
            bool isPagePoster = p.PosterNote == FBType.Fanpage ||p.PosterNote == FBType.GroupOn ||p.PosterNote == FBType.GroupOff;

            bool isPersonPoster = p.PosterNote == FBType.Person || p.PosterNote == FBType.PersonKOL;

            // ID người đăng
            string posterPageId = null;
            string posterPersonId = null;

            if (!string.IsNullOrWhiteSpace(p.PosterLink) && p.PosterLink != "N/A")
            {
                if (isPagePoster)
                    posterPageId = GenerateHashId(p.PosterLink);
                else
                    posterPersonId = GenerateHashId(p.PosterLink);
            }
           
            bool ownConn = false;
            var conn = externalConn;

            if (conn == null)
            {
                conn = SqliteHelper.Instance.GetConnection("MainDatabase.db");
                conn.Open();
                ownConn = true;
            }

            using (var tran = conn.BeginTransaction())
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;

                // 1. LƯU PAGE CHỨA
                if (pageContainerId != null)
                {
                    cmd.CommandText =
                    @"INSERT OR IGNORE INTO TablePageInfo
              (PageID, IDFBPage, PageLink, PageName, PageType, PageTimeSave)
              VALUES (@id, 'N/A', @link, @name, @type, datetime('now'));";

                    cmd.Parameters.AddWithValue("@id", pageContainerId);
                    cmd.Parameters.AddWithValue("@link", p.PageLink ?? "");
                    cmd.Parameters.AddWithValue("@name", p.PageName ?? "");
                    cmd.Parameters.AddWithValue("@type", p.PageLink.Contains("/groups/") ? "groups" : "fanpage");
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }
                if (isPagePoster)
                {
                    if (posterPageId != null)
                    {
                        cmd.CommandText =
                        @"INSERT OR IGNORE INTO TablePageInfo
              (PageID, IDFBPage, PageLink, PageName, PageType, PageTimeSave)
              VALUES (@id, 'N/A', @link, @name, @type, datetime('now'));";

                        cmd.Parameters.AddWithValue("@id", posterPageId);
                        cmd.Parameters.AddWithValue("@link", p.PosterLink ?? "");
                        cmd.Parameters.AddWithValue("@name", p.PosterName ?? "");
                        cmd.Parameters.AddWithValue("@type", "fanpage");
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }

                }
                else if (isPersonPoster)
                {
                    // 3. LƯU PERSON ĐĂNG
                    if (posterPersonId != null)
                    {
                        cmd.CommandText =
                        @"INSERT OR IGNORE INTO TablePersonInfo
              (PersonID, IDFBPerson, PersonLink, PersonName, PersonInfo, PersonNote, PersonTimeSave)
              VALUES (@id, 'N/A', @link, @name, 'N/A', @note, datetime('now'));";

                        cmd.Parameters.AddWithValue("@id", posterPersonId);
                        cmd.Parameters.AddWithValue("@link", p.PosterLink ?? "");
                        cmd.Parameters.AddWithValue("@name", p.PosterName ?? "");
                        cmd.Parameters.AddWithValue("@note", p.PosterNote);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }
                }
                // 4) LƯU POSTINFO (ĐÃ CÓ REALPOSTTIME)
                cmd.CommandText = @"
                INSERT OR REPLACE INTO TablePostInfo
                (PostID, IDFBPost, PostLink, PostContent, PostTime, RealPostTime,
                 LikeCount, ShareCount, CommentCount, PostInteraction,
                 PostAttachment, PostStatus, PostTimeSave)
                VALUES
                (@id, 'N/A', @link, @content, @time, @real,
                 @like, @share, @cmt, '',
                 @attachment, @status, datetime('now'));
                ";

                cmd.Parameters.AddWithValue("@id", postId);
                cmd.Parameters.AddWithValue("@link", p.PostLink ?? "");
                cmd.Parameters.AddWithValue("@content", p.Content ?? "");
                cmd.Parameters.AddWithValue("@time", p.PostTime ?? "");

                // ⭐ DÒNG QUAN TRỌNG
                cmd.Parameters.AddWithValue( "@real",p.RealPostTime.HasValue ? (object)p.RealPostTime.Value : DBNull.Value );


                cmd.Parameters.AddWithValue("@like", p.LikeCount ?? 0);
                cmd.Parameters.AddWithValue("@share", p.ShareCount ?? 0);
                cmd.Parameters.AddWithValue("@cmt", p.CommentCount ?? 0);
                cmd.Parameters.AddWithValue("@attachment", p.Attachment ?? "N/A");
                cmd.Parameters.AddWithValue("@status", p.PostType.ToString() ?? "N/A");

                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();


                cmd.CommandText =
                  @"INSERT OR IGNORE INTO TablePost
                (ID, PostID, PageIDCreate, PageIDContainer, PersonIDCreate)
                VALUES(NULL, @postId, @createPage, @containerPage, @createPerson);";

                cmd.Parameters.AddWithValue("@postId", postId);
                cmd.Parameters.AddWithValue("@createPage", (object)posterPageId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@containerPage", (object)pageContainerId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@createPerson", (object)posterPersonId ?? DBNull.Value);

                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                tran.Commit();
                using (var chk = new SQLiteCommand("PRAGMA wal_checkpoint(FULL);", conn))
                {
                    chk.ExecuteNonQuery();
                }
            }
            if (ownConn) conn.Close();
        }
        // -------------------------
        // Update post content/info
        // -------------------------
        public void UpdatePost(PostPage post)
        {
            if (post == null || string.IsNullOrEmpty(post.PostID)) return;

            try
            {
                using (var conn = SqliteHelper.Instance.GetConnection(MAIN_DB))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        string interaction = BuildInteractionString(post);

                        cmd.CommandText = @"
                            UPDATE TablePostInfo
                            SET PostLink = @PostLink,
                                PostContent = @Postcontent,
                                PostTime = @PostTime,
                                PostInteraction = @PostInteraction,
                                PostTimeSave = datetime('now')
                            WHERE PostID = @PostID;
                        ";
                        cmd.Parameters.AddWithValue("@PostID", post.PostID);
                        cmd.Parameters.AddWithValue("@PostLink", post.PostLink ?? "");
                        cmd.Parameters.AddWithValue("@Postcontent", post.Content ?? "");
                        cmd.Parameters.AddWithValue("@PostTime", post.PostTime ?? "");
                        cmd.Parameters.AddWithValue("@PostInteraction", interaction ?? "");
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ UpdatePost error: " + ex.Message);
            }
        }
        // -------------------------
        // Delete post (will cascade if FK ON DELETE CASCADE set)
        // -------------------------
        public void DeletePost(string postId)
        {
            if (string.IsNullOrEmpty(postId)) return;

            try
            {
                using (var conn = SqliteHelper.Instance.GetConnection(MAIN_DB))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        // delete from TablePostInfo then TablePost (FK may cascade)
                        cmd.CommandText = "DELETE FROM TablePostInfo WHERE PostID = @PostID;";
                        cmd.Parameters.AddWithValue("@PostID", postId);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();

                        cmd.CommandText = "DELETE FROM TablePost WHERE PostID = @PostID;";
                        cmd.Parameters.AddWithValue("@PostID", postId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ DeletePost error: " + ex.Message);
            }
        }
        // -------------------------
        /*
        public List<PostPage> GetAllPosts(SQLiteConnection externalConn = null)
        {
            var list = new List<PostPage>();

            try
            {
                bool ownConn = false;
                var conn = externalConn;
                if (conn == null)
                {
                    conn = SqliteHelper.Instance.GetConnection("MainDatabase.db");
                    conn.Open();
                    ownConn = true;
                }

                string sql = @"
        SELECT
            pi.PostID,
            pi.PostLink,
            pi.PostContent,
            pi.RealPostTime,
            pi.LikeCount,
            pi.ShareCount,
            pi.CommentCount,
            pi.PostAttachment,
            pi.PostStatus,

            p.PageIDContainer,
            pg_container.PageName AS ContainerPageName,
            pg_container.PageLink AS ContainerPageLink,

            p.PageIDCreate,
            pg_create.PageName AS PosterPageName,
            pg_create.PageLink AS PosterPageLink,

            p.PersonIDCreate,
            ps.PersonName AS PosterPersonName,
            ps.PersonLink AS PosterPersonLink,
            ps.PersonNote AS PosterPersonNote

        FROM TablePostInfo pi
        JOIN TablePost p ON p.PostID = pi.PostID
        LEFT JOIN TablePageInfo pg_container ON pg_container.PageID = p.PageIDContainer
        LEFT JOIN TablePageInfo pg_create ON pg_create.PageID = p.PageIDCreate
        LEFT JOIN TablePersonInfo ps ON ps.PersonID = p.PersonIDCreate

        ORDER BY pi.PostTimeSave DESC;
        ";

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    using (var reader = cmd.ExecuteReader())
                    {
                        // build a HashSet of available column names for quick HasCol checks
                        var availableCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        var schema = reader.GetSchemaTable();
                        if (schema != null)
                        {
                            foreach (System.Data.DataRow r in schema.Rows)
                            {
                                var col = r["ColumnName"]?.ToString();
                                if (!string.IsNullOrWhiteSpace(col))
                                    availableCols.Add(col);
                            }
                        }

                        bool HasCol(string name) => availableCols.Contains(name);

                        while (reader.Read())
                        {
                            var dto = new PostPage();

                            // safe-get helpers
                            string GetStringSafe(string col)
                            {
                                try
                                {
                                    if (!HasCol(col)) return null;
                                    var v = reader[col];
                                    return v == DBNull.Value ? null : v.ToString();
                                }
                                catch { return null; }
                            }
                            int GetIntSafe(string col)
                            {
                                try
                                {
                                    if (!HasCol(col)) return 0;
                                    var v = reader[col];
                                    if (v == DBNull.Value || v == null) return 0;
                                    if (v is long) return Convert.ToInt32((long)v);
                                    if (v is int) return (int)v;
                                    int n;
                                    if (int.TryParse(v.ToString(), out n)) return n;
                                    return 0;
                                }
                                catch { return 0; }
                            }

                            dto.PostID = GetStringSafe("PostID") ?? "";
                            dto.PostLink = GetStringSafe("PostLink") ?? "";
                            dto.Content = GetStringSafe("PostContent") ?? "";
                            dto.PostTime = GetStringSafe("RealPostTime") ?? "";

                            dto.LikeCount = GetIntSafe("LikeCount");
                            dto.ShareCount = GetIntSafe("ShareCount");
                            dto.CommentCount = GetIntSafe("CommentCount");

                            dto.Attachment = GetStringSafe("PostAttachment") ?? "";
                            dto.PostStatus = GetStringSafe("PostStatus") ?? "";

                            // page chứa
                            dto.PageName = GetStringSafe("ContainerPageName") ?? "";
                            dto.PageLink = GetStringSafe("ContainerPageLink") ?? "";

                            // người đăng (page hoặc person)
                            string posterPageName = GetStringSafe("PosterPageName");
                            string posterPersonName = GetStringSafe("PosterPersonName");

                            if (!string.IsNullOrWhiteSpace(posterPageName))
                            {
                                dto.PosterName = posterPageName;
                                dto.PosterLink = GetStringSafe("PosterPageLink") ?? "";
                                dto.PosterNote = "page";
                            }
                            else if (!string.IsNullOrWhiteSpace(posterPersonName))
                            {
                                dto.PosterName = posterPersonName;
                                dto.PosterLink = GetStringSafe("PosterPersonLink") ?? "";
                                dto.PosterNote = GetStringSafe("PosterPersonNote") ?? "person";
                            }
                            else
                            {
                                dto.PosterName = "N/A";
                                dto.PosterLink = "N/A";
                                dto.PosterNote = "unknown";
                            }

                            list.Add(dto);
                        }
                    }
                }

                if (ownConn) conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ GetAllPosts NEW error: " + ex.Message);
                Libary.Instance.CreateLog($"❌ GetAllPosts NEW error: {ex.Message}");
            }

            return list;
        }*/
        public List<PostPage> GetAllPosts(SQLiteConnection externalConn = null)
        {
            var list = new List<PostPage>();

            try
            {
                bool own = false;
                var conn = externalConn;

                if (conn == null)
                {
                    string db = PathHelper.Instance.GetMainDatabasePath();
                    conn = SqliteHelper.Instance.GetConnection(db);
                    conn.Open();
                    own = true;
                }

                string sql = @"
SELECT
    pi.PostID,
    pi.PostLink,
    pi.PostContent,
    pi.RealPostTime,
    pi.LikeCount,
    pi.ShareCount,
    pi.CommentCount,
    pi.PostAttachment,
    pi.PostStatus,

    p.PageIDContainer,
    pg_container.PageName AS ContainerPageName,
    pg_container.PageLink AS ContainerPageLink,

    p.PageIDCreate,
    pg_create.PageName AS PosterPageName,
    pg_create.PageLink AS PosterPageLink,

    p.PersonIDCreate,
    ps.PersonName AS PosterPersonName,
    ps.PersonLink AS PosterPersonLink,
    ps.PersonNote AS PosterPersonNote

FROM TablePostInfo pi
JOIN TablePost p ON p.PostID = pi.PostID
LEFT JOIN TablePageInfo pg_container ON pg_container.PageID = p.PageIDContainer
LEFT JOIN TablePageInfo pg_create ON pg_create.PageID = p.PageIDCreate
LEFT JOIN TablePersonInfo ps ON ps.PersonID = p.PersonIDCreate

ORDER BY datetime(pi.PostTimeSave) DESC;
";

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;

                    using (var da = new SQLiteDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow r in dt.Rows)
                        {
                            var dto = new PostPage()
                            {
                                PostID = r["PostID"].ToString(),
                                PostLink = r["PostLink"].ToString(),
                                Content = r["PostContent"].ToString(),

                                // NHƯ CŨ: PostTime = RealPostTime
                                PostTime = r["RealPostTime"].ToString(),

                                LikeCount = Convert.ToInt32(r["LikeCount"]),
                                ShareCount = Convert.ToInt32(r["ShareCount"]),
                                CommentCount = Convert.ToInt32(r["CommentCount"]),

                                Attachment = r["PostAttachment"].ToString(),
                                PostType = r.GetEnum("PostStatus", PostType.UnknowType),

                                // Page chứa bài viết
                                PageName = r["ContainerPageName"].ToString(),
                                PageLink = r["ContainerPageLink"].ToString(),
                            };

                            // Người đăng (y hệt logic cũ)
                            if (!string.IsNullOrEmpty(r["PosterPageName"].ToString()))
                            {
                                dto.PosterName = r["PosterPageName"].ToString();
                                dto.PosterLink = r["PosterPageLink"].ToString();
                                dto.PosterNote = FBType.Fanpage;
                            }
                            else if (!string.IsNullOrEmpty(r["PosterPersonName"].ToString()))
                            {
                                dto.PosterName = r["PosterPersonName"].ToString();
                                dto.PosterLink = r["PosterPersonLink"].ToString();
                                 dto.PosterNote = Enum.TryParse(
                                 r["PosterPersonNote"]?.ToString(),
                                 true, // ignore case
                                 out FBType type
                             ) ? type : FBType.Unknown;
                            }
                            else
                            {
                                dto.PosterName = "N/A";
                                dto.PosterLink = "N/A";
                                dto.PosterNote = FBType.Unknown;
                            }

                            list.Add(dto);
                        }
                    }
                }

                if (own) conn.Close();
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("❌ GetAllPosts SAFE error: " + ex.Message);
            }

            return list;
        }
        public DataTable GetPosts(string timeFilter = "Toàn thời gian", int limit = 0)
        {
            Libary.Instance.CreateLog("📌 GetPosts() DB path = " + Path.GetFullPath("MainDatabase.db"));
            var dt = new DataTable();
            try
            {
                string timeCondition = BuildTimeCondition(timeFilter);

                using (var conn = SqliteHelper.Instance.GetConnection("MainDatabase.db"))
                {
                    conn.Open();

                    string sql = $@"
                    SELECT 
                        pi.RealPostTime      AS 'Thời gian đăng',
                        pi.PostContent       AS 'Nội dung',
                        pi.PostLink          AS 'Link bài viết',

                        -- Người đăng (Page hoặc Person)
                        COALESCE(pg_create.PageName, ps.PersonName, 'N/A') AS 'Người đăng',
                        COALESCE(pg_create.PageLink, ps.PersonLink, 'N/A') AS 'Link người đăng',

                        -- Page chứa
                        pg_container.PageName AS 'Page chứa',
                        pg_container.PageLink AS 'Link Page chứa',

                        pi.LikeCount    AS 'Like',
                        pi.ShareCount   AS 'Share',
                        pi.CommentCount AS 'Comment',

                        pi.PostAttachment AS 'Ảnh/Video',
                        pi.PostStatus     AS 'Trạng thái',

                        pi.PostTimeSave   AS 'Thời gian lưu'

                    FROM TablePostInfo pi
                    JOIN TablePost p 
                        ON p.PostID = pi.PostID

                    LEFT JOIN TablePageInfo pg_container 
                        ON pg_container.PageID = p.PageIDContainer

                    LEFT JOIN TablePageInfo pg_create 
                        ON pg_create.PageID = p.PageIDCreate

                    LEFT JOIN TablePersonInfo ps 
                        ON ps.PersonID = p.PersonIDCreate

                    WHERE 1=1 {timeCondition}

                    ORDER BY 
                        CASE WHEN pi.RealPostTime IS NULL THEN 1 ELSE 0 END,
                        datetime(pi.RealPostTime) DESC
                    ";

                    if (limit > 0)
                        sql += $" LIMIT {limit}";

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        using (var reader = cmd.ExecuteReader())
                            dt.Load(reader);
                    }
                }

                // THÊM CỘT STT
                dt.Columns.Add("STT", typeof(int));

                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["STT"] = i + 1;

                // ĐƯA STT LÊN ĐẦU
                dt.Columns["STT"].SetOrdinal(0);

                // Format lại cột "Thời gian đăng"
                foreach (DataRow row in dt.Rows)
                {
                    string iso = row["Thời gian đăng"]?.ToString();

                    if (string.IsNullOrWhiteSpace(iso))
                    {
                        row["Thời gian đăng"] = "N/A";
                        continue;
                    }

                    if (iso.Length <= 10) // yyyy-MM-dd
                        row["Thời gian đăng"] = DateTime.Parse(iso).ToString("dd/MM/yyyy");
                    else
                        row["Thời gian đăng"] = DateTime.Parse(iso).ToString("dd/MM/yyyy HH:mm");
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("❌ GetPosts error: " + ex.Message);
            }

            return dt;
        }
        //---hàm bổ trợ cho getpost
        private string BuildTimeCondition(string filter)
        {
            string condition = "";
            if (filter == "1 Ngày trước")
                condition = " AND datetime(i.PostTime) >= datetime('now', '-1 day')";
            else if (filter == "1 Tuần trước")
                condition = " AND datetime(i.PostTime) >= datetime('now', '-7 day')";
            else if (filter == "1 Tháng trước")
                condition = " AND datetime(i.PostTime) >= datetime('now', '-30 day')";
            else
                condition = "";

            return condition;
        }

        public static int ParseMaxCount(string text)
        {
            int limit = 100;
            if (text == "Tất cả")
                limit = 0;
            else
            {
                int.TryParse(text, out limit);
            }
            return limit;

        }

        private void ParseInteractionString(string inter, ref PostPage dto)
        {
            // simple parser for "likes=10;shares=2;comments=3"
            try
            {
                var parts = inter.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in parts)
                {
                    var kv = p.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (kv.Length != 2) continue;
                    var key = kv[0].Trim().ToLowerInvariant();
                    var val = kv[1].Trim();
                    int n;
                    if (!int.TryParse(val, out n)) continue;
                    if (key == "likes") dto.LikeCount = n;
                    else if (key == "shares") dto.ShareCount = n;
                    else if (key == "comments") dto.CommentCount = n;
                }
            }
            catch { }
        }

        public void InsertOrIgnorePage(string pageLink, string pageName)
        {
            try
            {
                if (string.IsNullOrEmpty(pageLink) || pageLink == "N/A") return;

                // ✅ Sinh PageID từ link
                string pageId = GenerateHashId(pageLink);

                // ✅ Xác định loại page (Group hoặc Fanpage)
                string pageType = pageLink.ToLower().Contains("/groups/") ? "Group" : "Fanpage";

                using (var conn = SqliteHelper.Instance.GetConnection(MAIN_DB))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                    INSERT OR IGNORE INTO TablePageInfo
                    (PageID, PageLink, PageName, PageType, PageTimeSave)
                    VALUES (@PageID, @PageLink, @PageName, @PageType, datetime('now'));
                ";
                        cmd.Parameters.AddWithValue("@PageID", pageId);
                        cmd.Parameters.AddWithValue("@PageLink", pageLink ?? "");
                        cmd.Parameters.AddWithValue("@PageName", pageName ?? "");
                        cmd.Parameters.AddWithValue("@PageType", pageType);
                        cmd.ExecuteNonQuery();
                    }
                }

                Console.WriteLine($"✅ Page '{pageName}' ({pageType}) đã được lưu hoặc đã tồn tại.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ InsertOrIgnorePage error: " + ex.Message);
            }
        }
        public void InsertOrIgnorePerson(string personLink, string personName)
        {
            try
            {
                if (string.IsNullOrEmpty(personLink) || personLink == "N/A") return;

                string personId = GenerateHashId(personLink);

                using (var conn = SqliteHelper.Instance.GetConnection(MAIN_DB))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                    INSERT OR IGNORE INTO TablePersonInfo
                    (PersonID, PersonLink, PersonName, PersonTimeSave)
                    VALUES (@PersonID, @PersonLink, @PersonName, datetime('now'));
                ";
                        cmd.Parameters.AddWithValue("@PersonID", personId);
                        cmd.Parameters.AddWithValue("@PersonLink", personLink ?? "");
                        cmd.Parameters.AddWithValue("@PersonName", personName ?? "");
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ InsertOrIgnorePerson error: " + ex.Message);
            }
        }
        //-----------------Page------------------------
        public List<PageInfo> GetAllPages()
        {
            var list = new List<PageInfo>();
            try
            {
                using (var conn = SqliteHelper.Instance.GetConnection("MainDatabase.db"))
                {
                    conn.Open();
                    string sql = @"
                SELECT PageID, IDFBPage, PageLink, PageName, PageType, PageMembers, 
                       PageInteraction, PageEvaluation,PageInfoText, PageTimeSave
                FROM TablePageInfo
                ORDER BY PageTimeSave DESC;
            ";

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = sql;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new PageInfo
                                {
                                    PageID = reader.GetStringOrNull("PageID"),
                                    IDFBPage = reader.GetStringOrNull("IDFBPage"),
                                    PageLink = reader.GetStringOrNull("PageLink"),
                                    PageName = reader.GetStringOrNull("PageName"),
                                    // 🔥 enum
                                    PageType = reader.GetEnum("PageType", FBType.Unknown),
                                    PageMembers = reader.GetStringOrNull("PageMembers"),
                                    PageInteraction = reader.GetStringOrNull("PageInteraction"),
                                    PageEvaluation = reader.GetStringOrNull("PageEvaluation"),
                                    PageInfoText = reader.GetStringOrNull("PageInfoText"),
                                    // ⚠️ tạm để string nếu chưa đổi model
                                    PageTimeSave = reader.GetStringOrNull("PageTimeSave")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ GetAllPages error: " + ex.Message);
            }
            return list;
        }
        public DataTable GetAllPageInfo()
        {
            var dt = new DataTable();

            try
            {
                string dbPath = PathHelper.Instance.GetMainDatabasePath();

                using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
                {
                    conn.Open();

                    string sql = @"
                SELECT 
                    PageID,
                    PageName,
                    PageLink,
                    PageType,
                    PageMembers,
                    PageInteraction,
                    PageEvaluation,
                    PageInfoText,
                    IsScanned,
                    PageTimeSave
                FROM TablePageInfo
                ORDER BY datetime(PageTimeSave) DESC;
            ";

                    using (var da = new SQLiteDataAdapter(sql, conn))
                    {
                        da.Fill(dt);
                    }
                }

                // Thêm STT
                dt.Columns.Add("STT", typeof(int));
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["STT"] = i + 1;

                dt.Columns["STT"].SetOrdinal(0);
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"❌ GetAllPageInfo ERROR: {ex.Message}");
            }

            return dt;
        }
        public DataTable GetPages(int limit = 0)
        {
            var dt = new DataTable();
            try
            {
                using (var conn = SqliteHelper.Instance.GetConnection("MainDatabase.db"))
                {
                    conn.Open();

                    string sql = @"
SELECT 
    PageName AS 'Tên Page',
    PageLink AS 'Link Page',
    PageType AS 'Loại',
    PageTimeSave AS 'Thời gian lưu'
FROM TablePageInfo
ORDER BY PageTimeSave DESC
";

                    if (limit > 0)
                        sql += $" LIMIT {limit}";

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        using (var reader = cmd.ExecuteReader())
                            dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("❌ GetPages error: " + ex.Message);
            }

            return dt;
        }
        public PageInfo GetPageInfoByID(string pageID)
        {
            string dbPath = PathHelper.Instance.GetMainDatabasePath();

            using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
            {
                conn.Open();

                string sql = @"SELECT PageID, IDFBPage, PageLink, PageName, PageType,
                              PageMembers, PageInteraction, PageEvaluation,
                              PageInfoText, PageTimeSave
                       FROM TablePageInfo
                       WHERE PageID = @id";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", pageID);

                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            return new PageInfo
                            {
                                PageID = rd.GetStringOrNull("PageID"),
                                IDFBPage = rd.GetStringOrNull("IDFBPage"),
                                PageLink = rd.GetStringOrNull("PageLink"),
                                PageName = rd.GetStringOrNull("PageName"),
                                PageType = rd.GetEnum("PageType", FBType.Unknown),
                                PageMembers = rd.GetStringOrNull("PageMembers"),
                                PageInteraction = rd.GetStringOrNull("PageInteraction"),
                                PageEvaluation = rd.GetStringOrNull("PageEvaluation"),
                                PageInfoText = rd.GetStringOrNull("PageInfoText"),
                                PageTimeSave = rd.GetStringOrNull("PageTimeSave")
                            };
                        }
                    }
                }
            }

            return null;
        }
        public string GetPageNoteDetail(string pageId)
        {
            try
            {
                using (var conn = SqliteHelper.Instance.GetConnection("MainDatabase.db"))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                    SELECT PageInfoText
                    FROM TablePageInfo 
                    WHERE PageID = @id LIMIT 1;
                ";
                        cmd.Parameters.AddWithValue("@id", pageId);

                        return cmd.ExecuteScalar()?.ToString() ?? "{}";
                    }
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"❌ Lỗi GetPageNoteDetail: {ex.Message}");
                return "{}";
            }
        }
        public void UpdateIsScanned(string pageId, int value)
        {
            string dbPath = PathHelper.Instance.GetMainDatabasePath();

            using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
            {
                conn.Open();

                string sql = "UPDATE TablePageInfo SET IsScanned=@v WHERE PageID=@pid";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@v", value);
                    cmd.Parameters.AddWithValue("@pid", pageId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public int GetIsScanned(string pageId)
        {
            string dbPath = PathHelper.Instance.GetMainDatabasePath();

            using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
            {
                conn.Open();

                string sql = "SELECT IsScanned FROM TablePageInfo WHERE PageID=@pid LIMIT 1";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@pid", pageId);

                    var v = cmd.ExecuteScalar();
                    if (v == null || v == DBNull.Value)
                        return 0;

                    return Convert.ToInt32(v);
                }
            }
        }

        //==pagenote
        public class PageNoteItem
        {
            public string PageID { get; set; }
            public string TimeSave { get; set; }
        }
        public void AddPageToNote(string pageID)
        {
            try
            {
                string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Page", "page_note.json");

                List<string> list = new List<string>();

                // Nếu file tồn tại → đọc
                if (File.Exists(file))
                {
                    var json = File.ReadAllText(file);
                    list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
                }

                // Thêm nếu chưa có
                if (!list.Contains(pageID))
                    list.Add(pageID);

                // Lưu lại
                File.WriteAllText(file, Newtonsoft.Json.JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.Indented));

                Libary.Instance.CreateLog($"[PageNote JSON] ✔ Đã thêm page {pageID}");
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"❌ AddPageToNote ERROR: {ex.Message}");
            }
        }
        public void RemovePageFromNote(string pageID)
        {
            try
            {
                string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Page", "page_note.json");

                if (!File.Exists(file)) return;

                var json = File.ReadAllText(file);
                var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();

                if (list.Contains(pageID))
                    list.Remove(pageID);

                File.WriteAllText(file, Newtonsoft.Json.JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.Indented));

                Libary.Instance.CreateLog($"[PageNote JSON] ✔ Đã xóa page {pageID}");
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"❌ RemovePageFromNote ERROR: {ex.Message}");
            }
        }
        public List<(PageInfo Info, string TimeSave)> GetAllPageNote()
        {
            var result = new List<(PageInfo, string)>();

            try
            {
                string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Page", "page_note.json");

                if (!File.Exists(file))
                    return result;

                var json = File.ReadAllText(file);
                var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PageNoteItem>>(json)
                           ?? new List<PageNoteItem>();

                foreach (var item in list)
                {
                    var info = GetPageByID(item.PageID);
                    if (info != null)
                        result.Add((info, item.TimeSave));
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"❌ GetAllPageNote JSON ERROR: {ex.Message}");
            }

            return result;
        }
        public List<(PageInfo Info, string TimeSave)> GetAllPageNote2()
        {
            var result = new List<(PageInfo, string)>();

            try
            {
                string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Page", "page_note.json");

                if (!File.Exists(file))
                    return result;

                var json = File.ReadAllText(file);

                // JSON hiện tại: ["pageID1", "pageID2", ...]
                var pageIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(json)
                               ?? new List<string>();

                foreach (var pid in pageIds)
                {
                    var info = GetPageByID(pid);
                    if (info != null)
                    {
                        // TimeSave = lấy thẳng PageTimeSave trong DB
                        result.Add((info, info.PageTimeSave));
                    }
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"❌ GetAllPageNote JSON ERROR: {ex.Message}");
            }

            return result;
        }
        public void InsertPageNote(string pageID)
        {
            try
            {
                string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Page", "page_note.json");

                List<PageNoteItem> list = new List<PageNoteItem>();

                if (File.Exists(file))
                {
                    var json = File.ReadAllText(file);
                    list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PageNoteItem>>(json)
                           ?? new List<PageNoteItem>();
                }

                // Không thêm trùng
                if (!list.Any(p => p.PageID == pageID))
                {
                    list.Add(new PageNoteItem
                    {
                        PageID = pageID,
                        TimeSave = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }

                File.WriteAllText(file, Newtonsoft.Json.JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.Indented));

                Libary.Instance.CreateLog($"[PageNote JSON] ✔ Đã thêm page {pageID}");
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"❌ InsertPageNote ERROR: {ex.Message}");
            }
        }
        public void DeletePageNote(string pageID)
        {
            string dbPath = PathHelper.Instance.GetProfileDatabasePath();
            using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
            {
                conn.Open();
                string sql = "DELETE FROM TablePageNote WHERE PageID=@id";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", pageID);
                    cmd.ExecuteNonQuery();
                }
            }
        }    
        //----------------------person
        public List<PersonInfo> GetAllPersons()
        {
            var list = new List<PersonInfo>();
            try
            {
                using (var conn = SqliteHelper.Instance.GetConnection("MainDatabase.db"))
                {
                    conn.Open();
                    string sql = @"
                SELECT PersonID, IDFBPerson, PersonLink, PersonName, 
                       PersonInfo, PersonNote, PersonTimeSave
                FROM TablePersonInfo
                ORDER BY PersonTimeSave DESC;
            ";

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new PersonInfo
                                {
                                    PersonID = reader.GetStringOrNull("PersonID"),
                                    IDFBPerson = reader.GetStringOrNull("IDFBPerson"),
                                    PersonLink = reader.GetStringOrNull("PersonLink"),
                                    PersonName = reader.GetStringOrNull("PersonName"),
                                    PersonInfoText = reader.GetStringOrNull("PersonInfo"),
                                    PersonNote = reader.GetEnum("PersonNote", FBType.Unknown),
                                    PersonTimeSave = reader.GetStringOrNull("PersonTimeSave")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ GetAllPersons error: " + ex.Message);
            }
            return list;
        }
        public DataTable GetPersons(int limit = 0)
        {
            var dt = new DataTable();
            try
            {
                using (var conn = SqliteHelper.Instance.GetConnection("MainDatabase.db"))
                {
                    conn.Open();

                    string sql = @"
SELECT
    PersonName AS 'Tên người',
    PersonLink AS 'Link Facebook',
    PersonNote AS 'Phân loại',
    PersonTimeSave AS 'Thời gian lưu'
FROM TablePersonInfo
ORDER BY PersonTimeSave DESC
";

                    if (limit > 0)
                        sql += $" LIMIT {limit}";

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        using (var reader = cmd.ExecuteReader())
                            dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("❌ GetPersons error: " + ex.Message);
            }

            return dt;
        }

        //-------hàm đếm số bài dùng lấy cho lần đầu trong DB Temp
        public int CountPostsInTempDb(string tempDbPath)
        {
            try
            {
                using (var conn = SqliteHelper.Instance.GetConnection(tempDbPath))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM TablePostInfo;";
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch
            {
                return 0;
            }
        }
        // lấy pageid từ temp
        public string GetPageIdFromTempDb(string tempDbPath)
        {
            try
            {
                using (var conn = SqliteHelper.Instance.GetConnection(tempDbPath))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT PageID FROM TablePageInfo LIMIT 1;";
                        var result = cmd.ExecuteScalar();
                        return result?.ToString() ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"❌ Lỗi GetPageIdFromTempDb: {ex.Message}");
                return "";
            }
        }

        // -------------------------
        // Merge tất cả DB tạm vào MainDatabase
        // -------------------------
        public void MergeTempDbFile(string tempDbPath)
        {
            string mainDb = PathHelper.Instance.GetMainDatabasePath();

            try
            {
                using (var connMain = SqliteHelper.Instance.GetConnection(mainDb))
                {
                    connMain.Open();

                    // ⭐ Bao toàn bộ merge trong transaction
                    using (var tran = connMain.BeginTransaction())
                    {
                        using (var cmd = connMain.CreateCommand())
                        {
                            // ATTACH temp DB
                            cmd.CommandText = $"ATTACH DATABASE '{tempDbPath.Replace("'", "''")}' AS tempDB;";
                            cmd.ExecuteNonQuery();

                            // MERGE TABLES
                            cmd.CommandText = @"
                        INSERT OR IGNORE INTO TablePost
                        SELECT * FROM tempDB.TablePost;

                        INSERT OR IGNORE INTO TablePostInfo
                        SELECT * FROM tempDB.TablePostInfo;

                        INSERT OR IGNORE INTO TablePageInfo
                        SELECT * FROM tempDB.TablePageInfo;

                        INSERT OR IGNORE INTO TablePersonInfo
                        SELECT * FROM tempDB.TablePersonInfo;

                        INSERT OR IGNORE INTO TablePostShare
                        SELECT * FROM tempDB.TablePostShare;

                        INSERT OR IGNORE INTO TableCommentInfo
                        SELECT * FROM tempDB.TableCommentInfo;

                        INSERT OR IGNORE INTO TablePostComment
                        SELECT * FROM tempDB.TablePostComment;
                    ";
                            cmd.ExecuteNonQuery();

                            // DETACH temp
                            cmd.CommandText = "DETACH DATABASE tempDB;";
                            cmd.ExecuteNonQuery();
                        }

                        // COMMIT TRANSACTION
                        tran.Commit();
                    }

                    // ⭐ FLUSH WAL – đảm bảo unlock main DB
                    using (var chk = new SQLiteCommand("PRAGMA wal_checkpoint(FULL);", connMain))
                        chk.ExecuteNonQuery();
                }

                // ⭐ Đảm bảo mọi handle SQLite đã giải phóng
                GC.Collect();
                GC.WaitForPendingFinalizers();

                Libary.Instance.CreateLog($"[Merge] OK → {tempDbPath}");
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"❌ Lỗi MergeTempDbFile: {ex.Message}");
            }
        }
        public void MergeTempDbFile1(string tempDbPath)
        {
            string mainDb = PathHelper.Instance.GetMainDatabasePath();

            try
            {
                using (var connMain = SqliteHelper.Instance.GetConnection(mainDb))
                {
                    connMain.Open();

                    using (var tran = connMain.BeginTransaction())
                    using (var cmd = connMain.CreateCommand())
                    {
                        // 1) ATTACH temp DB
                        cmd.CommandText =
                            $"ATTACH DATABASE '{tempDbPath.Replace("'", "''")}' AS tempDB;";
                        cmd.ExecuteNonQuery();

                        // 2) DS bảng có thể tồn tại trong tempDB
                        string[] tables =
                        {
                    "TablePost",
                    "TablePostInfo",
                    "TablePageInfo",
                    "TablePersonInfo",
                    "TablePostShare",
                    "TableCommentInfo",
                    "TablePostComment"
                };

                        foreach (var table in tables)
                        {
                            // Check bảng có tồn tại trong tempDB hay không
                            cmd.CommandText = $@"
                        SELECT name 
                        FROM tempDB.sqlite_master 
                        WHERE type='table' AND name='{table}';
                    ";

                            var exists = cmd.ExecuteScalar();
                            if (exists != null)
                            {
                                // Merge bảng
                                cmd.CommandText =
                                    $"INSERT OR IGNORE INTO {table} SELECT * FROM tempDB.{table};";
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 3) DETACH temp DB
                        cmd.CommandText = "DETACH DATABASE tempDB;";
                        cmd.ExecuteNonQuery();

                        // 4) Commit
                        tran.Commit();
                    }

                    // 5) Flush WAL (cho chắc ăn)
                    using (var chk = new SQLiteCommand("PRAGMA wal_checkpoint(FULL);", connMain))
                        chk.ExecuteNonQuery();
                }

                // 6) Giải phóng handle .NET trước khi xoá file
                GC.Collect();
                GC.WaitForPendingFinalizers();

                Libary.Instance.CreateLog($"[Merge] OK → {tempDbPath}");
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"❌ Lỗi MergeTempDbFile: {ex.Message}");
            }
        }

        // -------------------------
        // tạm lấy url thế này sửa sau
        public List<string> GetAllPageLinksFromPageNoteDbs()
        {
            var list = new List<string>();

            try
            {
                // ✅ Xác định đường dẫn thư mục PageNote
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string folderPath = Path.Combine(baseDir, "Data", "Page", "PageNote");

                // ✅ Nếu chưa có folder → tạo mới
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    Libary.Instance.CreateLog($"[PageNote] 📁 Tạo mới thư mục {folderPath}");
                }

                // ✅ Lấy tất cả file PageNote_*.db
                var dbFiles = Directory.GetFiles(folderPath, "PageNote_*.db", SearchOption.TopDirectoryOnly);

                // ✅ Nếu chưa có file nào → tạo file mặc định
                if (dbFiles.Length == 0)
                {
                    string defaultDb = Path.Combine(folderPath, "PageNote_Default.db");
                    CreateDatabase(defaultDb); // tái sử dụng hàm tạo DB sẵn có
                    Libary.Instance.CreateLog($"[PageNote] 🆕 Tạo file DB mặc định: {defaultDb}");
                    dbFiles = new[] { defaultDb };
                }

                // ✅ Duyệt từng DB để lấy danh sách PageLink
                foreach (var dbPath in dbFiles)
                {
                    try
                    {
                        using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
                        {
                            conn.Open();
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "SELECT DISTINCT PageLink FROM TablePageInfo WHERE PageLink IS NOT NULL AND PageLink <> '';";
                                using (var reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        string link = reader["PageLink"]?.ToString()?.Trim();
                                        if (!string.IsNullOrEmpty(link))
                                            list.Add(link);
                                    }
                                }
                            }
                        }
                        Libary.Instance.CreateLog($"[PageNote] ✅ Đọc thành công {Path.GetFileName(dbPath)} ({list.Count} link)");
                    }
                    catch (Exception ex)
                    {
                        Libary.Instance.CreateLog($"[PageNote] ⚠️ Lỗi đọc {dbPath}: {ex.Message}");
                    }
                }

                // ✅ Lọc trùng
                list = list.Distinct().ToList();
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"❌ GetAllPageLinksFromPageNoteDbs error: {ex.Message}");
            }

            return list;
        }
        //------------------------các hàm MONITOR------------
        //--------------------- các hàm thêm/INSERT CÁC BẢNG
        public void InsertOrUpdatePageMonitor(string pageId, bool isAuto, int postsCount = 0)
        {
            using (var conn = SqliteHelper.Instance.GetConnection("MainDatabase.db"))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                INSERT INTO TablePageMonitor
                    (PageID, IsAuto, Status, TotalPostsScanned, FirstScanTime, LastScanTime, TimeSave)
                VALUES
                    (@id, @isAuto, 'Chưa auto', @count, datetime('now'), datetime('now'), datetime('now'))

                ON CONFLICT(PageID) DO UPDATE SET
                    IsAuto = @isAuto,
                    TotalPostsScanned = TotalPostsScanned + @count,
                    LastScanTime = datetime('now'),
                    TimeSave = datetime('now');
                ";

                    cmd.Parameters.AddWithValue("@id", pageId);
                    cmd.Parameters.AddWithValue("@isAuto", isAuto ? 1 : 0);
                    cmd.Parameters.AddWithValue("@count", postsCount);

                    cmd.ExecuteNonQuery();
                }
            }
        }
        public void InsertOrIgnorePageInfo(PageInfo page)
        {
            try
            {
                string dbPath = PathHelper.Instance.GetMainDatabasePath();

                using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
                {
                    conn.Open();

                    string sql = @"
            INSERT OR IGNORE INTO TablePageInfo
            (PageID, IDFBPage, PageLink, PageName, PageType, PageMembers, PageInteraction, PageEvaluation, PageInfoText, PageTimeSave)
            VALUES
            (@id, @fbid, @link, @name, @type, @members, @interaction, @eval, @info, @timesave);";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", (object)page.PageID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@fbid", (object)page.IDFBPage ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@link", (object)page.PageLink ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@name", (object)page.PageName ?? DBNull.Value);

                        // enum -> string (DB đang là string)
                        cmd.Parameters.AddWithValue("@type", page.PageType.ToString());

                        cmd.Parameters.AddWithValue("@members", (object)page.PageMembers ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@interaction", (object)page.PageInteraction ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@eval", (object)page.PageEvaluation ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@info", (object)page.PageInfoText ?? DBNull.Value);

                        cmd.Parameters.AddWithValue("@timesave",
                            (object)page.PageTimeSave ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("❌ InsertOrIgnorePageInfo ERROR: " + ex.Message);
            }
        }
        public void InsertPageMonitor(string pageID)
        {
            try
            {
                string dbPath = PathHelper.Instance.GetMainDatabasePath();

                using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
                {
                    conn.Open();

                    // Lấy thông tin page từ TablePageInfo
                    PageInfo info = GetPageByID(pageID);
                    if (info == null)
                    {
                        Libary.Instance.CreateLog($"[InsertPageMonitor] ❌ PageID '{pageID}' không tồn tại trong TablePageInfo");
                        return;
                    }

                    // Tạo monitor record đầy đủ
                    string sql = @"
                INSERT OR IGNORE INTO TablePageMonitor
                    (PageID, IsAuto, Status, FirstScanTime, LastScanTime, TotalPostsScanned, TimeSave)
                VALUES
                    (@id, 0, 'Chưa auto',
                     @now, @now,
                     0, @now);

                -- Nếu bản ghi đã tồn tại, cập nhật TimeSave = now
                UPDATE TablePageMonitor
                SET TimeSave = @now
                WHERE PageID = @id;
            ";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        cmd.Parameters.AddWithValue("@id", pageID);
                        cmd.Parameters.AddWithValue("@now", now);

                        cmd.ExecuteNonQuery();
                    }

                    Libary.Instance.CreateLog($"[InsertPageMonitor] ✔ Đã thêm/refresh monitor cho page: {info.PageName}");
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("❌ InsertPageMonitor ERROR: " + ex.Message);
            }
        }
        public void DeletePageMonitor(string pageID)
        {
            try
            {
                string dbPath = PathHelper.Instance.GetMainDatabasePath();

                using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
                {
                    conn.Open();

                    string sql = @"DELETE FROM TablePageMonitor WHERE PageID=@id";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", pageID);
                        cmd.ExecuteNonQuery();
                    }
                }

                Libary.Instance.CreateLog($"[DeletePageMonitor] ✔ Xóa monitor cho PageID: {pageID}");
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog($"❌ DeletePageMonitor ERROR: {ex.Message}");
            }
        }
        public DataTable GetMonitoredPages()
        {
            var dt = new DataTable();
            string dbPath = PathHelper.Instance.GetMainDatabasePath();
            Libary.Instance.CreateLog("📌 [Monitor] DB PATH = " + dbPath);
            using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                SELECT 
                    m.PageID,
                    p.PageName,
                    p.PageLink,
                    m.IsAuto,
                    m.Status,
                    m.TotalPostsScanned,
                    m.FirstScanTime,
                    m.LastScanTime
                FROM TablePageMonitor m
                LEFT JOIN TablePageInfo p
                    ON p.PageID = m.PageID
                ORDER BY m.TimeSave DESC;
            ";

                    using (var reader = cmd.ExecuteReader())
                        dt.Load(reader);
                }
            }
            return dt;
        }
        public void UpdateMonitorStatus(string pageId, string status)
        {
            try
            {
                using (var conn = SqliteHelper.Instance.GetConnection("MainDatabase.db"))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                    UPDATE TablePageMonitor
                    SET Status = @st, LastScanTime = datetime('now')
                    WHERE PageID = @id;
                ";
                        cmd.Parameters.AddWithValue("@st", status);
                        cmd.Parameters.AddWithValue("@id", pageId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("❌ UpdateMonitorStatus: " + ex.Message);
            }
        }
        //-------------------profike
        public void CreateDatabase2(string dbPath)
        {
            try
            {
                using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                PRAGMA foreign_keys = ON;
                PRAGMA journal_mode = WAL;
                PRAGMA synchronous = NORMAL;

                --------------------------------------------------------
                -- TABLE PROFILE INFO
                --------------------------------------------------------
                CREATE TABLE IF NOT EXISTS TableProfileInfo (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    IDAdbrowser TEXT,
                    ProfileName TEXT,
                    ProfileLink TEXT,
                    ProfileStatus TEXT DEFAULT 'Die',
                    UseTab INTEGER DEFAULT 0,
                    ProfileType TEXT DEFAULT 'Person'
                );

                --------------------------------------------------------
                -- TABLE MANAGER PROFILE (mapping Profile <-> Page)
                --------------------------------------------------------
                CREATE TABLE IF NOT EXISTS TableManagerProfile (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProfileID INTEGER NOT NULL,
                    PageID TEXT NOT NULL,
                    TimeSave TEXT DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY(ProfileID) REFERENCES TableProfileInfo(ID)
                );
                --------------------------------------------------------
                -- TABLE PAGE NOTE (ghi chú page đã scan)
                --------------------------------------------------------
                CREATE TABLE IF NOT EXISTS TablePageNote (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    PageID TEXT NOT NULL,
                    TimeSave TEXT DEFAULT CURRENT_TIMESTAMP
                );
                ";

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ CreateDatabase2 error: " + ex.Message);
            }
        }

        //---các hàm GET LẺ
        public PageInfo GetPageByID(string pageID)
        {
            try
            {
                string dbPath = PathHelper.Instance.GetMainDatabasePath();

                using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
                {
                    conn.Open();

                    string sql = @"
                SELECT PageID, IDFBPage, PageLink, PageName, PageType,
                       PageMembers, PageInteraction, PageEvaluation,
                       PageInfoText, PageTimeSave, IsScanned
                FROM TablePageInfo
                WHERE PageID=@id
                LIMIT 1;
            ";

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        cmd.Parameters.AddWithValue("@id", pageID);

                        using (var da = new SQLiteDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count == 0)
                                return null;

                            var r = dt.Rows[0];

                            return new PageInfo
                            {
                                PageID = r["PageID"] == DBNull.Value ? null : r["PageID"].ToString(),
                                IDFBPage = r["IDFBPage"] == DBNull.Value ? null : r["IDFBPage"].ToString(),
                                PageLink = r["PageLink"] == DBNull.Value ? null : r["PageLink"].ToString(),
                                PageName = r["PageName"] == DBNull.Value ? null : r["PageName"].ToString(),

                                // 🔥 enum
                                PageType = r["PageType"] == DBNull.Value
                                    ? FBType.Unknown
                                    : Enum.TryParse<FBType>(r["PageType"].ToString(), true, out var t)
                                        ? t
                                        : FBType.Unknown,

                                PageMembers = r["PageMembers"] == DBNull.Value ? null : r["PageMembers"].ToString(),
                                PageInteraction = r["PageInteraction"] == DBNull.Value ? null : r["PageInteraction"].ToString(),
                                PageEvaluation = r["PageEvaluation"] == DBNull.Value ? null : r["PageEvaluation"].ToString(),
                                PageInfoText = r["PageInfoText"] == DBNull.Value ? null : r["PageInfoText"].ToString(),
                                PageTimeSave = r["PageTimeSave"] == DBNull.Value ? null : r["PageTimeSave"].ToString(),

                                IsScanned = r.Table.Columns.Contains("IsScanned") &&
                                            r["IsScanned"] != DBNull.Value &&
                                            Convert.ToInt32(r["IsScanned"]) == 1
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("[GetPageByID] ERROR: " + ex.Message);
                return null;
            }
        }
        public bool ExistPostByLink(string postLink)
        {
            string dbPath = PathHelper.Instance.GetMainDatabasePath();
            using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
            {
                conn.Open();
                string sql = "SELECT 1 FROM TablePostInfo WHERE PostLink = @link LIMIT 1";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@link", postLink);
                    var result = cmd.ExecuteScalar();
                    return result != null;
                }
            }
        }
        public DateTime GetPageLastScan(string pageId)
        {
            try
            {
                string dbPath = PathHelper.Instance.GetMainDatabasePath();

                using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
                {
                    conn.Open();
                    string sql = @"SELECT LastScanTime 
                           FROM TablePageMonitor 
                           WHERE PageID = @id";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", pageId);
                        var raw = cmd.ExecuteScalar();

                        if (raw == null || raw == DBNull.Value)
                            return DateTime.MinValue;

                        if (DateTime.TryParse(raw.ToString(), out var dt))
                            return dt;

                        return DateTime.MinValue;
                    }
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("[GetPageLastScan] ERROR: " + ex.Message);
                return DateTime.MinValue;
            }
        }
        public void UpdatePageLastScan(string pageId)
        {
            try
            {
                string dbPath = PathHelper.Instance.GetMainDatabasePath();

                using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
                {
                    conn.Open();
                    string sql = @"UPDATE TablePageMonitor 
                           SET LastScanTime = @t 
                           WHERE PageID = @id";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@t", DateTime.Now);
                        cmd.Parameters.AddWithValue("@id", pageId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("[UpdatePageLastScan] ERROR: " + ex.Message);
            }
        }
        public PageMonitorRow GetMonitorRow(string pageId)
        {
            try
            {
                string dbPath = PathHelper.Instance.GetMainDatabasePath();
                using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
                {
                    conn.Open();
                    string sql = @"SELECT PageID, Status, FirstScanTime, LastScanTime, TotalPostsScanned
                           FROM TablePageMonitor
                           WHERE PageID = @id";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", pageId);

                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                return new PageMonitorRow
                                {
                                    PageID = rd["PageID"].ToString(),
                                    Status = rd["Status"].ToString(),
                                    FirstScanTime = rd["FirstScanTime"]?.ToString(),
                                    LastScanTime = rd["LastScanTime"]?.ToString(),
                                    TotalPostsScanned = rd["TotalPostsScanned"] != DBNull.Value
                                        ? Convert.ToInt32(rd["TotalPostsScanned"])
                                        : 0
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Libary.Instance.CreateLog("[GetMonitorRow] ERROR: " + ex.Message);
            }

            return null;
        }
        public void ResetAllPageMonitorAuto()
        {
            string sql = "UPDATE TablePageMonitor SET IsAuto = 0";
            SQLDAO.Instance.ExecuteNonQuery(sql);
        }

        public void UpdateMonitorIsAuto(string pageId, int isAuto)
        {
            string dbPath = PathHelper.Instance.GetMainDatabasePath();
            using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
            {
                conn.Open();
                string sql = "UPDATE TablePageMonitor SET IsAuto=@auto WHERE PageID=@id";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@auto", isAuto);
                    cmd.Parameters.AddWithValue("@id", pageId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        ///--Đếm số bài viết theo PageID
       
        public class PageStats
        {
            public int TotalPosts { get; set; }
            public int TotalLikes { get; set; }
            public int TotalComments { get; set; }
            public int TotalShares { get; set; }
            public int Followers { get; set; }
        }
        public PageStats GetPageStats(string pageID)
        {
            string db = PathHelper.Instance.GetMainDatabasePath();
            var stats = new PageStats();

            // 1) Followers
            stats.Followers = ExecuteScalarSafe<int>(db,
                "SELECT PageMembers FROM TablePageInfo WHERE PageID=@id",
                new SQLiteParameter("@id", pageID));

            // 2) Post count
            stats.TotalPosts = ExecuteScalarSafe<int>(db,
                "SELECT COUNT(*) FROM TablePost WHERE PageIDContainer=@pid",
                new SQLiteParameter("@pid", pageID));

            // 3) Like + Share + Comment
            var dt = ExecuteDataTableSafe(db,
                @"SELECT 
            SUM(LikeCount) AS Likes,
            SUM(CommentCount) AS Comments,
            SUM(ShareCount) AS Shares
          FROM TablePostInfo
          WHERE PostID IN (
                SELECT PostID FROM TablePost WHERE PageIDContainer=@pid
          )",
                new SQLiteParameter("@pid", pageID));

            if (dt.Rows.Count > 0)
            {
                var r = dt.Rows[0];
                stats.TotalLikes = r["Likes"] == DBNull.Value ? 0 : Convert.ToInt32(r["Likes"]);
                stats.TotalComments = r["Comments"] == DBNull.Value ? 0 : Convert.ToInt32(r["Comments"]);
                stats.TotalShares = r["Shares"] == DBNull.Value ? 0 : Convert.ToInt32(r["Shares"]);
            }

            return stats;
        }

        //-----------các hàm đảm bảo k khóa DB
        public T ExecuteScalarSafe<T>(string dbPath, string sql, params SQLiteParameter[] ps)
        {
            using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    if (ps != null)
                        cmd.Parameters.AddRange(ps);

                    object v = cmd.ExecuteScalar();
                    if (v == null || v == DBNull.Value)
                        return default(T);

                    return (T)Convert.ChangeType(v, typeof(T));
                }
            }
        }
        public DataTable ExecuteDataTableSafe(string dbPath, string sql, params SQLiteParameter[] ps)
        {
            var dt = new DataTable();

            using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    if (ps != null)
                        cmd.Parameters.AddRange(ps);

                    using (var da = new SQLiteDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            return dt;
        }
        public void ExecuteNonQuerySafe(string dbPath, string sql, params SQLiteParameter[] ps)
        {
            using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    if (ps != null)
                        cmd.Parameters.AddRange(ps);

                    cmd.ExecuteNonQuery();
                }
            }
        }


    }

}
