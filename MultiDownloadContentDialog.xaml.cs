using BilibiliDanmakuDownloader.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BilibiliDanmakuDownloader
{
    public sealed partial class MultiDownloadContentDialog : ContentDialog
    {
        public static event Action<MultiDownloadStartedEventArgs> MultiDownloadStartedEvent;
        ObservableCollection<DownloadItemModel> ItemCollection = new ObservableCollection<DownloadItemModel>();
        public MultiDownloadContentDialog()
        {
            this.InitializeComponent();
        }

        public void SetContent(ObservableCollection<DownloadItemModel> downloadItemModels)
        {
            ItemCollection = downloadItemModels;
        }

        private void Start(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var CIDList = from item in ItemCollection where item.IsSelected select item.CID;
            MultiDownloadStartedEvent?.Invoke(new MultiDownloadStartedEventArgs() { CIDArray = CIDList.ToArray(),BVID=ItemCollection[0].BVID});
        }
    }
}
