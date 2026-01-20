using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CrawlFB_PW._1._0.DTO;

namespace CrawlFB_PW._1._0.DAO
{
    public class ProfileInfoDAO
    {
        public ProfileInfoDAO() { }

        private SqlConnection GetConn()
        {
            return SQLDAO.Instance.OpenConnection();
        }

        // ============================================================
        // 1️⃣ Lấy toàn bộ profile
        // ============================================================
        public List<ProfileDB> GetAllProfiles()
        {
            var list = new List<ProfileDB>();

            try
            {
                Libary.Instance.LogTech("GetAllProfiles: START");
                using (var conn = GetConn())
                using (var cmd = new SqlCommand(@"
            SELECT ID, IDAdbrowser, ProfileName, ProfileLink, ProfileStatus, UseTab, ProfileType
            FROM TableProfileInfo
            ORDER BY ID ASC", conn))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new ProfileDB
                        {
                            ID = Convert.ToInt32(rd["ID"]),
                            IDAdbrowser = rd["IDAdbrowser"]?.ToString(),
                            ProfileName = rd["ProfileName"]?.ToString(),
                            ProfileLink = rd["ProfileLink"]?.ToString(),
                            ProfileStatus = rd["ProfileStatus"]?.ToString(),
                            UseTab = Convert.ToInt32(rd["UseTab"]),
                            ProfileType = rd["ProfileType"]?.ToString()
                        });
                    }
                }
                Libary.Instance.LogTech($"GetAllProfiles: OK ({list.Count} rows)");
                return list;
            }
            catch (Exception ex)
            {
                Libary.Instance.LogTech("❌ GetAllProfiles FAILED: " + ex.Message);
                return list; // hoặc throw; tùy chiến lược
            }
        }


        // ============================================================
        // 2️⃣ Thêm Profile
        // ============================================================
        public bool InsertProfile(ProfileDB p)
        {
            using (var conn = GetConn())
            using (var cmd = new SqlCommand(@"
                INSERT INTO TableProfileInfo 
                (IDAdbrowser, ProfileName, ProfileLink, ProfileStatus, UseTab, ProfileType)
                VALUES (@adb, @name, @link, @status, @usetab, @type)", conn))
            {
                cmd.Parameters.AddWithValue("@adb", p.IDAdbrowser);
                cmd.Parameters.AddWithValue("@name", p.ProfileName);
                cmd.Parameters.AddWithValue("@link", p.ProfileLink);
                cmd.Parameters.AddWithValue("@status", p.ProfileStatus);
                cmd.Parameters.AddWithValue("@usetab", p.UseTab);
                cmd.Parameters.AddWithValue("@type", p.ProfileType);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ============================================================
        // 3️⃣ Update FULL profile
        // ============================================================
        public bool UpdateProfile(ProfileDB p)
        {
            using (var conn = GetConn())
            using (var cmd = new SqlCommand(@"
                UPDATE TableProfileInfo 
                SET IDAdbrowser=@adb, ProfileName=@name, ProfileLink=@link,
                    ProfileStatus=@status, UseTab=@usetab, ProfileType=@type
                WHERE ID=@id", conn))
            {
                cmd.Parameters.AddWithValue("@adb", p.IDAdbrowser);
                cmd.Parameters.AddWithValue("@name", p.ProfileName);
                cmd.Parameters.AddWithValue("@link", p.ProfileLink);
                cmd.Parameters.AddWithValue("@status", p.ProfileStatus);
                cmd.Parameters.AddWithValue("@usetab", p.UseTab);
                cmd.Parameters.AddWithValue("@type", p.ProfileType);
                cmd.Parameters.AddWithValue("@id", p.ID);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ============================================================
        // 4️⃣ Update trạng thái + tab
        // ============================================================
        public bool UpdateProfileStatus(ProfileDB p)
        {
            using (var conn = GetConn())
            using (var cmd = new SqlCommand(@"
                UPDATE TableProfileInfo 
                SET ProfileStatus=@status, UseTab=@usetab
                WHERE ID=@id", conn))
            {
                cmd.Parameters.AddWithValue("@status", p.ProfileStatus);
                cmd.Parameters.AddWithValue("@usetab", p.UseTab);
                cmd.Parameters.AddWithValue("@id", p.ID);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ============================================================
        // 5️⃣ Xóa profile
        // ============================================================
        public bool DeleteProfile(int id)
        {
            using (var conn = GetConn())
            using (var cmd = new SqlCommand(
                "DELETE FROM TableProfileInfo WHERE ID=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ============================================================
        // 6️⃣ Lấy Profile theo ID
        // ============================================================
        public ProfileDB GetProfileByID(int id)
        {
            using (var conn = GetConn())
            using (var cmd = new SqlCommand(@"
                SELECT ID, IDAdbrowser, ProfileName, ProfileLink,
                       ProfileStatus, UseTab, ProfileType
                FROM TableProfileInfo WHERE ID=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);

                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        return new ProfileDB
                        {
                            ID = Convert.ToInt32(rd["ID"]),
                            IDAdbrowser = rd["IDAdbrowser"].ToString(),
                            ProfileName = rd["ProfileName"].ToString(),
                            ProfileLink = rd["ProfileLink"].ToString(),
                            ProfileStatus = rd["ProfileStatus"].ToString(),
                            UseTab = Convert.ToInt32(rd["UseTab"]),
                            ProfileType = rd["ProfileType"].ToString()
                        };
                    }
                }
            }
            return null;
        }

        // ============================================================
        // 7️⃣ Lấy Profile theo IDAdbrowser
        // ============================================================
        public ProfileDB GetProfileByAdbrowser(string adb)
        {
            using (var conn = GetConn())
            using (var cmd = new SqlCommand(@"
                SELECT ID, IDAdbrowser, ProfileName, ProfileLink,
                       ProfileStatus, UseTab, ProfileType
                FROM TableProfileInfo WHERE IDAdbrowser=@adb", conn))
            {
                cmd.Parameters.AddWithValue("@adb", adb);

                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        return new ProfileDB
                        {
                            ID = Convert.ToInt32(rd["ID"]),
                            IDAdbrowser = rd["IDAdbrowser"].ToString(),
                            ProfileName = rd["ProfileName"].ToString(),
                            ProfileLink = rd["ProfileLink"].ToString(),
                            ProfileStatus = rd["ProfileStatus"].ToString(),
                            UseTab = Convert.ToInt32(rd["UseTab"]),
                            ProfileType = rd["ProfileType"].ToString()
                        };
                    }
                }
            }
            return null;
        }

        // ============================================================
        // 8️⃣ Lấy profile đang mở tab (UseTab=1)
        // ============================================================
        public ProfileDB GetActiveTabProfile()
        {
            using (var conn = GetConn())
            using (var cmd = new SqlCommand(@"
                SELECT TOP 1 ID, IDAdbrowser, ProfileName, ProfileLink,
                             ProfileStatus, UseTab, ProfileType
                FROM TableProfileInfo WHERE UseTab = 1", conn))
            using (var rd = cmd.ExecuteReader())
            {
                if (rd.Read())
                {
                    return new ProfileDB
                    {
                        ID = Convert.ToInt32(rd["ID"]),
                        IDAdbrowser = rd["IDAdbrowser"].ToString(),
                        ProfileName = rd["ProfileName"].ToString(),
                        ProfileLink = rd["ProfileLink"].ToString(),
                        ProfileStatus = rd["ProfileStatus"].ToString(),
                        UseTab = Convert.ToInt32(rd["UseTab"]),
                        ProfileType = rd["ProfileType"].ToString()
                    };
                }
            }
            return null;
        }

        // ============================================================
        // 9️⃣ Reset UseTab = 0 cho tất cả profile
        // ============================================================
        public void ResetAllTabs()
        {
            using (var conn = GetConn())
            using (var cmd = new SqlCommand(
                "UPDATE TableProfileInfo SET UseTab = 0", conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        // ============================================================
        // 🔟 Kiểm tra trùng AdbrowserID
        // ============================================================
        public bool ExistsAdbrowser(string adb)
        {
            using (var conn = GetConn())
            using (var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM TableProfileInfo WHERE IDAdbrowser=@adb", conn))
            {
                cmd.Parameters.AddWithValue("@adb", adb);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // ============================================================
        // 1️⃣1️⃣ Thay đổi runtime UseTab (+1 / -1)
        // ============================================================
        public void ChangeRuntimeUseTab(int profileId, int delta)
        {
            using (var conn = GetConn())
            {
                // Lấy UseTab hiện tại
                int current = 0;

                using (var cmdGet = new SqlCommand(
                    "SELECT UseTab FROM TableProfileInfo WHERE ID=@id", conn))
                {
                    cmdGet.Parameters.AddWithValue("@id", profileId);
                    var v = cmdGet.ExecuteScalar();
                    if (v != null && v != DBNull.Value)
                        current = Convert.ToInt32(v);
                }

                int newVal = Math.Max(0, Math.Min(3, current + delta));

                using (var cmdUpdate = new SqlCommand(
                    "UPDATE TableProfileInfo SET UseTab=@v WHERE ID=@id", conn))
                {
                    cmdUpdate.Parameters.AddWithValue("@v", newVal);
                    cmdUpdate.Parameters.AddWithValue("@id", profileId);
                    cmdUpdate.ExecuteNonQuery();
                }
            }
        }

        // ============================================================
        // RESET MAPPING PAGE (TableManagerProfile)
        // ============================================================
        public void ResetAllMappings()
        {
            using (var conn = GetConn())
            using (var cmd = new SqlCommand(
                "DELETE FROM TableManagerProfile", conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        // ============================================================
        // RESET STATUS toàn bộ Monitor
        // ============================================================
        public void ResetAllMonitorStatus()
        {
            using (var conn = GetConn())
            using (var cmd = new SqlCommand(
                "UPDATE TablePageMonitor SET Status='Idle', LastScanTime=NULL", conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        // ============================================================
        // Lấy UseTab theo ProfileID
        // ============================================================
        public int GetUseTab(int profileId)
        {
            using (var conn = GetConn())
            using (var cmd = new SqlCommand(@"
                SELECT TOP 1 UseTab FROM TableProfileInfo
                WHERE ID=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", profileId);

                var v = cmd.ExecuteScalar();
                if (v == null || v == DBNull.Value)
                    return 0;

                return Convert.ToInt32(v);
            }
        }
    }
}
