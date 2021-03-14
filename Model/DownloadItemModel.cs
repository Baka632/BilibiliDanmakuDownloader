using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliDanmakuDownloader
{
    public class DownloadItemModel
    {
        public string ItemTitle { get; set; }
        public bool IsSelected { get; set; }
        public int CID { get; set; }
        public string BVID { get; set; }
    }
}
