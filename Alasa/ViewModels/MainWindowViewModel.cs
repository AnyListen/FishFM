using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Alasa.Models;
using Alasa.Views;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ImageMagick;
using Newtonsoft.Json;
using ReactiveUI;
using Un4seen.Bass;

namespace Alasa.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly Random _random = new();
        private DOWNLOADPROC? _downloadProc;
        private BASSTimer? _updateTimer;
        private int _currentStream;
        

        private IBitmap? _albumPic;

        public IBitmap? AlbumPic
        {
            get => _albumPic;
            set => this.RaiseAndSetIfChanged(ref _albumPic, value); 
        }

        private bool _playing;

        public bool Playing
        {
            get => _playing;
            set => this.RaiseAndSetIfChanged(ref _playing, value); 
        }
        
        private ObservableCollection<SongResult>? _songList;

        private ObservableCollection<SongResult>? SongList 
        { 
            get => _songList;
            set => this.RaiseAndSetIfChanged(ref _songList, value); 
        }

        private SongResult? _currentSong;

        public SongResult? CurrentSong
        {
            get => _currentSong;
            private set => this.RaiseAndSetIfChanged(ref _currentSong, value); 
        }
        
        public MainWindowViewModel()
        {
            InitBassAndSongs();
        }

        private double _currentPosition;

        public double CurrentPosition
        {
            get => _currentPosition;
            set => this.RaiseAndSetIfChanged(ref _currentPosition, value);
        }
        
        private double _trackLength;

        public double TrackLength
        {
            get => _trackLength;
            set
            {
                if (Math.Abs(_trackLength - value) < 0.5)
                {
                    return;
                }
                this.RaiseAndSetIfChanged(ref _trackLength, value);
            }
        }
        
        private double _processWidth;

        public double ProcessWidth
        {
            get => _processWidth;
            set => this.RaiseAndSetIfChanged(ref _processWidth, value);
        }
        
        public void ShowCapture()
        {
            var image = new MagickImage();
            image.Read("SCREENSHOT", MagickFormat.Screenshot);
            image.Format = MagickFormat.Bmp;
            var window = new CaptureWindow();
            var ms = new MemoryStream();
            image.Write(ms);
            window.Background = new ImageBrush(new Bitmap(ms));
            window.Show();
        }

        private void InitBassAndSongs()
        {
            Task.Factory.StartNew(() =>
            {
                BassInit();
                InitSongs();
            });
        }

        private void InitSongs()
        {
            var url = "https://ifish.fun/api/music/daily?t=xm";
            using var client = new HttpClient();
            var resp = client.GetAsync(url).Result;
            if (!resp.IsSuccessStatusCode) return;
            var html = resp.Content.ReadAsStringAsync().Result;
            if (String.IsNullOrEmpty(html))
            {
                return;
            }
            var list = JsonConvert.DeserializeObject<ObservableCollection<SongResult>>(html);
            if (list == null)
            {
                return;
            }
            SongList = list;
            PlayRandom();
        }

        private void PlayRandom()
        {
            if (SongList == null)
            {
                return;
            }
            var index = _random.Next(0, SongList.Count);
            CurrentSong = SongList[index];
            PlaySong(CurrentSong);
        }

        private void BassInit()
        {
            BassNet.Registration("shelher@163.com", "2X2831371512622");
            if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
            {
                return;
            }
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_NET_TIMEOUT, 15000);
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_NET_READTIMEOUT, 15000);
            _updateTimer = new BASSTimer(200);
            _updateTimer.Tick += UpdateTimerOnTick;
            _downloadProc = CachePlayingSong;
        }

        private void UpdateTimerOnTick(object? sender, EventArgs e)
        {
            if (_currentStream == 0)
            {
                return;
            }
            switch (Bass.BASS_ChannelIsActive(_currentStream))
            {
                case BASSActive.BASS_ACTIVE_PAUSED:
                case BASSActive.BASS_ACTIVE_STOPPED:
                    Playing = false;
                    if ((int)ProcessWidth >= 9960)
                    {
                        PlayRandom();
                    }
                    break;
                case BASSActive.BASS_ACTIVE_STALLED:
                case BASSActive.BASS_ACTIVE_PLAYING:
                    if (!Playing)
                    {
                        Playing = true;
                    }
                    var pos = Bass.BASS_ChannelGetPosition(_currentStream);
                    CurrentPosition = Bass.BASS_ChannelBytes2Seconds(_currentStream, pos);
                    ProcessWidth = Math.Ceiling(CurrentPosition * 10000.0 / TrackLength);
                    break;
            }
        }

        private Task? _playTask;
        private CancellationTokenSource? _cancellationToken;
        private void PlaySong(SongResult? songResult)
        {
            if (songResult == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(songResult.CopyUrl))
            {
                return;
            }
            try
            {
                if (_playTask != null && _cancellationToken != null)
                {
                    _cancellationToken.Cancel();
                    _playTask.Wait();
                    _playTask.Dispose();
                    _cancellationToken.Dispose();
                }
            }
            catch (Exception)
            {
                //
            }
            _cancellationToken = new CancellationTokenSource();
            if (_currentStream != 0)
            {
                Bass.BASS_StreamFree(_currentStream);
            }
            _playTask = Task.Factory.StartNew(() =>
            {
                if (CurrentSong == null)
                {
                    return;
                }
                var path = "https://ifish.fun" + CurrentSong.CopyUrl;
                using (var client = new HttpClient())
                {
                    var imgArr = client.GetByteArrayAsync(CurrentSong.PicUrl).Result;
                    using (var ms = new MemoryStream(imgArr))
                    {
                        if (AlbumPic != null)
                        {
                            AlbumPic.Dispose();
                            AlbumPic = null;
                        }
                        AlbumPic = new Bitmap(ms);
                    }
                }
                if (path.ToLower().Contains("http"))
                {
                    _currentStream = Bass.BASS_StreamCreateURL(path, 0, BASSFlag.BASS_DEFAULT, _downloadProc, IntPtr.Zero);
                }
                else
                {
                    _currentStream = Bass.BASS_StreamCreateFile(path, 0, 0, BASSFlag.BASS_DEFAULT);
                }
                if (Bass.BASS_ChannelIsActive(_currentStream) != BASSActive.BASS_ACTIVE_PLAYING)
                {
                    Bass.BASS_Start();
                }
                if (_currentStream != 0 && Bass.BASS_ChannelPlay(_currentStream, true))
                {
                    TrackLength = Bass.BASS_ChannelBytes2Seconds(_currentStream,
                        Bass.BASS_ChannelGetLength(_currentStream));
                    if (_updateTimer is {Enabled: false})
                    {
                        _updateTimer.Start();
                    }
                    return;
                }
                _currentStream = 0;
                Bass.BASS_Stop();
            }, _cancellationToken.Token);
        }
        
        private void CachePlayingSong(IntPtr buffer, int length, IntPtr user)
        {
            // download finish
            if (buffer == IntPtr.Zero)
            {
                //save files
                Console.WriteLine("Cache Done!");
            }
            else
            {
                //copy bytes
                //Marshal.Copy(buffer, new byte[length], 0 ,length);
                //Console.WriteLine(buffer.ToInt64()+"/"+length+"#"+user.ToInt64());
            }
        }

        public void PlayNext()
        {
            PlayRandom();
        }

        public void PlayPrev()
        {
            PlayRandom();
        }

        public void Play()
        {
            Bass.BASS_ChannelPlay(_currentStream, false);
        }
        
        public void Pause()
        {
            Bass.BASS_ChannelPause(_currentStream);
        }

        public void FreeBass()
        {
            if (_updateTimer != null)
            {
                _updateTimer.Stop();
                _updateTimer.Dispose();
            }
            Bass.BASS_Stop();
            Bass.BASS_Free();
        }
    }
}