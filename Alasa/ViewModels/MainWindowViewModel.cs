using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Alasa.Views;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ImageMagick;
using ReactiveUI;

namespace Alasa.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Welcome!";

        private ObservableCollection<string> _songList = new ObservableCollection<string>();
        public ObservableCollection<string> SongList { get => _songList; set => this.RaiseAndSetIfChanged(ref _songList, value); }

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
    }
}