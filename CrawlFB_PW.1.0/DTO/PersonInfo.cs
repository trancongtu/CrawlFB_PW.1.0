using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.DTO
{
    public class PersonInfo
    {
        public string PersonID { get; set; } = "N/A";

        public string IDFBPerson { get; set; } = "N/A";

        public string PersonLink { get; set; } = "N/A";

        public string PersonName { get; set; } = "N/A";

        public string PersonInfoText { get; set; } = "N/A"; // Bio, mô tả

        public string PersonNote { get; set; } = "N/A"; // person / personKOL / unknown

        public string PersonTimeSave { get; set; }
            = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        public PersonInfo() { }

        // Constructor đầy đủ
        public PersonInfo(string personId, string name, string link, string note = "N/A")
        {
            this.PersonID = personId;
            this.PersonName = name;
            this.PersonLink = link;
            this.PersonNote = note;
            this.PersonInfoText = "N/A";
            this.IDFBPerson = "N/A";
            this.PersonTimeSave = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public override string ToString()
        {
            return $"{PersonName} - {PersonLink}";
        }
    }

}
