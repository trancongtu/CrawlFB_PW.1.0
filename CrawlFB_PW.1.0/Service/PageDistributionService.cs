using CrawlFB_PW._1._0.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrawlFB_PW._1._0.Service
{
    internal class PageDistributionService
    {
        public Dictionary<ProfileDB, List<T>> Distribute<T>(List<ProfileDB> profiles, List<T> items, Func<T, string> getKey,string itemName = "item")
        {
            Libary.Instance.LogService($"▶ Bắt đầu chia {itemName}");

            var result = profiles.ToDictionary(p => p, _ => new List<T>());

            int index = 0;

            foreach (var item in items)
            {
                var profile = profiles[index];
                result[profile].Add(item);

                Libary.Instance.LogService(
                    $"Profile {profile.ProfileName} ({profile.IDAdbrowser}) nhận {itemName}: {getKey(item)}"
                );

                index = (index + 1) % profiles.Count;
            }

            foreach (var kv in result)
            {
                Libary.Instance.LogService(
                    $"Profile {kv.Key.ProfileName} phụ trách {kv.Value.Count} {itemName}"
                );
            }

            Libary.Instance.LogService($"✔ Kết thúc chia {itemName}");

            return result;
        }

    }
}
