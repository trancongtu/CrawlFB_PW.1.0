using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlFB_PW._1._0.ViewModels
{
    public class PageMonitorViewModel : INotifyPropertyChanged
    {
        public int STT { get; set; }
        public bool Select { get; set; }

        public string PageID { get; set; }
        public string PageName { get; set; }

        private string _status;
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        private int _postScan;
        public int PostScan
        {
            get => _postScan;
            set { _postScan = value; OnPropertyChanged(nameof(PostScan)); }
        }
        private int _postSaved;
        public int PostSaved
        {
            get => _postSaved;
            set { _postSaved = value; OnPropertyChanged(nameof(PostSaved)); }
        }
        public int Countdown { get; set; }
        public int DelayExtra { get; set; } // 🔥 delay tăng thêm
        private DateTime? _lastScanTime;
        public DateTime? LastScanTime
        {
            get => _lastScanTime;
            set { _lastScanTime = value; OnPropertyChanged(nameof(LastScanTime)); }
        }
        public DateTime? TimeLastPost { get; set; }
        // 🔥 realtime chạy
        public DateTime? StartRunTime { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
