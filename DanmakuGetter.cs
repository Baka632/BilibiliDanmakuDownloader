using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.UI.Xaml.Controls;

namespace BilibiliDanmakuDownloader
{
    class DanmakuGetter : DependencyObject
    {
        static AppInfomation appInfomation = App.appInfomation;
        public async void GetDanmaku(long aid, StorageFile saveFile)
        {
            try
            {
                if (aid < 0)
                {
                    throw new ArgumentException();
                }
                BackgroundDownloader downloader = new BackgroundDownloader();
                DownloadOperation downloadRawCidData = downloader.CreateDownload(new Uri($"https://www.bilibili.com/widget/getPageList?aid={aid}"), await ApplicationData.Current.TemporaryFolder.CreateFileAsync(aid.ToString()));
                await downloadRawCidData.StartAsync();
                StorageFile jsonFile = await ApplicationData.Current.TemporaryFolder.GetFileAsync(aid.ToString());
                string rawCidData = File.ReadAllText(jsonFile.Path).Replace("[", "").Replace("]", "").Trim();
                JObject jObject = JObject.Parse(rawCidData);
                JProperty jProperty = jObject.Property("cid");
                int cid = int.Parse(jProperty.Value.ToString());
                await jsonFile.DeleteAsync();


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
            appInfomation.InfoBarMessage = $"文件已保存为{path}";
            appInfomation.IsInfoBarOpen = true;
        }

        public async void GetDanmaku(long cid,bool IsCid,StorageFile saveFile)
        {
            if (IsCid)
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
            else
            {
                GetDanmaku(cid, saveFile);
            }
        }

        public async void GetDanmaku(string bvid, StorageFile saveFile)
        {
            try
            {
                BackgroundDownloader downloader = new BackgroundDownloader();
                DownloadOperation downloadRawCidData = downloader.CreateDownload(new Uri($"https://api.bilibili.com/x/player/pagelist?bvid={bvid}"), await ApplicationData.Current.TemporaryFolder.CreateFileAsync(bvid));
                await downloadRawCidData.StartAsync();
                StorageFile jsonFile = await ApplicationData.Current.TemporaryFolder.GetFileAsync(bvid);
                string rawCidData = File.ReadAllText(jsonFile.Path).Replace("[", "").Replace("]", "").Trim();
                JObject jObject = JObject.Parse(rawCidData);
                JProperty jProperty = jObject.Property("cid");
                int cid = int.Parse(jProperty.Value.ToString());
                await jsonFile.DeleteAsync();

                string site = $"http://comment.bilibili.com/{cid}.xml";
                string fileName = $"{cid}.xml";
                DownloadOperation download = downloader.CreateDownload(new Uri(site), saveFile);
                await download.StartAsync();
                ShowInfoBar(saveFile.Path);
            }
            catch (Exception ex)
            {
                ShowInfoBar(ex);
            }
        }
    }
}
