using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CrawlFB_PW._1._0.DAO
{
    public class ProfileSlotManager
    {
        private static readonly Lazy<ProfileSlotManager> _inst = new Lazy<ProfileSlotManager>(() => new ProfileSlotManager());
        public static ProfileSlotManager Instance => _inst.Value;

        // lưu số slot đang dùng cho profile
        private readonly ConcurrentDictionary<string, int> _usedSlots = new ConcurrentDictionary<string, int>();
        private ProfileSlotManager() { }

        // try reserve 1 slot (true nếu còn)
        public bool TryReserveSlot(string profileId)
        {
            try
            {
                var profilesFile = PathHelper.Instance.GetProfilesFilePath();
                if (!File.Exists(profilesFile)) return false;
                var json = File.ReadAllText(profilesFile);
                var profiles = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DTO.ProfileInfo>>(json);
                var p = profiles?.FirstOrDefault(x => x.ProfileId == profileId);
                if (p == null) return false;

                int used = _usedSlots.GetOrAdd(profileId, p.CurrentTabs);
                // currenttabs in profile may be initial value; we count extra reservations here
                if (used >= p.MaxTabs) return false;
                _usedSlots[profileId] = used + 1;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void ReleaseSlot(string profileId)
        {
            _usedSlots.AddOrUpdate(profileId, 0, (k, v) => Math.Max(0, v - 1));
        }

        public int GetUsedSlots(string profileId) => _usedSlots.TryGetValue(profileId, out var v) ? v : 0;
        public void InitializeSlotsFromProfiles(List<DTO.ProfileInfo> profiles)
        {
            if (profiles == null) return;

            foreach (var p in profiles)
            {
                // Nếu currentTabs < 1 => mặc định 1 (tab gốc)
                int used = Math.Max(0, p.CurrentTabs - 1); // số tab phụ đang dùng
                _usedSlots[p.ProfileId] = used;
                Libary.Instance.CreateLog($"[SlotMgr] Init {p.ProfileId}: used={used}, current={p.CurrentTabs}, max={p.MaxTabs}");
            }
        }
        public void SetUsedSlots(string profileId, int count)
        {
            _usedSlots[profileId] = Math.Max(0, count);
            Libary.Instance.CreateLog($"[SlotMgr] SetUsedSlots {profileId} = {count}");
        }
        public DTO.ProfileInfo GetProfileByName(string fbName)
        {
            try
            {
                string file = PathHelper.Instance.GetProfilesFilePath();
                if (!File.Exists(file)) return null;

                var json = File.ReadAllText(file);
                var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DTO.ProfileInfo>>(json);
                if (list == null) return null;

                return list.FirstOrDefault(p =>
                    string.Equals(p.FacebookName, fbName, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(p.Name, fbName, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return null;
            }
        }
    }
}
