using System;
using System.Collections.Generic;
using System.Data.SQLite;
using CrawlFB_PW._1._0;
using CrawlFB_PW._1._0.DAO;
using CrawlFB_PW._1._0.DTO;
using Ads = CrawlFB_PW._1._0.DAO.AdsPowerPlaywrightManager;
public class ManagerProfileDAO
{
    private readonly string dbPath;

    public ManagerProfileDAO()
    {
        // ⭐ Lấy đúng DB ProfileInfo.db từ PathHelper
        dbPath = PathHelper.Instance.GetProfileDatabasePath();
    }

    private SQLiteConnection GetConn()
    {
        return SqliteHelper.Instance.GetConnection(dbPath);
    }

    // Lấy tất cả mapping profile ↔ page
    public List<ManagerProfileDTO> GetAllMappings()
    {
        var list = new List<ManagerProfileDTO>();

        using (var conn = GetConn())
        {
            conn.Open();

            string sql = @"SELECT ID, PageIDCrawl, LinkFBCrawl, IDProfile 
                           FROM TableManagerProfile";

            using (var cmd = new SQLiteCommand(sql, conn))
            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    list.Add(new ManagerProfileDTO
                    {
                        ID = rd.GetInt32(0),
                        PageIDCrawl = rd.GetString(1),
                        LinkFBCrawl = rd.GetString(2),
                        IDProfile = rd.GetInt32(3)
                    });
                }
            }
        }

        return list;
    }

    // Thêm mapping mới
    public bool InsertMapping(ManagerProfileDTO m)
    {
        using (var conn = GetConn())
        {
            conn.Open();

            string sql = @"INSERT INTO TableManagerProfile (PageIDCrawl, LinkFBCrawl, IDProfile)
                           VALUES (@page, @link, @pid)";

            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@page", m.PageIDCrawl);
                cmd.Parameters.AddWithValue("@link", m.LinkFBCrawl);
                cmd.Parameters.AddWithValue("@pid", m.IDProfile);

                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }

    // Đếm tab = số page được assign
    public int CountMappingByProfile(int profileId)
    {
        using (var conn = GetConn())
        {
            conn.Open();

            string sql = @"SELECT COUNT(*) FROM TableManagerProfile WHERE IDProfile=@id";

            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", profileId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }

    // Xóa mapping
    public bool DeleteMapping(int id)
    {
        using (var conn = GetConn())
        {
            conn.Open();

            string sql = @"DELETE FROM TableManagerProfile WHERE ID=@id";

            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
    //
    public void DeleteByProfile(int profileId)
    {
        using (var conn = GetConn())
        {
            conn.Open();

            string sql = @"DELETE FROM TableManagerProfile WHERE IDProfile = @id";

            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", profileId);
                cmd.ExecuteNonQuery();
            }
        }
    }
    public void UpdateProfileUseTab(int profileId, int useTab)
    {
        string dbPath = PathHelper.Instance.GetProfileDatabasePath();
        using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
        {
            conn.Open();
            string sql = @"UPDATE ProfileInfo SET UseTab=@tab WHERE ID=@id";

            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@tab", useTab);
                cmd.Parameters.AddWithValue("@id", profileId);
                cmd.ExecuteNonQuery();
            }
        }
    }
    public void InsertManagerProfile(int profileId, string pageId, string pageLink)
    {
        try
        {
            string dbPath = PathHelper.Instance.GetProfileDatabasePath();
            using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
            {
                conn.Open();
                string sql = @"INSERT INTO TableManagerProfile (PageIDCrawl, LinkFBCrawl, IDProfile)
                           VALUES (@page, @link, @pid)";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@page", pageId);
                    cmd.Parameters.AddWithValue("@link", pageLink);
                    cmd.Parameters.AddWithValue("@pid", profileId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("InsertManagerProfile error: " + ex.Message);
        }
    }
    public List<ManagerProfileDTO> GetMappingByProfile(int profileId)
    {
        var list = new List<ManagerProfileDTO>();

        try
        {
            string dbPath = PathHelper.Instance.GetProfileDatabasePath();

            using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
            {
                conn.Open();

                string sql = @"SELECT ID, PageIDCrawl, LinkFBCrawl, IDProfile 
                           FROM TableManagerProfile
                           WHERE IDProfile = @id";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", profileId);

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            list.Add(new ManagerProfileDTO
                            {
                                ID = rd.GetInt32(0),
                                PageIDCrawl = rd.GetString(1),
                                LinkFBCrawl = rd.GetString(2),
                                IDProfile = rd.GetInt32(3)
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Libary.Instance.CreateLog("[GetMappingByProfile] ERROR: " + ex.Message);
        }

        return list;
    }
    public void RemoveMapping(int profileId, string pageId)
    {
        string dbPath = PathHelper.Instance.GetProfileDatabasePath();

        using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
        {
            conn.Open();

            string sql = "DELETE FROM TableManagerProfile WHERE IDProfile=@p AND PageIDCrawl=@page";

            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@p", profileId);
                cmd.Parameters.AddWithValue("@page", pageId);
                cmd.ExecuteNonQuery();
            }
        }
    }
    public List<ManagerProfileDTO> GetMappingByPageID(string pageId)
    {
        var list = new List<ManagerProfileDTO>();
        string dbPath = PathHelper.Instance.GetProfileDatabasePath();
        using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
        {
            conn.Open();
            string sql = @"SELECT ID, IDProfile, PageIDCrawl, LinkFBCrawl
                       FROM TableManagerProfile
                       WHERE PageIDCrawl = @page";

            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@page", pageId);
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new ManagerProfileDTO
                        {
                            ID = Convert.ToInt32(rd["ID"]),
                            IDProfile = Convert.ToInt32(rd["IDProfile"]),
                            PageIDCrawl = rd["PageIDCrawl"].ToString(),
                            LinkFBCrawl = rd["LinkFBCrawl"].ToString()
                        });
                    }
                }
            }
        }
        return list;
    }
    public void RemoveMappingByID(int id)
    {
        string dbPath = PathHelper.Instance.GetProfileDatabasePath();

        using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
        {
            conn.Open();
            string sql = "DELETE FROM TableManagerProfile WHERE ID=@id";

            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }
    }
    public void RemoveMappingByPageID(int profileId, string pageId)
    {
        string dbPath = PathHelper.Instance.GetProfileDatabasePath();
        using (var conn = SqliteHelper.Instance.GetConnection(dbPath))
        {
            conn.Open();
            string sql = @"DELETE FROM TableManagerProfile
                       WHERE IDProfile=@pid AND PageIDCrawl=@page";

            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@pid", profileId);
                cmd.Parameters.AddWithValue("@page", pageId);
                cmd.ExecuteNonQuery();
            }
        }
    }
    public bool IsProfileRunning(int profileId)
    {
        return CountMappingByProfile(profileId) > 0;
    }

}
