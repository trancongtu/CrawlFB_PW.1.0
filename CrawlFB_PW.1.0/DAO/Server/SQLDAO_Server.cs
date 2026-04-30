using System;
using System.Collections.Generic;
using System.Data;
using CrawlFB_PW._1._0.DTO;
using CrawlFB_PW._1._0.Enums;
using CrawlFB_PW._1._0.Helper.Data;

public class SQLDAO_Server
{
    private static SQLDAO_Server instance;

    public static SQLDAO_Server Instance
    {
        get
        {
            if (instance == null)
                instance = new SQLDAO_Server();
            return instance;
        }
    }

    private SQLDAO_Server() { }

    // ==============================
    // INIT TABLE (USER MAPPING)
    // ==============================
    public void InitUserTables()
    {
        string sql = @"

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserPosts')
CREATE TABLE UserPosts (
    Id INT IDENTITY PRIMARY KEY,
    UserId INT,
    PostId INT,
    Status NVARCHAR(20) DEFAULT 'Pending',
    CreatedAt DATETIME DEFAULT GETDATE(),
    UNIQUE(UserId, PostId)
)

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserPages')
CREATE TABLE UserPages (
    Id INT IDENTITY PRIMARY KEY,
    UserId INT,
    PageId INT,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UNIQUE(UserId, PageId)
)

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserPersons')
CREATE TABLE UserPersons (
    Id INT IDENTITY PRIMARY KEY,
    UserId INT,
    PersonId INT,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UNIQUE(UserId, PersonId)
)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users')
CREATE TABLE Users (
    Id INT IDENTITY PRIMARY KEY,

    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,

    Role NVARCHAR(20) DEFAULT 'User',  -- Admin / User

    IsActive BIT DEFAULT 1,

    CreatedAt DATETIME DEFAULT GETDATE()
)
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (Username, PasswordHash, Role)
    VALUES ('admin', '123456', 'Admin')
END
";

        SQLDAO.Instance.ExecuteNonQuery(sql);
    }
    // user
    public int? Login(string username, string password)
    {
        string sql = @"
    SELECT Id 
    FROM Users 
    WHERE Username = @Username 
    AND PasswordHash = @Password
    AND IsActive = 1";

        var result = SQLDAO.Instance.ExecuteScalar(sql, new Dictionary<string, object>
    {
        { "@Username", username },
        { "@Password", password }
    });

        if (result == null || result == DBNull.Value)
            return null;

        return Convert.ToInt32(result); // ✅ đúng
    }
    public void CreateUser(string username, string passwordHash)
    {
        string sql = @"
    INSERT INTO Users (Username, PasswordHash)
    VALUES (@Username, @PasswordHash)";

        SQLDAO.Instance.ExecuteNonQuery(sql, new Dictionary<string, object>
    {
        { "@Username", username },
        { "@PasswordHash", passwordHash }
    });
    }
    // ==============================
    // MAP USER
    // ==============================

    public void MapUserPost(int userId, int postId)
    {
        string sql = @"
IF NOT EXISTS (
    SELECT 1 FROM UserPosts WHERE UserId = @UserId AND PostId = @PostId
)
INSERT INTO UserPosts(UserId, PostId, Status)
VALUES (@UserId, @PostId, 'Pending')";

        SQLDAO.Instance.ExecuteNonQuery(sql, new Dictionary<string, object>
        {
            { "@UserId", userId },
            { "@PostId", postId }
        });
    }

    public void MapUserPage(int userId, int pageId)
    {
        string sql = @"
IF NOT EXISTS (
    SELECT 1 FROM UserPages WHERE UserId = @UserId AND PageId = @PageId
)
INSERT INTO UserPages(UserId, PageId)
VALUES (@UserId, @PageId)";

        SQLDAO.Instance.ExecuteNonQuery(sql, new Dictionary<string, object>
        {
            { "@UserId", userId },
            { "@PageId", pageId }
        });
    }

    public void MapUserPerson(int userId, int personId)
    {
        string sql = @"
IF NOT EXISTS (
    SELECT 1 FROM UserPersons WHERE UserId = @UserId AND PersonId = @PersonId
)
INSERT INTO UserPersons(UserId, PersonId)
VALUES (@UserId, @PersonId)";

        SQLDAO.Instance.ExecuteNonQuery(sql, new Dictionary<string, object>
        {
            { "@UserId", userId },
            { "@PersonId", personId }
        });
    }

    // ==============================
    // CORE GẮN USER → DATA
    // ==============================

    public void AttachUserToPost(int userId, int postId, int pageId, int personId)
    {
        SQLDAO.Instance.ExecuteInTransaction(() =>
        {
            MapUserPost(userId, postId);
            MapUserPage(userId, pageId);
            MapUserPerson(userId, personId);
        });
    }

    public string GetRole(int userId)
    {
        string sql = "SELECT Role FROM Users WHERE Id = @Id";

        var result = SQLDAO.Instance.ExecuteScalar(sql, new Dictionary<string, object>
    {
        { "@Id", userId }
    });

        return result?.ToString() ?? "User";
    }
    // ==============================
    // GET DATA
    // ==============================

    public List<PostPage> GetPostsByUser(int userId)
    {
        string sql = @"
    SELECT p.*
    FROM Posts p
    JOIN UserPosts up ON p.Id = up.PostId
    WHERE up.UserId = @UserId
    AND up.Status = 'Approved'
    ORDER BY p.CreatedAt DESC";

        return SQLDAO.Instance.QueryList(sql, reader => new PostPage
        {
            PostID = reader["PostIdFB"]?.ToString(),
            PageID = reader["PageId"]?.ToString(),

            Content = reader["Content"]?.ToString(),
            PostLink = reader["PostLink"]?.ToString(),

            PostTime = reader["PostTime"]?.ToString(),

            RealPostTime = reader["PostTime"] == DBNull.Value
                ? (DateTime?)null
                : Convert.ToDateTime(reader["PostTime"]),

            CommentCount = reader["CommentCount"] == DBNull.Value
                ? (int?)null
                : Convert.ToInt32(reader["CommentCount"]),

            ShareCount = reader["ShareCount"] == DBNull.Value
                ? (int?)null
                : Convert.ToInt32(reader["ShareCount"]),

            LikeCount = reader["LikeCount"] == DBNull.Value
                ? (int?)null
                : Convert.ToInt32(reader["LikeCount"]),

            PosterName = reader["PosterName"]?.ToString(),
            PosterLink = reader["PosterLink"]?.ToString(),
            PosterIdFB = reader["PosterIdFB"]?.ToString(),

            PageName = reader["PageName"]?.ToString(),
            PageLink = reader["PageLink"]?.ToString(),

            Attachment = reader["Attachment"]?.ToString(),
            Topic = reader["Topic"]?.ToString()
        },
        new Dictionary<string, object>
        {
        { "@UserId", userId }
        });
    }

    // ==============================
    // APPROVE (ADMIN)
    // ==============================

    public void ApprovePost(int userId, int postId)
    {
        string sql = @"
UPDATE UserPosts
SET Status = 'Approved'
WHERE UserId = @UserId AND PostId = @PostId";

        SQLDAO.Instance.ExecuteNonQuery(sql, new Dictionary<string, object>
        {
            { "@UserId", userId },
            { "@PostId", postId }
        });
    }
    public PostPage MapPost(IDataReader rd)
    {
        return new PostPage
        {
            PostID = rd.GetStringOrNull("PostID"),  // ✅ đúng
            PageID = rd.GetStringOrNull("PageId"),

            Content = rd.GetStringOrNull("Content"),
            PostLink = rd.GetStringOrNull("PostLink"),

            PostTime = rd.GetStringOrNull("PostTime"),
            RealPostTime = rd.GetDateTimeOrNull("PostTime"),

            LikeCount = rd.GetIntOrNull("LikeCount"),
            CommentCount = rd.GetIntOrNull("CommentCount"),
            ShareCount = rd.GetIntOrNull("ShareCount"),

            PosterName = rd.GetStringOrNull("PosterName"),
            PosterLink = rd.GetStringOrNull("PosterLink"),
            PosterIdFB = rd.GetStringOrNull("PosterIdFB"),
            PosterNote = rd.GetEnum("PosterNote", FBType.Unknown),

            PageName = rd.GetStringOrNull("PageName"),
            PageLink = rd.GetStringOrNull("PageLink"),

            ContainerIdFB = rd.GetStringOrNull("ContainerIdFB"),
            ContainerType = rd.GetEnum("ContainerType", FBType.Unknown),

            Attachment = rd.GetStringOrNull("Attachment"),
            Topic = rd.GetStringOrNull("Topic"),
            PostType = rd.GetEnum("PostType", PostType.UnknowType)
        };
    }
}