using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace BilibiliDanmakuDownloader
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DanmakuGetter danmakuGetter = new DanmakuGetter();
        bool IsStarting = true;
        AppInfomation appInfomation = App.appInfomation;

        public MainPage()
        {
            this.InitializeComponent();
            IdProperty = idComboBox.SelectedItem as string;
            IsStarting = false;
        }

        public string IdProperty { get; set; }

        private void GetDanmaku(object sender, RoutedEventArgs e)
        {
            GetSaveFile();
        }

        private async void GetSaveFile()
        {
            FileSavePicker fileSavePicker = new FileSavePicker();
            fileSavePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            fileSavePicker.FileTypeChoices.Add("XML文件", new List<string>() { ".xml" });
            switch (IdProperty)
            {
                case "CID":
                    int cid;
                    if (int.TryParse(idTextBox.Text, out cid))
                    {
                        fileSavePicker.SuggestedFileName = $"{cid}";
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                        {
                            StorageFile storageFile = await fileSavePicker.PickSaveFileAsync();
                            if (storageFile != null)
                            {
                                danmakuGetter.GetDanmaku(cid, storageFile);
                            }
                            else
                            {
                                appInfomation.InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning;
                                appInfomation.InfoBarTitle = "已取消操作";
                                appInfomation.InfoBarMessage = "已取消操作";
                                appInfomation.IsInfoBarOpen = true;
                            }
                        });
                    }
                    else
                    {
                        getDanmakuButton.IsEnabled = false;
                    }
                    break;
                case "BV号":
                    string bvid = idTextBox.Text;
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,async () =>
                    {
                        FolderPicker folderPicker = new FolderPicker();
                        folderPicker.FileTypeFilter.Add("*");
                        folderPicker.CommitButtonText = "选择下载文件夹";
                        StorageFolder saveFolder = await folderPicker.PickSingleFolderAsync();
                        if (folderPicker != null)
                        {
                            danmakuGetter.GetDanmaku(bvid, saveFolder);
                        }
                        else
                        {
                            appInfomation.InfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning;
                            appInfomation.InfoBarTitle = "已取消操作";
                            appInfomation.InfoBarMessage = "已取消操作";
                            appInfomation.IsInfoBarOpen = true;
                        }
                    });
                    break;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selection = e.AddedItems[0].ToString();
            IdProperty = selection;
            if (!IsStarting)
            {
                CheckidTextBoxValue();
            }
        }


        private void idTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckidTextBoxValue();
        }

        private void CheckidTextBoxValue()
        {
            if (string.IsNullOrWhiteSpace(idTextBox.Text))
            {
                getDanmakuButton.IsEnabled = false;
            }
            else if (long.TryParse(idTextBox.Text.ToLower().Replace("av", "").Trim(), out _) == false && IdProperty != "BV号")
            {
                getDanmakuButton.IsEnabled = false;
            }
            else
            {
                getDanmakuButton.IsEnabled = true;
            }
        }
    }
}
