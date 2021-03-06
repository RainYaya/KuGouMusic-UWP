﻿using ImageUtility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace KuGouUWP.Pages.Setting
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SkinSetPage : Page
    {
        public SkinSetPage()
        {
            this.InitializeComponent();
            init();
        }

        private void init()
        {
            ThemeList.ItemsSource = Theme.GetThemes();
            ThemeList.SelectionMode = ListViewSelectionMode.Single;
            ThemeList.SelectionChanged += ThemeList_SelectionChanged;
        }

        private async void ThemeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as GridView;
            if (list.SelectedItem != null)
            {
                var data = list.SelectedItem as Theme;
                if (data.id == 4)
                {
                    Process.IsActive = true;
                    await BingThemeData.GetBingTheme();
                    Process.IsActive = false;
                }
                Class.Setting.Theme.NowTheme = data.theme;
                ShowDialog();
                list.SelectedIndex = -1;
                init();
            }
        }

        

        public class BingThemeData
        {
            public Color BackgroundColor { get; set; }
            public Color BackgroundColor1 { get; set; }
            public Color BackgroundColor2 { get; set; }
            public Color Foreground { get; set; }
            public Color Front1 { get; set; }
            public Color Front2 { get; set; }
            public Color List_Background { get; set; }
            public static async Task GetBingTheme()
            {
                try
                {
                    var httpclient = new System.Net.Http.HttpClient();
                    var picbytes = await httpclient.GetByteArrayAsync("http://appserver.m.bing.net/BackgroundImageService/TodayImageService.svc/GetTodayImage?dateOffset=0&urlEncodeHeaders=true&osName=windowsPhone&osVersion=8.10&orientation=480x800&deviceName=WP8&mkt=en-US");
                    var picfile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Theme/Skin/Bing/background.jpg"));
                    await FileIO.WriteBytesAsync(picfile, picbytes);
                    WriteableBitmap wb = new WriteableBitmap(1000, 1500);
                    await wb.SetSourceAsync(await picfile.OpenAsync(FileAccessMode.Read));
                    var bingtheme = new BingThemeData();
                    bingtheme.BackgroundColor = GetColor.GetMajorColor(wb);
                    bingtheme.BackgroundColor1 = Color.FromArgb(190, bingtheme.BackgroundColor.R, bingtheme.BackgroundColor.G, bingtheme.BackgroundColor.B);
                    var rgb = bingtheme.BackgroundColor.R * 0.299 + bingtheme.BackgroundColor.G * 0.587 + bingtheme.BackgroundColor.B * 0.114;
                    bingtheme.BackgroundColor2 = bingtheme.BackgroundColor;
                    if (rgb >= 192) {
                        bingtheme.Foreground = Colors.White;
                        bingtheme.Front1 = Colors.White;
                        bingtheme.Front2 = Color.FromArgb(255, 185, 185, 185);
                        bingtheme.List_Background = Color.FromArgb(100, 103, 103, 103);
                    }
                    else
                    {
                        bingtheme.Foreground = Colors.White;
                        bingtheme.Front1 = Colors.Black;
                        bingtheme.Front2 = Color.FromArgb(255, 124, 124, 124);
                        bingtheme.List_Background = Color.FromArgb(180, 255, 255, 255);
                    }
                    string xamlstring = await PathIO.ReadTextAsync("ms-appx:///Theme/BingTemp.txt");
                    xamlstring = String.Format(xamlstring,bingtheme.BackgroundColor.ToString(),bingtheme.BackgroundColor1.ToString(),bingtheme.BackgroundColor2.ToString(),bingtheme.Foreground.ToString(),bingtheme.Front1.ToString(), bingtheme.Front2.ToString(),bingtheme.List_Background.ToString());
                    var xamlfile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Theme/BingTheme.xaml"));
                    await FileIO.WriteTextAsync(xamlfile, xamlstring);
                }
                catch (Exception)
                {
                    await new MessageDialog("无网络连接!").ShowAsync();
                }
            }
        }

        private async void ShowDialog()
        {
            var dialog = new MessageDialog(ResourceLoader.GetForCurrentView().GetString("ThemeLoadDialog"));
            dialog.Commands.Add(new UICommand(ResourceLoader.GetForCurrentView().GetString("RestartBtn"), DialogHandler,0));
            dialog.Commands.Add(new UICommand(ResourceLoader.GetForCurrentView().GetString("LaterBtn"), DialogHandler, 1));
            await dialog.ShowAsync();
        }

        private void DialogHandler(IUICommand command)
        {
            var num = (int)(command.Id);
            if (num == 0)
            {
                Application.Current.Exit();
            }
        }

        public class Theme
        {
            public int id { get; set; }
            public string title { get; set; }
            public string pic { get; set; }
            public string isnow
            {
                get
                {
                    if (theme == Class.Setting.Theme.NowTheme)
                    {
                        return "Visible";
                    }
                    else
                    {
                        return "Collapsed";
                    }
                }
            }
            public Class.Setting.Theme.Type theme
            {
                get
                {
                    return (Class.Setting.Theme.Type)id;
                }
            }
            public static ObservableCollection<Theme> GetThemes()
            {
                var result = new ObservableCollection<Theme>();
                result.Add(new Theme { id=0,title="默认皮肤",pic= "ms-appx:///Theme/Skin/Default/thumb.png" });
                result.Add(new Theme { id = 1, title = "碧水蓝", pic = "ms-appx:///Theme/Skin/BiShuiLan/thumb.png" });
                result.Add(new Theme { id = 2, title = "星空", pic = "ms-appx:///Theme/Skin/StarNight/thumb.png" });
                result.Add(new Theme { id = 3, title = "兔子的梦境", pic = "ms-appx:///Theme/Skin/Rabbit/thumb.png" });
                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {
                    result.Add(new Theme { id = 4, title = "Bing主题", pic = "ms-appx:///Theme/Skin/Bing/thumb.jpg" });
                }
                return result;
            }
        }

        private void BackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
