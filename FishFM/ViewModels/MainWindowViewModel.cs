using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using FishFM.Helper;
using FishFM.Models;
using Newtonsoft.Json;
using ReactiveUI;
using Un4seen.Bass;

namespace FishFM.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly Random _random = new();
        private DOWNLOADPROC? _downloadProc;
        private BASSTimer? _updateTimer;
        private int _playCount;
        private int _currentStream;
        private bool _isDiscoveryInit;

        #region Params
        
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
        
        private List<SongResult>? _songList;

        private SongResult? _currentSong;

        public SongResult? CurrentSong
        {
            get => _currentSong;
            private set
            {
                this.RaiseAndSetIfChanged(ref _currentSong, value);
                UpdateLiked();
            }
        }

        private void UpdateLiked()
        {
            Liked = DbHelper.IsSongLiked(CurrentSong);
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

        private int _tabIndex;

        public int TabIndex
        {
            get => _tabIndex;
            set => this.RaiseAndSetIfChanged(ref _tabIndex, value);
        }

        private const string DefaultTip = "有的鱼是永远都关不住的，因为他们属于天空";
        private string _tipText = DefaultTip;
        public string TipText
        {
            get => _tipText;
            set => this.RaiseAndSetIfChanged(ref _tipText, value);
        }

        private bool _liked;
        public bool Liked
        {
            get => _liked;
            set => this.RaiseAndSetIfChanged(ref _liked, value);
        }
        
        
        public MainWindowViewModel()
        {
            InitBassAndSongs();
        }
        
        #endregion
        
        public void PlayPauseMusic()
        {
            if (Playing)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        private void InitBassAndSongs()
        {
            Task.Factory.StartNew(() =>
            {
                BassInit();
                RefreshList();
            });
        }

        private void PlayRandom()
        {
            if (_songList == null)
            {
                return;
            }
            var index = _random.Next(0, _songList.Count);
            CurrentSong = _songList[index];
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
            _updateTimer = new BASSTimer(250);
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
                        PlayNext();
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
            ProcessWidth = 0;
            CurrentPosition = 0;
            try
            {
                if (_playTask != null && _cancellationToken != null)
                {
                    _cancellationToken.Cancel();
                    _playTask.Wait(1000);
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
                Bass.BASS_ChannelStop(_currentStream);
                Bass.BASS_StreamFree(_currentStream);
                _currentStream = 0;
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
                    _currentStream = Bass.BASS_StreamCreateURL(path, 0, BASSFlag.BASS_DEFAULT | BASSFlag.BASS_STREAM_AUTOFREE | BASSFlag.BASS_MUSIC_AUTOFREE, _downloadProc, IntPtr.Zero);
                }
                else
                {
                    _currentStream = Bass.BASS_StreamCreateFile(path, 0, 0, BASSFlag.BASS_DEFAULT | BASSFlag.BASS_STREAM_AUTOFREE | BASSFlag.BASS_MUSIC_AUTOFREE);
                }
                if (Bass.BASS_ChannelIsActive(_currentStream) != BASSActive.BASS_ACTIVE_PLAYING)
                {
                    Bass.BASS_Start();
                }
                if (_currentStream == 0)
                {
                    PlayNext();
                    return;
                }
                if (Bass.BASS_ChannelPlay(_currentStream, true))
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
            if (TabIndex == 1)
            {
                RefreshList();
            }
            else
            {
                PlayRandom();
            }
            _playCount++;
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
            if (_currentStream != 0)
            {
                Bass.BASS_ChannelStop(_currentStream);
                Bass.BASS_StreamFree(_currentStream);
            }
            Bass.BASS_Stop();
            Bass.BASS_Free();
        }

        public void LikeSong()
        {
            if (Liked || CurrentSong == null)
            {
                return;
            }
            DbHelper.LikeSong(CurrentSong);
            UpdateLiked();
        }

        public void DislikeSong()
        {
            if (!Liked || CurrentSong == null)
            {
                return;
            }
            DbHelper.DislikeSong(CurrentSong);
            UpdateLiked();
        }

        public void ShareSong()
        {
            if (CurrentSong == null)
            {
                return;
            }
            var text = CurrentSong.Name + " - " + CurrentSong.ArtistInfo[0].Name 
                       + " <" + CurrentSong.AlbumInfo.Name + ">";
            var task = Application.Current.Clipboard.SetTextAsync(text);
            if (task.Status == TaskStatus.Created)
            {
                task.Start();
            }
            task.Wait();
            if (task.IsCompleted || task.IsCompletedSuccessfully)
            {
                SetTipText("歌曲信息已复制！");
            }
        }

        private void SetTipText(string tip)
        {
            TipText = tip;
            Task.Delay(TimeSpan.FromSeconds(2)).ContinueWith((_ =>
            {
                TipText = DefaultTip;
            }));
        }

        public void RefreshList()
        {
            SetTipText("列表更新中，请稍等...");
            List<SongResult> songs;
            var type = "daily";
            var date = DateTime.Now.ToString("yyyy-MM-dd");
            switch (TabIndex)
            {
                case 0:
                    songs = DbHelper.GetSongs(date, type);
                    if (songs.Count <= 0)
                    {
                        var list = getSongsByType("xm");
                        if (list == null)
                        {
                            return;
                        }
                        songs = list;
                        DbHelper.UpsertSongs(list, date, type);
                    }
                    if (_playCount % 10 == 0)
                    {
                        _isDiscoveryInit = true;
                        Task.Factory.StartNew(() =>
                        {
                            var list = getSongsByType("xm");
                            if (list == null)
                            {
                                return;
                            }
                            songs = list;
                            DbHelper.UpsertSongs(list, date, type);
                            _isDiscoveryInit = false;
                        });
                    }
                    break;
                case 1:
                    var lastSong = DbHelper.GetLastSong("init");
                    if (lastSong == null && !_isDiscoveryInit)
                    {
                        _isDiscoveryInit = true;
                        Task.Factory.StartNew(() =>
                        {
                            var list = getSongsByType("init");
                            if (list == null)
                            {
                                return;
                            }
                            DbHelper.UpsertSongs(list, date, "init");
                            _isDiscoveryInit = false;
                        });
                    }
                    songs = DbHelper.GetRandomSongs();
                    break;
                case 2:
                    var ids = DbHelper.GetAllLikedSong();
                    songs = ids.Count > 0 ? DbHelper.GetSongsByIds(ids) : DbHelper.GetRandomSongs();
                    break;
                default:
                    songs = DbHelper.GetRandomSongs();
                    break;
            }
            _songList = songs;
            PlayRandom();
        }

        private List<SongResult>? getSongsByType(string type)
        {
            var url = "https://ifish.fun/api/music/daily?t="+type;
            using var client = new HttpClient();
            var resp = client.GetAsync(url).Result;
            if (!resp.IsSuccessStatusCode) return null;
            var html = resp.Content.ReadAsStringAsync().Result;
            return string.IsNullOrEmpty(html) ? null : JsonConvert.DeserializeObject<List<SongResult>>(html);
        }
    }
}