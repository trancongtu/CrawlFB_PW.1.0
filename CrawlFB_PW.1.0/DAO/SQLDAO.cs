using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using CrawlFB_PW._1._0.DTO;
using System.Security.Cryptography;
using System.Text;
using CrawlFB_PW._1._0;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.Helper;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static CrawlFB_PW._1._0.DAO.DatabaseDAO;
using System.Data.SQLite;
using static CrawlFB_PW._1._0.UC.UCPageDB;
using DevExpress.Utils;
using CrawlFB_PW._1._0.Enums;
//using DevExpress.XtraPrinting;

public class SQLDAO
{
    private static SQLDAO instance;
    public static SQLDAO Instance
    {
        get
        {
            if (instance == null)
                instance = new SQLDAO();
            return instance;
        }
    }

    private string connectionString;

    private SQLDAO()
    {
        // 👉 Sửa chuỗi này nếu đổi server
        connectionString = @"Server=DESKTOP-GRC118H\SQLEXPRESS;Database=AutoScanDB;Trusted_Connection=True;";
    }

    // ======================================================
    //  HÀM CƠ BẢN: GetConnection (chưa open) + OpenConnection
    // ======================================================

    /// <summary>
    /// Trả về SqlConnection (CHƯA mở) — để caller chủ động Open().
    /// </summary>
    public SqlConnection GetConnection()
    {
        return new SqlConnection(connectionString);
    }

    /// <summary>
    /// Trả về SqlConnection đã Open() — dùng nhanh cho query.
    /// </summary>
    public SqlConnection OpenConnection()
    {
        var conn = new SqlConnection(connectionString);
        conn.Open();
        return conn; // caller phải Dispose !
    }

    // ======================================================
    //  HÀM TEST KẾT NỐI
    // ======================================================
    public bool TestConnection()
    {
        try
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    // ======================================================
    //  HÀM QUERY CHUNG
    // ======================================================
    public DataTable ExecuteQuery(string query, Dictionary<string, object> parameters = null)
    {
        DataTable dt = new DataTable();

        using (var conn = OpenConnection())
        using (var cmd = new SqlCommand(query, conn))
        {
            AddParams(cmd, parameters);
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                adapter.Fill(dt);
            }
        }

        return dt;
    }

    public int ExecuteNonQuery(string query, Dictionary<string, object> parameters = null)
    {
        using (var conn = OpenConnection())
        using (var cmd = new SqlCommand(query, conn))
        {
            AddParams(cmd, parameters);
            return cmd.ExecuteNonQuery();
        }
    }
    public object ExecuteScalar(string query, Dictionary<string, object> parameters = null)
    {
        using (var conn = OpenConnection())
        using (var cmd = new SqlCommand(query, conn))
        {
            AddParams(cmd, parameters);
            return cmd.ExecuteScalar();
        }
    }

    // → Query list of objects
    public List<T> QueryList<T>(string query, Func<IDataReader, T> parser, Dictionary<string, object> parameters = null)
    {
        List<T> list = new List<T>();

        using (var conn = OpenConnection())
        using (var cmd = new SqlCommand(query, conn))
        {
            AddParams(cmd, parameters);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(parser(reader));
                }
            }
        }

        return list;
    }

    // ======================================================
    //  HÀM THÊM PARAMETER
    // ======================================================
    private void AddParams(SqlCommand cmd, Dictionary<string, object> parameters)
    {
        if (parameters == null) return;

        foreach (var p in parameters)
        {
            cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
        }
    }
    //===============
    // HÀM TẠO TABLE
    public void CreateTableAuto()
    {
        try
        {
            using (var conn = SQLDAO.Instance.OpenConnection())
            {
                string sql = @"
            --------------------------------------------------------
            -- TABLE PAGE INFO
            --------------------------------------------------------
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TablePageInfo')
            BEGIN
                CREATE TABLE TablePageInfo (
                    PageID NVARCHAR(200) PRIMARY KEY,
                    IDFBPage NVARCHAR(200) DEFAULT 'N/A',
                    PageLink NVARCHAR(MAX),
                    PageName NVARCHAR(500),
                    PageType NVARCHAR(200),
                    PageMembers NVARCHAR(200) DEFAULT 'N/A',
                    PageInteraction NVARCHAR(200) DEFAULT 'N/A',
                    PageEvaluation NVARCHAR(200) DEFAULT 'N/A',
                    PageInfoText NVARCHAR(MAX) DEFAULT 'N/A',
                    IsScanned INT DEFAULT 0,
                    TimeLastPost DATETIME2(0) NULL,
                    PageTimeSave DATETIME2(0) DEFAULT SYSDATETIME()
                );
            END

            --------------------------------------------------------
            -- TABLE PERSON INFO
            --------------------------------------------------------
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TablePersonInfo')
            BEGIN
                CREATE TABLE TablePersonInfo (
                    PersonID NVARCHAR(200) PRIMARY KEY,
                    IDFBPerson NVARCHAR(200) DEFAULT 'N/A',
                    PersonLink NVARCHAR(MAX),
                    PersonName NVARCHAR(500),
                    PersonInfo NVARCHAR(MAX) DEFAULT 'N/A',
                    PersonNote NVARCHAR(MAX) DEFAULT 'N/A',
                    PersonTimeSave DATETIME2(0) DEFAULT SYSDATETIME()
                );
            END

            --------------------------------------------------------
            -- TABLE POST INFO
            --------------------------------------------------------
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TablePostInfo')
            BEGIN
                CREATE TABLE TablePostInfo (
                    PostID NVARCHAR(200) PRIMARY KEY,
                    IDFBPost NVARCHAR(200) DEFAULT 'N/A',
                    PostLink NVARCHAR(MAX),
                    PostContent NVARCHAR(MAX),

                    PostTime NVARCHAR(200),            -- RAW Facebook time
                    RealPostTime DATETIME2(0) NULL,    -- REAL datetime

                    LikeCount INT DEFAULT 0,
                    ShareCount INT DEFAULT 0,
                    CommentCount INT DEFAULT 0,
                    PostInteraction NVARCHAR(MAX) DEFAULT '',
                    PostAttachment NVARCHAR(MAX) DEFAULT 'N/A',
                    PostStatus NVARCHAR(200) DEFAULT 'N/A',

                    PostTimeSave DATETIME2(0) DEFAULT SYSDATETIME()
                );
            END

            --------------------------------------------------------
            -- TABLE POST (mapping)
            --------------------------------------------------------
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TablePost')
            BEGIN
                CREATE TABLE TablePost (
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    PostID NVARCHAR(200) NOT NULL,
                    PageIDCreate NVARCHAR(200),
                    PageIDContainer NVARCHAR(200),
                    PersonIDCreate NVARCHAR(200),

                    FOREIGN KEY(PostID) REFERENCES TablePostInfo(PostID) ON DELETE CASCADE,
                    FOREIGN KEY(PageIDCreate) REFERENCES TablePageInfo(PageID),
                    FOREIGN KEY(PageIDContainer) REFERENCES TablePageInfo(PageID),
                    FOREIGN KEY(PersonIDCreate) REFERENCES TablePersonInfo(PersonID)
                );
            END

            --------------------------------------------------------
            -- TABLE POST SHARE
            --------------------------------------------------------
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TablePostShare')
            BEGIN
                CREATE TABLE TablePostShare (
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    PostID NVARCHAR(200) NOT NULL,
                    PageID NVARCHAR(200),
                    PersonID NVARCHAR(200),

                    TimeShare NVARCHAR(200),           -- RAW
                    RealTimeShare DATETIME2(0) NULL,   -- REAL

                    FOREIGN KEY(PostID) REFERENCES TablePostInfo(PostID) ON DELETE CASCADE,
                    FOREIGN KEY(PageID) REFERENCES TablePageInfo(PageID),
                    FOREIGN KEY(PersonID) REFERENCES TablePersonInfo(PersonID)
                );
            END

            --------------------------------------------------------
            -- TABLE COMMENT INFO
            --------------------------------------------------------
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TableCommentInfo')
            BEGIN
                CREATE TABLE TableCommentInfo (
                    CommentID NVARCHAR(200) PRIMARY KEY,
                    PersonID NVARCHAR(200),
                    PageID NVARCHAR(200),
                    Content NVARCHAR(MAX),

                    TimeSave NVARCHAR(200),             -- RAW
                    RealTimeSave DATETIME2(0) NULL,     -- REAL

                    FOREIGN KEY(PersonID) REFERENCES TablePersonInfo(PersonID),
                    FOREIGN KEY(PageID) REFERENCES TablePageInfo(PageID)
                );
            END

            --------------------------------------------------------
            -- TABLE POST COMMENT
            --------------------------------------------------------
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TablePostComment')
            BEGIN
                CREATE TABLE TablePostComment (
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    PostID NVARCHAR(200) NOT NULL,
                    CommentID NVARCHAR(200) NOT NULL,
                    CommentTime NVARCHAR(200),
                    CONSTRAINT UQ_PostComment UNIQUE(PostID, CommentID)
                );
            END

            --------------------------------------------------------
            -- TABLE TOPIC
            --------------------------------------------------------
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TableTopic')
            BEGIN
                CREATE TABLE TableTopic (
                    TopicId INT IDENTITY(1,1) PRIMARY KEY,
                    TopicName NVARCHAR(500) NOT NULL UNIQUE,
                    TopicInfor NVARCHAR(MAX)
                );
            END

            --------------------------------------------------------
            -- TABLE KEYWORD
            --------------------------------------------------------
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TableKeyword')
            BEGIN
                CREATE TABLE TableKeyword (
                    KeywordId INT IDENTITY(1,1) PRIMARY KEY,
                    KeywordName NVARCHAR(500) NOT NULL UNIQUE
                );
            END

            --------------------------------------------------------
            -- TABLE TOPIC KEY
            --------------------------------------------------------
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TableTopicKey')
            BEGIN
                CREATE TABLE TableTopicKey (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    TopicId INT NOT NULL,
                    KeywordId INT NOT NULL,
                    CONSTRAINT UQ_TopicKey UNIQUE(TopicId, KeywordId)
                );
            END

            --------------------------------------------------------
            -- TABLE TOPIC POST
            --------------------------------------------------------
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TableTopicPost')
            BEGIN
                CREATE TABLE TableTopicPost (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    TopicId INT NOT NULL,
                    PostId NVARCHAR(200) NOT NULL,
                    CONSTRAINT UQ_TopicPost UNIQUE(TopicId, PostId)
                );
            END

            --------------------------------------------------------
            -- TABLE PAGE MONITOR
            --------------------------------------------------------
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TablePageMonitor')
            BEGIN
                CREATE TABLE TablePageMonitor (
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    PageID NVARCHAR(200) UNIQUE,
                    IsAuto INT DEFAULT 0,
                    Status NVARCHAR(200) DEFAULT 'Chưa auto',

                    FirstScanTime DATETIME2(0) NULL,
                    LastScanTime DATETIME2(0) NULL,
                    TotalPostsScanned INT,
                    TimeSave DATETIME2(0) DEFAULT SYSDATETIME()
                );
            END

            --------------------------------------------------------
            -- TABLE PROFILE INFO
            --------------------------------------------------------
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TableProfileInfo')
            BEGIN
                CREATE TABLE TableProfileInfo (
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    IDAdbrowser NVARCHAR(200),
                    ProfileName NVARCHAR(500),
                    ProfileLink NVARCHAR(MAX),
                    ProfileStatus NVARCHAR(200) DEFAULT 'Die',
                    UseTab INT DEFAULT 0,
                    ProfileType NVARCHAR(200) DEFAULT 'Person'
                );
            END

            --------------------------------------------------------
            -- TABLE MANAGER PROFILE
            --------------------------------------------------------
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TableManagerProfile')
            BEGIN
                CREATE TABLE TableManagerProfile (
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    ProfileID INT NOT NULL,
                    PageID NVARCHAR(200) NOT NULL,
                    TimeSave DATETIME2(0) DEFAULT SYSDATETIME(),
                    FOREIGN KEY(ProfileID) REFERENCES TableProfileInfo(ID)
                );
            END

            --------------------------------------------------------
            -- TABLE PAGE NOTE
            --------------------------------------------------------
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TablePageNote')
            BEGIN
                CREATE TABLE TablePageNote (
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    PageID NVARCHAR(200) NOT NULL,
                    TimeSave DATETIME2(0) DEFAULT SYSDATETIME()
                );
            END
            ";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ CreateTableAuto error: " + ex.Message);
        }
    }
    //===================
    //===== HÀM TIỆN ÍCH MÀ HASH=========
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
    // ===========================
    // II. POST
    // ===========================
    public bool ExistPostByLink(string link)
    {
        string sql = "SELECT TOP 1 1 FROM TablePostInfo WHERE PostLink = @link";

        object v = SQLDAO.Instance.ExecuteScalar(sql,
            new Dictionary<string, object> { { "@link", link } });

        return v != null;
    }
    public void DeletePost(string postId)
    {
        string sql = "DELETE FROM TablePostInfo WHERE PostID=@id; DELETE FROM TablePost WHERE PostID=@id;";

        SQLDAO.Instance.ExecuteNonQuery(sql,
            new Dictionary<string, object> { { "@id", postId } });
    }
    // xóa toàn bộ post và các bảng liên quan
    public void DeleteAllPosts()
    {
        using (var conn = OpenConnection())
        using (var tran = conn.BeginTransaction())
        using (var cmd = conn.CreateCommand())
        {
            cmd.Transaction = tran;

            cmd.CommandText = @"
            DELETE FROM TablePostComment;
            DELETE FROM TableCommentInfo;
            DELETE FROM TablePostShare;
            DELETE FROM TableTopicPost;
            DELETE FROM TablePost;
            DELETE FROM TablePostInfo;
        ";
            cmd.ExecuteNonQuery();

            tran.Commit();
        }
    }

    //Xóa 1 bài theo PostID
    public void DeletePostFull(string postId)
    {
        using (var conn = OpenConnection())
        using (var tran = conn.BeginTransaction())
        using (var cmd = conn.CreateCommand())
        {
            cmd.Transaction = tran;

            // 1. COMMENT
            cmd.CommandText = @"
            DELETE FROM TablePostComment WHERE PostID=@id;
            DELETE FROM TableCommentInfo 
                WHERE CommentID NOT IN (SELECT CommentID FROM TablePostComment);
        ";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@id", postId);
            cmd.ExecuteNonQuery();

            // 2. SHARE
            cmd.CommandText = "DELETE FROM TablePostShare WHERE PostID=@id";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@id", postId);
            cmd.ExecuteNonQuery();

            // 3. TOPIC
            cmd.CommandText = "DELETE FROM TableTopicPost WHERE PostID=@id";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@id", postId);
            cmd.ExecuteNonQuery();

            // 4. Mapping TablePost
            cmd.CommandText = "DELETE FROM TablePost WHERE PostID=@id";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@id", postId);
            cmd.ExecuteNonQuery();

            // 5. Cuối cùng xoá TablePostInfo
            cmd.CommandText = "DELETE FROM TablePostInfo WHERE PostID=@id";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@id", postId);
            cmd.ExecuteNonQuery();

            tran.Commit();
        }
    }
    /*
    public void InsertOrIgnorePost(PostPage p)
    {
        if (p == null || string.IsNullOrWhiteSpace(p.PostLink))
            return;

        string postId = GeneratePostId(p.PostLink);

        // =========================
        // XÁC ĐỊNH PAGE / PERSON
        // =========================

        string pageContainerId = null;
        if (!string.IsNullOrWhiteSpace(p.PageLink))
        {
            (pageContainerId, _) = CheckPageLink(p.PageLink);
        }              
        string posterPageId = null;
        string posterPersonId = null;
        if (!string.IsNullOrWhiteSpace(p.PosterLink))
        {
            bool isPagePoster = !string.IsNullOrEmpty(p.PosterNote) && p.PosterNote.ToLower().Contains("page");

            if (isPagePoster)
                (posterPageId, _) = CheckPageLink(p.PosterLink);
            else
                posterPersonId = GenerateHashId(p.PosterLink);
        }
        if (!string.IsNullOrEmpty(posterPageId) && posterPageId == pageContainerId)
        {
            posterPageId = null;
        }
        // =========================
        // REAL POST TIME
        // =========================
        DateTime? parsedTime = TimeHelper.ParseFacebookTime(p.PostTime);
        object realPostTime = parsedTime == DateTime.MinValue ? (object)DBNull.Value : parsedTime;
        using (var conn = SQLDAO.Instance.OpenConnection())
        using (var tran = conn.BeginTransaction())
        using (var cmd = new SqlCommand())
        {
            cmd.Connection = conn;
            cmd.Transaction = tran;
            try
            {
                // =========================
                // 1️⃣ PAGE CONTAINER
                // =========================
                if (pageContainerId != null)
                {
                    cmd.CommandText = @"
                                MERGE TablePageInfo t
                USING (SELECT @link AS PageLink) s
                ON t.PageLink = s.PageLink
                WHEN NOT MATCHED THEN
                    INSERT (PageID, PageLink, PageName, PageType, PageTimeSave)
                    VALUES (@id, @link, @name, @type, SYSDATETIME());
                ";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@id", SqlDbType.NVarChar, 200).Value = pageContainerId;
                    cmd.Parameters.Add("@link", SqlDbType.NVarChar).Value = p.PageLink ?? "";
                    cmd.Parameters.Add("@name", SqlDbType.NVarChar, 500).Value = p.PageName ?? "";
                    cmd.Parameters.Add("@type", SqlDbType.NVarChar, 50).Value =
                        p.PageLink.Contains("/groups/") ? "groups" : "fanpage";
                    cmd.ExecuteNonQuery();
                }

                // =========================
                // 2️⃣ PAGE POSTER
                // =========================
                if (posterPageId != null)
                {
                    cmd.CommandText = @"
                                MERGE TablePageInfo t
                USING (SELECT @link AS PageLink) s
                ON t.PageLink = s.PageLink
                WHEN NOT MATCHED THEN
                    INSERT (PageID, PageLink, PageName, PageType, PageTimeSave)
                    VALUES (@id, @link, @name, @type, SYSDATETIME());
                "; 

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@id", SqlDbType.NVarChar, 200).Value = posterPageId;
                    cmd.Parameters.Add("@link", SqlDbType.NVarChar).Value = p.PosterLink ?? "";
                    cmd.Parameters.Add("@name", SqlDbType.NVarChar, 500).Value = p.PosterName ?? "";
                    cmd.Parameters.Add("@type", SqlDbType.NVarChar, 50).Value = p.PageLink.Contains("/groups/") ? "groups" : "fanpage";
                    cmd.ExecuteNonQuery();
                }

                // =========================
                // 3️⃣ PERSON POSTER
                // =========================
                if (posterPersonId != null)
                {
                    cmd.CommandText = @"
                IF NOT EXISTS (SELECT 1 FROM TablePersonInfo WHERE PersonID=@id)
                INSERT INTO TablePersonInfo(PersonID, PersonLink, PersonName, PersonNote, PersonTimeSave)
                VALUES (@id, @link, @name, @note, SYSDATETIME());";

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@id", SqlDbType.NVarChar, 200).Value = posterPersonId;
                    cmd.Parameters.Add("@link", SqlDbType.NVarChar).Value = p.PosterLink ?? "";
                    cmd.Parameters.Add("@name", SqlDbType.NVarChar, 500).Value = p.PosterName ?? "";
                    cmd.Parameters.Add("@note", SqlDbType.NVarChar).Value = p.PosterNote ?? "";

                    cmd.ExecuteNonQuery();
                }

                // =========================
                // 4️⃣ POST INFO (MERGE)
                // =========================
                cmd.CommandText = @"
                MERGE TablePostInfo t
                USING (SELECT @id AS PostID) s
                ON t.PostID = s.PostID
                WHEN MATCHED THEN 
                    UPDATE SET 
                        PostLink=@link,
                        PostContent=@content,
                        PostTime=@timeRaw,
                        RealPostTime=@timeReal,
                        LikeCount=@like,
                        ShareCount=@share,
                        CommentCount=@comment,
                        PostAttachment=@attachment,
                        PostStatus=@status,
                        PostTimeSave=SYSDATETIME()
                WHEN NOT MATCHED THEN
                    INSERT (PostID, PostLink, PostContent, PostTime, RealPostTime,
                            LikeCount, ShareCount, CommentCount, PostAttachment, PostStatus, PostTimeSave)
                    VALUES(@id,@link,@content,@timeRaw,@timeReal,
                           @like,@share,@comment,@attachment,@status,SYSDATETIME());";

                cmd.Parameters.Clear();
                cmd.Parameters.Add("@id", SqlDbType.NVarChar, 200).Value = postId;
                cmd.Parameters.Add("@link", SqlDbType.NVarChar).Value = p.PostLink ?? "";
                cmd.Parameters.Add("@content", SqlDbType.NVarChar).Value = p.Content ?? "";
                cmd.Parameters.Add("@timeRaw", SqlDbType.NVarChar, 200).Value = p.PostTime ?? "";
                cmd.Parameters.Add("@timeReal", SqlDbType.DateTime2).Value = realPostTime;
                cmd.Parameters.Add("@like", SqlDbType.Int).Value = p.LikeCount ?? 0;
                cmd.Parameters.Add("@share", SqlDbType.Int).Value = p.ShareCount ?? 0;
                cmd.Parameters.Add("@comment", SqlDbType.Int).Value = p.CommentCount ?? 0;
                cmd.Parameters.Add("@attachment", SqlDbType.NVarChar).Value = p.Attachment ?? "";
                cmd.Parameters.Add("@status", SqlDbType.NVarChar, 200).Value = p.PostType ?? "";

                cmd.ExecuteNonQuery();

                // =========================
                // 5️⃣ POST MAP
                // =========================
                if (posterPageId == null && posterPersonId == null && pageContainerId != null)
                {
                    posterPageId = pageContainerId;
                }
                cmd.CommandText = @"
                IF NOT EXISTS (SELECT 1 FROM TablePost WHERE PostID=@id)
                INSERT INTO TablePost(PostID, PageIDCreate, PageIDContainer, PersonIDCreate)
                VALUES(@id, @createPage, @containerPage, @createPerson);";
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@id", SqlDbType.NVarChar, 200).Value = postId;
                cmd.Parameters.Add("@createPage", SqlDbType.NVarChar, 200)
                    .Value = (object)posterPageId ?? DBNull.Value;
                cmd.Parameters.Add("@containerPage", SqlDbType.NVarChar, 200)
                    .Value = (object)pageContainerId ?? DBNull.Value;
                cmd.Parameters.Add("@createPerson", SqlDbType.NVarChar, 200)
                    .Value = (object)posterPersonId ?? DBNull.Value;

                cmd.ExecuteNonQuery();

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
    }*/
    public void InsertOrIgnorePost(PostPage p)
    {
        if (p == null || string.IsNullOrWhiteSpace(p.PostLink))
            return;

        string postId = GeneratePostId(p.PostLink);

        // =================================================
        // 🔑 RESOLVE ID (IDFB FIRST, FALLBACK HASH)
        // =================================================
        string ResolveId(string idfb, string link)
        {
            if (!string.IsNullOrWhiteSpace(idfb))
                return idfb;

            if (!string.IsNullOrWhiteSpace(link))
                return GenerateHashId(link);

            return null;
        }

        // =================================================
        // PAGE CONTAINER
        // =================================================
        string pageContainerId = ResolveId(p.ContainerIdFB, p.PageLink);

        // =================================================
        // POSTER
        // =================================================
        string posterPageId = null;
        string posterPersonId = null;

        bool isPagePoster =
            !string.IsNullOrWhiteSpace(p.PosterNote) &&
            p.PosterNote.ToLower().Contains("page");

        if (isPagePoster)
            posterPageId = ResolveId(p.PosterIdFB, p.PosterLink);
        else
            posterPersonId = ResolveId(p.PosterIdFB, p.PosterLink);

        // tránh trùng container
        if (!string.IsNullOrWhiteSpace(posterPageId) &&
            posterPageId == pageContainerId)
        {
            posterPageId = null;
        }

        // =================================================
        // REAL POST TIME
        // =================================================
        DateTime? parsedTime = TimeHelper.ParseFacebookTime(p.PostTime);
        object realPostTime =
            parsedTime == DateTime.MinValue
                ? (object)DBNull.Value
                : parsedTime;

        using (var conn = SQLDAO.Instance.OpenConnection())
        using (var tran = conn.BeginTransaction())
        using (var cmd = new SqlCommand())
        {
            cmd.Connection = conn;
            cmd.Transaction = tran;

            try
            {
                // =================================================
                // HELPER: MERGE PAGE (KEY = PageID)
                // =================================================
                void MergePage(string pageId, string link, string name, string type)
                {
                    if (string.IsNullOrWhiteSpace(pageId))
                        return;

                    cmd.CommandText = @"
                MERGE TablePageInfo t
                USING (SELECT @id AS PageID) s
                ON t.PageID = s.PageID
                WHEN NOT MATCHED THEN
                 INSERT(PageID, PageLink, PageName, PageType, PageTimeSave)
                 VALUES(@id,@link,@name,@type,SYSDATETIME())
                WHEN MATCHED THEN
                 UPDATE SET
                   PageLink = COALESCE(NULLIF(@link,''), t.PageLink),
                   PageName = COALESCE(NULLIF(@name,''), t.PageName);";

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@id", SqlDbType.NVarChar, 200).Value = pageId;
                    cmd.Parameters.Add("@link", SqlDbType.NVarChar).Value = link ?? "";
                    cmd.Parameters.Add("@name", SqlDbType.NVarChar, 500).Value = name ?? "";
                    cmd.Parameters.Add("@type", SqlDbType.NVarChar, 50).Value = type;

                    cmd.ExecuteNonQuery();
                }

                // =================================================
                // 1️⃣ PAGE CONTAINER
                // =================================================
                MergePage(
                    pageContainerId,
                    p.PageLink,
                    p.PageName,
                    p.PageLink != null && p.PageLink.Contains("/groups/")
                        ? "groups"
                        : "fanpage"
                );

                // =================================================
                // 2️⃣ PAGE POSTER
                // =================================================
                MergePage(
                    posterPageId,
                    p.PosterLink,
                    p.PosterName,
                    p.PosterLink != null && p.PosterLink.Contains("/groups/")
                        ? "groups"
                        : "fanpage"
                );

                // =================================================
                // 3️⃣ PERSON POSTER
                // =================================================
                if (!string.IsNullOrWhiteSpace(posterPersonId))
                {
                    cmd.CommandText = @"
                IF NOT EXISTS (SELECT 1 FROM TablePersonInfo WHERE PersonID=@id)
                INSERT INTO TablePersonInfo
                (PersonID, PersonLink, PersonName, PersonNote, PersonTimeSave)
                VALUES (@id,@link,@name,@note,SYSDATETIME());";

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@id", SqlDbType.NVarChar, 200).Value = posterPersonId;
                    cmd.Parameters.Add("@link", SqlDbType.NVarChar).Value = p.PosterLink ?? "";
                    cmd.Parameters.Add("@name", SqlDbType.NVarChar, 500).Value = p.PosterName ?? "";
                    cmd.Parameters.Add("@note", SqlDbType.NVarChar).Value = p.PosterNote ?? "";

                    cmd.ExecuteNonQuery();
                }

                // =================================================
                // 4️⃣ POST INFO
                // =================================================
                cmd.CommandText = @"
                MERGE TablePostInfo t
                USING (SELECT @id AS PostID) s
                ON t.PostID = s.PostID
                WHEN MATCHED THEN
                 UPDATE SET
                  PostLink=@link,
                  PostContent=@content,
                  PostTime=@timeRaw,
                  RealPostTime=@timeReal,
                  LikeCount=@like,
                  ShareCount=@share,
                  CommentCount=@comment,
                  PostAttachment=@attachment,
                  PostStatus=@status,
                  PostTimeSave=SYSDATETIME()
                WHEN NOT MATCHED THEN
                 INSERT
                 (PostID,PostLink,PostContent,PostTime,RealPostTime,
                  LikeCount,ShareCount,CommentCount,PostAttachment,PostStatus,PostTimeSave)
                 VALUES
                 (@id,@link,@content,@timeRaw,@timeReal,
                  @like,@share,@comment,@attachment,@status,SYSDATETIME());";

                cmd.Parameters.Clear();
                cmd.Parameters.Add("@id", SqlDbType.NVarChar, 200).Value = postId;
                cmd.Parameters.Add("@link", SqlDbType.NVarChar).Value = p.PostLink ?? "";
                cmd.Parameters.Add("@content", SqlDbType.NVarChar).Value = p.Content ?? "";
                cmd.Parameters.Add("@timeRaw", SqlDbType.NVarChar, 200).Value = p.PostTime ?? "";
                cmd.Parameters.Add("@timeReal", SqlDbType.DateTime2).Value = realPostTime;
                cmd.Parameters.Add("@like", SqlDbType.Int).Value = p.LikeCount ?? 0;
                cmd.Parameters.Add("@share", SqlDbType.Int).Value = p.ShareCount ?? 0;
                cmd.Parameters.Add("@comment", SqlDbType.Int).Value = p.CommentCount ?? 0;
                cmd.Parameters.Add("@attachment", SqlDbType.NVarChar).Value = p.Attachment ?? "";
                cmd.Parameters.Add("@status", SqlDbType.NVarChar, 200).Value = p.PostType ?? "";

                cmd.ExecuteNonQuery();

                // =================================================
                // 5️⃣ POST MAP
                // =================================================
                if (posterPageId == null &&
                    posterPersonId == null &&
                    pageContainerId != null)
                {
                    posterPageId = pageContainerId;
                }

                cmd.CommandText = @"
            IF NOT EXISTS (SELECT 1 FROM TablePost WHERE PostID=@id)
            INSERT INTO TablePost
            (PostID, PageIDCreate, PageIDContainer, PersonIDCreate)
            VALUES
            (@id,@createPage,@containerPage,@createPerson);";

                cmd.Parameters.Clear();
                cmd.Parameters.Add("@id", SqlDbType.NVarChar, 200).Value = postId;
                cmd.Parameters.Add("@createPage", SqlDbType.NVarChar, 200)
                    .Value = (object)posterPageId ?? DBNull.Value;
                cmd.Parameters.Add("@containerPage", SqlDbType.NVarChar, 200)
                    .Value = (object)pageContainerId ?? DBNull.Value;
                cmd.Parameters.Add("@createPerson", SqlDbType.NVarChar, 200)
                    .Value = (object)posterPersonId ?? DBNull.Value;

                cmd.ExecuteNonQuery();

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
    }

    // HÀM LẤY TẤT CẢ POST Ở db
    public DataTable GetAllPostsDB(int? days, int maxCount)
    {
        var dt = new DataTable();

        string whereTime = "";
        if (days.HasValue && days.Value > 0)
        {
            whereTime = " AND pi.RealPostTime >= DATEADD(DAY, -@days, CAST(SYSDATETIME() AS DATE)) ";
        }

        string sql = $@"
SELECT TOP (@maxCount)
    pi.PostID,
    pi.PostLink,
    pi.PostContent,

    pi.PostTime,          -- RAW
    pi.RealPostTime,      -- REAL

    pi.LikeCount,
    pi.ShareCount,
    pi.CommentCount,
    pi.PostAttachment,
    pi.PostStatus,

    pi.PostTimeSave
FROM TablePostInfo pi
WHERE 1=1 {whereTime}
ORDER BY 
    pi.RealPostTime DESC,
    pi.PostTimeSave DESC;
";

        using (var conn = SQLDAO.Instance.OpenConnection())
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.Add("@maxCount", SqlDbType.Int).Value = maxCount;

            if (days.HasValue && days.Value > 0)
                cmd.Parameters.Add("@days", SqlDbType.Int).Value = days.Value;

            using (var da = new SqlDataAdapter(cmd))
            {
                da.Fill(dt);
            }
        }

        return dt;
    }
    // xem pagelink đã tồn tại thì lấy pageid
    // chia ra là cái dưới
    public DataTable GetPostsForPagesDB(int? daysFilter,int pageIndex,int pageSize,out int totalRows)
    {
        totalRows = 0;
        DataTable dt = new DataTable();
        Libary.Instance.LogTech($"pageIndex={pageIndex}, pageSize={pageSize}", AppConfig.ENABLE_LOG);
        try
        {
        using (var conn = OpenConnection())
        {
            // =====================================
            // 1️⃣ COUNT TỔNG (PHÂN TRANG)
            // =====================================
            string sqlCount = @"
            SELECT COUNT(*)
            FROM TablePostInfo pi
            JOIN TablePost p ON p.PostID = pi.PostID
            WHERE
                (
                    @days IS NULL
                    OR TRY_CONVERT(datetime, pi.RealPostTime) >= DATEADD(DAY, -@days, GETDATE())
                )
        ";
                using (var cmdCount = new SqlCommand(sqlCount, conn))
                {
                    var pDays = new SqlParameter("@days", SqlDbType.Int);
                    pDays.Value = daysFilter.HasValue ? (object)daysFilter.Value : DBNull.Value;
                    cmdCount.Parameters.Add(pDays);

                    totalRows = Convert.ToInt32(cmdCount.ExecuteScalar());
                }

                // =====================================
                // 2️⃣ DATA PHÂN TRANG
                // =====================================
                string sqlData = @"
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

                WHERE
                (
                    @days IS NULL
                    OR pi.RealPostTime >= DATEADD(DAY, -@days, SYSDATETIME())
                )

                ORDER BY
                    pi.RealPostTime DESC,
                    pi.PostTimeSave DESC

                OFFSET @offset ROWS
                FETCH NEXT @pageSize ROWS ONLY;
                        ";
                using (var cmd = new SqlCommand(sqlData, conn))
                {
                    int offset = (pageIndex - 1) * pageSize;

                    var pDays = new SqlParameter("@days", SqlDbType.Int);
                    pDays.Value = daysFilter.HasValue ? (object)daysFilter.Value : DBNull.Value;
                    cmd.Parameters.Add(pDays);

                    cmd.Parameters.Add("@offset", SqlDbType.Int).Value = offset;
                    cmd.Parameters.Add("@pageSize", SqlDbType.Int).Value = pageSize;

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

            }

            // =====================================
            // 3️⃣ STT THEO PAGE (BẮT ĐẦU TỪ 1)
            // =====================================
            dt.Columns.Add("STT", typeof(int)).SetOrdinal(0);

        for (int i = 0; i < dt.Rows.Count; i++)
            dt.Rows[i]["STT"] = (pageIndex - 1) * pageSize + i + 1;
    }
    catch (Exception ex)
    {
        Libary.Instance.LogTech("❌ GetPostsForPagesDB error: " + ex.Message);
    }

    return dt;
    }
    public string GetPostIdByPostLink(string postLink)
    {
        const string sql = @"
SELECT PostID
FROM TablePostInfo
WHERE PostLink = @link
";

        var result = ExecuteScalar(sql, new Dictionary<string, object>
        {
            ["@link"] = postLink
        });

        return result?.ToString();
    }
    public List<PostPage> GetPostsByPage(string pageID)
    {
        var list = new List<PostPage>();

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
            ps.PersonLink AS PosterPersonLink

        FROM TablePostInfo pi
        JOIN TablePost p ON p.PostID = pi.PostID
        LEFT JOIN TablePageInfo pg_container ON pg_container.PageID = p.PageIDContainer
        LEFT JOIN TablePageInfo pg_create ON pg_create.PageID = p.PageIDCreate
        LEFT JOIN TablePersonInfo ps ON ps.PersonID = p.PersonIDCreate

        WHERE p.PageIDContainer = @pid
        ORDER BY pi.RealPostTime DESC
    ";

        using (var conn = OpenConnection())
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@pid", pageID);

            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    list.Add(new PostPage
                    {
                        PostID = rd["PostID"].ToString(),
                        PostLink = rd["PostLink"].ToString(),
                        Content = rd["PostContent"].ToString(),
                        PostTime = rd["RealPostTime"].ToString(),

                        LikeCount = Convert.ToInt32(rd["LikeCount"]),
                        ShareCount = Convert.ToInt32(rd["ShareCount"]),
                        CommentCount = Convert.ToInt32(rd["CommentCount"]),


                        Attachment = rd["PostAttachment"].ToString(),
                        PostType = rd["PostStatus"].ToString(),

                        PageName = rd["ContainerPageName"].ToString(),
                        PageLink = rd["ContainerPageLink"].ToString(),

                        PosterName = rd["PosterPageName"].ToString() != ""
                            ? rd["PosterPageName"].ToString()
                            : rd["PosterPersonName"].ToString(),

                        PosterLink = rd["PosterPageName"].ToString() != ""
                            ? rd["PosterPageLink"].ToString()
                            : rd["PosterPersonLink"].ToString()
                    });
                }
            }
        }

        return list;
    }
    public DataTable GetPostsByPage_DataTable(string pageID)
    {
        string sql = @"
        SELECT
            pi.PostID,
            pi.PostLink,
            pi.PostContent,

            pi.PostTime,         -- 🔹 RAW Facebook time
            pi.RealPostTime,     -- 🔹 REAL datetime (DATETIME2)

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
            ps.PersonLink AS PosterPersonLink

        FROM TablePostInfo pi
        JOIN TablePost p ON p.PostID = pi.PostID
        LEFT JOIN TablePageInfo pg_container ON pg_container.PageID = p.PageIDContainer
        LEFT JOIN TablePageInfo pg_create ON pg_create.PageID = p.PageIDCreate
        LEFT JOIN TablePersonInfo ps ON ps.PersonID = p.PersonIDCreate

        WHERE p.PageIDContainer = @pid
        ORDER BY 
            pi.RealPostTime DESC,
            pi.PostTimeSave DESC;
        ";

        using (var conn = OpenConnection())
        using (var cmd = new SqlCommand(sql, conn))
        {
            // ❌ không dùng AddWithValue
            cmd.Parameters.Add("@pid", SqlDbType.NVarChar, 200).Value = pageID;

            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                Libary.Instance.CreateLog( "SQLDAO","[DEBUG] Tổng row = " + dt.Rows.Count);


                da.Fill(dt);
                if (dt.Columns.Contains("RealPostTime"))
                {
                    if (dt.Columns.Contains("RealPostTime"))
                    {
                        Libary.Instance.CreateLog("SQLDAO",
                            $"RealPostTime DataType = {dt.Columns["RealPostTime"].DataType.FullName}"
                        );
                    }
                    else
                    {
                        Libary.Instance.CreateLog("SQLDAO",
                            "KHÔNG CÓ CỘT RealPostTime"
                        );
                    }

                }
                else
                {
                    Libary.Instance.CreateLog("SQLDAO",
                        "[DT-CHECK] KHÔNG CÓ CỘT RealPostTime"
                    );
                }

                if (dt.Columns.Contains("RealPostTime") &&
      dt.Columns["RealPostTime"].DataType != typeof(DateTime))
                {
                    dt.Columns["RealPostTime"].DataType = typeof(DateTime);

                    Libary.Instance.CreateLog(
                        "Đã ép RealPostTime về DateTime"
                    );
                }

                // =========================
                // POSTER NAME / LINK (GIỮ NGUYÊN LOGIC CŨ)
                // =========================
                dt.Columns.Add("PosterName", typeof(string));
                dt.Columns.Add("PosterLink", typeof(string));

                foreach (DataRow row in dt.Rows)
                {
                    bool hasPagePoster =
                        row["PosterPageName"] != DBNull.Value &&
                        !string.IsNullOrWhiteSpace(row["PosterPageName"].ToString());

                    row["PosterName"] = hasPagePoster
                        ? row["PosterPageName"]
                        : row["PosterPersonName"];

                    row["PosterLink"] = hasPagePoster
                        ? row["PosterPageLink"]
                        : row["PosterPersonLink"];
                }

                return dt;
            }
        }
    }
    // HÀM LẤY ID fb post ĐÃ CÓ DB THEO LINK
    public string GetIDFBPostByLink(string postLink)
    {
        if (string.IsNullOrWhiteSpace(postLink))
            return null;

        string sql = @"
        SELECT IDFBPost
        FROM TablePostInfo
        WHERE PostLink = @link
          AND IDFBPost IS NOT NULL
          AND IDFBPost <> 'N/A'
    ";

        object v = ExecuteScalar(sql, new Dictionary<string, object>
    {
        { "@link", postLink }
    });

        return v?.ToString();
    }
    public int CountPostsByPage(string pageID)
    {
        string sql = @"
        SELECT COUNT(*)
        FROM TablePost p
        JOIN TablePostInfo pi ON pi.PostID = p.PostID
        WHERE p.PageIDContainer = @pid
    ";

        using (var conn = OpenConnection())
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@pid", pageID);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }

    // ===========================
    // III. PAGE
    // ===========================
    public void InsertOrIgnorePage(string pageLink, string pageName)
    {
        string id = GenerateHashId(pageLink);
        string type = pageLink.Contains("/groups/") ? "Group" : "Fanpage";

        string sql = @"
        IF NOT EXISTS (SELECT 1 FROM TablePageInfo WHERE PageID=@id)
        BEGIN
            INSERT INTO TablePageInfo(PageID, PageLink, PageName, PageType, PageTimeSave)
            VALUES (@id, @link, @name, @type, GETDATE());
        END";

        SQLDAO.Instance.ExecuteNonQuery(sql,
            new Dictionary<string, object>
            {
                {"@id", id}, {"@link", pageLink},
                {"@name", pageName}, {"@type", type}
            });
    }
    public void InsertOrIgnorePageInfo(PageInfo page)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(page.PageLink))
                throw new Exception("PageLink is NULL – không thể insert Page");
            // ⭐ PageID chỉ dùng khi INSERT lần đầu
            if (string.IsNullOrWhiteSpace(page.PageID) || page.PageID == "N/A")
                page.PageID = GenerateHashId(page.PageLink);

            const string sql = @"
        MERGE TablePageInfo AS t
        USING (
            SELECT
                @PageID        AS PageID,
                @IDFBPage      AS IDFBPage,
                @PageLink      AS PageLink
        ) AS s
        ON t.PageLink = s.PageLink
        WHEN MATCHED THEN
            UPDATE SET
                -- chỉ update IDFBPage nếu DB đang N/A mà dữ liệu mới có
                IDFBPage = CASE
                    WHEN t.IDFBPage = 'N/A' AND s.IDFBPage <> 'N/A'
                        THEN s.IDFBPage
                    ELSE t.IDFBPage
                END,

                PageName        = @PageName,
                PageType        = @PageType,
                PageMembers     = @PageMembers,
                PageInteraction = @PageInteraction,
                PageEvaluation  = @PageEvaluation,
                PageInfoText    = @PageInfoText,
                TimeLastPost    = @TimeLastPost

        WHEN NOT MATCHED THEN
            INSERT (
                PageID,
                IDFBPage,
                PageLink,
                PageName,
                PageType,
                PageMembers,
                PageInteraction,
                PageEvaluation,
                PageInfoText,
                PageTimeSave,
                TimeLastPost
            )
            VALUES (
                @PageID,
                @IDFBPage,
                @PageLink,
                @PageName,
                @PageType,
                @PageMembers,
                @PageInteraction,
                @PageEvaluation,
                @PageInfoText,
                SYSDATETIME(),
                @TimeLastPost
            );
        ";

            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@PageID", SqlDbType.NVarChar, 200).Value = page.PageID;
                cmd.Parameters.Add("@IDFBPage", SqlDbType.NVarChar, 200).Value = page.IDFBPage ?? "N/A";
                cmd.Parameters.Add("@PageLink", SqlDbType.NVarChar, -1).Value = page.PageLink;

                cmd.Parameters.Add("@PageName", SqlDbType.NVarChar, 500).Value = page.PageName ?? "N/A";
                cmd.Parameters.Add("@PageType", SqlDbType.NVarChar, 200).Value = page.PageType ?? "N/A";
                cmd.Parameters.Add("@PageMembers", SqlDbType.NVarChar, 200).Value = page.PageMembers ?? "N/A";
                cmd.Parameters.Add("@PageInteraction", SqlDbType.NVarChar, 200).Value = page.PageInteraction ?? "N/A";
                cmd.Parameters.Add("@PageEvaluation", SqlDbType.NVarChar, 200).Value = page.PageEvaluation ?? "N/A";
                cmd.Parameters.Add("@PageInfoText", SqlDbType.NVarChar, -1).Value = page.PageInfoText ?? "N/A";

                // ⭐ TimeLastPost – DATETIME2(0)
                var pTimeLastPost = new SqlParameter("@TimeLastPost", SqlDbType.DateTime2)
                {
                    Scale = 0,
                    Value = page.TimeLastPost.HasValue
                        ? (object)page.TimeLastPost.Value
                        : DBNull.Value
                };
                cmd.Parameters.Add(pTimeLastPost);

                cmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("❌ InsertOrIgnorePageInfo SQL ERROR: " + ex.Message);
            throw;
        }
    }

    public DateTime GetPageLastScan(string pageId)
    {
        try
        {
            string sql = @"SELECT LastScanTime 
                       FROM TablePageMonitor 
                       WHERE PageID = @id";

            object v = SQLDAO.Instance.ExecuteScalar(sql,
                new Dictionary<string, object> { { "@id", pageId } });

            if (v == null || v == DBNull.Value)
                return DateTime.MinValue;

            return Convert.ToDateTime(v);
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("[GetPageLastScan SQL] ERROR: " + ex.Message);
            return DateTime.MinValue;
        }
    }
    public PageInfo GetPageByID(string pageID)
    {
        try
        {
            string sql = @"
            SELECT PageID, IDFBPage, PageLink, PageName, PageType,
                   PageMembers, PageInteraction, PageEvaluation,
                   PageInfoText, PageTimeSave, TimeLastPost, IsScanned
            FROM TablePageInfo
            WHERE PageID = @id";

            using (var conn = SQLDAO.Instance.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@id", SqlDbType.NVarChar, 200).Value = pageID;

                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        return new PageInfo
                        {
                            PageID = rd["PageID"].ToString(),
                            IDFBPage = rd["IDFBPage"]?.ToString(),
                            PageLink = rd["PageLink"]?.ToString(),
                            PageName = rd["PageName"]?.ToString(),
                            PageType = rd["PageType"]?.ToString(),
                            PageMembers = rd["PageMembers"]?.ToString(),
                            PageInteraction = rd["PageInteraction"]?.ToString(),
                            PageEvaluation = rd["PageEvaluation"]?.ToString(),
                            PageInfoText = rd["PageInfoText"]?.ToString(),
                            PageTimeSave = rd["PageTimeSave"]?.ToString(),
                            TimeLastPost =
                        rd["TimeLastPost"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["TimeLastPost"]),
                            IsScanned = rd["IsScanned"] != DBNull.Value && Convert.ToBoolean(rd["IsScanned"])
                        };
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("[GetPageByID SQL] ERROR: " + ex.Message);
        }

        return null;
    }
    private string GetPageIdByPageLink(SqlConnection conn, string pageLink)
    {
        string sql = "SELECT PageID FROM TablePageInfo WHERE PageLink = @link";
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@link", pageLink);
            var val = cmd.ExecuteScalar();
            return val?.ToString();
        }
    }
    public DateTime? GetNewestPostTime(string pageId)
    {
        string sql = @"
        SELECT TOP 1 pi.RealPostTime
        FROM TablePostInfo pi
        JOIN TablePost p ON p.PostID = pi.PostID
        WHERE p.PageIDContainer = @pid
        ORDER BY pi.RealPostTime DESC";

        object v = SQLDAO.Instance.ExecuteScalar(sql, new Dictionary<string, object>
    {
        { "@pid", pageId }
    });

        if (v == null || v == DBNull.Value)
            return null;

        return Convert.ToDateTime(v);
    }
    public void UpdatePageLastPostTime(string pageId, DateTime? timeLastPost)
    {
        string sql = @"
        UPDATE TablePageInfo
        SET TimeLastPost = @time
        WHERE PageID = @pid";

        SQLDAO.Instance.ExecuteNonQuery(sql, new Dictionary<string, object>
    {
        { "@pid", pageId },
        { "@time", (object)timeLastPost ?? DBNull.Value }
    });
    }
    public string GetFacebookIdByLink(string link)
    {
        if (string.IsNullOrWhiteSpace(link))
            return null;

        // 1️⃣ Thử PAGE / GROUP trước
        string sqlPage = @"
        SELECT TOP 1 IDFBPage
        FROM TablePageInfo
        WHERE PageLink = @link
          AND IDFBPage IS NOT NULL
          AND IDFBPage <> 'N/A'
    ";

        object vPage = ExecuteScalar(sqlPage, new Dictionary<string, object>
        {
            ["@link"] = link
        });

        if (vPage != null)
            return vPage.ToString();

        // 2️⃣ Thử PERSON
        string sqlPerson = @"
        SELECT TOP 1 IDFBPerson
        FROM TablePersonInfo
        WHERE PersonLink = @link
          AND IDFBPerson IS NOT NULL
          AND IDFBPerson <> 'N/A'
    ";

        object vPerson = ExecuteScalar(sqlPerson, new Dictionary<string, object>
        {
            ["@link"] = link
        });

        return vPerson?.ToString();
    }
    public FBType GetPageTypeByID(string pageId)
    {
        if (string.IsNullOrWhiteSpace(pageId))
            return FBType.Unknown;

        string sql = @"
    SELECT PageType
    FROM TablePageInfo
    WHERE PageID = @id
    ";

        var val = ExecuteScalar(sql, new Dictionary<string, object>
    {
        { "@id", pageId }
    });

        return ProcessingHelper.MapPageTypeToFBType(val?.ToString());
    }

    public (FBType Type, string IdFB)? GetPageTypeIdFbByLink(string link)
    {
        if (string.IsNullOrWhiteSpace(link))
            return null;

        string sqlType = @"
    SELECT TOP 1 PageType
    FROM TablePageInfo
    WHERE PageLink = @link
       OR @link LIKE PageLink + '%'
    ";

        string sqlId = @"
    SELECT TOP 1 IDFBPage
    FROM TablePageInfo
    WHERE PageLink = @link
       OR @link LIKE PageLink + '%'
    ";

        var typeVal = ExecuteScalar(sqlType, new Dictionary<string, object>
    {
        { "@link", link }
    });

        var idVal = ExecuteScalar(sqlId, new Dictionary<string, object>
    {
        { "@link", link }
    });

        string idfb = idVal?.ToString();
        if (string.IsNullOrWhiteSpace(idfb))
            return null;

        return (
            ProcessingHelper.MapPageTypeToFBType(typeVal?.ToString()),
            idfb
        );
    }



    //==============hàm dưới phục vụ in ra page (trang)
    public DataTable GetPageNotePage(int pageIndex,int pageSize, out int totalRows)
    {
        using (var conn = OpenConnection())
        {
            totalRows = (int)new SqlCommand(
                "SELECT COUNT(*) FROM TablePageNote", conn
            ).ExecuteScalar();

            string sql = @"
            SELECT *
            FROM TablePageNote
            ORDER BY TimeSave DESC
            OFFSET (@PageIndex - 1) * @PageSize ROWS
            FETCH NEXT @PageSize ROWS ONLY";

            var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PageIndex", pageIndex);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            var dt = new DataTable();
            new SqlDataAdapter(cmd).Fill(dt);
            return dt;
        }
    }
    public DataTable GetPageInfoPage(int pageIndex, int pageSize, out int totalRows)
    {
        using (var conn = SQLDAO.Instance.OpenConnection())
        {
            // 1️⃣ Tổng số dòng
            using (var cmdCount = new SqlCommand(
                "SELECT COUNT(*) FROM TablePageInfo", conn))
            {
                totalRows = (int)cmdCount.ExecuteScalar();
            }

            // 2️⃣ Lấy dữ liệu trang
            string sql = @"
            SELECT *
            FROM TablePageInfo
            ORDER BY PageTimeSave DESC
            OFFSET (@PageIndex - 1) * @PageSize ROWS
            FETCH NEXT @PageSize ROWS ONLY";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@PageIndex", pageIndex);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }
    public DataTable GetPageMonitorPage(int pageIndex,int pageSize,out int totalRows)
    {
        using (var conn = OpenConnection())
        {
            // Tổng số dòng
            using (var cmdCount = new SqlCommand(
                "SELECT COUNT(*) FROM TablePageMonitor", conn))
            {
                totalRows = (int)cmdCount.ExecuteScalar();
            }

            // Lấy dữ liệu theo trang
            string sql = @"
            SELECT *
            FROM TablePageMonitor
            ORDER BY TimeSave DESC
            OFFSET (@PageIndex - 1) * @PageSize ROWS
            FETCH NEXT @PageSize ROWS ONLY";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@PageIndex", pageIndex);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }
    }
   /* public PageInfo GetPageInfoByID(string pageID)
    {
        try
        {
            const string sql = @"
            SELECT 
                PageID,
                IDFBPage,
                PageLink,
                PageName,
                PageType,
                PageMembers,
                PageInteraction,
                PageEvaluation,
                PageInfoText,
                IsScanned,
                PageTimeSave
            FROM TablePageInfo
            WHERE PageID = @id";

            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", pageID);

                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        return new PageInfo
                        {
                            PageID = rd["PageID"]?.ToString() ?? "",
                            IDFBPage = rd["IDFBPage"]?.ToString() ?? "N/A",
                            PageLink = rd["PageLink"]?.ToString() ?? "",
                            PageName = rd["PageName"]?.ToString() ?? "",
                            PageType = rd["PageType"]?.ToString() ?? "",
                            PageMembers = rd["PageMembers"]?.ToString() ?? "N/A",
                            PageInteraction = rd["PageInteraction"]?.ToString() ?? "N/A",
                            PageEvaluation = rd["PageEvaluation"]?.ToString() ?? "N/A",
                            PageInfoText = rd["PageInfoText"]?.ToString() ?? "N/A",
                            IsScanned = rd["IsScanned"] != DBNull.Value && Convert.ToInt32(rd["IsScanned"]) == 1,
                            PageTimeSave = rd["PageTimeSave"]?.ToString() ?? ""
                        };
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("❌ GetPageInfoByID SQL ERROR: " + ex.Message);
        }

        return null;
    }
   */
    public bool CheckPageExistsByLink(string pageLink)
    {
        try
        {
            string query = "SELECT COUNT(*) FROM TablePageInfo WHERE PageLink = @PageLink";

            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@PageLink", pageLink);

                conn.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("[SQL] CheckPageExistsByLink ERROR: " + ex.Message);
            return false;
        }
    }
    public bool PageExistsByLinkOrFbId(string pageLink, string fbId)
    {
        try
        {
            string sql = @"
        SELECT 1
        FROM TablePageInfo
        WHERE PageLink = @link
           OR (@fbid IS NOT NULL AND IDFBPage = @fbid)
        ";

            object v = ExecuteScalar(sql, new Dictionary<string, object>
        {
            { "@link", pageLink },
            { "@fbid", (object)fbId ?? DBNull.Value }
        });

            return v != null;
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("[SQL] PageExistsByLinkOrFbId ERROR: " + ex.Message);
            return false;
        }
    }

    public void UpdatePageLastScan(string pageId)
    {
        try
        {
            string sql = @"
            UPDATE TablePageMonitor
            SET LastScanTime = GETDATE()
            WHERE PageID = @id";

            SQLDAO.Instance.ExecuteNonQuery(sql,
                new Dictionary<string, object>
                {
                {"@id", pageId}
                });
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("[UpdatePageLastScan SQL] ERROR: " + ex.Message);
        }
    }
    public void UpdateMonitorIsAuto(string pageId, int isAuto)
    {
        try
        {
            string sql = @"
            UPDATE TablePageMonitor
            SET IsAuto = @auto
            WHERE PageID = @id";

            SQLDAO.Instance.ExecuteNonQuery(sql,
                new Dictionary<string, object>
                {
                {"@auto", isAuto},
                {"@id", pageId}
                });
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("[UpdateMonitorIsAuto SQL] ERROR: " + ex.Message);
        }
    }
    public DataTable GetAllPagesDB()
    {
        var dt = new DataTable();

        try
        {
            const string sql = @"
            SELECT 
                PageID,
                PageName,
                PageLink,
                IDFBPage,
                PageType,
                PageMembers,

                TimeLastPost,        -- DATETIME2
                PageInteraction,
                PageEvaluation,
                PageInfoText,
                IsScanned,

                PageTimeSave         -- DATETIME2
            FROM TablePageInfo
            ORDER BY 
                TimeLastPost DESC,
                PageTimeSave DESC;
            ";

            using (var conn = SQLDAO.Instance.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                da.Fill(dt);
            }

            // =========================
            // STT (UI)
            // =========================
            dt.Columns.Add("STT", typeof(int));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["STT"] = i + 1;

            dt.Columns["STT"].SetOrdinal(0);
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog($"❌ GetAllPagesDB SQL ERROR: {ex.Message}");
        }

        return dt;
    }

    public void DeleteAllPostsOfPage(string pageId)
    {
        try
        {
            List<string> postIds = new List<string>();

            // 1️⃣ LẤY DANH SÁCH POSTID
            string sqlGetPosts = @"
                SELECT PostID
                FROM TablePost
                WHERE PageIDCreate = @pid OR PageIDContainer = @pid
            ";

            using (var conn = SQLDAO.Instance.OpenConnection())
            using (var cmd = new SqlCommand(sqlGetPosts, conn))
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

            // 2️⃣ TIẾN HÀNH XÓA TRONG TRANSACTION
            using (var conn = SQLDAO.Instance.OpenConnection())
            using (var tran = conn.BeginTransaction())
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.Transaction = tran;

                foreach (var postId in postIds)
                {
                    // XÓA MAPPING POST
                    cmd.CommandText = "DELETE FROM TablePost WHERE PostID=@id";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", postId);
                    cmd.ExecuteNonQuery();

                    // XÓA POST INFO
                    cmd.CommandText = "DELETE FROM TablePostInfo WHERE PostID=@id";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", postId);
                    cmd.ExecuteNonQuery();

                    // XÓA SHARE
                    cmd.CommandText = "DELETE FROM TablePostShare WHERE PostID=@id";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", postId);
                    cmd.ExecuteNonQuery();

                    // XÓA COMMENT MAPPING
                    cmd.CommandText = "DELETE FROM TablePostComment WHERE PostID=@id";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", postId);
                    cmd.ExecuteNonQuery();

                    // XÓA TOPIC
                    cmd.CommandText = "DELETE FROM TableTopicPost WHERE PostID=@id";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", postId);
                    cmd.ExecuteNonQuery();
                }

                // Commit
                tran.Commit();
            }

            Libary.Instance.CreateLog($"[DeletePageData] ✔ Đã xóa dữ liệu trang PageID={pageId}");
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog($"[DeletePageData] ❌ Lỗi: {ex.Message}");
        }
    }
    //Xóa 1 page và các bảng liên quan
    public void DeletePageFull(string pageId)
    {
        // 1. Xóa bài viết liên quan Page
        DeleteAllPostsOfPage(pageId);

        // 2. Xóa PageNote
        ExecuteNonQuery("DELETE FROM TablePageNote WHERE PageID=@id",
            new Dictionary<string, object> { { "@id", pageId } });

        // 3. Xóa PageMonitor
        ExecuteNonQuery("DELETE FROM TablePageMonitor WHERE PageID=@id",
            new Dictionary<string, object> { { "@id", pageId } });

        // 4. Xóa PageInfo
        ExecuteNonQuery("DELETE FROM TablePageInfo WHERE PageID=@id",
            new Dictionary<string, object> { { "@id", pageId } });
    }
    // xóa toàn bộ page và các bảng liên quan page
    public void DeleteAllPages()
    {
        // Xóa toàn bộ bài viết của tất cả Page
        var pages = GetAllPagesDB();
        foreach (DataRow r in pages.Rows)
            DeletePageFull(r["PageID"].ToString());

        // Cuối cùng xóa bảng PageInfo
        ExecuteNonQuery("DELETE FROM TablePageInfo");
    }
  
    public PageStats GetPageStats(string pageID)
    {
        var stats = new PageStats
        {
            TotalPosts = 0,
            TotalLikes = 0,
            TotalComments = 0,
            TotalShares = 0,
            Followers = "N/A"
        };

        try
        {
            using (var conn = OpenConnection())
            {
                // Followers (PageMembers)
                // Followers = PageMembers (giữ nguyên định dạng)
                string sqlFol = "SELECT PageMembers FROM TablePageInfo WHERE PageID=@id";
                object fv = ExecuteScalar(sqlFol, new Dictionary<string, object> { { "@id", pageID } });

                // giữ nguyên string, không convert int
                stats.Followers = fv?.ToString() ?? "N/A";

                // Tổng Post, Like, Comment, Share
                string sql = @"
                SELECT 
                    COUNT(p.PostID) AS TotalPosts,
                    ISNULL(SUM(pi.LikeCount), 0) AS TotalLikes,
                    ISNULL(SUM(pi.CommentCount), 0) AS TotalComments,
                    ISNULL(SUM(pi.ShareCount), 0) AS TotalShares
                FROM TablePost p
                LEFT JOIN TablePostInfo pi ON pi.PostID = p.PostID
                WHERE p.PageIDContainer = @pid";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@pid", pageID);

                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            stats.TotalPosts = Convert.ToInt32(rd["TotalPosts"]);
                            stats.TotalLikes = Convert.ToInt32(rd["TotalLikes"]);
                            stats.TotalComments = Convert.ToInt32(rd["TotalComments"]);
                            stats.TotalShares = Convert.ToInt32(rd["TotalShares"]);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("❌ GetPageStats ERROR: " + ex.Message);
        }

        return stats;
    }
    // GET ID PAGE TRONG db NẾU CÓ
    public string GetIDFBPageByLink(string pageLink)
    {
        if (string.IsNullOrWhiteSpace(pageLink))
            return null;

        string sql = @"
        SELECT IDFBPage
        FROM TablePageInfo
        WHERE PageLink = @link
          AND IDFBPage IS NOT NULL
          AND IDFBPage <> 'N/A'
    ";

        object v = ExecuteScalar(sql, new Dictionary<string, object>
    {
        { "@link", pageLink }
    });

        return v?.ToString();
    }

    public class PageStats
    {
        public int TotalPosts { get; set; }
        public int TotalLikes { get; set; }
        public int TotalComments { get; set; }
        public int TotalShares { get; set; }
        public string Followers { get; set; }
    }
    //==PAGEMONITOR
    public void InsertOrUpdatePageMonitor(string pageId, bool isAuto, int postsCount = 0)
    {
        try
        {
            string sql = @"
        IF NOT EXISTS (SELECT 1 FROM TablePageMonitor WHERE PageID = @id)
        BEGIN
            INSERT INTO TablePageMonitor
                (PageID, IsAuto, Status, TotalPostsScanned, FirstScanTime, LastScanTime, TimeSave)
            VALUES
                (@id, @isAuto, 'Chưa auto', @count, GETDATE(), GETDATE(), GETDATE());
        END
        ELSE
        BEGIN
            UPDATE TablePageMonitor
            SET 
                IsAuto = @isAuto,
                TotalPostsScanned = TotalPostsScanned + @count,
                LastScanTime = GETDATE(),
                TimeSave = GETDATE()
            WHERE PageID = @id;
        END";

            SQLDAO.Instance.ExecuteNonQuery(sql, new Dictionary<string, object>
        {
            {"@id", pageId},
            {"@isAuto", isAuto ? 1 : 0},
            {"@count", postsCount}
        });
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("❌ InsertOrUpdatePageMonitor SQL ERROR: " + ex.Message);
        }
    }
    public void InsertPageMonitor(string pageID)
    {
        try
        {
            string sql = @"
        IF NOT EXISTS (SELECT 1 FROM TablePageMonitor WHERE PageID=@id)
        BEGIN
            INSERT INTO TablePageMonitor
            (PageID, IsAuto, Status, FirstScanTime, LastScanTime, TotalPostsScanned, TimeSave)
            VALUES
            (@id, 0, 'Chưa auto', GETDATE(), GETDATE(), 0, GETDATE());
        END
        ELSE
        BEGIN
            UPDATE TablePageMonitor
            SET TimeSave = GETDATE()
            WHERE PageID=@id;
        END";

            SQLDAO.Instance.ExecuteNonQuery(sql,
                new Dictionary<string, object> { { "@id", pageID } });
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("❌ InsertPageMonitor SQL ERROR: " + ex.Message);
        }
    }
    public void DeletePageMonitor(string pageID)
    {
        try
        {
            string sql = @"DELETE FROM TablePageMonitor WHERE PageID=@id";

            SQLDAO.Instance.ExecuteNonQuery(sql,
                new Dictionary<string, object> { { "@id", pageID } });

            Libary.Instance.CreateLog($"[DeletePageMonitor] ✔ Xóa monitor PageID={pageID}");
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("❌ DeletePageMonitor SQL ERROR: " + ex.Message);
        }
    }
    public DataTable GetMonitoredPages()
    {
        var dt = new DataTable();

        try
        {
            string sql = @"
        SELECT 
            m.PageID,
            p.PageName,
            p.PageLink,
            m.IsAuto,
            m.Status,
            m.TotalPostsScanned,
            m.FirstScanTime,
            m.LastScanTime,
            m.TimeSave
        FROM TablePageMonitor m
        LEFT JOIN TablePageInfo p ON p.PageID = m.PageID
        ORDER BY m.TimeSave DESC;";

            using (var conn = SQLDAO.Instance.OpenConnection())
            using (var da = new SqlDataAdapter(sql, conn))
            {
                da.Fill(dt);
            }
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("❌ GetMonitoredPages SQL ERROR: " + ex.Message);
        }

        return dt;
    }
    public void UpdateMonitorStatus(string pageId, string status)
    {
        try
        {
            string sql = @"
        UPDATE TablePageMonitor
        SET Status = @st,
            LastScanTime = GETDATE()
        WHERE PageID = @id;";

            SQLDAO.Instance.ExecuteNonQuery(sql,
                new Dictionary<string, object>
                {
                {"@st", status},
                {"@id", pageId}
                });
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("❌ UpdateMonitorStatus SQL ERROR: " + ex.Message);
        }
    }
    //==========
    //PAGENOTE
    public class PageNoteRecord
    {
        public PageInfo Info { get; set; }
        public string TimeSave { get; set; }
    }
    public void InsertPageNote(string pageId, DateTime? timeSave)
    {
        string sql = @"INSERT INTO TablePageNote (PageID, TimeSave) VALUES (@PageID, @TimeSave)";

        using (var conn = OpenConnection())
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@PageID", pageId);
            cmd.Parameters.AddWithValue("@TimeSave", (object)timeSave ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }
    }
    public void DeletePageNote(int id)
    {
        string sql = @"DELETE FROM TablePageNote WHERE ID = @ID";

        using (var conn = OpenConnection())
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.ExecuteNonQuery();
        }
    }
    public DataTable GetAllPageNote()
    {
        DataTable dt = new DataTable();

        string sql = @"SELECT ID, PageID, TimeSave 
                   FROM TablePageNote 
                   ORDER BY ID DESC";

        using (var conn = OpenConnection())
        using (var cmd = new SqlCommand(sql, conn))
        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        {
            da.Fill(dt);
        }

        return dt;
    }
    public bool PageNoteExists(string pageId)
    {
        string sql = "SELECT COUNT(1) FROM TablePageNote WHERE PageID = @id";

        object v = ExecuteScalar(sql, new Dictionary<string, object>
    {
        { "@id", pageId }
    });

        return Convert.ToInt32(v) > 0;
    }
    public void DeletePageNote(string pageId)
    {
        ExecuteNonQuery("DELETE FROM TablePageNote WHERE PageID=@id",
            new Dictionary<string, object> { { "@id", pageId } });
    }
    public string GetPageNoteDetail(string pageId)
    {
        string sql = "SELECT TimeSave FROM TablePageNote WHERE PageID=@id";

        object v = ExecuteScalar(sql, new Dictionary<string, object> { { "@id", pageId } });

        return v?.ToString() ?? "Không có ghi chú";
    }
    public int GetIsScanned(string pageId)
    {
        string sql = "SELECT IsScanned FROM TablePageInfo WHERE PageID=@id";

        object v = ExecuteScalar(sql, new Dictionary<string, object> { { "@id", pageId } });

        if (v == null || v == DBNull.Value)
            return 0;

        return Convert.ToInt32(v);
    }
    public void UpdatePageIsScanned(string pageId, int isScanned)
    {
        try
        {
            string sql = @"
            UPDATE TablePageInfo
            SET 
                IsScanned = @scan,
                PageTimeSave = GETDATE()   -- cập nhật thời gian lưu
            WHERE PageID = @id";

            ExecuteNonQuery(sql, new Dictionary<string, object>
        {
            { "@id", pageId },
            { "@scan", isScanned }
        });
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("❌ UpdatePageIsScanned ERROR: " + ex.Message);
        }
    }
    public DateTime? GetTimeLastPost(string pageId)
    {
        if (string.IsNullOrWhiteSpace(pageId))
            return null;

        try
        {
            const string sql = @"
            SELECT TimeLastPost
            FROM TablePageInfo
            WHERE PageID = @id
        ";

            object v = ExecuteScalar(sql, new Dictionary<string, object>
        {
            { "@id", pageId }
        });

            if (v == null || v == DBNull.Value)
                return null;

            // SQL Server DATETIME2 -> DateTime
            return Convert.ToDateTime(v);
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog(
                "[GetTimeLastPost SQL] ERROR: " + ex.Message
            );
            return null;
        }
    }



    //========================
    //==========DATABASE

    //================================////
    //========PERSON
    public List<PersonInfo> GetAllPersons()
    {
        var list = new List<PersonInfo>();

        try
        {
            string sql = @"
        SELECT 
            PersonID, IDFBPerson, PersonLink, PersonName, 
            PersonInfo, PersonNote, PersonTimeSave
        FROM TablePersonInfo
        ORDER BY PersonTimeSave DESC;";

            using (var conn = SQLDAO.Instance.OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    list.Add(new PersonInfo
                    {
                        PersonID = rd["PersonID"]?.ToString() ?? "N/A",
                        IDFBPerson = rd["IDFBPerson"]?.ToString() ?? "N/A",
                        PersonLink = rd["PersonLink"]?.ToString() ?? "N/A",
                        PersonName = rd["PersonName"]?.ToString() ?? "N/A",
                        PersonInfoText = rd["PersonInfo"]?.ToString() ?? "N/A",
                        PersonNote = rd["PersonNote"]?.ToString() ?? "N/A",
                        PersonTimeSave = rd["PersonTimeSave"]?.ToString() ?? "N/A"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("❌ GetAllPersons SQL ERROR: " + ex.Message);
        }

        return list;
    }
    public DataTable GetAllPersonDB()
    {
        DataTable dt = new DataTable();

        try
        {
            string sql = @"
            SELECT 
                PersonID,
                IDFBPerson,
                PersonLink,
                PersonName,
                PersonInfo AS PersonInfoText,
                PersonNote,
                PersonTimeSave
            FROM TablePersonInfo
            ORDER BY PersonTimeSave DESC;";

            using (var conn = OpenConnection())
            using (var cmd = new SqlCommand(sql, conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                da.Fill(dt);
            }

            // Thêm cột STT hiển thị
            dt.Columns.Add("STT", typeof(int)).SetOrdinal(0);

            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["STT"] = i + 1;
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("❌ GetAllPersonDB SQL ERROR: " + ex.Message);
        }

        return dt;
    }

    public void InsertOrIgnorePerson(string personLink, string personName)
    {
        try
        {
            if (string.IsNullOrEmpty(personLink) || personLink == "N/A")
                return;

            string personId = GenerateHashId(personLink);

            string sql = @"
        IF NOT EXISTS (SELECT 1 FROM TablePersonInfo WHERE PersonID = @id)
        BEGIN
            INSERT INTO TablePersonInfo
            (
                PersonID, PersonLink, PersonName, 
                PersonInfo, PersonNote, PersonTimeSave
            )
            VALUES
            (
                @id, @link, @name,
                'N/A', 'N/A', GETDATE()
            );
        END";

            SQLDAO.Instance.ExecuteNonQuery(sql,
                new Dictionary<string, object>
                {
                {"@id", personId},
                {"@link", personLink},
                {"@name", personName}
                });

            Libary.Instance.CreateLog($"[Person] ✔ Lưu person '{personName}' OK");
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("❌ InsertOrIgnorePerson SQL ERROR: " + ex.Message);
        }
    }
    public void DeletePersonFull(string personId)
    {
        using (var conn = OpenConnection())
        using (var tran = conn.BeginTransaction())
        using (var cmd = conn.CreateCommand())
        {
            cmd.Transaction = tran;

            // 1. COMMENT
            cmd.CommandText = @"
            DELETE FROM TablePostComment
            WHERE CommentID IN (SELECT CommentID FROM TableCommentInfo WHERE PersonID=@pid);

            DELETE FROM TableCommentInfo WHERE PersonID=@pid;
        ";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@pid", personId);
            cmd.ExecuteNonQuery();

            // 2. SHARE
            cmd.CommandText = "DELETE FROM TablePostShare WHERE PersonID=@pid;";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@pid", personId);
            cmd.ExecuteNonQuery();

            // 3. BÀI PERSON ĐĂNG
            cmd.CommandText = @"
            SELECT PostID FROM TablePost WHERE PersonIDCreate=@pid;
        ";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@pid", personId);

            List<string> posts = new List<string>();
            using (var rd = cmd.ExecuteReader())
                while (rd.Read())
                    posts.Add(rd["PostID"].ToString());

            foreach (var p in posts)
                DeletePostFull(p); // gọi hàm trên

            // 4. Xóa PERSON
            cmd.CommandText = "DELETE FROM TablePersonInfo WHERE PersonID=@pid;";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@pid", personId);
            cmd.ExecuteNonQuery();

            tran.Commit();
        }
    }
    public void DeleteAllPersons()
    {
        var dt = GetAllPersonDB();

        foreach (DataRow r in dt.Rows)
            DeletePersonFull(r["PersonID"].ToString());

        ExecuteNonQuery("DELETE FROM TablePersonInfo");
    }
    public FBType GetPersonNoteByID(string personId)
    {
        if (string.IsNullOrWhiteSpace(personId))
            return FBType.Unknown;

        string sql = @"
        SELECT PersonNote
        FROM TablePersonInfo
        WHERE PersonID = @id
        ";

        var val = ExecuteScalar(sql, new Dictionary<string, object>
    {
        { "@id", personId }
    });

        return ProcessingHelper.MapPersonNoteToFBType(val?.ToString());
    }
    public (FBType Type, string IdFB)? GetPersonNoteIdFbByLink(string link)
    {
        if (string.IsNullOrWhiteSpace(link))
            return null;

        string sqlNote = @"
    SELECT TOP 1 PersonNote
    FROM TablePersonInfo
    WHERE PersonLink = @link
       OR @link LIKE PersonLink + '%'
    ";

        string sqlId = @"
    SELECT TOP 1 IDFBPerson
    FROM TablePersonInfo
    WHERE PersonLink = @link
       OR @link LIKE PersonLink + '%'
    ";

        var noteVal = ExecuteScalar(sqlNote, new Dictionary<string, object>
    {
        { "@link", link }
    });

        var idVal = ExecuteScalar(sqlId, new Dictionary<string, object>
    {
        { "@link", link }
    });

        string idfb = idVal?.ToString();
        if (string.IsNullOrWhiteSpace(idfb))
            return null;

        return (
            ProcessingHelper.MapPersonNoteToFBType(noteVal?.ToString()),
            idfb
        );
    }


    //Table SHARE
    public void InsertPostShares(IEnumerable<ShareItem> shares)
    {
        if (shares == null) return;

        try
        {
            using (var conn = OpenConnection())
            using (var tran = conn.BeginTransaction())
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;

                // 1) chuẩn bị SQL insert (dùng PageID và PostID resolved)
                cmd.CommandText = @"
                INSERT INTO TablePostShare (PostID, PageID, PersonID, TimeShare)
                VALUES (@postid, @pageid, @personid, @timeshare);
            ";

                cmd.Parameters.Add(new SqlParameter("@postid", SqlDbType.NVarChar, 4000));
                cmd.Parameters.Add(new SqlParameter("@pageid", SqlDbType.NVarChar, 4000));
                cmd.Parameters.Add(new SqlParameter("@personid", SqlDbType.NVarChar, 4000));
                cmd.Parameters.Add(new SqlParameter("@timeshare", SqlDbType.NVarChar, 4000)); // or DATETIME if you pass real time

                foreach (var s in shares)
                {
                    // Resolve PageID từ PageLink (nếu không có -> leave null or Insert Page first)
                    string pageId = ResolvePageIdByLink(conn, s.PageLinkA); // helper (xem dưới)

                    // Resolve PostID bằng PostLink (TablePostInfo.PostLink)
                    string postId = ResolvePostIdByLink(conn, s.PostLinkB);

                    // If cannot resolve postId, you can skip or insert a minimal PostInfo first
                    if (string.IsNullOrEmpty(postId) || string.IsNullOrEmpty(pageId))
                    {
                        // choose behavior: skip, or insert placeholder
                        continue;
                    }

                    cmd.Parameters["@postid"].Value = postId;
                    cmd.Parameters["@pageid"].Value = pageId;
                    cmd.Parameters["@personid"].Value = DBNull.Value; // no person here
                    cmd.Parameters["@timeshare"].Value = s.ShareTimeReal ?? (object)s.ShareTimeRaw ?? DBNull.Value;

                    cmd.ExecuteNonQuery();
                }

                tran.Commit();
            }
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("❌ InsertPostShares error: " + ex.Message);
        }
    }
    private string ResolvePageIdByLink(SqlConnection conn, string pageLink)
    {
        if (string.IsNullOrWhiteSpace(pageLink)) return null;
        string sql = "SELECT PageID FROM TablePageInfo WHERE PageLink = @link";
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@link", pageLink);
            var r = cmd.ExecuteScalar();
            return r?.ToString();
        }
    }
    private string ResolvePostIdByLink(SqlConnection conn, string postLink)
    {
        if (string.IsNullOrWhiteSpace(postLink)) return null;
        string sql = "SELECT PostID FROM TablePostInfo WHERE PostLink = @link";
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@link", postLink);
            var r = cmd.ExecuteScalar();
            return r?.ToString();
        }
    }
    public void InsertShareList(List<ShareItem> shares)
    {
        if (shares == null || shares.Count == 0)
            return;
        try
        {
            using (var conn = OpenConnection())
            {
                foreach (var sh in shares)
                {
                    // 1️⃣ Resolve PageID từ PageLink A
                    string pageIdA = GetPageIdByPageLink(conn, sh.PageLinkA);
                    if (string.IsNullOrEmpty(pageIdA))
                        continue;

                    // 2️⃣ Resolve PostID từ PostLink B
                    string postIdB = GetPostIdByPostLink(sh.PostLinkB);
                    if (string.IsNullOrEmpty(postIdB))
                        continue;

                    // 3️⃣ Check duplicate
                    string checkSql = @"
                    SELECT 1 FROM TablePostShare
                    WHERE PostID=@post
                      AND PageID=@page
                      AND ISNULL(PersonID,'') = ''
                      AND ISNULL(TimeShare,'') = ISNULL(@time,'')
                ";

                    using (var checkCmd = new SqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@post", postIdB);
                        checkCmd.Parameters.AddWithValue("@page", pageIdA);
                        checkCmd.Parameters.AddWithValue("@time",
                            (object)sh.ShareTimeReal ?? DBNull.Value);

                        var exists = checkCmd.ExecuteScalar();
                        if (exists != null)
                            continue;   // đã tồn tại → bỏ qua
                    }

                    // 4️⃣ INSERT SHARE
                    string insertSql = @"
                    INSERT INTO TablePostShare (PostID, PageID, PersonID, TimeShare)
                    VALUES (@post, @page, NULL, @time)
                ";

                    using (var cmd = new SqlCommand(insertSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@post", postIdB);
                        cmd.Parameters.AddWithValue("@page", pageIdA);
                        cmd.Parameters.AddWithValue("@time",
                            (object)sh.ShareTimeReal ?? DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }

                    Libary.Instance.CreateLog("✔ Insert TableShare OK");
                }
            }
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("❌ InsertShareList error: " + ex.Message);
        }
    }

    //=================check tồn tại
    public (string pageId, bool isNew) CheckPageLink(string pageLink)
    {
        if (string.IsNullOrWhiteSpace(pageLink))
            return (null, false);

        string sql = @"
        SELECT PageID 
        FROM TablePageInfo
        WHERE PageLink = @PageLink
    ";

        using (var conn = SQLDAO.Instance.OpenConnection())
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@PageLink", pageLink);

            var result = cmd.ExecuteScalar();

            if (result != null)
            {
                // ĐÃ CÓ TRONG DB
                return (result.ToString(), false);
            }
        }

        // CHƯA CÓ → Tạo PageID mới
        string newPageId = GenerateHashId(pageLink);
        return (newPageId, true);
    }
    public void DeleteAllRowsInTables(SqlConnection conn, List<string> tables)
    {
        using (var cmd = conn.CreateCommand())
        {
            foreach (var table in tables)
            {
                cmd.CommandText = $"DELETE FROM {table};";
                cmd.ExecuteNonQuery();
            }
        }
    }
    // LẤY ID PERSON CÓ TRONG DB
    public string GetIDFBPersonByLink(string personLink)
    {
        if (string.IsNullOrWhiteSpace(personLink))
            return null;

        string sql = @"
        SELECT IDFBPerson
        FROM TablePersonInfo
        WHERE PersonLink = @link
          AND IDFBPerson IS NOT NULL
          AND IDFBPerson <> 'N/A'
    ";

        object v = ExecuteScalar(sql, new Dictionary<string, object>
    {
        { "@link", personLink }
    });

        return v?.ToString();
    }

    // xem page đã quét chưa
    public int ExecuteScalarInt(string query, Dictionary<string, object> parameters = null)
    {
        object v = ExecuteScalar(query, parameters);
        if (v == null || v == DBNull.Value) return 0;
        return Convert.ToInt32(v);
    }
    //=======================////
    ///===========PHẦN TOPIC=====//
    public void EnsureTopicTables()
    {
        string sql = @"
 --------------------------------------------------------
 IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TableTopic')
 BEGIN
     CREATE TABLE TableTopic (
         TopicId INT IDENTITY(1,1) PRIMARY KEY,
         TopicName NVARCHAR(500) NOT NULL UNIQUE,
         TopicInfor NVARCHAR(MAX)
     );
 END

 --------------------------------------------------------
 IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TableKeyword')
 BEGIN
     CREATE TABLE TableKeyword (
         KeywordId INT IDENTITY(1,1) PRIMARY KEY,
         KeywordName NVARCHAR(500) NOT NULL UNIQUE
     );
 END

 --------------------------------------------------------
 IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TableTopicKey')
 BEGIN
     CREATE TABLE TableTopicKey (
         Id INT IDENTITY(1,1) PRIMARY KEY,
         TopicId INT NOT NULL,
         KeywordId INT NOT NULL,
         CONSTRAINT UQ_TopicKey UNIQUE(TopicId, KeywordId)
     );
 END

 --------------------------------------------------------
 IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TableTopicPost')
 BEGIN
     CREATE TABLE TableTopicPost (
         Id INT IDENTITY(1,1) PRIMARY KEY,
         TopicId INT NOT NULL,
         PostId NVARCHAR(200) NOT NULL,
         CONSTRAINT UQ_TopicPost UNIQUE(TopicId, PostId)
     );
 END
    ";

        ExecuteNonQuery(sql);
    }


    // SQL MỞI THEO VIEMMODEL
 
    public int GetPostIdByLink(string postLink)
    {
        if (string.IsNullOrWhiteSpace(postLink))
            return 0;

        const string sql = @"
        SELECT TOP 1 PostID
        FROM TablePostInfo
        WHERE PostLink = @link;
    ";

        object result = ExecuteScalar(sql, new Dictionary<string, object>
        {
            ["@link"] = postLink
        });

        return result != null && int.TryParse(result.ToString(), out int id)
            ? id
            : 0;
    }
    public string EnsurePerson(string name, string link)
    {
        if (string.IsNullOrWhiteSpace(link))
            return null;

        // 1️⃣ Thử lấy PersonID theo link
        const string selectSql = @"
        SELECT TOP 1 PersonID
        FROM TablePersonInfo
        WHERE PersonLink = @link;
    ";

        object existing = ExecuteScalar(selectSql, new Dictionary<string, object>
        {
            ["@link"] = link
        });

        if (existing != null)
            return existing.ToString();

        // 2️⃣ Chưa có → insert
        const string insertSql = @"
        INSERT INTO TablePersonInfo
        (
            PersonName,
            PersonLink,
            PersonTimeSave
        )
        VALUES
        (
            @name,
            @link,
            SYSDATETIME()
        );

        SELECT SCOPE_IDENTITY();
    ";

        object newId = ExecuteScalar(insertSql, new Dictionary<string, object>
        {
            ["@name"] = name ?? "",
            ["@link"] = link
        });

        return newId?.ToString();
    }
    public void InsertTablePostShare(string postID, string pageID,string personID, string timeShare, DateTime? realTimeShare)
    {
        const string sql = @"
    IF NOT EXISTS (
        SELECT 1 FROM TablePostShare
        WHERE PostID = @postID
          AND ISNULL(PageID, '') = ISNULL(@pageID, '')
          AND ISNULL(PersonID, '') = ISNULL(@personID, '')
          AND ISNULL(TimeShare, '') = ISNULL(@timeShare, '')
    )
    BEGIN
        INSERT INTO TablePostShare
        (
            PostID,
            PageID,
            PersonID,
            TimeShare,
            RealTimeShare
        )
        VALUES
        (
            @postID,
            @pageID,
            @personID,
            @timeShare,
            @realTimeShare
        );
    END;
    ";

        ExecuteNonQuery(sql, new Dictionary<string, object>
        {
            ["@postID"] = postID,
            ["@pageID"] = (object)pageID ?? DBNull.Value,
            ["@personID"] = (object)personID ?? DBNull.Value,
            ["@timeShare"] = timeShare ?? "",
            ["@realTimeShare"] = (object)realTimeShare ?? DBNull.Value
        });
    }
    public  string GetPostStatusUI(string postType)
    {
        if (string.IsNullOrEmpty(postType))
            return "Tự đăng";

        if (postType.StartsWith("Share", StringComparison.OrdinalIgnoreCase))
            return "Bài share";

        if (postType.StartsWith("Page", StringComparison.OrdinalIgnoreCase))
            return "Bài gốc";

        return "Tự đăng";
    }

}
