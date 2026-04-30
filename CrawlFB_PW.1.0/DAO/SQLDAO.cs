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
using CrawlFB_PW._1._0.ViewModels;
using System.Data.Common;
using CrawlFB_PW._1._0.DAO.phantich;
using CrawlFB_PW._1._0.DAO.Data;
using CrawlFB_PW._1._0.Helper.Data;
//using DevExpress.XtraPrinting;

public class SQLDAO
{
    private static SQLDAO instance;
    private readonly string _connectionString;
    private SqlConnection _connection;
    private SqlTransaction _transaction;
    public static SQLDAO Instance
    {
        get
        {
            if (instance == null)
                instance = new SQLDAO();
            return instance;
        }
    }
    private SQLDAO()
    {
        // 👉 Sửa chuỗi này nếu đổi server
        _connectionString = @"Server=DESKTOP-GRC118H\SQLEXPRESS;Database=AutoScanDB;Trusted_Connection=True;";
    }

    // ======================================================
    //  HÀM CƠ BẢN: GetConnection (chưa open) + OpenConnection
    // ======================================================

    /// <summary>
    /// Trả về SqlConnection (CHƯA mở) — để caller chủ động Open().
    /// </summary>
    /*public SqlConnection GetConnection()
    {
        return new SqlConnection(connectionString);
    }*/
    public SqlConnection GetConnection()
    {
        return _connection ?? new SqlConnection(_connectionString);
    }


    /// <summary>
    /// Trả về SqlConnection đã Open() — dùng nhanh cho query.
    /// </summary>
    /*public SqlConnection OpenConnection()
    {
        var conn = new SqlConnection(connectionString);
        conn.Open();
        return conn; // caller phải Dispose !
    }
    */
    public SqlConnection OpenConnection()
    {
        if (_connection != null)
            return _connection;

        var conn = new SqlConnection(_connectionString);
        conn.Open();
        return conn;
    }

    // ======================================================
    //  HÀM TEST KẾT NỐI
    // ======================================================
   /* public bool TestConnection()
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
    }*/
    public bool TestConnection()
    {
        try
        {
            using (var conn = new SqlConnection(_connectionString))
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
    /*public DataTable ExecuteQuery(string query, Dictionary<string, object> parameters = null)
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
    */
    public DataTable ExecuteQuery(
     string sql,
     Dictionary<string, object> parameters = null)
    {
        using (var cmd = new SqlCommand(sql))
        {
            // 1️⃣ Connection
            cmd.Connection = _connection ?? new SqlConnection(_connectionString);

            // 2️⃣ Transaction – chỉ khi còn hiệu lực
            if (_transaction != null && _transaction.Connection != null)
            {
                cmd.Transaction = _transaction;
            }

            // 3️⃣ Parameters
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    cmd.Parameters.AddWithValue(
                        p.Key,
                        p.Value ?? DBNull.Value
                    );
                }
            }

            // 4️⃣ Open connection
            if (cmd.Connection.State != ConnectionState.Open)
                cmd.Connection.Open();

            // 5️⃣ Fill DataTable
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                da.Fill(dt);

                // 6️⃣ Dispose connection nếu ngoài transaction
                if (_transaction == null)
                    cmd.Connection.Dispose();

                return dt;
            }
        }
    }

    /* public int ExecuteNonQuery(string query, Dictionary<string, object> parameters = null)
     {
         using (var conn = OpenConnection())
         using (var cmd = new SqlCommand(query, conn))
         {
             AddParams(cmd, parameters);
             return cmd.ExecuteNonQuery();
         }
     }
    */
    public int ExecuteNonQuery(
     string sql,
     Dictionary<string, object> parameters = null)
    {
        using (var cmd = new SqlCommand(sql))
        {
            // 1️⃣ Connection
            cmd.Connection = _connection ?? new SqlConnection(_connectionString);

            // 2️⃣ Transaction – CHỈ gắn khi còn ACTIVE
            if (_transaction != null && _transaction.Connection != null)
            {
                cmd.Transaction = _transaction;
            }

            // 3️⃣ Parameters
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    cmd.Parameters.AddWithValue(
                        p.Key,
                        p.Value ?? DBNull.Value
                    );
                }
            }

            // 4️⃣ Open connection nếu cần
            if (cmd.Connection.State != ConnectionState.Open)
                cmd.Connection.Open();

            int affected = cmd.ExecuteNonQuery();

            // 5️⃣ Chỉ dispose connection nếu KHÔNG trong transaction
            if (_transaction == null)
                cmd.Connection.Dispose();

            return affected;
        }
    }

    public void ExecuteInTransaction(Action action)
    {
        // Tránh lồng transaction
        if (_transaction != null)
        {
            action.Invoke();
            return;
        }

        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();

            using (var tran = conn.BeginTransaction())
            {
                try
                {
                    _connection = conn;
                    _transaction = tran;

                    action.Invoke();

                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
                finally
                {
                    if (_transaction != null)
                        _transaction.Dispose();

                    _transaction = null;
                    _connection = null;
                }
            }
        }
    }


    /*public object ExecuteScalar(string query, Dictionary<string, object> parameters = null)
    {
        using (var conn = OpenConnection())
        using (var cmd = new SqlCommand(query, conn))
        {
            AddParams(cmd, parameters);
            return cmd.ExecuteScalar();
        }
    }*/
    public object ExecuteScalar(
    string sql,
    Dictionary<string, object> parameters = null)
    {
        using (var cmd = new SqlCommand(sql))
        {
            // 1️⃣ Connection
            cmd.Connection = _connection ?? new SqlConnection(_connectionString);

            // 2️⃣ Transaction – chỉ khi còn hiệu lực
            if (_transaction != null && _transaction.Connection != null)
            {
                cmd.Transaction = _transaction;
            }

            // 3️⃣ Parameters
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    cmd.Parameters.AddWithValue(
                        p.Key,
                        p.Value ?? DBNull.Value
                    );
                }
            }

            // 4️⃣ Open connection
            if (cmd.Connection.State != ConnectionState.Open)
                cmd.Connection.Open();

            object result = cmd.ExecuteScalar();

            // 5️⃣ Dispose connection nếu ngoài transaction
            if (_transaction == null)
                cmd.Connection.Dispose();

            return result == DBNull.Value ? null : result;
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
    public T ExecuteScalar<T>(string sql, Dictionary<string, object> parameters)
    {
        var result = ExecuteScalar(sql, parameters);

        if (result == null || result == DBNull.Value)
            return default(T);

        return (T)Convert.ChangeType(result, typeof(T));
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
                    KeywordName NVARCHAR(500) NOT NULL UNIQUE,
                    CreatedTime DATETIME NOT NULL
                    CONSTRAINT DF_TableKeyword_CreatedTime DEFAULT GETDATE()
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
                    CreatedTime DATETIME NOT NULL
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
        -----------------------
            IF NOT EXISTS (
                SELECT 1 
                FROM sys.columns 
                WHERE Name = N'CreatedTime'
                  AND Object_ID = Object_ID(N'TableKeyword')
            )
            BEGIN
                ALTER TABLE TableKeyword
                ADD CreatedTime DATETIME NOT NULL
                    CONSTRAINT DF_TableKeyword_CreatedTime
                    DEFAULT GETDATE();
            END
          -----------------
            IF NOT EXISTS (
                SELECT 1 
                FROM sys.columns 
                WHERE Name = N'CreatedTime'
                  AND Object_ID = Object_ID(N'TableTopicPost')
            )
            BEGIN
                ALTER TABLE TableTopicPost
                ADD CreatedTime DATETIME NOT NULL
                    CONSTRAINT DF_TableTopicPost_CreatedTime
                    DEFAULT GETDATE();
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
    public void EnsureTempTables()
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();

            var sql = @"

                --------------------------------------------------------
                -- POST TEMP (CHỈ FIELD CẦN INSERT)
                --------------------------------------------------------
                IF OBJECT_ID('TablePostInfo_Temp', 'U') IS NULL
                BEGIN
                    CREATE TABLE TablePostInfo_Temp (
                        PostID NVARCHAR(200),
                        PostLink NVARCHAR(MAX),
                        PostContent NVARCHAR(MAX),
                        PostTime NVARCHAR(200),
                        RealPostTime DATETIME2(0),
                        LikeCount INT,
                        ShareCount INT,
                        CommentCount INT,
                        PostAttachment NVARCHAR(MAX),
                        PostStatus NVARCHAR(200)
                    );
                END

                --------------------------------------------------------
                -- PAGE TEMP
                --------------------------------------------------------
                IF OBJECT_ID('TablePageInfo_Temp', 'U') IS NULL
                BEGIN
                    CREATE TABLE TablePageInfo_Temp (
                        PageID NVARCHAR(200),
                        IDFBPage NVARCHAR(200),
                        PageLink NVARCHAR(MAX),
                        PageName NVARCHAR(500)
                    );
                END

                --------------------------------------------------------
                -- PERSON TEMP
                --------------------------------------------------------
                IF OBJECT_ID('TablePersonInfo_Temp', 'U') IS NULL
                BEGIN
                    CREATE TABLE TablePersonInfo_Temp (
                        PersonID NVARCHAR(200),
                        PersonLink NVARCHAR(MAX),
                        PersonName NVARCHAR(500),
                        PersonNote NVARCHAR(MAX)
                    );
                END

                --------------------------------------------------------
                -- POST MAP TEMP
                --------------------------------------------------------
                IF OBJECT_ID('TablePost_Temp', 'U') IS NULL
                BEGIN
                    CREATE TABLE TablePost_Temp (
                        PostID NVARCHAR(200),
                        PageIDCreate NVARCHAR(200),
                        PageIDContainer NVARCHAR(200),
                        PersonIDCreate NVARCHAR(200)
                    );
                END

                --------------------------------------------------------
                -- SHARE TEMP
                --------------------------------------------------------
                IF OBJECT_ID('TablePostShare_Temp', 'U') IS NULL
                BEGIN
                    CREATE TABLE TablePostShare_Temp (
                        PostID NVARCHAR(200),
                        PageID NVARCHAR(200),
                        PersonID NVARCHAR(200),
                        TimeShare NVARCHAR(200),
                        RealTimeShare DATETIME2(0)
                    );
                END
                                --------------------------------------------------------
                -- SEED DATA MẶC ĐỊNH
                --------------------------------------------------------

                IF NOT EXISTS (
                    SELECT 1 FROM TablePersonInfo 
                    WHERE PersonID = '00000000'
                )
                BEGIN
                    INSERT INTO TablePersonInfo (PersonID, PersonName)
                    VALUES ('00000000', N'Ẩn danh');
                END

                ";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
    public void ClearTempTables()
    {
        try
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                var sql = @"

IF OBJECT_ID('TablePostInfo_Temp', 'U') IS NOT NULL
    TRUNCATE TABLE TablePostInfo_Temp;

IF OBJECT_ID('TablePageInfo_Temp', 'U') IS NOT NULL
    TRUNCATE TABLE TablePageInfo_Temp;

IF OBJECT_ID('TablePersonInfo_Temp', 'U') IS NOT NULL
    TRUNCATE TABLE TablePersonInfo_Temp;

IF OBJECT_ID('TablePost_Temp', 'U') IS NOT NULL
    TRUNCATE TABLE TablePost_Temp;

IF OBJECT_ID('TablePostShare_Temp', 'U') IS NOT NULL
    TRUNCATE TABLE TablePostShare_Temp;

";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ ClearTempTables error: " + ex.Message);
        }
    }
    public void EnsureEvaluationTables()
    {
        string sql = @"
               -- =========================
        -- ATTENTION KEYWORD SCORE
        -- =========================
        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TableAttentionKeywordScore')
        BEGIN
            CREATE TABLE TableAttentionKeywordScore (
                KeywordId INT PRIMARY KEY,
                Score INT NOT NULL DEFAULT 0 CHECK (Score BETWEEN 0 AND 30),
                TrackingLevel INT NOT NULL DEFAULT 1 CHECK (TrackingLevel BETWEEN 1 AND 5),
                Note NVARCHAR(200),

                CONSTRAINT FK_Attention_Keyword
                    FOREIGN KEY (KeywordId)
                    REFERENCES TableKeyword(KeywordId)
                    ON DELETE CASCADE
            );
        END
        ELSE
        BEGIN
            -- Thêm TrackingLevel nếu DB cũ chưa có
            IF COL_LENGTH('TableAttentionKeywordScore', 'TrackingLevel') IS NULL
            BEGIN
                ALTER TABLE TableAttentionKeywordScore
                ADD TrackingLevel INT NOT NULL DEFAULT 1
                CHECK (TrackingLevel BETWEEN 1 AND 5);
            END
        END
                  -- =========================
            -- NEGATIVE KEYWORD SCORE
            -- =========================
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TableNegativeKeywordScore')
            BEGIN
                CREATE TABLE TableNegativeKeywordScore (
                    KeywordId INT PRIMARY KEY,
                    Score INT NOT NULL DEFAULT 0 CHECK (Score BETWEEN 0 AND 50),
                    NegativeLevel INT NOT NULL DEFAULT 1 CHECK (NegativeLevel BETWEEN 1 AND 5),
                    IsCritical BIT DEFAULT 0,
                    Note NVARCHAR(200),

                    CONSTRAINT FK_Negative_Keyword
                        FOREIGN KEY (KeywordId)
                        REFERENCES TableKeyword(KeywordId)
                        ON DELETE CASCADE
                );
            END
            ELSE
            BEGIN
                -- Thêm NegativeLevel nếu DB cũ chưa có
                IF COL_LENGTH('TableNegativeKeywordScore', 'NegativeLevel') IS NULL
                BEGIN
                    ALTER TABLE TableNegativeKeywordScore
                    ADD NegativeLevel INT NOT NULL DEFAULT 1
                    CHECK (NegativeLevel BETWEEN 1 AND 5);
                END
                END
     -- =========================
        -- EXCLUDE KEYWORD
        -- =========================
        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TableExcludeKeyword')
        BEGIN
                   CREATE TABLE TableExcludeKeyword (
            KeywordId INT PRIMARY KEY,
            Level INT NOT NULL DEFAULT 1 CHECK (Level BETWEEN 1 AND 7),
            Note NVARCHAR(200),
            CreatedAt DATETIME2 DEFAULT SYSDATETIME(),

            CONSTRAINT FK_Exclude_Keyword
                FOREIGN KEY (KeywordId)
                REFERENCES TableKeyword(KeywordId)
                ON DELETE CASCADE
        );

        END
            -- Nếu bảng đã tồn tại nhưng thiếu Level
            IF EXISTS (SELECT * FROM sysobjects WHERE name='TableExcludeKeyword')
            BEGIN
                IF COL_LENGTH('TableExcludeKeyword', 'Level') IS NULL
                BEGIN
                    ALTER TABLE TableExcludeKeyword
                    ADD Level INT NOT NULL DEFAULT 1
                    CHECK (Level BETWEEN 1 AND 7);
                END
            END

             -- =========================
        -- POST EVALUATION (FINAL)
        -- =========================
        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TablePostEvaluation')
        BEGIN
          CREATE TABLE TablePostEvaluation (
            PostID NVARCHAR(200) PRIMARY KEY,

            AttentionScore INT NOT NULL DEFAULT 0,
            AttentionLevel INT NOT NULL DEFAULT 0,

            NegativeScore INT NOT NULL DEFAULT 0,
            NegativeLevel INT NOT NULL DEFAULT 0,

            InteractionScore INT NOT NULL DEFAULT 0,

            TotalScore INT NOT NULL DEFAULT 0,

            ResultLevel INT NOT NULL DEFAULT 0,

            AttentionKeywordIds NVARCHAR(MAX) NULL,   -- JSON
            NegativeKeywordIds NVARCHAR(MAX) NULL,    -- JSON

            KeywordVersion INT NOT NULL DEFAULT 1,

            EvaluatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

            CONSTRAINT FK_PostEvaluation_Post
                FOREIGN KEY (PostID)
                REFERENCES TablePostInfo(PostID)
                ON DELETE CASCADE
            );
        END
        -- =========================
        -- KEYWORD VERSION (CACHE CONTROL)
        -- =========================
        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TableKeywordVersion')
        BEGIN
            CREATE TABLE TableKeywordVersion (
                Id INT PRIMARY KEY DEFAULT 1,
                CurrentVersion INT NOT NULL DEFAULT 1,
                LastUpdated DATETIME2 DEFAULT SYSDATETIME()
            );

            -- Insert bản ghi mặc định
            INSERT INTO TableKeywordVersion (Id, CurrentVersion)
            VALUES (1, 1);
        END
        -- =========================
        -- ADD KeywordVersion COLUMN (if not exists)
        -- =========================
        IF EXISTS (SELECT * FROM sysobjects WHERE name='TablePostEvaluation')
        BEGIN
            IF COL_LENGTH('TablePostEvaluation', 'KeywordVersion') IS NULL
            BEGIN
                ALTER TABLE TablePostEvaluation
                ADD KeywordVersion INT NOT NULL DEFAULT 1;
            END
        END
        ";
        ExecuteNonQuery(sql);
    }

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
    public int DeletePosts(List<string> postIds)
    {
        if (postIds == null || postIds.Count == 0)
            return 0;

        var param = new Dictionary<string, object>();
        var paramNames = new List<string>();

        for (int i = 0; i < postIds.Count; i++)
        {
            string key = "@id" + i;
            paramNames.Add(key);
            param[key] = postIds[i];
        }

        string sql = $@"
DELETE FROM TablePostInfo WHERE PostID IN ({string.Join(",", paramNames)});
DELETE FROM TablePost WHERE PostID IN ({string.Join(",", paramNames)});
";

        return SQLDAO.Instance.ExecuteNonQuery(sql, param);
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
    public void InsertOrIgnorePost(PostPage p)
    {
        if (p == null || !ProcessingHelper.IsValidContent(p.PostLink))
            return;

        string postId = GeneratePostId(p.PostLink);

        using (var conn = SQLDAO.Instance.OpenConnection())
        using (var tran = conn.BeginTransaction())
        using (var cmd = new SqlCommand())
        {
            cmd.Connection = conn;
            cmd.Transaction = tran;

            try
            {
                // =================================================
                // 🔥 HELPER: RESOLVE + CREATE PAGE
                // =================================================
                // =========================
                // RESOLVE + CREATE PAGE
                // =========================
                string ResolveOrCreatePageId(string idfb, string link, string name)
                {
                    // normalize link
                    link = ProcessingHelper.NormalizeInputUrl(link);

                    string pageId = null;

                    // =========================
                    // 1️⃣ ƯU TIÊN IDFB
                    // =========================
                    if (ProcessingHelper.IsValidContent(idfb))
                        pageId = idfb;
                    else if (ProcessingHelper.IsValidContent(link))
                        pageId = GenerateHashId(link);
                    else
                        return null;

                    // =========================
                    // 2️⃣ CHECK DB (ID OR LINK)
                    // =========================
                    cmd.CommandText = @"
        SELECT TOP 1 PageID 
        FROM TablePageInfo 
        WHERE PageID=@id OR PageLink=@link";

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@id", SqlDbType.NVarChar, 200).Value = pageId;
                    cmd.Parameters.Add("@link", SqlDbType.NVarChar).Value = link ?? "";

                    var exist = cmd.ExecuteScalar();

                    if (exist != null)
                    {
                        string existId = exist.ToString();

                        // =========================
                        // 🔥 ENRICH (UPDATE THÊM IDFB NẾU CHƯA CÓ)
                        // =========================
                        cmd.CommandText = @"
            UPDATE TablePageInfo
            SET 
                IDFBPage = COALESCE(IDFBPage, @idfb),
                PageLink = COALESCE(NULLIF(@link,''), PageLink),
                PageName = COALESCE(NULLIF(@name,''), PageName)
            WHERE PageID=@id";

                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@id", SqlDbType.NVarChar, 200).Value = existId;
                        cmd.Parameters.Add("@idfb", SqlDbType.NVarChar, 200)
                            .Value = ProcessingHelper.IsValidContent(idfb)
                                ? (object)idfb
                                : DBNull.Value;
                        cmd.Parameters.Add("@link", SqlDbType.NVarChar).Value = link ?? "";
                        cmd.Parameters.Add("@name", SqlDbType.NVarChar, 500).Value = name ?? "";

                        cmd.ExecuteNonQuery();

                        return existId;
                    }

                    // =========================
                    // 3️⃣ INSERT SEED
                    // =========================
                    cmd.CommandText = @"
        INSERT INTO TablePageInfo
        (PageID, IDFBPage, PageLink, PageName, PageType, PageTimeSave)
        VALUES (@id,@idfb,@link,@name,@type,SYSDATETIME());";

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@id", SqlDbType.NVarChar, 200).Value = pageId;

                    cmd.Parameters.Add("@idfb", SqlDbType.NVarChar, 200)
                        .Value = ProcessingHelper.IsValidContent(idfb)
                            ? (object)idfb
                            : DBNull.Value;

                    cmd.Parameters.Add("@link", SqlDbType.NVarChar).Value = link ?? "";
                    cmd.Parameters.Add("@name", SqlDbType.NVarChar, 500).Value = name ?? "";

                    cmd.Parameters.Add("@type", SqlDbType.NVarChar, 50).Value =
                        link != null && link.Contains("/groups/")
                            ? "groups"
                            : "fanpage";

                    cmd.ExecuteNonQuery();

                    return pageId;
                }

                // =================================================
                // 1️⃣ CONTAINER (QUAN TRỌNG NHẤT)
                // =================================================
                string pageContainerId = ResolveOrCreatePageId(
                    p.ContainerIdFB,
                    p.PageLink,
                    p.PageName
                );
                Libary.Instance.LogTech($"[DB-RESOLVE] Post={postId} | ContainerID={pageContainerId} | RawContainer={p.ContainerIdFB} | PageLink={p.PageLink}");
                // =================================================
                // 2️⃣ POSTER
                // =================================================
                string posterPageId = null;
                string posterPersonId = null;

                bool isPagePoster = p.PosterNote == FBType.Fanpage;

                if (isPagePoster)
                {
                    posterPageId = ResolveOrCreatePageId(
                        p.PosterIdFB,
                        p.PosterLink,
                        p.PosterName
                    );
                }
                else
                {
                    if (p.PosterNote == FBType.PersonHidden)
                    {
                        posterPersonId = SystemIds.PERSON_ANONYMOUS_ID;
                    }
                    else if (ProcessingHelper.IsValidContent(p.PosterIdFB))
                    {
                        posterPersonId = p.PosterIdFB;

                        cmd.CommandText = @"
                        IF NOT EXISTS (SELECT 1 FROM TablePersonInfo WHERE PersonID=@id)
                        INSERT INTO TablePersonInfo
                        (PersonID, PersonLink, PersonName, PersonNote, PersonTimeSave)
                        VALUES (@id,@link,@name,@note,SYSDATETIME());";
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@id", SqlDbType.NVarChar, 200).Value = posterPersonId;
                        cmd.Parameters.Add("@link", SqlDbType.NVarChar).Value = p.PosterLink ?? "";
                        cmd.Parameters.Add("@name", SqlDbType.NVarChar, 500).Value = p.PosterName ?? "";
                        cmd.Parameters.Add("@note", SqlDbType.NVarChar).Value = p.PosterNote;

                        cmd.ExecuteNonQuery();
                    }
                }
                Libary.Instance.LogTech($"[DB-POSTER] Post={postId} | PosterPage={posterPageId} | PosterPerson={posterPersonId} | Note={p.PosterNote}");
                // =================================================
                // 3️⃣ REAL TIME
                // =================================================
                DateTime? parsedTime = TimeHelper.ParseFacebookTime(p.PostTime);
                object realPostTime =
                    parsedTime == DateTime.MinValue
                        ? (object)DBNull.Value
                        : parsedTime;
                Libary.Instance.LogTech(
    $"[DB-MAP] Post={postId} | CreatePage={posterPageId} | ContainerPage={pageContainerId} | Person={posterPersonId}"
);
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
                cmd.Parameters.Add("@status", SqlDbType.NVarChar, 200).Value = p.PostType.ToString();

                cmd.ExecuteNonQuery();

                // =================================================
                // 5️⃣ POST MAP (FIX LỖI CŨ)
                // =================================================
                cmd.CommandText = @"
                MERGE TablePost t
                USING (SELECT @id AS PostID) s
                ON t.PostID = s.PostID

                WHEN MATCHED THEN
                 UPDATE SET
                   PageIDCreate = COALESCE(@createPage, t.PageIDCreate),
                   PageIDContainer = COALESCE(@containerPage, t.PageIDContainer),
                   PersonIDCreate = COALESCE(@createPerson, t.PersonIDCreate)

                WHEN NOT MATCHED THEN
                 INSERT (PostID, PageIDCreate, PageIDContainer, PersonIDCreate)
                 VALUES (@id,@createPage,@containerPage,@createPerson);";

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
    // dùng save post Auto 
    public int InsertPostBatchAuto(List<PostPage> posts)
    {
        if (posts == null || posts.Count == 0)
            return 0;

        int saved = 0;

        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();

            using (var tran = conn.BeginTransaction())
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.Transaction = tran;

                try
                {
                    cmd.CommandText = @"
                IF NOT EXISTS (SELECT 1 FROM TablePostInfo WHERE PostID = @id)
                BEGIN
                    INSERT INTO TablePostInfo
                    (
                        PostID, PostLink, PostContent,
                        PostTime, RealPostTime,
                        LikeCount, ShareCount, CommentCount,
                        PostAttachment, PostStatus, PostTimeSave
                    )
                    VALUES
                    (
                        @id, @link, @content,
                        @timeRaw, @timeReal,
                        @like, @share, @comment,
                        @attachment, @status, SYSDATETIME()
                    )
                END
                ";

                    foreach (var p in posts)
                    {
                        if (p == null || string.IsNullOrEmpty(p.PostLink))
                            continue;

                        string postId = GeneratePostId(p.PostLink);

                        DateTime? parsedTime = p.RealPostTime ??
                                               TimeHelper.ParseFacebookTime(p.PostTime);

                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@id", postId);
                        cmd.Parameters.AddWithValue("@link", p.PostLink ?? "");
                        cmd.Parameters.AddWithValue("@content", p.Content ?? "");
                        cmd.Parameters.AddWithValue("@timeRaw", p.PostTime ?? "");
                        cmd.Parameters.AddWithValue("@timeReal",
                            parsedTime ?? (object)DBNull.Value);

                        cmd.Parameters.AddWithValue("@like", p.LikeCount ?? 0);
                        cmd.Parameters.AddWithValue("@share", p.ShareCount ?? 0);
                        cmd.Parameters.AddWithValue("@comment", p.CommentCount ?? 0);

                        cmd.Parameters.AddWithValue("@attachment", p.Attachment ?? "");
                        cmd.Parameters.AddWithValue("@status", p.PostType.ToString());

                        cmd.ExecuteNonQuery();

                        saved++;
                    }

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Libary.Instance.LogTech("❌ InsertPostBatchAuto ERROR: " + ex.Message);
                }
            }
        }

        return saved;
    }
    // V2
    
    public int InsertPostBatchAuto_V3(
    List<PostPage> posts,
    List<ShareItem> shares,
    string pageCrawlId,
    string pageCrawlLink)
    {
        if (posts == null || posts.Count == 0) return 0;

        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();

            // =========================================
            // 1. FILTER + POSTID
            // =========================================
            var validPosts = posts
                .Where(p => !string.IsNullOrEmpty(p.PostLink))
                .ToList();

            var postIdMap = validPosts.ToDictionary(
                p => p,
                p => GeneratePostId(
                    UrlHelper.ShortenFacebookPostLink(p.PostLink)
                )
            );

            // =========================================
            // 2. LOAD EXISTING POST
            // =========================================
            var existing = SqlBulkHelper.LoadExistingPostIds(conn, postIdMap.Values.ToList());

            var newPosts = validPosts
                .Where(p => !existing.Contains(postIdMap[p]))
                .ToList();

            if (newPosts.Count == 0) return 0;

            // =========================================
            // 3. 🔥 RESOLVE PAGE + PERSON (QUAN TRỌNG NHẤT)
            // =========================================
            var pageMap = new Dictionary<string, string>();
            var pageResolveCache = new Dictionary<string, string>();
            var personMap = new Dictionary<string, string>();
            var personResolveCache = new Dictionary<string, string>();
            // 🔥 seed anonymous
            personMap[SystemIds.PERSON_ANONYMOUS_ID] = SystemIds.PERSON_ANONYMOUS_ID;

            foreach (var p in newPosts)
            {
                Libary.Instance.LogForm("savelog",
    $"[INPUT] Link={p.PostLink} | PosterIdFB='{p.PosterIdFB}' | PosterLink='{p.PosterLink}' | PosterNote={p.PosterNote}");
                // =========================================
                // 🔥 PAGE CONTAINER
                // =========================================
                string containerKey = !string.IsNullOrEmpty(p.ContainerIdFB)
                    ? p.ContainerIdFB
                    : UrlHelper.NormalizeFacebookUrl(p.PageLink);

                if (!string.IsNullOrEmpty(containerKey) && !pageMap.ContainsKey(containerKey))
                {
                    if (!pageResolveCache.TryGetValue(containerKey, out var pageId))
                    {
                        pageId = SQLDAO.Instance.ResolvePageId(p.ContainerIdFB, p.PageLink);
                        pageResolveCache[containerKey] = pageId;
                    }

                    pageMap[containerKey] = pageId;
                }
                Libary.Instance.LogForm("savelog", $"[DEBUG] Fanpage detected | PosterId={p.PosterIdFB} , PosterNote = {p.PosterNote}");
                // =========================================
                // 🔥 POSTER
                // =========================================
                if (string.IsNullOrEmpty(p.PosterIdFB) && string.IsNullOrEmpty(p.PosterLink) && p.PosterNote != FBType.PersonHidden && p.PosterNote != FBType.Unknown)
                    continue;
                if (p.PosterNote == FBType.PersonHidden || p.PosterNote == FBType.Unknown)
                {
                    p.PosterIdFB = SystemIds.PERSON_ANONYMOUS_ID;
                }
                switch (p.PosterNote)
                {
                    // =========================
                    // 🔥 FANPAGE
                    // =========================
                    case FBType.Fanpage:
                        {
                            string pagekey = p.PosterIdFB;
                            if (!string.IsNullOrEmpty(pagekey) && !pageMap.ContainsKey(pagekey))
                            {
                                if (!pageResolveCache.TryGetValue(pagekey, out var pageId))
                                {
                                    pageId = SQLDAO.Instance.ResolvePageId(p.PosterIdFB, p.PosterLink);
                                    pageResolveCache[pagekey] = pageId;
                                }
                                pageMap[pagekey] = pageId;
                                Libary.Instance.LogForm("savelog",$"[PAGE_MAP_ADD] Fanpage {pagekey} → {pageId}");
                            }
                            break;
                        }

                    // =========================
                    // 🔥 PERSON
                    // =========================
                    case FBType.Person:
                    case FBType.PersonKOL:
                        {
                            string key = !string.IsNullOrEmpty(p.PosterIdFB)
                                ? p.PosterIdFB
                                : UrlHelper.NormalizeFacebookUrl(p.PosterLink);

                            if (!personMap.ContainsKey(key))
                            {
                                if (!personResolveCache.TryGetValue(key, out var personId))
                                {
                                    personId = SQLDAO.Instance.ResolvePersonId(p.PosterIdFB, p.PosterLink);
                                    personResolveCache[key] = personId;
                                }

                                personMap[key] = personId;
                            }
                            break;
                        }

                    // =========================
                    // 🔥 ANONYMOUS / UNKNOWN
                    // =========================
                    case FBType.PersonHidden:
                    case FBType.Unknown:
                        {
                            string key = SystemIds.PERSON_ANONYMOUS_ID;

                            if (!personMap.ContainsKey(key))
                            {
                                personMap[key] = SystemIds.PERSON_ANONYMOUS_ID;
                            }

                            break;
                        }
                }
            }
            Libary.Instance.LogForm("savelog", $"[PAGE_MAP] Count = {pageMap.Count}");

            foreach (var kv in pageMap)
            {
                Libary.Instance.LogForm("savelog", $"[PAGE_MAP] Key: {kv.Key} | PageId: {kv.Value}");
            }

            // =========================================
            // 4. BUILD DATATABLE
            // =========================================
            var builder = new PostBatchBuilder();

            var dtPost = builder.BuildPostTable(newPosts, postIdMap);

            var dtPage = builder.BuildPageTable(newPosts, pageMap);

            var dtPerson = builder.BuildPersonTable( personMap, newPosts );

            var dtMap = builder.BuildPostMapTable_V4(
                newPosts,
                postIdMap,
                pageMap,
                personMap,
                pageCrawlId
            );
           
            // =========================================
            // 5. MAP SHARE
            // =========================================
            if (shares != null && shares.Count > 0)
            {
                foreach (var share in shares)
                {
                    var post = validPosts.FirstOrDefault(p =>
                        UrlHelper.ShortenFacebookPostLink(p.PostLink) ==
                        UrlHelper.ShortenFacebookPostLink(share.PostLinkB));

                    if (post != null)
                    {
                        share.PostID_B = postIdMap[post];
                    }
                }
            }

            var dtShare = builder.BuildShareTable(shares);

            // =========================================
            // 6. BULK COPY
            // =========================================
            SqlBulkHelper.BulkCopy(conn, "TablePostInfo_Temp", dtPost);
            SqlBulkHelper.BulkCopy(conn, "TablePageInfo_Temp", dtPage);
            SqlBulkHelper.BulkCopy(conn, "TablePersonInfo_Temp", dtPerson);
            SqlBulkHelper.BulkCopy(conn, "TablePost_Temp", dtMap);
            SqlBulkHelper.BulkCopy(conn, "TablePostShare_Temp", dtShare);

            // =========================================
            // 7. MERGE
            // =========================================
            ExecuteMerge(conn);

            return newPosts.Count;
        }
    }
    public string ResolvePageId(string idfb, string link)
    {
        var normalizedLink = UrlHelper.NormalizeFacebookUrl(link);

        // =========================================
        // 🔥 1. Có IDFB → check DB theo IDFB
        // =========================================
        if (!string.IsNullOrEmpty(idfb))
        {
            var page = GetPageByIDFB(idfb);
            if (page != null)
                return page.PageID;
        }

        // =========================================
        // 🔥 2. Check DB theo LINK (QUAN TRỌNG)
        // =========================================
        if (!string.IsNullOrEmpty(normalizedLink))
        {
            var page = GetPageByLink(normalizedLink); // 👈 bạn cần có hàm này
            if (page != null)
                return page.PageID;
        }

        // =========================================
        // 🔥 3. Nếu có IDFB → dùng IDFB
        // =========================================
        if (!string.IsNullOrEmpty(idfb))
        {
            return idfb;
        }

        // =========================================
        // 🔥 4. Không có IDFB → hash LINK
        // =========================================
        if (!string.IsNullOrEmpty(normalizedLink))
        {
            return GenerateStableId(normalizedLink);
        }

        return null;
    }
    public string ResolvePersonId(string idfb, string link)
    {
        var normalizedLink = UrlHelper.NormalizeFacebookUrl(link);

        // 🔥 1. ưu tiên IDFB
        if (!string.IsNullOrEmpty(idfb))
        {
            var person = GetPersonByID(idfb);
            if (person != null)
                return person.PersonID;
        }

        // 🔥 2. check theo link
        if (!string.IsNullOrEmpty(normalizedLink))
        {
            var person = GetPersonByLink(normalizedLink);
            if (person != null)
                return person.PersonID;
        }

        // 🔥 3. fallback
        if (!string.IsNullOrEmpty(idfb))
            return idfb;

        if (!string.IsNullOrEmpty(normalizedLink))
            return GenerateStableId(normalizedLink);

        return SystemIds.PERSON_ANONYMOUS_ID;
    }
    public PageInfo GetPageByLink(string link)
    {
        const string sql = @"
        SELECT TOP 1 *
        FROM TablePageInfo
        WHERE PageLink = @link
    ";

        using (var conn = OpenConnection())
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.Add("@link", SqlDbType.NVarChar).Value = link;

            using (var rd = cmd.ExecuteReader())
            {
                if (rd.Read())
                {
                    return new PageInfo
                    {
                        PageID = rd["PageID"].ToString(),
                        IDFBPage = rd["IDFBPage"]?.ToString(),
                        PageLink = rd["PageLink"]?.ToString(),
                        PageName = rd["PageName"]?.ToString()
                    };
                }
            }
        }

        return null;
    }
    public static string GenerateStableId(string input)
    {
        if (string.IsNullOrEmpty(input))
            return Guid.NewGuid().ToString("N").Substring(0, 16);

        using (var sha1 = System.Security.Cryptography.SHA1.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(input.Trim().ToLowerInvariant());
            var hash = sha1.ComputeHash(bytes);

            var sb = new StringBuilder(16);
            for (int i = 0; i < 8; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
    private void ExecuteMerge(SqlConnection conn)
    {
        var sql = @"

-- PAGE
INSERT INTO TablePageInfo (PageID, IDFBPage, PageLink, PageName)
SELECT 
    t.PageID,
    t.IDFBPage,
    t.PageLink,
    t.PageName
FROM TablePageInfo_Temp t
LEFT JOIN TablePageInfo p ON p.PageID = t.PageID
WHERE p.PageID IS NULL;

-- PERSON
INSERT INTO TablePersonInfo (PersonID, PersonLink, PersonName, PersonNote)
SELECT 
    t.PersonID,
    t.PersonLink,
    t.PersonName,
    t.PersonNote
FROM TablePersonInfo_Temp t
LEFT JOIN TablePersonInfo p ON p.PersonID = t.PersonID
WHERE p.PersonID IS NULL;

-- POST
INSERT INTO TablePostInfo (
    PostID,
    PostLink,
    PostContent,
    PostTime,
    RealPostTime,
    LikeCount,
    ShareCount,
    CommentCount,
    PostAttachment,
    PostStatus
)
SELECT 
    t.PostID,
    t.PostLink,
    t.PostContent,
    t.PostTime,
    t.RealPostTime,
    t.LikeCount,
    t.ShareCount,
    t.CommentCount,
    t.PostAttachment,
    t.PostStatus
FROM TablePostInfo_Temp t
LEFT JOIN TablePostInfo p ON p.PostID = t.PostID
WHERE p.PostID IS NULL;

-- MAP
INSERT INTO TablePost (PostID, PageIDCreate, PageIDContainer, PersonIDCreate)
SELECT 
    t.PostID,
    t.PageIDCreate,
    t.PageIDContainer,
    t.PersonIDCreate
FROM TablePost_Temp t
LEFT JOIN TablePost p ON p.PostID = t.PostID
WHERE p.PostID IS NULL;

-- SHARE
INSERT INTO TablePostShare (PostID, PageID, PersonID, TimeShare, RealTimeShare)
SELECT 
    t.PostID,
    t.PageID,
    t.PersonID,
    t.TimeShare,
    t.RealTimeShare
FROM TablePostShare_Temp t
LEFT JOIN TablePostShare s
    ON s.PostID = t.PostID
    AND ISNULL(s.PageID,'') = ISNULL(t.PageID,'')
    AND ISNULL(s.PersonID,'') = ISNULL(t.PersonID,'')
WHERE s.PostID IS NULL;

-- CLEAN
TRUNCATE TABLE TablePostInfo_Temp;
TRUNCATE TABLE TablePageInfo_Temp;
TRUNCATE TABLE TablePersonInfo_Temp;
TRUNCATE TABLE TablePost_Temp;
TRUNCATE TABLE TablePostShare_Temp;
";

        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.ExecuteNonQuery();
        }
    }
    public int InsertPostShareBatchAuto(List<ShareItem> shares)
    {
        if (shares == null || shares.Count == 0)
            return 0;

        int saved = 0;

        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();

            using (var tran = conn.BeginTransaction())
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.Transaction = tran;

                try
                {
                    cmd.CommandText = @"
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
    )
END
";

                    foreach (var s in shares)
                    {
                        if (s == null)
                            continue;

                        // =========================
                        // 🔥 MAP FIELD CHUẨN
                        // =========================
                        string postID = s.PostID_B;

                        if (string.IsNullOrEmpty(postID) && !string.IsNullOrEmpty(s.PostLinkB))
                        {
                            postID = GeneratePostId(s.PostLinkB);
                        }

                        if (string.IsNullOrEmpty(postID))
                            continue;

                        DateTime? realTime = s.ShareTimeReal ??
                                             TimeHelper.ParseFacebookTime(s.ShareTimeRaw);

                        cmd.Parameters.Clear();

                        cmd.Parameters.AddWithValue("@postID", postID);
                        cmd.Parameters.AddWithValue("@pageID", (object)s.PageID_A ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@personID", (object)s.PersonIDShare ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@timeShare", s.ShareTimeRaw ?? "");
                        cmd.Parameters.AddWithValue("@realTimeShare", (object)realTime ?? DBNull.Value);

                        cmd.ExecuteNonQuery();

                        saved++;
                    }

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Libary.Instance.LogTech("❌ InsertPostShareBatchAuto ERROR: " + ex.Message);
                }
            }
        }

        return saved;
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
                )";
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
                        PostType = rd.GetEnum("PostStatus", PostType.UnknowType),

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

    public bool IsPostExist(string postLink)
    {
        const string sql = @"
        SELECT TOP 1 1
        FROM TablePostInfo
        WHERE PostLink = @link
    ";

        object v = ExecuteScalar(sql, new Dictionary<string, object>
    {
        { "@link", postLink }
    });

        return v != null;
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
                cmd.Parameters.Add("@PageID", SqlDbType.NVarChar, 200)
                    .Value = (object)page.PageID ?? DBNull.Value;

                cmd.Parameters.Add("@IDFBPage", SqlDbType.NVarChar, 200)
                    .Value = (object)page.IDFBPage ?? DBNull.Value;

                cmd.Parameters.Add("@PageLink", SqlDbType.NVarChar, -1)
                    .Value = (object)page.PageLink ?? DBNull.Value;

                cmd.Parameters.Add("@PageName", SqlDbType.NVarChar, 500)
                    .Value = (object)page.PageName ?? DBNull.Value;

                // enum -> string (DB là NVARCHAR)
                cmd.Parameters.Add("@PageType", SqlDbType.NVarChar, 200)
                    .Value = page.PageType.ToString();

                cmd.Parameters.Add("@PageMembers", SqlDbType.NVarChar, 200)
                    .Value = (object)page.PageMembers ?? DBNull.Value;

                cmd.Parameters.Add("@PageInteraction", SqlDbType.NVarChar, 200)
                    .Value = (object)page.PageInteraction ?? DBNull.Value;

                cmd.Parameters.Add("@PageEvaluation", SqlDbType.NVarChar, 200)
                    .Value = (object)page.PageEvaluation ?? DBNull.Value;

                cmd.Parameters.Add("@PageInfoText", SqlDbType.NVarChar, -1)
                    .Value = (object)page.PageInfoText ?? DBNull.Value;

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
                    if (!rd.Read()) return null;

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
                        PageTimeSave = rd.GetStringOrNull("PageTimeSave"),
                        TimeLastPost = rd["TimeLastPost"] == DBNull.Value
                            ? (DateTime?)null
                            : Convert.ToDateTime(rd["TimeLastPost"]),
                        IsScanned = rd["IsScanned"] != DBNull.Value
                            && Convert.ToBoolean(rd["IsScanned"])
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("[GetPageByID SQL] ERROR: " + ex.Message);
            return null;
        }
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
    public string GetIDFBPageByPageID(string pageId)
    {
        if (string.IsNullOrWhiteSpace(pageId))
            return null;

        const string sql = @"
    SELECT TOP 1 IDFBPage
    FROM TablePageInfo
    WHERE PageID = @id
      AND IDFBPage IS NOT NULL
      AND IDFBPage <> 'N/A'
    ";

        object v = ExecuteScalar(sql, new Dictionary<string, object>
        {
            ["@id"] = pageId
        });

        return v?.ToString();
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
    public PageInfo GetPageByIDFB(string idfbPage)
    {
        if (string.IsNullOrWhiteSpace(idfbPage))
            return null;

        const string sql = @"
            SELECT TOP 1
                PageID,
                IDFBPage,
                PageName,
                PageLink,
                PageType
            FROM TablePageInfo
            WHERE IDFBPage = @idfb
            ";

        using (var conn = OpenConnection())
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.Add("@idfb", SqlDbType.NVarChar, 200).Value = idfbPage;

            using (var rd = cmd.ExecuteReader())
            {
                if (!rd.Read())
                    return null;

                return new PageInfo
                {
                    PageID = rd.GetStringOrNull("PageID"),
                    IDFBPage = rd.GetStringOrNull("IDFBPage"),
                    PageName = rd.GetStringOrNull("PageName"),
                    PageLink = rd.GetStringOrNull("PageLink"),
                    PageType = rd.GetEnum("PageType", FBType.Unknown)
                };
            }
        }
    }

    //==============hàm dưới phục vụ in ra page (trang)
    public DataTable GetPageNotePaging(int pageIndex, int pageSize, out int totalRows)
    {
        using (var conn = SQLDAO.Instance.OpenConnection())
        {
            // 1️⃣ total
            using (var cmdCount = new SqlCommand(
                "SELECT COUNT(*) FROM TablePageNote", conn))
            {
                totalRows = (int)cmdCount.ExecuteScalar();
            }

            // 2️⃣ data (chỉ lấy cần thiết)
            string sql = @"
            SELECT PageID, TimeSave
            FROM TablePageNote
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
    public DataTable GetPageInfoPage_ByType(int pageIndex, int pageSize, bool isScan, out int totalRows)
    {
        using (var conn = SQLDAO.Instance.OpenConnection())
        {
            // ✔️ điều kiện lọc
            string where = isScan
                ? "IsScanned = 1"
                : "ISNULL(IsScanned, 0) = 0";

            // 1️⃣ Tổng số dòng
            using (var cmdCount = new SqlCommand(
                $"SELECT COUNT(*) FROM TablePageInfo WHERE {where}", conn))
            {
                totalRows = (int)cmdCount.ExecuteScalar();
            }

            // 2️⃣ Data
            string sql = $@"
        SELECT *
        FROM TablePageInfo
        WHERE {where}
        ORDER BY PageTimeSave DESC
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
    public DataTable GetPageMonitorPaging(int pageIndex, int pageSize, out int totalRows)
    {
        using (var conn = SQLDAO.Instance.OpenConnection())
        {
            // total
            using (var cmdCount = new SqlCommand(
                "SELECT COUNT(*) FROM TablePageMonitor", conn))
            {
                totalRows = (int)cmdCount.ExecuteScalar();
            }

            // data (chỉ lấy cần thiết)
            string sql = @"
            SELECT 
                PageID,
                IsAuto,
                Status,
                FirstScanTime,
                LastScanTime,
                TotalPostsScanned,
                TimeSave
            FROM TablePageMonitor
            ORDER BY ISNULL(LastScanTime, TimeSave) DESC
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
    //--phân trang cho post lấy theo page
    public List<PostPage> GetPostsByPagePaging(
    string pageId,
    int pageIndex,
    int pageSize,
    out int totalRows)
    {
        var list = new List<PostPage>();

        using (var conn = OpenConnection())
        {
            // 🔹 1. COUNT
            using (var cmdCount = new SqlCommand(@"
            SELECT COUNT(*)
            FROM TablePost
            WHERE PageIDContainer = @PageID", conn))
            {
                cmdCount.Parameters.AddWithValue("@PageID", pageId);
                totalRows = (int)cmdCount.ExecuteScalar();
            }

            // 🔹 2. DATA
            string sql = @"
                    SELECT 
                p.PostID,

                -- 🔹 mapping
                p.PageIDCreate,
                p.PageIDContainer,
                p.PersonIDCreate,

                -- 🔹 post info
                pi.PostLink,
                pi.PostContent,
                pi.PostTime,
                pi.RealPostTime,
                pi.LikeCount,
                pi.ShareCount,
                pi.CommentCount,
                pi.PostAttachment,

                -- 🔹 page create
                pc.PageName AS PageCreateName,
                pc.PageLink AS PageCreateLink,
                pc.PageType AS PageCreateType,

                -- 🔹 page container
                pg.PageName AS PageContainerName,
                pg.PageLink AS PageContainerLink,
                pg.PageType AS PageContainerType,

                -- 🔹 person
                pe.PersonName,
                pe.PersonLink,
                pe.PersonNote

            FROM TablePost p

            LEFT JOIN TablePostInfo pi ON p.PostID = pi.PostID

            LEFT JOIN TablePageInfo pc ON p.PageIDCreate = pc.PageID
            LEFT JOIN TablePageInfo pg ON p.PageIDContainer = pg.PageID
            LEFT JOIN TablePersonInfo pe ON p.PersonIDCreate = pe.PersonID

            WHERE p.PageIDContainer = @PageID   -- 🔥 QUAN TRỌNG

            ORDER BY 
                CASE WHEN pi.RealPostTime IS NULL THEN 1 ELSE 0 END,
                pi.RealPostTime DESC

            OFFSET (@PageIndex - 1) * @PageSize ROWS
            FETCH NEXT @PageSize ROWS ONLY";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@PageID", pageId);
                cmd.Parameters.AddWithValue("@PageIndex", pageIndex);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var p = new PostPage
                        {
                            PostID = reader.GetStringOrNull("PostID"),

                            // ===== POST =====
                            PostLink = reader.GetStringOrNull("PostLink"),
                            Content = reader.GetStringOrNull("PostContent"),
                            PostTime = reader.GetStringOrNull("PostTime"),

                            RealPostTime = reader["RealPostTime"] == DBNull.Value? (DateTime?)null: Convert.ToDateTime(reader["RealPostTime"]),

                            LikeCount = reader["LikeCount"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["LikeCount"]),
                            ShareCount = reader["ShareCount"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["ShareCount"]),
                            CommentCount = reader["CommentCount"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["CommentCount"]),

                            Attachment = reader.GetStringOrNull("PostAttachment"),

                            // ===== POSTER =====
                            PosterName = reader.GetStringOrNull("PersonName"),
                            PosterLink = reader.GetStringOrNull("PersonLink"),
                            PosterNote = reader.GetEnum("PersonNote", FBType.Unknown),

                            // ===== PAGE =====
                            PageID = reader.GetStringOrNull("PageIDContainer"),
                            PageName = reader.GetStringOrNull("PageContainerName"),
                            PageLink = reader.GetStringOrNull("PageContainerLink"),

                            ContainerIdFB = reader.GetStringOrNull("PageIDContainer"),
                            ContainerType = reader.GetEnum("PageContainerType", FBType.Unknown)
                        };

                        list.Add(p);
                    }
                }
            }
        }

        return list;
    }
   
    // hết phân trang
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
    public List<PageMonitorViewModel> GetPageMonitorVM()
    {
        string sql = @"
    SELECT 
        m.PageID, 
        p.PageName, 
        p.TimeLastPost,   -- 🔥 QUAN TRỌNG
        m.Status, 
        m.LastScanTime
    FROM TablePageMonitor m
    LEFT JOIN TablePageInfo p ON m.PageID = p.PageID
    ORDER BY m.TimeSave DESC
    ";

        return QueryList(sql, rd =>
        {
            return new PageMonitorViewModel
            {
                PageID = rd["PageID"]?.ToString(),
                PageName = rd["PageName"]?.ToString(),
                Status = rd["Status"]?.ToString(),
                PostScan = 0,
                LastScanTime = rd["LastScanTime"] as DateTime?,

                // 🔥 FIX CHÍNH
                TimeLastPost = rd["TimeLastPost"] == DBNull.Value
                    ? (DateTime?)null
                    : Convert.ToDateTime(rd["TimeLastPost"])
            };
        });
    }
    public HashSet<string> GetAllPageInMonitor()
    {
        var set = new HashSet<string>();

        using (var conn = OpenConnection())
        {
            string sql = "SELECT PageID FROM TablePageMonitor";

            using (var cmd = new SqlCommand(sql, conn))
            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    set.Add(rd["PageID"].ToString());
                }
            }
        }

        return set;
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
    public bool CheckPageInMonitor(string pageId)
    {
        using (var conn = OpenConnection())
        {
            string sql = "SELECT COUNT(1) FROM TablePageMonitor WHERE PageID = @id";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", pageId);
                return (int)cmd.ExecuteScalar() > 0;
            }
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
                        PersonID = rd.GetStringOrNull("PersonID"),
                        IDFBPerson = rd.GetStringOrNull("IDFBPerson"),
                        PersonLink = rd.GetStringOrNull("PersonLink"),
                        PersonName = rd.GetStringOrNull("PersonName"),
                        PersonInfoText = rd.GetStringOrNull("PersonInfo"),
                        PersonNote = rd.GetEnum("PersonNote", FBType.Unknown),
                        PersonTimeSave = rd.GetStringOrNull("PersonTimeSave")
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
    public PersonInfo GetPersonByID(string personId)
    {
        if (string.IsNullOrWhiteSpace(personId) || personId == "N/A")
            return null;

        const string sql = @"
        SELECT TOP 1
            PersonID,
            PersonLink,
            PersonName,
            PersonNote
        FROM TablePersonInfo
        WHERE PersonID = @id
    ";

        using (var conn = OpenConnection())
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.Add("@id", SqlDbType.NVarChar, 200).Value = personId;

            using (var rd = cmd.ExecuteReader())
            {
                if (!rd.Read())
                    return null;

                return new PersonInfo
                {
                    PersonID = rd.GetStringOrNull("PersonID"),
                    PersonLink = rd.GetStringOrNull("PersonLink"),
                    PersonName = rd.GetStringOrNull("PersonName"),
                    PersonNote = rd.GetEnum("PersonNote", FBType.Unknown)
                };
            }
        }
    }
    public PersonInfo GetPersonByLink(string link)
    {
        if (string.IsNullOrWhiteSpace(link))
            return null;

        var normalizedLink = UrlHelper.NormalizeFacebookUrl(link);

        const string sql = @"
        SELECT TOP 1
            PersonID,
            PersonLink,
            PersonName,
            PersonNote
        FROM TablePersonInfo
        WHERE PersonLink = @link
    ";

        using (var conn = OpenConnection())
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.Add("@link", SqlDbType.NVarChar).Value = normalizedLink;

            using (var rd = cmd.ExecuteReader())
            {
                if (!rd.Read())
                    return null;

                return new PersonInfo
                {
                    PersonID = rd.GetStringOrNull("PersonID"),
                    PersonLink = rd.GetStringOrNull("PersonLink"),
                    PersonName = rd.GetStringOrNull("PersonName"),
                    PersonNote = rd.GetEnum("PersonNote", FBType.Unknown)
                };
            }
        }
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
    public void InsertTablePostShare(string postID, string pageID, string personID, string timeShare, DateTime? realTimeShare)
    {
        const string sql = @"
IF NOT EXISTS (
    SELECT 1 FROM TablePostShare
    WHERE PostID = @postID
      AND (PageID = @pageID OR (PageID IS NULL AND @pageID IS NULL))
      AND (PersonID = @personID OR (PersonID IS NULL AND @personID IS NULL))
      AND (TimeShare = @timeShare OR (TimeShare IS NULL AND @timeShare IS NULL))
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
            ["@timeShare"] = (object)timeShare ?? DBNull.Value, // 🔥 FIX
            ["@realTimeShare"] = (object)realTimeShare ?? DBNull.Value
        });
    }
    public string GetPostStatusUI(PostType postType)
    {
        switch (postType)
        {
            case PostType.Share_WithContent:
            case PostType.Share_NoContent:
                return "Bài share";

            case PostType.Page_Normal:
            case PostType.Page_Photo_Cap:
            case PostType.Page_Photo_NoCap:
            case PostType.Page_Video_Cap:
            case PostType.Page_Video_Nocap:
            case PostType.page_Real_Cap:
            case PostType.Page_Reel_NoCap:
            case PostType.Page_BackGround:
            case PostType.Page_LinkWeb:
            case PostType.Page_NoConent:
                return "Bài gốc";

            case PostType.Page_Unknow:
            default:
                return "Tự đăng";
        }
    }
    //SQL Keyword
    public List<KeywordDTO> GetAllKeyword()
    {
        string sql = @"
        SELECT KeywordId, KeywordName
        FROM TableKeyword
        ORDER BY KeywordName;
    ";

        return QueryList(sql, rd => new KeywordDTO
        {
            KeywordId = Convert.ToInt32(rd["KeywordId"]),
            KeywordName = rd["KeywordName"].ToString()
        });
    }
    public bool KeywordExists(string keywordName)
    {
        object v = ExecuteScalar(@"
        SELECT 1
        FROM TableKeyword
        WHERE KeywordName = @name;
    ", new Dictionary<string, object>
    {
        { "@name", keywordName }
    });

        return v != null;
    }
    public int InsertKeyword(string keywordName)
    {
        object v = ExecuteScalar(@"
        INSERT INTO TableKeyword (KeywordName)
        VALUES (@name);
        SELECT CAST(SCOPE_IDENTITY() AS INT);
    ", new Dictionary<string, object>
    {
        { "@name", keywordName }
    });

        int newId = Convert.ToInt32(v);

        // 🔥 Tăng version vì đã thay đổi keyword
        IncreaseKeywordVersion();

        return newId;
    }
    public void UpdateKeyword(int keywordId, string keywordName)
    {
        ExecuteNonQuery(@"
        UPDATE TableKeyword
        SET KeywordName = @name
        WHERE KeywordId = @id;
    ", new Dictionary<string, object>
    {
        { "@id", keywordId },
        { "@name", keywordName }
    });
    }
    public void DeleteKeyword(int keywordId)
    {
        ExecuteNonQuery(@"
        DELETE FROM TableKeyword
        WHERE KeywordId = @id;
    ", new Dictionary<string, object>
    {
        { "@id", keywordId }
    });
    }
    public void DeleteKeywordFull(int keywordId)
    {
        // 1. Xoá mapping Topic
        ExecuteNonQuery(@"
        DELETE FROM TableTopicKey
        WHERE KeywordId = @id;
    ", new Dictionary<string, object>
    {
        { "@id", keywordId }
    });

        // 2. Xoá điểm theo dõi
        ExecuteNonQuery(@"
        DELETE FROM TableAttentionKeywordScore
        WHERE KeywordId = @id;
    ", new Dictionary<string, object>
    {
        { "@id", keywordId }
    });

        // 3. Xoá điểm tiêu cực
        ExecuteNonQuery(@"
        DELETE FROM TableNegativeKeywordScore
        WHERE KeywordId = @id;
    ", new Dictionary<string, object>
    {
        { "@id", keywordId }
    });

        // 4. Cuối cùng mới xoá Keyword
        ExecuteNonQuery(@"
        DELETE FROM TableKeyword
        WHERE KeywordId = @id;
    ", new Dictionary<string, object>
    {
        { "@id", keywordId }
    });
        // 🔥 keyword thay đổi → tăng version
        IncreaseKeywordVersion();
    }
    public int EnsureKeyword(string keywordName)
    {
        object v = ExecuteScalar(@"
        SELECT KeywordId
        FROM TableKeyword
        WHERE KeywordName = @name;
    ", new Dictionary<string, object>
        {
            ["@name"] = keywordName
        });

        if (v != null)
            return Convert.ToInt32(v);

        object newId = ExecuteScalar(@"
        INSERT INTO TableKeyword (KeywordName)
        VALUES (@name);
        SELECT CAST(SCOPE_IDENTITY() AS INT);
    ", new Dictionary<string, object>
        {
            ["@name"] = keywordName
        });

        return Convert.ToInt32(newId);
    }

    //lấy điểm
    public int? GetAttentionScoreByKeywordId(int keywordId)
    {
        const string sql = @"
        SELECT Score
        FROM TableAttentionKeywordScore
        WHERE KeywordId = @id;
    ";

        object v = ExecuteScalar(sql, new Dictionary<string, object>
    {
        { "@id", keywordId }
    });

        if (v == null || v == DBNull.Value)
            return null;

        return Convert.ToInt32(v);
    }
    public int? GetNegativeScoreByKeywordId(int keywordId)
    {
        const string sql = @"
        SELECT Score
        FROM TableNegativeKeywordScore
        WHERE KeywordId = @id;
    ";

        object v = ExecuteScalar(sql, new Dictionary<string, object>
    {
        { "@id", keywordId }
    });

        if (v == null || v == DBNull.Value)
            return null;

        return Convert.ToInt32(v);
    }
    // lấy topic ID
    public int? GetTopicIdByKeywordId(int keywordId)
    {
        object v = ExecuteScalar(@"
        SELECT TOP 1 TopicId
        FROM TableTopicKey
        WHERE KeywordId = @kid;
    ", new Dictionary<string, object>
    {
        { "@kid", keywordId }
    });

        if (v == null || v == DBNull.Value)
            return null;

        return Convert.ToInt32(v);
    }
    public List<TopicDTO> GetAllTopic()
    {
        string sql = @"
        SELECT TopicId, TopicName
        FROM TableTopic
        ORDER BY TopicName;
    ";

        return QueryList(sql, rd => new TopicDTO
        {
            TopicId = Convert.ToInt32(rd["TopicId"]),
            TopicName = rd["TopicName"].ToString()
        });
    }
    public TopicDTO GetTopicById(int topicId)
    {
        string sql = @"
        SELECT TopicId, TopicName
        FROM TableTopic
        WHERE TopicId = @id;
    ";

        var list = QueryList(
     sql,
     rd => new TopicDTO
     {
         TopicId = Convert.ToInt32(rd["TopicId"]),
         TopicName = rd["TopicName"].ToString()
     },
     new Dictionary<string, object>
     {
        { "@id", topicId }
     }
 );

        return list.FirstOrDefault();

    }
    public TopicDTO GetTopicByName(string topicName)
    {
        string sql = @"
        SELECT TopicId, TopicName
        FROM TableTopic
        WHERE TopicName = @name;
    ";

        var list = QueryList(
            sql,
            rd => new TopicDTO
            {
                TopicId = Convert.ToInt32(rd["TopicId"]),
                TopicName = rd["TopicName"].ToString()
            },
            new Dictionary<string, object>
            {
            { "@name", topicName }
            }
        );

        return list.FirstOrDefault();
    }
    public List<TopicDTO> GetTopicsByKeywordId(int keywordId)
    {
        string sql = @"
        SELECT t.TopicId, t.TopicName
        FROM TableTopicKey tk
        JOIN TableTopic t ON tk.TopicId = t.TopicId
        WHERE tk.KeywordId = @kid;
    ";

        return QueryList(
            sql,
            rd => new TopicDTO
            {
                TopicId = Convert.ToInt32(rd["TopicId"]),
                TopicName = rd["TopicName"].ToString()
            },
            new Dictionary<string, object>
            {
            { "@kid", keywordId }
            }
        );
    }
    public int CountKeywordInTopic(int topicId)
    {
        object v = ExecuteScalar(@"
        SELECT COUNT(*)
        FROM TableTopicKey
        WHERE TopicId = @tid;
    ", new Dictionary<string, object>
    {
        { "@tid", topicId }
    });

        return v == null ? 0 : Convert.ToInt32(v);
    }
    public int InsertTopic(string topicName)
    {
        object v = ExecuteScalar(@"
        INSERT INTO TableTopic (TopicName)
        VALUES (@name);
        SELECT CAST(SCOPE_IDENTITY() AS INT);
    ", new Dictionary<string, object>
    {
        { "@name", topicName }
    });

        return Convert.ToInt32(v);
    }
    public bool TopicExists(string topicName)
    {
        object v = ExecuteScalar(@"
        SELECT 1
        FROM TableTopic
        WHERE TopicName = @name;
    ", new Dictionary<string, object>
    {
        { "@name", topicName }
    });

        return v != null;
    }
    public void UpdateTopic(int topicId, string topicName)
    {
        ExecuteNonQuery(@"
        UPDATE TableTopic
        SET TopicName = @name
        WHERE TopicId = @id;
    ", new Dictionary<string, object>
    {
        { "@id", topicId },
        { "@name", topicName }
    });
    }
    public void DeleteTopicFull(int topicId)
    {
        // 1. Xoá mapping Topic - Keyword
        SQLDAO.Instance.ExecuteNonQuery(@"
        DELETE FROM TableTopicKey
        WHERE TopicId = @tid;
    ", new Dictionary<string, object>
    {
        { "@tid", topicId }
    });

        // 2. Xoá Topic
        SQLDAO.Instance.ExecuteNonQuery(@"
        DELETE FROM TableTopic
        WHERE TopicId = @tid;
    ", new Dictionary<string, object>
    {
        { "@tid", topicId }
    });
    }
    public bool TopicKeywordExists(int topicId, int keywordId)
    {
        object v = ExecuteScalar(@"
        SELECT 1
        FROM TableTopicKey
        WHERE TopicId = @t AND KeywordId = @k;
    ", new Dictionary<string, object>
    {
        { "@t", topicId },
        { "@k", keywordId }
    });

        return v != null;
    }
    public void InsertTopicKeyword(int topicId, int keywordId)
    {
        ExecuteNonQuery(@"
        IF NOT EXISTS (
            SELECT 1 FROM TableTopicKey
            WHERE TopicId = @t AND KeywordId = @k
        )
        INSERT INTO TableTopicKey (TopicId, KeywordId)
        VALUES (@t, @k);
    ", new Dictionary<string, object>
    {
        { "@t", topicId },
        { "@k", keywordId }
    });
    }
    public void DeleteTopicKeyword(int topicId, int keywordId)
    {
        ExecuteNonQuery(@"
        DELETE FROM TableTopicKey
        WHERE TopicId = @t AND KeywordId = @k;
    ", new Dictionary<string, object>
    {
        { "@t", topicId },
        { "@k", keywordId }
    });
    }
    public int CountTopicByKeywordId(int keywordId)
    {
        object v = ExecuteScalar(@"
        SELECT COUNT(*)
        FROM TableTopicKey
        WHERE KeywordId = @kid;
    ", new Dictionary<string, object>
    {
        { "@kid", keywordId }
    });

        return v == null ? 0 : Convert.ToInt32(v);
    }
    public bool AddTopicIfNotExists(string topicName)
    {
        string sql = @"
        IF NOT EXISTS (
            SELECT 1 FROM TableTopic WHERE TopicName = @name
        )
        BEGIN
            INSERT INTO TableTopic (TopicName)
            VALUES (@name)
            SELECT 1
        END
        ELSE
            SELECT 0
        ";

                int result = Convert.ToInt32(
                    ExecuteScalar(sql, new Dictionary<string, object>
                    {
                        ["@name"] = topicName
                    })
                );

                return result == 1;
    }
    public int EnsureTopic(string topicName)
    {
        object v = ExecuteScalar(@"
        SELECT TopicId
        FROM TableTopic
        WHERE TopicName = @name;
    ", new Dictionary<string, object>
        {
            ["@name"] = topicName
        });

        if (v != null)
            return Convert.ToInt32(v);

        object newId = ExecuteScalar(@"
        INSERT INTO TableTopic (TopicName)
        VALUES (@name);
        SELECT CAST(SCOPE_IDENTITY() AS INT);
    ", new Dictionary<string, object>
        {
            ["@name"] = topicName
        });

        return Convert.ToInt32(newId);
    }


    // đánh giá keyword
    
    public bool AddKeywordToTopic(int keywordId, int topicId)
    {
        string sql = @"
        IF NOT EXISTS (
            SELECT 1 FROM TableTopicKey
            WHERE KeywordId = @kid AND TopicId = @tid
        )
        BEGIN
            INSERT INTO TableTopicKey (TopicId, KeywordId)
            VALUES (@tid, @kid)
            SELECT 1
        END
        ELSE
            SELECT 0
    ";

        int result = Convert.ToInt32(
            ExecuteScalar(sql, new Dictionary<string, object>
            {
                ["@kid"] = keywordId,
                ["@tid"] = topicId
            })
        );

        return result == 1; // true = đã thêm
    }
    public DataTable GetAllKeywordTopic()
    {
        return ExecuteQuery(@"
        SELECT 
            tk.TopicId,
            t.TopicName,
            k.KeywordId,
            k.KeywordName
        FROM TableTopicKey tk
        JOIN TableTopic t ON tk.TopicId = t.TopicId
        JOIN TableKeyword k ON tk.KeywordId = k.KeywordId
    ");
    }
    // add keyword cơ bản, k thêm điểm
    public bool AddKeywordIfNotExists(string keywordName, out int keywordId)
    {
        keywordId = 0;

        if (string.IsNullOrWhiteSpace(keywordName))
            return false;

        // 1. Kiểm tra tồn tại
        object exist = ExecuteScalar(
            "SELECT KeywordId FROM TableKeyword WHERE KeywordName = @name",
            new Dictionary<string, object>
            {
                ["@name"] = keywordName.Trim()
            });

        if (exist != null)
        {
            keywordId = Convert.ToInt32(exist);
            return false; // đã tồn tại
        }

        // 2. Insert keyword
        keywordId = Convert.ToInt32(
            ExecuteScalar(@"
            INSERT INTO TableKeyword (KeywordName)
            VALUES (@name);
            SELECT SCOPE_IDENTITY();
        ",
            new Dictionary<string, object>
            {
                ["@name"] = keywordName.Trim()
            })
        );

        return true; // thêm mới
    }

    public bool IsKeywordInTopic(int keywordId, int topicId)
    {
        string sql = @"
        SELECT COUNT(1)
        FROM TableTopicKey
        WHERE KeywordId = @kid AND TopicId = @tid
    ";

        int count = Convert.ToInt32(
            ExecuteScalar(sql, new Dictionary<string, object>
            {
                ["@kid"] = keywordId,
                ["@tid"] = topicId
            }) ?? 0
        );

        return count > 0;
    }
    public List<KeywordViewModel> GetKeywordsByTopic(int topicId)
    {
        var excludedIds = AnalyzeSQLDAO.Instance.GetExcludedKeywordIds();

        string sql = @"
        SELECT k.KeywordId,
               k.KeywordName,
               ISNULL(a.Score, 0) AS AttentionScore,
               ISNULL(n.Score, 0) AS NegativeScore
        FROM TableKeyword k
        INNER JOIN TableTopicKey tk
            ON k.KeywordId = tk.KeywordId
        LEFT JOIN TableAttentionKeywordScore a
            ON a.KeywordId = k.KeywordId
        LEFT JOIN TableNegativeKeywordScore n
            ON n.KeywordId = k.KeywordId
        WHERE tk.TopicId = @tid
        ORDER BY k.KeywordName
    ";

        return QueryList(sql, rd =>
        {
            int kid = Convert.ToInt32(rd["KeywordId"]);

            return new KeywordViewModel
            {
                KeywordId = kid,
                KeywordName = rd["KeywordName"].ToString(),
                AttentionScore = Convert.ToInt32(rd["AttentionScore"]),
                NegativeScore = Convert.ToInt32(rd["NegativeScore"]),
                IsExcluded = excludedIds.Contains(kid),   // 🔥 QUAN TRỌNG
                Select = false
            };
        },
        new Dictionary<string, object>
        {
            ["@tid"] = topicId
        });
    }
    //Negative
    public bool NegativeKeywordScoreExists(int keywordId)
    {
        object v = ExecuteScalar(@"
        SELECT 1
        FROM TableNegativeKeywordScore
        WHERE KeywordId = @id;
    ", new Dictionary<string, object>
    {
        { "@id", keywordId }
    });

        return v != null;
    }
    public void InsertNegativeKeywordScore(int keywordId,int score,int negativeLevel,bool isCritical,string note = null)
    {
        ExecuteNonQuery(@"
        INSERT INTO TableNegativeKeywordScore
            (KeywordId, Score, NegativeLevel, IsCritical, Note)
        VALUES
            (@id, @score, @level, @critical, @note);
    ", new Dictionary<string, object>
    {
        { "@id", keywordId },
        { "@score", score },
        { "@level", negativeLevel },
        { "@critical", isCritical },
        { "@note", (object)note ?? DBNull.Value }
    });
    }
    public void UpdateNegativeKeywordScore(int keywordId,int score,int negativeLevel,bool isCritical,string note = null)
    {
        ExecuteNonQuery(@"
        UPDATE TableNegativeKeywordScore
        SET Score = @score,
            NegativeLevel = @level,
            IsCritical = @critical,
            Note = @note
        WHERE KeywordId = @id;
    ", new Dictionary<string, object>
    {
        { "@id", keywordId },
        { "@score", score },
        { "@level", negativeLevel },
        { "@critical", isCritical },
        { "@note", (object)note ?? DBNull.Value }
    });
    }

    public void DeleteNegativeKeywordScore(int keywordId)
    {
        ExecuteNonQuery(@"
        DELETE FROM TableNegativeKeywordScore
        WHERE KeywordId = @id;
    ", new Dictionary<string, object>
    {
        { "@id", keywordId }
    });
    }
    public void SaveNegativeKeywordScore(int keywordId,int score, int negativeLevel,bool isCritical, string note = null)
    {
        if (NegativeKeywordScoreExists(keywordId))
            UpdateNegativeKeywordScore(keywordId, score, negativeLevel, isCritical, note);
        else
            InsertNegativeKeywordScore(keywordId, score, negativeLevel, isCritical, note);
    }
    public bool UpsertNegativeScore(int keywordId, int score,int negativeLevel,bool isCritical,string note = null)
    {
        int rows = ExecuteNonQuery(@"
        MERGE TableNegativeKeywordScore AS t
        USING (SELECT @KeywordId AS KeywordId) s
        ON t.KeywordId = s.KeywordId
        WHEN MATCHED THEN
            UPDATE SET
                Score = @Score,
                NegativeLevel = @Level,
                IsCritical = @Critical,
                Note = @Note
        WHEN NOT MATCHED THEN
            INSERT (KeywordId, Score, NegativeLevel, IsCritical, Note)
            VALUES (@KeywordId, @Score, @Level, @Critical, @Note);
    ", new Dictionary<string, object>
    {
        { "@KeywordId", keywordId },
        { "@Score", score },
        { "@Level", negativeLevel },
        { "@Critical", isCritical },
        { "@Note", (object)note ?? DBNull.Value }
    });

        if (rows > 0)
        {
            // 🔥 có thay đổi → tăng version
            IncreaseKeywordVersion();
            return true;
        }

        return false;
    }
    public int? GetTrackingLevelByKeywordId(int keywordId)
    {
        object v = ExecuteScalar(@"
        SELECT TrackingLevel
        FROM TableAttentionKeywordScore
        WHERE KeywordId = @id
    ", new Dictionary<string, object>
    {
        { "@id", keywordId }
    });

        if (v == null || v == DBNull.Value)
            return null;

        return Convert.ToInt32(v);
    }
    public int? GetNegativeLevelByKeywordId(int keywordId)
    {
        object v = ExecuteScalar(@"
        SELECT NegativeLevel
        FROM TableNegativeKeywordScore
        WHERE KeywordId = @id
    ", new Dictionary<string, object>
    {
        { "@id", keywordId }
    });

        if (v == null || v == DBNull.Value)
            return null;

        return Convert.ToInt32(v);
    }

    //TableAttentionKeywordScore
    public bool AttentionKeywordScoreExists(int keywordId)
    {
        object v = ExecuteScalar(@"
        SELECT 1
        FROM TableAttentionKeywordScore
        WHERE KeywordId = @id;
    ", new Dictionary<string, object>
    {
        { "@id", keywordId }
    });

        return v != null;
    }
    public void InsertAttentionKeywordScore(int keywordId,int score,int trackingLevel,string note = null)
    {
        ExecuteNonQuery(@"
        INSERT INTO TableAttentionKeywordScore
            (KeywordId, Score, TrackingLevel, Note)
        VALUES
            (@id, @score, @level, @note);
    ", new Dictionary<string, object>
    {
        { "@id", keywordId },
        { "@score", score },
        { "@level", trackingLevel },
        { "@note", (object)note ?? DBNull.Value }
    });
    }
    public void UpdateAttentionKeywordScore(int keywordId, int score,int trackingLevel,string note = null)
    {
        ExecuteNonQuery(@"
        UPDATE TableAttentionKeywordScore
        SET Score = @score,
            TrackingLevel = @level,
            Note = @note
        WHERE KeywordId = @id;
    ", new Dictionary<string, object>
    {
        { "@id", keywordId },
        { "@score", score },
        { "@level", trackingLevel },
        { "@note", (object)note ?? DBNull.Value }
    });
    }
    public void DeleteAttentionKeywordScore(int keywordId)
    {
        ExecuteNonQuery(@"
        DELETE FROM TableAttentionKeywordScore
        WHERE KeywordId = @id;
    ", new Dictionary<string, object>
    {
        { "@id", keywordId }
    });
    }
    public void SaveAttentionKeywordScore(int keywordId,int score,int trackingLevel,string note = null)
    {
        if (AttentionKeywordScoreExists(keywordId))
            UpdateAttentionKeywordScore(keywordId, score, trackingLevel, note);
        else
            InsertAttentionKeywordScore(keywordId, score, trackingLevel, note);
    }
    //-===mới
    public bool UpsertAttentionScore(int keywordId,int score,int trackingLevel,string note = null)
    {
        int rows = ExecuteNonQuery(@"
        MERGE TableAttentionKeywordScore AS t
        USING (SELECT @KeywordId AS KeywordId) s
        ON t.KeywordId = s.KeywordId
        WHEN MATCHED THEN
            UPDATE SET 
                Score = @Score,
                TrackingLevel = @Level,
                Note = @Note
        WHEN NOT MATCHED THEN
            INSERT (KeywordId, Score, TrackingLevel, Note)
            VALUES (@KeywordId, @Score, @Level, @Note);
    ", new Dictionary<string, object>
    {
        { "@KeywordId", keywordId },
        { "@Score", score },
        { "@Level", trackingLevel },
        { "@Note", (object)note ?? DBNull.Value }
    });

        if (rows > 0)
        {
            // 🔥 có thay đổi → tăng version
            IncreaseKeywordVersion();
            return true;
        }

        return false;
    }
    // group loại trừ
    public void InsertOrUpdateExcludeKeyword(
     int keywordId,
     int? level,
     string note = null)
    {
        int safeLevel = level.HasValue && level.Value >= 1 && level.Value <= 7
            ? level.Value
            : 1;

        ExecuteNonQuery(@"
        IF EXISTS (
            SELECT 1 FROM TableExcludeKeyword WHERE KeywordId = @KeywordId
        )
        BEGIN
            UPDATE TableExcludeKeyword
            SET Level = @Level,
                Note = @Note
            WHERE KeywordId = @KeywordId
        END
        ELSE
        BEGIN
            INSERT INTO TableExcludeKeyword (KeywordId, Level, Note)
            VALUES (@KeywordId, @Level, @Note)
        END
    ", new Dictionary<string, object>
    {
        { "@KeywordId", keywordId },
        { "@Level", safeLevel },
        { "@Note", (object)note ?? DBNull.Value }
    });
    }

   
    public bool UpsertExcludeKeyword(
    int keywordId,
    int? level,
    string note = null)
    {
        int safeLevel = level.HasValue && level.Value >= 1 && level.Value <= 7
            ? level.Value
            : 1;

        int rows = ExecuteNonQuery(@"
        IF EXISTS (
            SELECT 1 FROM TableExcludeKeyword WHERE KeywordId = @KeywordId
        )
        BEGIN
            UPDATE TableExcludeKeyword
            SET Level = @Level,
                Note = @Note
            WHERE KeywordId = @KeywordId
        END
        ELSE
        BEGIN
            INSERT INTO TableExcludeKeyword (KeywordId, Level, Note)
            VALUES (@KeywordId, @Level, @Note)
        END
    ", new Dictionary<string, object>
    {
        { "@KeywordId", keywordId },
        { "@Level", safeLevel },
        { "@Note", (object)note ?? DBNull.Value }
    });

        if (rows > 0)
        {
            IncreaseKeywordVersion();
            return true;
        }

        return false;
    }
   
  
    // cũng là lấy post phục vụ gán topic
    public DataTable GetAllPosts()
    {
        return ExecuteQuery(
            "SELECT PostID, PostContent FROM TablePostInfo"
        );
    }
    public DataTable GetPostsWithoutTopic()
    {
        return ExecuteQuery(@"
        SELECT PostID, PostContent
        FROM TablePostInfo
        WHERE PostID NOT IN (
            SELECT DISTINCT PostId FROM TableTopicPost
        )");
    }
    public void ClearAllTopicPost()
    {
        ExecuteNonQuery("DELETE FROM TableTopicPost");
    }
    public bool TopicPostExists(int topicId, string postId)
    {
        object v = ExecuteScalar(@"
        SELECT 1 FROM TableTopicPost
        WHERE TopicId=@t AND PostId=@p",
            new Dictionary<string, object>
            {
                ["@t"] = topicId,
                ["@p"] = postId
            });

        return v != null;
    }
    public void InsertTopicPost(int topicId, string postId)
    {
        ExecuteNonQuery(@"
        IF NOT EXISTS (
            SELECT 1 
            FROM TableTopicPost
            WHERE TopicId = @t AND PostId = @p
        )
        INSERT INTO TableTopicPost (TopicId, PostId, CreatedTime)
        VALUES (@t, @p, GETDATE())
    ",
        new Dictionary<string, object>
        {
            ["@t"] = topicId,
            ["@p"] = postId
        });
    }
    public DataTable GetPostTopicForView()
    {
            return ExecuteQuery(@"
          SELECT 
            tp.PostId,
            t.TopicName,
            p.PostContent,

            -- 🔥 PHẢI alias RÕ
            p.RealPostTime AS RealPostTime,

            tp.CreatedTime AS ConvertTime
        FROM TableTopicPost tp
        JOIN TablePostInfo p ON tp.PostId = p.PostID
        JOIN TableTopic t ON tp.TopicId = t.TopicId
        ORDER BY tp.CreatedTime DESC;


    ");
    }
    //V. Verson keyword
    public int GetKeywordVersion()
    {
        try
        {
            using (var conn = OpenConnection())
            {
                string sql = "SELECT CurrentVersion FROM TableKeywordVersion WHERE Id = 1";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 1;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ GetKeywordVersion error: " + ex.Message);
            return 1;
        }
    }
    public void IncreaseKeywordVersion()
    {
        ExecuteNonQuery(@"
        UPDATE TableKeywordVersion
        SET CurrentVersion = CurrentVersion + 1,
            LastUpdated = SYSDATETIME()
        WHERE Id = 1
    ");
    }
    ///======các hàm tăng tốc độ query
    ///
    public Dictionary<string, (int TotalPost, DateTime? LastPostTime)> GetPostStatsBatch(List<string> pageIds)
    {
        var result = new Dictionary<string, (int, DateTime?)>();

        if (pageIds == null || pageIds.Count == 0)
            return result;

        using (var conn = OpenConnection())
        {
            // Tạo parameter an toàn
            var parameters = pageIds
                .Select((id, i) => new SqlParameter($"@id{i}", id))
                .ToList();

            string inClause = string.Join(",", parameters.Select(p => p.ParameterName));

            string sql = $@"
        SELECT 
            p.PageIDContainer AS PageID,
            COUNT(*) AS TotalPost,
            MAX(pi.RealPostTime) AS LastPostTime
        FROM TablePost p
        LEFT JOIN TablePostInfo pi ON p.PostID = pi.PostID
        WHERE p.PageIDContainer IN ({inClause})
        GROUP BY p.PageIDContainer";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddRange(parameters.ToArray());

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string pageId = reader["PageID"].ToString();
                        int total = Convert.ToInt32(reader["TotalPost"]);

                        DateTime? last = reader["LastPostTime"] == DBNull.Value
                            ? (DateTime?)null
                            : Convert.ToDateTime(reader["LastPostTime"]);

                        result[pageId] = (total, last);
                    }
                }
            }
        }

        return result;
    }
}
