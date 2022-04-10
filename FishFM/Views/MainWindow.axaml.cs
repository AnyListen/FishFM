using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FishFM.ViewModels;

namespace FishFM.Views
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
            var ctx = DataContext;
            if (ctx is MainWindowViewModel model)
            {
                _dataContext = model;
            }
        }

        private void ShareSong(object? sender, RoutedEventArgs e)
        {
            _dataContext?.ShareSong();
        }

        private void LikeSong(object? sender, RoutedEventArgs e)
        {
            _dataContext?.LikeSong();
        }

        private void DislikeSong(object? sender, RoutedEventArgs e)
        {
            _dataContext?.DislikeSong();
        }

        private void ChangeTab(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is not TabControl tab) return;
            if (_dataContext == null) return;
            _dataContext.TabIndex = tab.SelectedIndex;
            _dataContext.RefreshList();
        }
    }
}