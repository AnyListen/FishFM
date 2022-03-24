using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Text.RegularExpressions;
using Alasa.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ManagedBass;

namespace Alasa.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Window_OnClosing(object? sender, CancelEventArgs e)
        {
            Bass.Stop();
            Bass.Free();
        }

        private void TopLevel_OnOpened(object? sender, EventArgs e)
        {
            BassInit();
            InitSongList();
        }

        private void InitSongList()
        {
            var url = "https://ifish.fun/music/daily?t=xm";
            using (var client = new HttpClient())
            {
                var resp = client.GetAsync(url).Result;
                if (resp.IsSuccessStatusCode)
                {
                    var html = resp.Content.ReadAsStringAsync().Result;
                    var matches = Regex.Matches(html, ">(/music/cloud/[^<]+)", RegexOptions.IgnoreCase);
                    ObservableCollection<string> list = new ObservableCollection<string>();
                    foreach (Match match in matches)
                    {
                        list.Add("https://ifish.fun" + match.Groups[1].Value);
                    }
                    var context = (MainWindowViewModel) DataContext;
                    context.SongList = list;
                }
            }
        }

        private void BassInit()
        {
            if (!Bass.Init(-1, 44100, DeviceInitFlags.Frequency))
            {
                return;
            }
            Bass.Configure(Configuration.NetTimeOut, 15000);
            Bass.Configure(Configuration.NetReadTimeOut, 15000);
            PlaySong("https://ifish.fun/music/cloud/wy_320_441491828.mp3?sign=f820c0b75d29c93f76ebeaaed74595fa");
        }

        private int _currentStream = -1;

        private void PlaySong(string url)
        {
            if (_currentStream != -1)
            {
                Bass.StreamFree(_currentStream);
            }
            Console.WriteLine(url);
            _currentStream = Bass.CreateStream(url, 0, BassFlags.Default, CachePlayingSong, IntPtr.Zero);
            if (Bass.ChannelIsActive(_currentStream) == PlaybackState.Stopped)
            {
                Bass.ChannelPlay(_currentStream);
            }
        }
        
        private void CachePlayingSong(IntPtr buffer, int length, IntPtr user)
        {
            // download finish
            if (buffer == IntPtr.Zero)
            {
                //save files
            }
            else
            {
                //copy bytes
                //Marshal.Copy(buffer, new byte[length], 0 ,length);
                Console.WriteLine(buffer.ToInt64()+"/"+length+"#"+user.ToInt64());
            }
            
        }

        private string clickSongUrl = "";
        private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.AddedItems)
            {
                clickSongUrl = item.ToString();
            }

        }

        private void InputElement_OnDoubleTapped(object? sender, RoutedEventArgs e)
        {
            PlaySong(clickSongUrl);
        }
    }
}