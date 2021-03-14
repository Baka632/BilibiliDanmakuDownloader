using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Text.RegularExpressions;
using System.Json;
using System.Net.Sockets;
using System.Collections.ObjectModel;
using Windows.System;
using Microsoft.Toolkit.Uwp.Helpers;

namespace BilibiliDanmakuDownloader
{
    class DanmakuGetter
    {
        static AppInfomation appInfomation = App.appInfomation;
        StorageFolder storageFolder = null;

        public DanmakuGetter()
        {
            MultiDownloadContentDialog.MultiDownloadStartedEvent += MultiDownloadContentDialog_MultiDownloadStartedEvent;
        }

        /* 因为B站相关api已经关闭,因此无法通过aid获取cid
        public void GetDanmaku(long aid, StorageFile saveFile)
        {
            try
            {
                if (aid < 0)
                {
                    throw new ArgumentException();
                }
                string rawCidData = GetWebContent($"https://www.bilibili.com/widget/getPageList?aid={aid}");
                JsonValue jsonValue = JsonObject.Parse(rawCidData);
                int cid = int.Parse(jsonValue["cid"]);
                GetDanmaku(cid,true,saveFile);
            }
            catch (Exception ex)
            {
                ShowInfoBar(ex);
            }
        }
        */

        private static void ShowInfoBar(Exception exception)
        {
            if (exception.HResult == -2145844844)
            {
                appInfomation.InfoBarSeverity = InfoBarSeverity.Error;
                appInfomation.InfoBarTitle = "无效的ID";
                appInfomation.InfoBarMessage = "请重试。";
                appInfomation.IsInfoBarOpen = true;
            }
            else
            {
                appInfomation.InfoBarSeverity = InfoBarSeverity.Error;
                appInfomation.InfoBarTitle = "出现问题";
                appInfomation.InfoBarMessage = exception.Message;
                appInfomation.IsInfoBarOpen = true;
            }
        }

        private static void ShowInfoBar(string path)
        {
            appInfomation.InfoBarSeverity = InfoBarSeverity.Success;
            appInfomation.InfoBarTitle = "保存成功";
            appInfomation.InfoBarMessage = $"文件已保存在{path}";
            appInfomation.IsInfoBarOpen = true;
        }
        
        private static void ShowInfoBar(InfoBarSeverity severity, string title, string message)
        {
            appInfomation.InfoBarSeverity = severity;
            appInfomation.InfoBarTitle = title;
            appInfomation.InfoBarMessage = message;
            appInfomation.IsInfoBarOpen = true;
        }

        private void MultiDownloadContentDialog_MultiDownloadStartedEvent(Model.MultiDownloadStartedEventArgs args)
        {
            GetDanmaku(args.CIDArray, args.BVID, storageFolder);
        }

        public async void GetDanmaku(long cid,StorageFile saveFile)
        {
            if (cid < 0)
            {
                throw new ArgumentException();
            }
            try
            {
                BackgroundDownloader downloader = new BackgroundDownloader();
                string site = $"http://comment.bilibili.com/{cid}.xml";
                DownloadOperation download = downloader.CreateDownload(new Uri(site), saveFile);
                await download.StartAsync();
                ShowInfoBar(saveFile.Path);
            }
            catch (Exception ex)
            {
                ShowInfoBar(ex);
            }
        }
        public async void GetDanmaku(int[] cidList,string bvid,StorageFolder folder)
        {
            try
            {
                foreach (var cid in cidList)
                {
                    BackgroundDownloader downloader = new BackgroundDownloader();
                    string site = $"http://comment.bilibili.com/{cid}.xml";
                    DownloadOperation download = downloader.CreateDownload(new Uri(site), await folder.CreateFileAsync($"{bvid}.xml",CreationCollisionOption.GenerateUniqueName));
                    await download.StartAsync();
                }
                
                ShowInfoBar(InfoBarSeverity.Success,"下载已成功",$"下载的弹幕文件已经保存在\"{folder.Path}\"");
            }
            catch (Exception ex)
            {
                ShowInfoBar(ex);
            }
        }

        public async void GetDanmaku(string bvid, StorageFolder saveFolder)
        {
            try
            {
                string rawJson = GetWebContent(bvid);
                JsonValue jsonValue = JsonObject.Parse(rawJson);
                JsonValue content = jsonValue["data"];
                if (content.Count > 1)
                {
                    storageFolder = saveFolder;
                    ObservableCollection<DownloadItemModel> downloadItemModels = new ObservableCollection<DownloadItemModel>();
                    ShowInfoBar(InfoBarSeverity.Informational, "正在处理一些事情...", "请稍候。");
                    await Task.Run(() =>
                    {
                        foreach (JsonValue item in content)
                        {
                            DownloadItemModel downloadItem = new DownloadItemModel() { CID = item["cid"], ItemTitle = item["part"], BVID = bvid };
                            lock (this)
                            {
                                downloadItemModels.Add(downloadItem);
                            }
                        }
                    });
                    MultiDownloadContentDialog contentDialog = new MultiDownloadContentDialog();
                    contentDialog.SetContent(downloadItemModels);
                    Windows.UI.Xaml.Controls.ContentDialogResult result = await contentDialog.ShowAsync();
                    switch (result)
                    {
                        case Windows.UI.Xaml.Controls.ContentDialogResult.None:
                            ShowInfoBar(InfoBarSeverity.Informational, "已取消操作", "操作已被用户取消。");
                            break;
                        case Windows.UI.Xaml.Controls.ContentDialogResult.Primary:
                            ShowInfoBar(InfoBarSeverity.Informational, "正在下载...","请稍候...");
                            break;
                        default:
                            throw new ArgumentException("未知操作");
                    }
                }
                else
                {
                    int cid = int.Parse(content[0]["cid"].ToString());
                    StorageFile saveFile = await saveFolder.CreateFileAsync($"{bvid}.xml");
                    GetDanmaku(cid, saveFile);
                    ShowInfoBar(saveFile.Path);
                }
            }
            catch (Exception ex)
            {
                ShowInfoBar(ex);
            }
        }

        private static string GetWebContent(string bvid)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://api.bilibili.com/x/player/pagelist?bvid={bvid}");
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            return new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
        }
    }
}
