using System;
using System.ComponentModel;
using Alasa.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Un4seen.Bass;

namespace Alasa.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? _dataContext;
        
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
            _dataContext?.FreeBass();
        }

        private void NextSong(object? sender, RoutedEventArgs e)
        {
            _dataContext?.PlayNext();
        }

        private void PrevSong(object? sender, RoutedEventArgs e)
        {
            _dataContext?.PlayPrev();
        }

        private void PlaySong(object? sender, RoutedEventArgs e)
        {
            _dataContext?.Play();
        }

        private void PauseSong(object? sender, RoutedEventArgs e)
        {
            _dataContext?.Pause();
        }

        private void TopLevel_OnOpened(object? sender, EventArgs e)
        {
            var ctx = this.DataContext;
            if (ctx is MainWindowViewModel model)
            {
                _dataContext = model;
            }
        }
    }
}