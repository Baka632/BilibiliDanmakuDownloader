using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace BilibiliDanmakuDownloader.Model
{
    public class MultiDownloadStartedEventArgs : EventArgs
    {
        public int[] CIDArray { get; set; }
        public string BVID { get; set; }
    }
}
