using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.DTO
{
    public class ProfileDB
    {
        public int ID { get; set; }
        public string IDAdbrowser { get; set; }
        public string ProfileName { get; set; }
        public string ProfileLink { get; set; }
        public string ProfileStatus { get; set; }   // Live / Die
        public int UseTab { get; set; }
        public string ProfileType { get; set; }     // Person / Pa
        public string StatusText => string.IsNullOrEmpty(ProfileStatus) ? "Chưa kiểm tra" : ProfileStatus;
        public string LinkText => string.IsNullOrEmpty(ProfileLink) ? "Chưa kiểm tra" : ProfileLink;
        public string TypeText => string.IsNullOrEmpty(ProfileType) ? "Chưa kiểm tra" : ProfileType;
        public string TabText => UseTab <= 0 ? "0 (chưa kiểm tra)" : UseTab.ToString();

    }
}
