using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrawlFB_PW._1._0.Enums;

namespace CrawlFB_PW._1._0.DTO
{
    public class PersonInfo
    {
        public string PersonID { get; set; }

        public string IDFBPerson { get; set; }

        public string PersonLink { get; set; }

        public string PersonName { get; set; }

        public string PersonInfoText { get; set; }  // Bio, mô tả

        public FBType PersonNote { get; set; } // person / personKOL / unknown

        public string PersonTimeSave { get; set; }
            = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        public PersonInfo() { }

        // Constructor đầy đủ
        public PersonInfo(string personId, string name, string link, FBType note = FBType.Unknown)
        {
            this.PersonID = Clean(personId);
            this.PersonName = Clean(name);
            this.PersonLink = Clean(link);
            this.PersonNote = note;

            this.PersonInfoText = null;
            this.IDFBPerson = null;

            this.PersonTimeSave = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        string Clean(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }
        public override string ToString()
        {
            return $"{PersonName} - {PersonLink}";
        }
    }

}
